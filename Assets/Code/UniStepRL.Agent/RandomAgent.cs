using System;
using System.Linq;
using System.Runtime.CompilerServices;

namespace UniStepRL.Agent
{
    public class RandomAgent : IAgent
    {
        private readonly Random m_AgentRandom;

        public RandomAgent(int seed)
        {
            seed = seed == -1 ? new Random().Next() : seed;
            m_AgentRandom = new Random(seed);
        }

        public EnvironmentAction Simulate(SimpleEnvironment environment, EnvironmentState rootState)
        {
            var actionTypeMask = environment.GetActionTypeMask(rootState);

            while (true)
            {
                var actionType = (ActionType)SelectRandomIndex(actionTypeMask);
                
                switch (actionType)
                {
                    case ActionType.BuildTower:
                    {
                        var mask = environment.GetBuildTowerMask(rootState);
                        var valid = TrySelectRandomIndex(mask, out var coordinates);

                        if (!valid)
                        {
                            actionTypeMask[(int)ActionType.BuildTower] = 0;
                            break;
                        }

                        return new EnvironmentAction
                        {
                            Type = ActionType.BuildTower,
                            BuildTowerP0 = coordinates
                        };
                    }
                    case ActionType.PlaceUnit:
                    {
                        var unitMask = environment.GetPlaceUnitP0Mask(rootState);
                        var unitIndex = SelectRandomIndex(unitMask);
                        
                        if (unitIndex == -1)
                        {
                            actionTypeMask[(int)ActionType.PlaceUnit] = 0;
                            break;
                        }

                        var unitToPlace = IndexToUnit(unitIndex);
                        var cellMask = environment.GetPlaceUnitP1Mask(rootState, unitToPlace);
                        var valid = TrySelectRandomIndex(cellMask, out var coordinates);

                        if (!valid)
                        {
                            actionTypeMask[(int)ActionType.PlaceUnit] = 0;
                            break;
                        }
                        
                        return new EnvironmentAction
                        {
                            Type = ActionType.PlaceUnit,
                            PlaceUnitP0 = unitToPlace,
                            PlaceUnitP1 = coordinates
                        };
                    }
                    case ActionType.MoveUnit:
                    {
                        var fromMask = environment.GetMoveUnitP0Mask(rootState);
                        var valid0 = TrySelectRandomIndex(fromMask, out var fromCoordinates);
                        
                        if (!valid0)
                        {
                            actionTypeMask[(int)ActionType.MoveUnit] = 0;
                            break;
                        }

                        var toMask = environment.GetMoveUnitP1Mask(rootState, fromCoordinates);
                        var valid1 = TrySelectRandomIndex(toMask, out var toCoordinates);

                        if (!valid1)
                        {
                            actionTypeMask[(int)ActionType.MoveUnit] = 0;
                            break;
                        }
                        
                        return new EnvironmentAction
                        {
                            Type = ActionType.MoveUnit,
                            MoveUnitP0 = fromCoordinates,
                            MoveUnitP1 = toCoordinates
                        };
                    }
                    case ActionType.EndTurn:
                    {
                        return new EnvironmentAction
                        {
                            Type = ActionType.EndTurn,
                        };
                    }
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
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
    
        private int SelectRandomIndex(int[] mask)
        {
            var indicesOfOne = mask
                .Select((value, index) => (value, index))
                .Where(item => item.value == 1)
                .Select(item => item.index)
                .ToList();

            if (indicesOfOne.Count == 0)
            {
                return -1;
            }

            var randomIndexInList = m_AgentRandom.Next(0, indicesOfOne.Count);

            return indicesOfOne[randomIndexInList];
        }

        private bool TrySelectRandomIndex(int[,] mask, out OffsetCoordinates coordinates)
        {
            var rows = mask.GetLength(0);
            var cols = mask.GetLength(1);

            var totalOnes = 0;
            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    if (mask[row, col] == 1)
                    {
                        totalOnes++;
                    }
                }
            }

            if (totalOnes == 0)
            {
                coordinates = new OffsetCoordinates(-1, -1);
                return false;
            }

            var selected = m_AgentRandom.Next(totalOnes);

            var count = 0;
            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    if (mask[row, col] == 1)
                    {
                        if (count == selected)
                        {
                            coordinates = new OffsetCoordinates(col, row);
                            return true;
                        }

                        count++;
                    }
                }
            }

            throw new InvalidOperationException("Unexpected error.");
        }
    }
}
