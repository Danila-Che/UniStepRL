using System;
using System.Collections.Generic;
using System.Linq;
using Unity.InferenceEngine;
using Random = Unity.Mathematics.Random;
using static Unity.Mathematics.math;
using System.Runtime.CompilerServices;

namespace UniStepRL.Agent
{
    public class DRLAgent : IAgent, IDisposable
    {
        private class StateNode
        {
            public EnvironmentState State;

            public int N; // Σ_b N(s,b) Visits
            public float V; // V(s)
            public bool IsTerminated;

            // Parent:
            public ActionNode ParentActionNode;

            // Children:
            public List<ActionNode> ActionNodes;
        }
        private class ActionNode
        {
            public EnvironmentAction Action;

            public float Logit; // logits(a) // log softmax (π)
            public float P; // P(s,a)
            public float G; // g(a)
            public float W; // W(s,a)
            public int N; // N(s,a)
            
            public float Q => N == 0 ? 0 : W / N; // Q(s,a)

            // Parent:
            public StateNode ParentStateNode;

            // Children:
            public StateNode NextStateNode;

            public float SigmaQ() // σ(Q(s,a))
            {
                // TODO: normalize.
                return Q;
            }

            public float PUCT(float c = 1.4f)
            {
                return Q + c * P * sqrt(ParentStateNode.N) / (1.0f + N); // Q(s,a) + U(s,a)
            }
        }
       
        private readonly AgentModel m_AgentModel;
        private readonly int m_SimulationBudget;

        private Random m_AgentRandom;
        private bool m_Disposed;

        public DRLAgent(ModelAsset modelAsset, int simulationBudget, uint seed = 0)
        {
            m_AgentModel = new AgentModel(modelAsset);
            m_SimulationBudget = simulationBudget;
            
            seed = seed == 0 ? (uint)DateTime.Now.Ticks : seed;
            m_AgentRandom = new Random(seed);
        }

        public void Dispose()
        {
            if (!m_Disposed)
            {
                m_AgentModel?.Dispose();
                m_Disposed = true;
            }
        }

        public EnvironmentAction Simulate(SimpleEnvironment environment, EnvironmentState rootState)
        {
            if (m_Disposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }

            var input = AgentModel.ToInput(environment, rootState);
            var output = m_AgentModel.Evaluate(input);

            var root = InitializeNode(rootState, output);
            SelectActions(environment, root, output);
            SelectTopKActionsUsingGumbelNoise(root, k: 16);

            var actionNodes = root.ActionNodes;

            while (actionNodes.Count > 1)
            {
                var simsPerAction = max(1, m_SimulationBudget / actionNodes.Count);

                for (int i = 0; i < actionNodes.Count; i++)
                {
                    for (int s = 0; s < simsPerAction; s++)
                    {
                        var selectedActionNode = SelectActionNode(actionNodes[i]);
                        var expandedStateNode = ExpandActionNode(environment, selectedActionNode);

                        Backpropagate(expandedStateNode);
                    }
                }

                actionNodes = PerformSequentialHalving(actionNodes);
            }

            // return root.ActionNodes[0].Action;
            // return actionNodes[0].Action;
            return SelectBestAction(root);
        }

        private void SelectTopKActionsUsingGumbelNoise(StateNode stateNode, int k)
        {
            foreach (var actionNode in stateNode.ActionNodes)
            {
                actionNode.G = SampleStandardGumbel();
            }
            
            if (stateNode.ActionNodes.Count > k)
            {
                stateNode.ActionNodes = stateNode.ActionNodes
                    .OrderByDescending(a => a.Logit + a.G)
                    .Take(k)
                    .ToList();
            }
        }

        private void PerformSequentialHalving(StateNode stateNode)
        {
            var k = stateNode.ActionNodes.Count / 2;

            var maxQ = stateNode.ActionNodes[0].Q;
            var minQ = stateNode.ActionNodes[0].Q;

            for (int i = 1; i < stateNode.ActionNodes.Count; i++)
            {
                var q = stateNode.ActionNodes[i].Q;

                if (q > maxQ)
                {
                    maxQ = q;
                }

                if (q < minQ)
                {
                    minQ = q;
                }
            }

            var m = (maxQ - minQ) > 0.0f ? 1.0f / (maxQ - minQ) : 0.0f;

            stateNode.ActionNodes = stateNode.ActionNodes
                .OrderByDescending(a => a.Logit + a.G + (a.Q - minQ) * m)
                .Take(k)
                .ToList();
        }

