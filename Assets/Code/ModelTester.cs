using System;
using UniStepRL;
using UniStepRL.Agent;
using UnityEngine;
using Unity.InferenceEngine;

public class ModelTester : MonoBehaviour
{
    [SerializeField] private ModelAsset m_ModelAsset;
    
    private AgentModel m_AgentModel;

    private void OnEnable()
    {
        m_AgentModel = new AgentModel(m_ModelAsset);

        var environment = new SimpleEnvironment(new EnvironmentConfiguration(5, 5));

        var input = AgentModel.ToInput(environment, environment.Reset());
        var output = m_AgentModel.Evaluate(input);

        var actionTypeIndex = ArgMax(output.ActionTypeOutput);
        Debug.Log($"Action Type: {(ActionType)actionTypeIndex}");
        Debug.Log(string.Join(' ', output.ActionTypeOutput));

        Debug.Log($"Build Tower P0:");
        for (int row = 0; row < 5; row++)
        {
            var line = new float[5];

            for (int col = 0; col < 5; col++)
            {
                line[col] = output.BuildTowerP0Output[row, col];
            }

            Debug.Log(string.Join(' ', line));
        }
    }

    private void OnDisable()
    {
        m_AgentModel.Dispose();
    }
    
    private static int ArgMax(float[] array)
    {
        if (array == null || array.Length == 0)
        {
            throw new ArgumentException("Array cannot be null or empty.");
        }

        var maxIndex = 0;
        
        for (int i = 1; i < array.Length; i++)
        {
            if (array[i] > array[maxIndex])
            {
                maxIndex = i;
            }
        }
        
        return maxIndex;
    }
}
