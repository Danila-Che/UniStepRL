using System;
using System.Diagnostics;
using TMPro;
using UniStepRL;
using UniStepRL.Agent;
using Unity.InferenceEngine;
using UnityEngine;
using UnityEngine.Profiling;

namespace UniStepBenchmark
{
    public class DRLAgentBenchmark : MonoBehaviour
    {
        private const int k_SampleCount = 32;

        [Header("Benchmark Configurations")]
        [SerializeField] private string m_Identifier;
        [Min(1)]
        [SerializeField] private int m_Iterations = 1; 

        [Header("Agent Configurations")]
        [SerializeField] private ModelAsset m_AgentModelAsset;
        [SerializeField] private uint m_AgentSeed = 0;
        [SerializeField] private int m_SimulationBudget = 64;

        [Header("Environment Configurations")]
        [Min(5)]
        [SerializeField] private int m_Width = 5;
        [Min(5)]
        [SerializeField] private int m_Height = 5;
        [Min(1)]
        [SerializeField] private int m_TimeLimit = 500;

        [Header("UI")]
        [SerializeField] private TMP_Text m_Text;

        private readonly Stopwatch m_Stopwatch = new();
        private readonly RingBuffer<long> m_Samples = new(k_SampleCount);

        private EnvironmentConfiguration m_Config;
        private SimpleEnvironment m_Environment;
        private DRLAgent m_DRLAgent;
        private EnvironmentState m_State;

        private void OnEnable()
        {
            m_Config = new EnvironmentConfiguration(m_Width, m_Height);
            m_Environment = new SimpleEnvironment(m_Config);
            m_DRLAgent = new DRLAgent(m_AgentModelAsset, m_SimulationBudget, m_AgentSeed);

            m_State = m_Environment.Reset();
        }

        private void OnDisable()
        {
            m_DRLAgent.Dispose();
        }

        private void Update()
        {
            EnvironmentState nextState = null;
            bool isTerminal = false;

            m_Stopwatch.Restart();
            Profiler.BeginSample(m_Identifier);

            for (int i = 0; i < m_Iterations; i++)
            {
                (nextState, isTerminal) = Sample();
            }

            Profiler.EndSample();
            m_Stopwatch.Stop();
            m_Samples.Push(m_Stopwatch.ElapsedMilliseconds);

            if (isTerminal)
            {
                m_State = m_Environment.Reset();
            }
            else
            {
                m_State = nextState;
            }

            UpdateUI();
        }

        private void UpdateUI()
        {
            m_Text.SetText($"{m_Identifier}: {Average(m_Samples)} ms");
        }

        private void OnTerminalOrTruncated()
        {
            m_State = m_Environment.Reset();
        }

        private (EnvironmentState nextState, bool isTerminal) Sample()
        {
            var action = m_DRLAgent.Simulate(m_Environment, m_State);
            return m_Environment.Step(m_State, action);
        }

        private static long Average(RingBuffer<long> buffer)
        {
            long total = 0;

            for (int i = 0; i < buffer.Length; i++)
            {
                total += buffer[i];
            }

            return total / buffer.Length;
        }
    }
}
