using System.Collections.Generic;
using System.Diagnostics;
using UniStepRL.Agent;
using Unity.InferenceEngine;
using UnityEngine;
using UnityEngine.InputSystem;
using Debug = UnityEngine.Debug;

namespace UniStepRL
{
    [RequireComponent(typeof(EnvironmentRenderer))]
    public class Demo : MonoBehaviour
    {
        private enum AgentType
        {
            DRL,
            Random,
        }

        [Header("Map")]
        [SerializeField] private int m_Seed = -1;
        [Space]
        [Min(5)]
        [SerializeField] private int m_Width = 5;
        [Min(5)]
        [SerializeField] private int m_Height = 5;
        
        [Header("UI")]
        [SerializeField] private DemoView m_DemoView;

        [Header("Environment")]
        [Min(1)]
        [SerializeField] private int m_TimeLimit = 100;

        [Header("Agent")]
        [SerializeField] private AgentType m_Player1AgentType;
        [SerializeField] private AgentType m_Player2AgentType;
        
        [Header("Random Agent Configuration")]
        [SerializeField] private int m_RandomAgentSeed = -1;


        [Header("DRL Agent Configuration")]
        [SerializeField] private uint m_DRLAgentSeed = 0;
        [SerializeField] private ModelAsset m_AgentModelAsset;
        [Min(0)]
        [SerializeField] private int m_SimulationBudget = 64;

        private EnvironmentConfiguration m_Config;
        private SimpleEnvironment m_Environment;
        private int m_CurrentStep;

        // s -> a -> s -> a -> s
        private List<EnvironmentState> m_States;
        private List<EnvironmentAction> m_Actions;
        
        private EnvironmentRenderer m_Renderer;
        
        private DRLAgent m_DRLAgent;
        private RandomAgent m_RandomAgent;
        
        private System.Random m_MapRandom;

        private void OnEnable()
        {
            var mapSeed = m_Seed == -1 ? new System.Random().Next() : m_Seed;

            m_MapRandom = new System.Random(mapSeed);
            
            m_CurrentStep = 0;

            m_Config = new EnvironmentConfiguration(m_Width, m_Height);
            m_Environment = new SimpleEnvironment(m_Config);
            
            m_States = new List<EnvironmentState>();
            m_Actions = new List<EnvironmentAction>();
            
            m_Renderer = GetComponent<EnvironmentRenderer>();
            m_Renderer.SeedState(m_Config, m_Environment);
            
            m_DemoView.SetMapSize(m_Config.Width, m_Config.Height);
            m_DemoView.SetMapSeed(mapSeed);

            m_RandomAgent = new RandomAgent(m_RandomAgentSeed);
            m_DRLAgent = new DRLAgent(m_AgentModelAsset, m_SimulationBudget, m_DRLAgentSeed);
        }

        private void Start()
        {
            var state = m_Environment.Reset();
            m_States.Add(state);

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var timeLimit = GenerateRandomTrajectory();
            stopwatch.Stop();
            
            UpdateStep();
            
            m_DemoView.SetTimeLimit(timeLimit);

            Debug.Log($"{nameof(GenerateRandomTrajectory)}: {stopwatch.ElapsedMilliseconds} ms");
        }

        private void OnDisable()
        {
            m_DRLAgent.Dispose();
        }

        // private void Update()
        // {
        //     if (Keyboard.current.rightArrowKey.wasPressedThisFrame)
        //     {
        //         GoToNextStep();
        //     }

        //     if (Keyboard.current.leftArrowKey.wasPressedThisFrame)
        //     {
        //         GoToPreviousStep();
        //     }
        // }

        public void GoToStep(float step)
        {
            m_CurrentStep = (int)step;
            
            UpdateStep();
        }

        public void GoToNextStep()
        {
            m_CurrentStep++;

            UpdateStep();
        }

        public void GoToPreviousStep()
        {
            m_CurrentStep--;
            
            UpdateStep();
        }

        private void UpdateStep()
        {
            var currentStep = Mathf.Clamp(m_CurrentStep, 0, m_States.Count - 1);
            
            m_DemoView.SetCurrentStep(currentStep);

            if (currentStep == m_CurrentStep)
            {
                var state = m_States[currentStep];
                
                m_Renderer.UpdateState(m_Config, state);

                var actionIndex = m_CurrentStep - 1;
                
                if (actionIndex < 0)
                {
                    m_DemoView.SetNoneAction();
                }
                else
                {
                    var playerType = m_States[m_CurrentStep - 1].CurrentPlayer switch
                    {
                        Player.Player1 => m_Player1AgentType,
                        Player.Player2 => m_Player2AgentType,
                        _ => throw new System.NotImplementedException(),
                    };
                    m_DemoView.SetAction(m_States[m_CurrentStep - 1].CurrentPlayer, playerType.ToString(), m_Actions[actionIndex]);
                }
                
                m_DemoView.SetGold(Player.Player1, state.Player1Stats.GoldAmount);
                m_DemoView.SetGold(Player.Player2, state.Player2Stats.GoldAmount);
                m_DemoView.SetEntityLUT(state.EntitiesLookUpTable);
            }
            
            m_CurrentStep = currentStep;
        }

        private int GenerateRandomTrajectory()
        {
            var state = m_States[0];

            for (int i = 0; i < m_TimeLimit; i++)
            {
                m_DemoView.UpdateSelfPlayProgress(i, m_TimeLimit);

                var agent = state.CurrentPlayer switch
                {
                    Player.Player1 => GetAgent(m_Player1AgentType),
                    Player.Player2 => GetAgent(m_Player2AgentType),
                    _ => throw new System.NotImplementedException(),
                };
                
                var action = agent.Simulate(m_Environment, state);
                var (nextState, isTerminal) = m_Environment.Step(state, action);
                
                m_Actions.Add(action);
                m_States.Add(nextState);

                if (isTerminal)
                {
                    return i + 1;
                }
                
                state = nextState;
            }

            return m_TimeLimit;
        }

        private IAgent GetAgent(AgentType agentType)
        {
            return agentType switch
            {
                AgentType.Random => m_RandomAgent,
                AgentType.DRL => m_DRLAgent,
                _ => throw new System.NotImplementedException(),
            };
        }
    }
}