        private List<ActionNode> PerformSequentialHalving(List<ActionNode> actionNodes)
        {
            var k = actionNodes.Count / 2;

            var maxQ = actionNodes[0].Q;
            var minQ = actionNodes[0].Q;

            for (int i = 1; i < actionNodes.Count; i++)
            {
                var q = actionNodes[i].Q;

                if (q > maxQ)
                {
                    maxQ = q;
                }

                if (q < minQ)
                {
                    minQ = q;
                }
            }

            var m = (maxQ - minQ) > 0.0f ? 1.0f / (maxQ - minQ) : 0.0f;

            return actionNodes
                .OrderByDescending(a => a.Logit + a.G + (a.Q - minQ) * m)
                .Take(k)
                .ToList();
        }

        private EnvironmentAction SelectBestAction(StateNode stateNode)
        {
            var selectedActionNode = stateNode.ActionNodes[0];
            var maxN = stateNode.ActionNodes[0].N;

            for (int i = 1; i < stateNode.ActionNodes.Count; i++)
            {
                var n = stateNode.ActionNodes[i].N;

                if (n > maxN)
                {
                    maxN = n;
                    selectedActionNode = stateNode.ActionNodes[i];
                }
            }

            return selectedActionNode.Action;
        }

        #region Root initialization and Gumbel action selection

        private StateNode InitializeNode(EnvironmentState rootState, AgentModel.Output output)
        {
            return new StateNode()
            {
                State = rootState,
                V = output.V,
                ParentActionNode = null,
                ActionNodes = new List<ActionNode>(),
            };
        }

        private void SelectActions(SimpleEnvironment environment, StateNode rootNode, AgentModel.Output output)
        {
            var actionTypeLogits = output.ActionTypeOutput;
            var actionTypeMask = environment.GetActionTypeMask(rootNode.State);
            var probabilities = Softmax(actionTypeLogits, actionTypeMask);

            for (int i = 0; i < actionTypeLogits.Length; i++)
            {
                if (actionTypeMask[i] == 1)
                {
                    switch ((ActionType)i)
                    {
                        case ActionType.BuildTower:
                            ExpandBuildTower(environment, rootNode, output, probabilities[i]);
                            break;

                        case ActionType.PlaceUnit:
                            ExpandPlaceUnit(environment, rootNode, output, probabilities[i]);
                            break;

                        case ActionType.MoveUnit:
                            ExpandMoveUnit(environment, rootNode, output, probabilities[i]);
                            break;
                            
                        case ActionType.EndTurn:
                            ExpandEndTurn(rootNode, probabilities[i]);
                            break;
                    }
                }
            }
        }

        private void ExpandBuildTower(
            SimpleEnvironment environment,
            StateNode rootStateNode,
            AgentModel.Output output,
            float rootProbability)
        {
            const int kP0 = 4;

            var logitsP0 = output.BuildTowerP0Output;
            var maskP0 = environment.GetBuildTowerMask(rootStateNode.State);
            var topKP0 = Argtop(logitsP0, maskP0, kP0);
            var probabilitiesP0 = Softmax(logitsP0, maskP0);

            var rootLogProbability = log(rootProbability);

            for (int i = 0; i < topKP0.Length; i++)
            {
                var parameter0 = topKP0[i];
                var probability0 = probabilitiesP0[parameter0.Row, parameter0.Col];

                var action = new EnvironmentAction
                {
                    Type = ActionType.BuildTower,
                    BuildTowerP0 = parameter0,
                };

                var actionNode = new ActionNode
                {
                    Action = action,
                    Logit = rootLogProbability + log(probability0),
                    P = rootProbability * probability0,
                };

                rootStateNode.ActionNodes.Add(actionNode);
                actionNode.ParentStateNode = rootStateNode;
            }
        }

