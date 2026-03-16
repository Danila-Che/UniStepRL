using System;
using Unity.InferenceEngine;

namespace UniStepRL.Agent
{
    public class AgentModel : IDisposable
    {
        public struct Input
        {
            public float[] MapInput; // Chanel Height Width
            public float[] UnitsInput; // Chanel Height Width
            public float[] CanMoveInput; // Only One Chanel Height Width
            public float[] GlobalFeaturesInput;

            public Input(int width, int height)
            {
                MapInput = new float[2 *  height * width];
                UnitsInput = new float[7 *  height * width];
                CanMoveInput = new float[1 * height * width];
                GlobalFeaturesInput = new float[3];
            }
        }

        public struct Output
        {
            public float[] ActionTypeOutput;

            public float[,] BuildTowerP0Output;

            public float[] PlaceUnitP0Output;
            public float[,] PlaceUnitP1Output;
            
            public float[,] MoveUnitP0Output;
            public float[,] MoveUnitP1Output;

            public float V; // V(s)
        }
        
        private readonly Model m_RuntimeModel;
        private readonly Worker m_Worker;

        private readonly TensorShape m_MapInputTensorShape;
        private readonly TensorShape m_UnitsInputTensorShape;
        private readonly TensorShape m_CanMoveInputTensorShape;
        private readonly TensorShape m_GlobalFeaturesInputTensorShape;
        
        public AgentModel(ModelAsset modelAsset)
        {
            m_RuntimeModel = ModelLoader.Load(modelAsset);
            m_Worker = new Worker(m_RuntimeModel, BackendType.CPU);

            m_MapInputTensorShape = new TensorShape(1, 2, 5, 5); // 0 water // 1 land
            m_UnitsInputTensorShape = new TensorShape(1, 7, 5, 5); // 1 for player unit, -1 for opponent unit, 0 for nothing.
            m_CanMoveInputTensorShape = new TensorShape(1, 1, 5, 5); // Indicates can move.
            m_GlobalFeaturesInputTensorShape = new TensorShape(1, 3); // Amount, incoming, maintenance.
        }
        
        public void Dispose()
        {
            m_Worker?.Dispose();
        }

        public Output Evaluate(Input input)
        {
            using var mapInputTensor = new Tensor<float>(m_MapInputTensorShape, input.MapInput);
            using var unitsInputTensor = new Tensor<float>(m_UnitsInputTensorShape, input.UnitsInput);
            using var moveInputTensor = new Tensor<float>(m_CanMoveInputTensorShape, input.CanMoveInput);
            using var globalFeaturesInputTensor = new Tensor<float>(m_GlobalFeaturesInputTensorShape, input.GlobalFeaturesInput);
            
            m_Worker.SetInput("map_input", mapInputTensor);
            m_Worker.SetInput("units_input", unitsInputTensor);
            m_Worker.SetInput("can_move_input", moveInputTensor);
            m_Worker.SetInput("global_features_input", globalFeaturesInputTensor);
            
            m_Worker.Schedule();

            var output = new Output
            {
                ActionTypeOutput = GetOutput("action_type_output"),
                
                BuildTowerP0Output = GetSpatialOutput("build_tower_p0_output"),

                PlaceUnitP0Output = GetOutput("place_unit_p0_output"),
                PlaceUnitP1Output = GetSpatialOutput("place_unit_p1_output"),

                MoveUnitP0Output = GetSpatialOutput("move_unit_p0_output"),
                MoveUnitP1Output = GetSpatialOutput("move_unit_p1_output"),

                V = GetValueOutput("v_value_output")
            };

            return output;
        }

        public static Input ToInput(SimpleEnvironment environment, EnvironmentState state)
        {
            var input = new Input(5, 5);

            for (int row = 0; row < 5; row++)
            {
                for (int col = 0; col < 5; col++)
                {
                    var coordinates = new OffsetCoordinates(col, row);

                    if (environment.GetCellType(coordinates) == CellType.Water)
                    {
                        input.MapInput[row * 5 + col] = 1;
                    }
                    else
                    {
                        input.MapInput[25 + row * 5 + col] = 1;
                    }

                    var player = state.GetPlayer(coordinates);

                    if (player == Player.None)
                    {
                        
                    }
                    else if (player == state.CurrentPlayer)
                    {
                        input.UnitsInput[25 * (int)state.GetUnit(coordinates) + row * 5 + col] = 1;
                    }
                    else
                    {
                        input.UnitsInput[25 * (int)state.GetUnit(coordinates) + row * 5 + col] = -1;
                    }
                }
            }

            var stats = state.GetCurrentPlayerStats();

            foreach (var entity in stats.Entities)
            {
                var coordinates = entity.Coordinates;

                input.CanMoveInput[coordinates.Row * 5 + coordinates.Col] = 1;
            }

            input.GlobalFeaturesInput[0] = stats.GoldAmount;
            
            return input;
        }

        private float[] GetOutput(string outputName)
        {
            if (m_Worker.PeekOutput(outputName) is Tensor<float> outputTensor)
            {
                var result = outputTensor.DownloadToArray();
                outputTensor.Dispose();

                return result;
            }

            return null;
        }

        private float[,] GetSpatialOutput(string outputName)
        {
            if (m_Worker.PeekOutput(outputName) is Tensor<float> outputTensor)
            {
                var output = outputTensor.DownloadToArray();
                outputTensor.Dispose();

                var result = new float[5, 5];

                for (int row = 0; row < 5; row++)
                {
                    for (int col = 0; col < 5; col++)
                    {
                        var i = row * 5 + col;

                        result[row, col] = output[i];
                    }
                }

                return result;
            }

            return null;
        }

        private float GetValueOutput(string outputName)
        {
            if (m_Worker.PeekOutput(outputName) is Tensor<float> outputTensor)
            {
                var result = outputTensor.DownloadToArray();
                outputTensor.Dispose();

                return result[0];
            }

            return float.NaN;
        }

        private float[] GetSoftmaxOutput(string outputName)
        {
            if (m_Worker.PeekOutput(outputName) is Tensor<float> logits)
            {
                var result = logits.DownloadToArray();
                logits.Dispose();

                return result;
            }

            return null;
        }
    }
}
