using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using UniStepRL;
using UniStepRL.Agent;
using Unity.InferenceEngine;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class Trainer : MonoBehaviour
{
    private static readonly WaitForSeconds k_WaitForSeconds = new(0.01f);

    private struct Experience
    {
        public AgentModel.Input State;
        public AgentModel.Output TargetPolicy; // π* + Z
    }

    private struct EnvironmentExperience
    {
        public EnvironmentState State;
        public EnvironmentAction Action;
    }

    private class Episode
    {
        public DRLAgent Agent;
        public SimpleEnvironment Environment;
        public Player Winner;
        public List<EnvironmentExperience> Trajectory;
    }

    private class ReplayBuffer
    {
        public List<Episode> Episodes;
    }

    [SerializeField] private ModelAsset m_AgentModelAsset;

    [Header("Agent Configuration")]
    [Min(0)]
    [SerializeField] private int m_SimulationBudget = 64;

    [Header("Environment Configuration")]
    [Min(5)]
    [SerializeField] private int m_Width = 5;
    [Min(5)]
    [SerializeField] private int m_Height = 5;
    [Min(1)]
    [SerializeField] private int m_TimeLimit = 500;

    [Header("Self-play Configuration")]
    [SerializeField] private int m_BatchCount = 16;
    [SerializeField] private int m_BatchSize = 256;

    private List<Episode> m_Episodes;

    private void OnEnable()
    {
        m_Episodes = new List<Episode>();

        // for (int i = 0; i < m_EpisodesCount; i++)
        // {
        //     m_Episodes.Add(SetUpEpisode());
        // }
    }

    private IEnumerator Start()
    {
        var stopwatch = new Stopwatch();
        var totalTimeInMilliseconds = 0L;

        while (true)
        {
            stopwatch.Restart();

            var episode = SetUpEpisode();
            var isTerminal = PerformSelfPlay(episode, m_TimeLimit);

            stopwatch.Stop();
            
            if (isTerminal)
            {
                m_Episodes.Add(episode);
            }

            Debug.Log($"Is Terminal: {isTerminal}");
            Debug.Log($"Self-play: {stopwatch.ElapsedMilliseconds} ms");
            Debug.Log($"Episode Size: {episode.Trajectory.Count}");

            totalTimeInMilliseconds += stopwatch.ElapsedMilliseconds;

            var totalSize = 0;

            foreach (var e in m_Episodes)
            {
                totalSize += e.Trajectory.Count;
            }

            if (totalSize >= m_BatchCount * m_BatchSize)
            {
                break;
            }

            yield return k_WaitForSeconds;
        }

        Debug.Log($"Total Time: {totalTimeInMilliseconds} ms");

        {
            var totalSize = 0;

            foreach (var episode in m_Episodes)
            {
                totalSize += episode.Trajectory.Count;
            }

            Debug.Log($"Replay Buffer Size: {totalSize}");
            
            stopwatch.Restart();

            var experiences = Postprocess(totalSize);
            Save(experiences);

            stopwatch.Stop();

            Debug.Log($"Postprocess Time: {stopwatch.ElapsedMilliseconds} ms");
        }
    }

    private void OnDisable()
    {
        foreach (var episode in m_Episodes)
        {
            episode.Agent.Dispose();
        }

        m_Episodes.Clear();
    }

    private Episode SetUpEpisode()
    {
        var agent = new DRLAgent(m_AgentModelAsset, m_SimulationBudget, seed: 0);
        var config = new EnvironmentConfiguration(m_Width, m_Height);
        var environment = new SimpleEnvironment(config);

        return new Episode
        {
            Agent = agent,
            Environment = environment,
            Trajectory = new List<EnvironmentExperience>()
        };
    }

    private bool PerformSelfPlay(Episode episode, int timeLimit)
    {
        var environment = episode.Environment;
        var agent = episode.Agent;
        var trajectory = episode.Trajectory;

        var state = environment.Reset();

        for (int i = 0; i < timeLimit; i++)
        {
            var action = agent.Simulate(environment, state);
            var (nextState, isTerminal) = environment.Step(state, action);

            trajectory.Add(new EnvironmentExperience
            {
                State = state,
                Action = action,
            });

            if (isTerminal)
            {
                episode.Winner = state.CurrentPlayer;
                return true;
            }

            state = nextState;
        }

        return false;
    }

    private List<Experience> Postprocess(int capacity)
    {
        var result = new List<Experience>(capacity);

        foreach (var episode in m_Episodes)
        {
            for (int i = 0; i < episode.Trajectory.Count; i++)
            {
                var environmentExperience = episode.Trajectory[i];

                var experience = new Experience
                {
                    State = AgentModel.ToInput(episode.Environment, environmentExperience.State),
                    TargetPolicy = CreateTargetPolicyFromAction(environmentExperience.Action, environmentExperience.State.CurrentPlayer == episode.Winner ? 1.0f : -1.0f),
                };

                result.Add(experience);         
            }
        }

        return result;
    }

    private void Save(List<Experience> experiences)
    {
        var path = Path.Combine(Application.dataPath, "dataset.bin");

        using var writer = new BinaryWriter(File.Open(path, FileMode.Create));

        writer.Write(experiences.Count);

        foreach (var exp in experiences)
        {
            WriteArray(writer, exp.State.MapInput);
            WriteArray(writer, exp.State.UnitsInput);
            WriteArray(writer, exp.State.CanMoveInput);
            WriteArray(writer, exp.State.GlobalFeaturesInput);

            WriteArray(writer, exp.TargetPolicy.ActionTypeOutput);

            WriteArray(writer, exp.TargetPolicy.BuildTowerP0Output);
            WriteArray(writer, exp.TargetPolicy.PlaceUnitP0Output);
            WriteArray(writer, exp.TargetPolicy.PlaceUnitP1Output);
            WriteArray(writer, exp.TargetPolicy.MoveUnitP0Output);
            WriteArray(writer, exp.TargetPolicy.MoveUnitP1Output);

            writer.Write(exp.TargetPolicy.V);
        }

        Debug.Log($"Saved to: {path}");
    }

    private AgentModel.Output CreateTargetPolicyFromAction(EnvironmentAction action, float z)
    {
        var targetPolicy = new AgentModel.Output
        {
            ActionTypeOutput = new float[4],

            BuildTowerP0Output = new float[5, 5],

            PlaceUnitP0Output = new float[4],
            PlaceUnitP1Output = new float[5, 5],
            
            MoveUnitP0Output = new float[5, 5],
            MoveUnitP1Output = new float[5, 5],
        };

        targetPolicy.ActionTypeOutput[(int)action.Type] = 1;
        targetPolicy.V = z;

        switch (action.Type)
        {
            case ActionType.BuildTower:
            {
                var p0 = action.BuildTowerP0;
                targetPolicy.BuildTowerP0Output[p0.Row, p0.Col] = 1;
                break;
            }
            case ActionType.PlaceUnit:
            {
                var p0 = action.PlaceUnitP0;
                var p1 = action.PlaceUnitP1;
                targetPolicy.PlaceUnitP0Output[UnitToIndex(p0)] = 1;
                targetPolicy.PlaceUnitP1Output[p1.Row, p1.Col] = 1;
                break;
            }
            case ActionType.MoveUnit:
            {
                var p0 = action.MoveUnitP0;
                var p1 = action.MoveUnitP1;
                targetPolicy.MoveUnitP0Output[p0.Row, p0.Col] = 1;
                targetPolicy.MoveUnitP1Output[p1.Row, p1.Col] = 1;
                break;
            }
            case ActionType.EndTurn:
                break;
        }

        return targetPolicy;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int UnitToIndex(Unit unit)
    {
        return unit switch
        {
            Unit.Peasant => 0,
            Unit.Spearman => 1,
            Unit.Knight => 2,
            Unit.Baron => 3,
            _ => throw new ArgumentOutOfRangeException(nameof(unit), unit, null)
        };
    }

    void WriteArray(BinaryWriter writer, float[] array)
    {
        writer.Write(array.Length);
        
        foreach (var v in array)
        {
            writer.Write(v);
        }
    }

    void WriteArray(BinaryWriter writer, float[,] array)
    {
        int height = array.GetLength(0);
        int width = array.GetLength(1);

        writer.Write(height);
        writer.Write(width);

        for (int row = 0; row < height; row++)
        {
            for (int col = 0; col < width; col++)
            {
                writer.Write(array[row, col]);
            }
        }
    }
}