        private void ExpandPlaceUnit(
            SimpleEnvironment environment,
            StateNode rootStateNode,
            AgentModel.Output output,
            float rootProbability)
        {
            const int kP0 = 4;
            const int kP1 = 4;

            var logitsP0 = output.PlaceUnitP0Output;
            var logitsP1 = output.PlaceUnitP1Output;

            var maskP0 = environment.GetPlaceUnitP0Mask(rootStateNode.State);
            var topKP0 = Argtop(logitsP0, maskP0, kP0);
            var probabilitiesP0 = Softmax(logitsP0, maskP0);

            var rootLogProbability = log(rootProbability);

            for (int i = 0; i < topKP0.Length; i++)
            {
                var parameter0 = IndexToUnit(topKP0[i]);
                var probability0 = probabilitiesP0[topKP0[i]];

                var maskP1 = environment.GetPlaceUnitP1Mask(rootStateNode.State, parameter0);
                var topKP1 = Argtop(logitsP1, maskP1, kP1);
                var probabilitiesP1 = Softmax(logitsP1, maskP1);

                for (int j = 0; j < topKP1.Length; j++)
                {
                    var parameter1 = topKP1[j];
                    var probability1 = probabilitiesP1[parameter1.Row, parameter1.Col];

                    var action = new EnvironmentAction
                    {
                        Type = ActionType.PlaceUnit,
                        PlaceUnitP0 = parameter0,
                        PlaceUnitP1 = parameter1,
                    };

                    var actionNode = new ActionNode
                    {
                        Action = action,
                        Logit = rootLogProbability + log(probability0) + log(probability1),
                        P = rootProbability * probability0 * probability1,
                    };

                    rootStateNode.ActionNodes.Add(actionNode);
                    actionNode.ParentStateNode = rootStateNode;
                }
            }
        }

        private void ExpandMoveUnit(
            SimpleEnvironment environment,
            StateNode rootStateNode,
            AgentModel.Output output,
            float rootProbability)
        {
            const int kP0 = 4;
            const int kP1 = 4;

            var logitsP0 = output.MoveUnitP0Output;
            var logitsP1 = output.MoveUnitP1Output;

            var maskP0 = environment.GetMoveUnitP0Mask(rootStateNode.State);
            var topKP0 = Argtop(logitsP0, maskP0, kP0);
            var probabilitiesP0 = Softmax(logitsP0, maskP0);

            var rootLogProbability = log(rootProbability);

            for (int i = 0; i < topKP0.Length; i++)
            {
                var parameter0 = topKP0[i];
                var probability0 = probabilitiesP0[parameter0.Row, parameter0.Col];

                var maskP1 = environment.GetMoveUnitP1Mask(rootStateNode.State, parameter0);
                var topKP1 = Argtop(logitsP1, maskP1, kP1);
                var probabilitiesP1 = Softmax(logitsP1, maskP1);

                for (int j = 0; j < topKP1.Length; j++)
                {
                    var parameter1 = topKP1[j];
                    var probability1 = probabilitiesP1[parameter1.Row, parameter1.Col];

                    var action = new EnvironmentAction
                    {
                        Type = ActionType.MoveUnit,
                        MoveUnitP0 = parameter0,
                        MoveUnitP1 = parameter1,
                    };

                    var actionNode = new ActionNode
                    {
                        Action = action,
                        Logit = rootLogProbability + log(probability0) + log(probability1),
                        P = rootProbability * probability0 * probability1,
                    };

                    rootStateNode.ActionNodes.Add(actionNode);
                    actionNode.ParentStateNode = rootStateNode;
                }
            }
        }

        private void ExpandEndTurn(
            StateNode rootStateNode,
            float rootProbability)
        {
            var action = new EnvironmentAction
            {
                Type = ActionType.EndTurn,
            };

            var actionNode = new ActionNode
            {
                Action = action,
                Logit = log(rootProbability),
                P = rootProbability,
            };

            rootStateNode.ActionNodes.Add(actionNode);
            actionNode.ParentStateNode = rootStateNode;
        }

        #endregion

        #region Simulation

        private ActionNode SelectActionNode(ActionNode actionNode)
        {
            while (!(actionNode.NextStateNode == null || actionNode.NextStateNode.IsTerminated))
            {
                var actionNodes = actionNode.NextStateNode.ActionNodes;
                var selectedActionNode = actionNodes[0];
                var maxPUCT = selectedActionNode.PUCT();

                for (int i = 1; i < actionNodes.Count; i++)
                {
                    var puct = actionNodes[i].PUCT();

                    if (puct > maxPUCT)
                    {
                        selectedActionNode = actionNodes[i];
                        maxPUCT = puct;
                    }
                }

                actionNode = selectedActionNode;
            }

            return actionNode;
        }

        private StateNode ExpandActionNode(SimpleEnvironment environment, ActionNode actionNode)
        {
            var nextStateNode = actionNode.NextStateNode;

            if (nextStateNode != null)
            {
                if (nextStateNode.IsTerminated)
                {
                    return nextStateNode;
                }
                else
                {
                    throw new InvalidOperationException();
                }
            }

            var (nextState, isTerminated) = environment.Step(actionNode.ParentStateNode.State, actionNode.Action);

            var input = AgentModel.ToInput(environment, nextState);
            var output = m_AgentModel.Evaluate(input);

            nextStateNode = new StateNode
            {
                State = nextState,
                N = 0,
                V = isTerminated ? 1.0f : output.V, // If the last move is terminal, then it is obvious that whoever made the last move won.
                IsTerminated = isTerminated,
                ParentActionNode = actionNode,
            };

            actionNode.NextStateNode = nextStateNode;

            if (!nextStateNode.IsTerminated)
            {
                nextStateNode.ActionNodes = new List<ActionNode>();

                SelectActions(environment, nextStateNode, output);
                SelectTopKActions(nextStateNode, 8);
            }

            return actionNode.NextStateNode;
        }

        private void SelectTopKActions(StateNode stateNode, int k)
        {
            if (stateNode.ActionNodes.Count <= k)
            {
                return;
            }

            stateNode.ActionNodes = stateNode.ActionNodes
                .OrderByDescending(a => a.Logit)
                .Take(k)
                .ToList();
        }

        private void Backpropagate(StateNode stateNode)
        {
            var v = stateNode.V;
            var player = stateNode.State.CurrentPlayer;

            while (true)
            {
                var actionNode = stateNode.ParentActionNode;

                if (actionNode == null) // Reached root state node.
                {
                    break;
                }

                actionNode.N++;

                var currentPlayerWon = actionNode.ParentStateNode.State.CurrentPlayer == player;

                if (currentPlayerWon)
                {
                    actionNode.W += v;
                }
                else
                {
                    actionNode.W -= v;
                }

                stateNode = actionNode.ParentStateNode;
                stateNode.N++;
            }
        }

        #endregion

        #region Helpers

        private static float[,] Softmax(float[,] logits, int[,] mask)
        {
            var rows = logits.GetLength(0);
            var cols = logits.GetLength(1);

            var result = new float[rows, cols];

            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    if (mask[row, col] == 1)
                    {
                        result[row, col] = exp(logits[row, col]);
                    }
                }
            }

            var sumExp = 0.0f;

            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    if (mask[row, col] == 1)
                    {
                        sumExp += result[row, col];
                    }
                }
            }

            if (sumExp == 0.0f)
            {
                return null;
            }

            sumExp = 1.0f / sumExp;

            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    if (mask[row, col] == 1)
                    {
                        result[row, col] *= sumExp;
                    }
                }
            }

            return result;
        }

        private static float[] Softmax(float[] logits, int[] mask)
        {
            var result = new float[logits.Length];

            for (int i = 0; i < logits.Length; i++)
            {
                if (mask[i] == 1)
                {
                    result[i] = exp(logits[i]);
                }
            }

            var sumExp = 0.0f;

            for (int i = 0; i < logits.Length; i++)
            {
                sumExp += result[i];
            }

            if (sumExp == 0.0f)
            {
                return null;
            }

            sumExp = 1.0f / sumExp;

            for (int i = 0; i < logits.Length; i++)
            {
                if (mask[i] == 1)
                {
                    result[i] *= sumExp;
                }
            }

            return result;
        }
        
        private float SampleStandardGumbel()
        {
            var u = m_AgentRandom.NextFloat();

            u = clamp(u, EPSILON, 1.0f - EPSILON);

            return -log(-log(u)); // U ~ Uniform(0,1) // mu = 0 beta = 1 // mu - beta * log(-log(U))
        }

        private int[] Argtop(float[] scores, int[] mask, int k)
        {
            return Enumerable.Range(0, scores.Length)
                .Where(i => mask[i] == 1)
                .OrderByDescending(i => scores[i])
                .Take(k)
                .ToArray();
        }

        private OffsetCoordinates[] Argtop(float[,] logits, int[,] mask, int k)
        {
            var rows = logits.GetLength(0);
            var cols = logits.GetLength(1);

            var candidates = new List<(float logit, int row, int col)>();
            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    if (mask[row, col] != 0)
                    {
                        candidates.Add((logits[row, col], row, col));
                    }
                }
            }

            if (candidates.Count <= k)
            {
                return candidates
                    .OrderByDescending(x => x.logit)
                    .Select(x => new OffsetCoordinates(x.col, x.row))
                    .ToArray();
            }

            return candidates
                .OrderByDescending(x => x.logit)
                .Take(k)
                .Select(x => new OffsetCoordinates(x.col, x.row))
                .ToArray();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Unit IndexToUnit(int index)
        {
            return index switch
            {
                0 => Unit.Peasant,
                1 => Unit.Spearman,
                2 => Unit.Knight,
                3 => Unit.Baron,
                _ => throw new ArgumentOutOfRangeException(nameof(index), index, null)
            };
        }

        #endregion
    }
}
