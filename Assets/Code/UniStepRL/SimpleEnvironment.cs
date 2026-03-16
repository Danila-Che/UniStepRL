using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace UniStepRL
{
    public enum CellType
    {
        Land,
        Water,
    }

    public enum Player
    {
        None,
        Player1,
        Player2,
    }

    public enum Unit
    {
        None,
        Fort,
        Tower,
        Peasant,
        Spearman,
        Knight,
        Baron,
    }
    
    public static class CoordinatesUtils
    {
        public static readonly OffsetCoordinates[][] s_OddRDirections =
        {
            // even rows 
            new OffsetCoordinates[]
            {
                new(+1, 0), new( 0, -1), new(-1, -1),
                new(-1, 0), new(-1, +1), new( 0, +1)
            },
            // odd rows 
            new OffsetCoordinates[]
            {
                new(+1, 0), new(+1, -1), new( 0, -1),
                new(-1, 0), new( 0, +1), new(+1, +1)
            },
        };
        
        // public static readonly CubeCoordinates[] s_CubeDirectionVectors =
        // {
        //     new(+1, 0, -1), new(+1, -1, 0), new(0, -1, +1),
        //     new(-1, 0, +1), new(-1, +1, 0), new(0, +1, -1)
        // };
    }
    
    public readonly struct OffsetCoordinates : IEquatable<OffsetCoordinates>
    {
        public readonly int Col;
        public readonly int Row;

        public OffsetCoordinates(int col, int row)
        {
            Col = col;
            Row = row;
        }

        public int Parity => Row & 1;

        public static OffsetCoordinates operator +(OffsetCoordinates lc, OffsetCoordinates rc)
        {
            return new OffsetCoordinates(lc.Col + rc.Col, lc.Row + rc.Row);
        }
        
        // public CubeCoordinates OddRToCube()
        // {
        //     var q = Col - (Row - Parity) / 2;
        //     var r = Row;

        //     return new CubeCoordinates(q, r, -q - r);
        // }

        public override string ToString()
        {
            return $"OffsetCoordinate (col: {Col} row: {Row})";
        }

        public bool Equals(OffsetCoordinates other)
        {
            return Col == other.Col && Row == other.Row;
        }

        public override bool Equals(object obj)
        {
            return obj is OffsetCoordinates other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Col, Row);
        }

        public static bool operator ==(OffsetCoordinates left, OffsetCoordinates right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(OffsetCoordinates left, OffsetCoordinates right)
        {
            return !(left == right);
        }
        
        public IEnumerable<OffsetCoordinates> IterateOddRNeighbors()
        {
            var diff = CoordinatesUtils.s_OddRDirections[Parity];

            for (int i = 0; i < 6; i++)
            {
                yield return new OffsetCoordinates(Col + diff[i].Col, Row + diff[i].Row);
            }
        }
    }

    // public readonly struct CubeCoordinates
    // {
    //     public readonly int Q;
    //     public readonly int R;
    //     public readonly int S;

    //     public CubeCoordinates(int q, int r, int s)
    //     {
    //         Q = q;
    //         R = r;
    //         S = s;
    //     }
        
    //     public static CubeCoordinates operator +(CubeCoordinates l, CubeCoordinates r)
    //     {
    //         return new CubeCoordinates(l.Q + r.Q, l.R + r.R, l.S + r.S);
    //     }
        
    //     public static CubeCoordinates operator -(CubeCoordinates l, CubeCoordinates r)
    //     {
    //         return new CubeCoordinates(l.Q - r.Q, l.R - r.R, l.S - r.S);
    //     }

    //     public static int Distance(CubeCoordinates l, CubeCoordinates r)
    //     {
    //         var vector = l - r;
            
    //         return (Math.Abs(vector.Q) + Math.Abs(vector.R) + Math.Abs(vector.S)) / 2;
    //     }
        
    //     public override string ToString()
    //     {
    //         return $"OffsetCoordinate (q: {Q} r: {R} s: {S})";
    //     }

    //     public IEnumerable<CubeCoordinates> IterateNeighbors()
    //     {
    //         for (int i = 0; i < 6; i++)
    //         {
    //             yield return this + CoordinatesUtils.s_CubeDirectionVectors[i];
    //         }
    //     }
    // }
    
    public enum ActionType
    {
        BuildTower,
        PlaceUnit,
        MoveUnit,
        EndTurn
    }
    
    public struct EnvironmentAction
    {
        public ActionType Type;
        
        /// <summary>
        /// The selected cell for placing the tower.
        /// </summary>
        public OffsetCoordinates BuildTowerP0;

        /// <summary>
        /// Selected unit for placement.
        /// </summary>
        public Unit PlaceUnitP0;
        /// <summary>
        /// The selected cell for placing the unit.
        /// </summary>
        public OffsetCoordinates PlaceUnitP1;
        
        /// <summary>
        /// The selected cell where the selected unit for movement is located.
        /// </summary>
        public OffsetCoordinates MoveUnitP0;
        /// <summary>
        /// The target cell to move the unit to.
        /// </summary>
        public OffsetCoordinates MoveUnitP1;
    }
    
    public struct Entity
    {
        public OffsetCoordinates Coordinates;
        public Unit Unit;
        public Player Player;
        public bool Moved;
    }

    public class PlayerStats
    {
        public OffsetCoordinates FortCoordinates;

        public int GoldAmount;
        public int GoldReceipt;
        public int GoldExpenditureOnUnits;

        public List<Entity> Entities;
    }
    
    public class EnvironmentState
    {
        public Unit[,] Units;
        public Player[,] Players;
        public int[,] EntitiesLookUpTable; // Stores indices pointing to entities.
        
        public Player CurrentPlayer; // The player who takes the next step.

        public PlayerStats Player1Stats;
        public PlayerStats Player2Stats;

        public int Turn;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Unit GetUnit(OffsetCoordinates coordinates)
        {
            return Units[coordinates.Row, coordinates.Col];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetUnit(OffsetCoordinates coordinates, Unit unit)
        {
            Units[coordinates.Row, coordinates.Col] = unit;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Player GetPlayer(OffsetCoordinates coordinates)
        {
            return Players[coordinates.Row, coordinates.Col];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetPlayer(OffsetCoordinates coordinates, Player player)
        {
            Players[coordinates.Row, coordinates.Col] = player;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetEntityLUTIndex(OffsetCoordinates coordinates)
        {
            return EntitiesLookUpTable[coordinates.Row, coordinates.Col];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetEntityLUTIndex(OffsetCoordinates coordinates, int index)
        {
            EntitiesLookUpTable[coordinates.Row, coordinates.Col] = index;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HasEntity(OffsetCoordinates coordinates)
        {
            return EntitiesLookUpTable[coordinates.Row, coordinates.Col] >= 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddEntity(OffsetCoordinates coordinates, Entity entity)
        {
            var entities = GetPlayer(coordinates) == Player.Player1 ? Player1Stats.Entities : Player2Stats.Entities;
            SetEntityLUTIndex(coordinates, entities.Count);
            entities.Add(entity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ReplaceEntity(OffsetCoordinates coordinates, Entity entity)
        {
            var index = GetEntityLUTIndex(coordinates);
            var entities = GetPlayer(coordinates) == Player.Player1 ? Player1Stats.Entities : Player2Stats.Entities;
            entities[index] = entity;
        }

        public void RemoveEntityWithSwap(OffsetCoordinates coordinates)
        {
            var entityIndex = EntitiesLookUpTable[coordinates.Row, coordinates.Col];
            var entities = GetPlayer(coordinates) == Player.Player1 ? Player1Stats.Entities : Player2Stats.Entities;
            var lastIndex = entities.Count - 1;

            SetUnit(coordinates, Unit.None);
            SetEntityLUTIndex(coordinates, -1);

            if (entityIndex != lastIndex)
            {
                var lastEntity = entities[lastIndex];
                
                entities[entityIndex] = lastEntity;
                SetEntityLUTIndex(lastEntity.Coordinates, entityIndex);
            }

            entities.RemoveAt(lastIndex);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Player GetOpponentPlayer()
        {
            return CurrentPlayer switch
            {
                Player.Player1 => Player.Player2,
                Player.Player2 => Player.Player1,
                _ => throw new NotImplementedException()
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public PlayerStats GetCurrentPlayerStats()
        {
            return CurrentPlayer switch
            {
                Player.Player1 => Player1Stats,
                Player.Player2 => Player2Stats,
                _ => throw new NotImplementedException()
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public PlayerStats GetOpponentPlayerStats()
        {
            return CurrentPlayer switch
            {
                Player.Player1 => Player2Stats,
                Player.Player2 => Player1Stats,
                _ => throw new NotImplementedException()
            };
        }
    }

    public readonly struct EnvironmentConfiguration
    {
        public readonly int Width;
        public readonly int Height;

        public EnvironmentConfiguration(int width, int height)
        {
            Width = width;
            Height = height;
        }
        
        public int CellCount => Width * Height - Height/2;
    }
    
    /// <summary>
    /// Pointy hex. Odd-R.
    /// </summary>
    public class SimpleEnvironment
    {
        private const int k_MaxDefencePoint = 4;
        
        private readonly EnvironmentConfiguration m_Config;
        private readonly CellType[,] m_Cells;
        private readonly bool[,] m_VisitedBuffer;

        public SimpleEnvironment(EnvironmentConfiguration config)
        {
            m_Config = config;
            m_Cells = new CellType[m_Config.Height, m_Config.Width];
            m_VisitedBuffer = new bool[m_Config.Height, m_Config.Width];
        }

        public CellType GetCellType(OffsetCoordinates coordinates)
        {
            return m_Cells[coordinates.Row, coordinates.Col];
        }
        
        public EnvironmentState Reset()
        {
            Fill(m_Cells, CellType.Land);

            GenerateMap();

            var state = CreateEmptyState();
            state.CurrentPlayer = Player.Player1;

            state.Player1Stats.GoldAmount = 10;
            state.Player1Stats.Entities = new List<Entity>();

            state.Player2Stats.GoldAmount = 10;
            state.Player2Stats.Entities = new List<Entity>();

            Fill(state.Units, Unit.None);
            Fill(state.Players, Player.None);
            Fill(state.EntitiesLookUpTable, -1);

            state.Player1Stats.FortCoordinates = new OffsetCoordinates(0, 0);
            state.Player2Stats.FortCoordinates = new OffsetCoordinates(m_Config.Width - 1, m_Config.Height - 1);

            PlaceFort(state, state.Player1Stats.FortCoordinates, Player.Player1);
            PlaceFort(state, state.Player2Stats.FortCoordinates, Player.Player2);

            return state;
        }
        
        public (EnvironmentState, bool) Step(EnvironmentState state, EnvironmentAction action)
        {
            state = CopyState(state);

            switch (action.Type)
            {
                case ActionType.BuildTower:
                {
                    BuildTower(state, action);
                    break;
                }
                case ActionType.PlaceUnit:
                {
                    PlaceUnit(state, action);
                    break;
                }
                case ActionType.MoveUnit:
                {
                    MoveUnit(state, action);
                    break;
                }
                case ActionType.EndTurn:
                {
                    EndTurn(state, action);
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }

            var totalFortTiles = RecalculateTerritory(state);
            var isTerminal = totalFortTiles <= 1;

            return (state, isTerminal);
        }

        private void GenerateMap()
        {
            for (int row = 0; row < m_Config.Height; row++)
            {
                if ((row & 1) == 1)
                {
                    m_Cells[row, m_Config.Width - 1] = CellType.Water;
                }
            }
        }

        private static void BuildTower(EnvironmentState state, EnvironmentAction action)
        {
            var coordinates = action.BuildTowerP0;

            state.SetUnit(coordinates, Unit.Tower);
            state.SetPlayer(coordinates, state.CurrentPlayer);

            state.GetCurrentPlayerStats().GoldAmount -= GetUnitCost(Unit.Tower);
        }

        private static void PlaceUnit(EnvironmentState state, EnvironmentAction action)
        {
            var unitToPlace = action.PlaceUnitP0;
            var coordinates = action.PlaceUnitP1;

            if (state.GetPlayer(coordinates) == state.CurrentPlayer) // Place if empty or merge.
            {
                var placedUnit = state.GetUnit(coordinates);
                
                if (placedUnit == Unit.None)
                {
                    PlaceUnit(state, coordinates, unitToPlace, moved: false);
                }
                else
                {
                    ReplaceUnit(state, coordinates, placedUnit, unitToPlace, moved: false);
                }
            }
            else if (state.GetPlayer(coordinates) == Player.None) // Always empty.
            {
                PlaceUnit(state, coordinates, unitToPlace, moved: true);
            }
            else // Attack opponent.
            {
                if (state.HasEntity(coordinates))
                {
                    AttackEntity(state, coordinates, unitToPlace, moved: true);
                }
                else
                {
                    PlaceUnit(state, coordinates, unitToPlace, moved: true);
                }
            }

            state.GetCurrentPlayerStats().GoldAmount -= GetUnitCost(unitToPlace);
        }
        
        private void MoveUnit(EnvironmentState state, EnvironmentAction action)
        {
            var fromCoordinates = action.MoveUnitP0;
            var toCoordinates = action.MoveUnitP1;
            var unitToMove = state.GetUnit(fromCoordinates);
            
            if (state.GetPlayer(toCoordinates) == state.CurrentPlayer) // Move if empty or merge.
            {
                var placedUnit = state.GetUnit(toCoordinates);
                
                if (placedUnit == Unit.None)
                {
                    state.RemoveEntityWithSwap(fromCoordinates);
                    PlaceUnit(state, toCoordinates, unitToMove, moved: true);
                }
                else
                {
                    state.RemoveEntityWithSwap(fromCoordinates);
                    ReplaceUnit(state, toCoordinates, placedUnit, unitToMove, moved: true);
                }
            }
            else if (state.GetPlayer(toCoordinates) == Player.None) // Always empty.
            {
                state.RemoveEntityWithSwap(fromCoordinates);
                PlaceUnit(state, toCoordinates, unitToMove, moved: true);
            }
            else // Attack opponent.
            {
                if (state.HasEntity(toCoordinates))
                {
                    state.RemoveEntityWithSwap(fromCoordinates);
                    AttackEntity(state, toCoordinates, unitToMove, moved: true);
                }
                else
                {
                    state.RemoveEntityWithSwap(fromCoordinates);
                    PlaceUnit(state, toCoordinates, unitToMove, moved: true);
                }
            }
        }

        private void EndTurn(EnvironmentState state, EnvironmentAction action)
        {
            state.CurrentPlayer = state.GetOpponentPlayer();

            if (state.CurrentPlayer == Player.Player1)
            {
                state.Turn++;
            }
                    
            if (state.Turn > 0)
            {
                CalculateIncoming(state);
                ConductUnitMaintenance(state);
            }

            var entities = state.GetCurrentPlayerStats().Entities;
            
            for (int i = 0; i < entities.Count; i++)
            {
                var entity = entities[i];
                
                entity.Moved = false;

                entities[i] = entity;
            }
        }

        private int RecalculateTerritory(EnvironmentState state)
        {
            var totalFortTiles = 0;

            ClearVisits();
            
            var fortCoordinates = state.GetOpponentPlayerStats().FortCoordinates;
            var opponent = state.GetOpponentPlayer();

            var stack = new Stack<OffsetCoordinates>();

            if (state.GetPlayer(fortCoordinates) == opponent)
            {
                stack.Push(fortCoordinates);
                SetVisited(fortCoordinates);
            }

            while (stack.Count > 0)
            {
                var coordinates = stack.Pop();

                totalFortTiles++;
            
                foreach (var neighbor in coordinates.IterateOddRNeighbors())
                {
                    if (IsValid(neighbor) && IsLand(neighbor))
                    {
                        if (!WasVisited(neighbor) && state.GetPlayer(neighbor) == opponent)
                        {
                            stack.Push(neighbor);
                            SetVisited(neighbor);
                        }
                    }
                }
            }

            for (int row = 0; row < m_Config.Height; row++)
            {
                for (int col = 0; col < m_Config.Width; col++)
                {
                    var coordinates = new OffsetCoordinates(col, row);

                    if (!WasVisited(coordinates) && state.GetPlayer(coordinates) == opponent)
                    {
                        if (state.HasEntity(coordinates))
                        {
                            state.RemoveEntityWithSwap(coordinates);
                        }

                        state.SetPlayer(coordinates, Player.None);
                        state.SetUnit(coordinates, Unit.None);
                    }
                }
            }

            return totalFortTiles;
        }

        private static void PlaceUnit(EnvironmentState state, OffsetCoordinates coordinates, Unit unitToPlace, bool moved)
        {
            state.SetUnit(coordinates, unitToPlace);
            state.SetPlayer(coordinates, state.CurrentPlayer);
                    
            var entity = new Entity
            {
                Coordinates = coordinates,
                Unit = unitToPlace,
                Player = state.CurrentPlayer,
                Moved = moved
            };

            state.AddEntity(coordinates, entity);
        }

        private static void ReplaceUnit(EnvironmentState state, OffsetCoordinates coordinates, Unit placedUnit, Unit unitToPlace, bool moved)
        {
            var mergedUnit = MergeUnits(placedUnit, unitToPlace);

            state.SetUnit(coordinates, mergedUnit);
            state.SetPlayer(coordinates, state.CurrentPlayer);
                    
            var entity = new Entity
            {
                Coordinates = coordinates,
                Unit = mergedUnit,
                Player = state.CurrentPlayer,
                Moved = moved
            };

            state.ReplaceEntity(coordinates, entity);
        }
        
        private static void AttackEntity(EnvironmentState state, OffsetCoordinates coordinates, Unit unitToPlace, bool moved)
        {
            state.RemoveEntityWithSwap(coordinates);

            state.SetUnit(coordinates, unitToPlace);
            state.SetPlayer(coordinates, state.CurrentPlayer);
                    
            var entity = new Entity
            {
                Coordinates = coordinates,
                Unit = unitToPlace,
                Player = state.CurrentPlayer,
                Moved = moved
            };

            state.AddEntity(coordinates, entity);
        }
        
        private void PlaceFort(EnvironmentState state, OffsetCoordinates coordinates, Player player)
        {
            state.Players[coordinates.Row, coordinates.Col] = player;
            state.Units[coordinates.Row, coordinates.Col] = Unit.Fort;
            
            foreach (var neighbor in coordinates.IterateOddRNeighbors())
            {
                if (IsValid(neighbor) && IsLand(neighbor))
                {
                    state.Players[neighbor.Row, neighbor.Col] = player;
                }
            }
        }
        
        private bool CanAttack(EnvironmentState state, OffsetCoordinates coordinates, Unit unitToPlace, Player opponent)
        {
            if (unitToPlace == Unit.Baron)
            {
                return true;
            }
            
            var unitStrength = UnitToStrength(unitToPlace);
            var opponentMaxDefencePoint = 0;
            
            if (unitStrength <= UnitToDefence(state.GetUnit(coordinates)))
            {
                return false;
            }

            foreach (var neighbor in coordinates.IterateOddRNeighbors())
            {
                if (IsValid(neighbor) && IsLand(neighbor))
                {
                    if (state.GetPlayer(neighbor) == opponent)
                    {
                        var defencePoint = UnitToDefence(state.GetUnit(neighbor));

                        if (defencePoint > opponentMaxDefencePoint)
                        {
                            opponentMaxDefencePoint = defencePoint;
                        }

                        if (unitStrength <= opponentMaxDefencePoint)
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }
        
        public int[] GetActionTypeMask(EnvironmentState state)
        {
            var mask = new int[4];
            var goldAmount = state.GetCurrentPlayerStats().GoldAmount;
            
            mask[(int)ActionType.BuildTower] = goldAmount >= GetUnitCost(Unit.Tower) ? 1 : 0;
            mask[(int)ActionType.PlaceUnit] = goldAmount >= GetUnitCost(Unit.Peasant) ? 1 : 0;
            mask[(int)ActionType.MoveUnit] = state.GetCurrentPlayerStats().Entities.Any(e => !e.Moved) ? 1 : 0;
            mask[(int)ActionType.EndTurn] = 1;

            return mask;
        }
        
        public int[,] GetBuildTowerMask(EnvironmentState state)
        {
            var mask = new int[m_Config.Height, m_Config.Width];
            
            ClearVisits();
            
            var fortCoordinates = state.GetCurrentPlayerStats().FortCoordinates;

            var stack = new Stack<OffsetCoordinates>();
            
            stack.Push(fortCoordinates);
            SetVisited(fortCoordinates);
            
            while (stack.Count > 0)
            {
                var coordinates = stack.Pop();

                if (state.GetUnit(coordinates) == Unit.None)
                {
                    mask[coordinates.Row, coordinates.Col] = 1;
                }
            
                foreach (var neighbor in coordinates.IterateOddRNeighbors())
                {
                    if (IsValid(neighbor) && IsLand(neighbor) && !WasVisited(neighbor))
                    {
                        if (state.GetPlayer(neighbor) == state.CurrentPlayer)
                        {
                            stack.Push(neighbor);
                            SetVisited(neighbor);
                        }
                    }
                }
            }

            return mask;
        }

        public int[] GetPlaceUnitP0Mask(EnvironmentState state)
        {
            var goldAmount = state.GetCurrentPlayerStats().GoldAmount;
            
            return new[]
            {
                goldAmount >= GetUnitCost(Unit.Peasant) ? 1 : 0,
                goldAmount >= GetUnitCost(Unit.Spearman) ? 1 : 0,
                goldAmount >= GetUnitCost(Unit.Knight) ? 1 : 0,
                goldAmount >= GetUnitCost(Unit.Baron) ? 1 : 0
            };
        }

        public int[,] GetPlaceUnitP1Mask(EnvironmentState state, Unit unitToPlace)
        {
            var mask = new int[m_Config.Height, m_Config.Width];
            
            ClearVisits();
            
            var fortCoordinates = state.GetCurrentPlayerStats().FortCoordinates;
            
            var stack = new Stack<OffsetCoordinates>();
            
            stack.Push(fortCoordinates);
            SetVisited(fortCoordinates);
            
            while (stack.Count > 0)
            {
                var coordinates = stack.Pop();
                
                if (state.GetPlayer(coordinates) == state.CurrentPlayer)
                {
                    foreach (var neighbor in coordinates.IterateOddRNeighbors())
                    {
                        if (IsValid(neighbor) && IsLand(neighbor) && !WasVisited(neighbor))
                        {
                            stack.Push(neighbor);
                            SetVisited(neighbor);
                        }
                    }
                }

                if (state.GetPlayer(coordinates) == state.CurrentPlayer)
                {
                    var placedUnit = state.GetUnit(coordinates);
                    
                    if (placedUnit == Unit.None)
                    {
                        mask[coordinates.Row, coordinates.Col] = 1;
                    }
                    else if (CanMergeUnits(placedUnit, unitToPlace))
                    {
                        mask[coordinates.Row, coordinates.Col] = 1;
                    }
                }
                else if (state.GetPlayer(coordinates) == Player.None) // Always empty.
                {
                    mask[coordinates.Row, coordinates.Col] = 1;
                }
                else // Opponent.
                {
                    if (CanAttack(state, coordinates, unitToPlace, state.GetPlayer(coordinates)))
                    {
                        mask[coordinates.Row, coordinates.Col] = 1;
                    }
                }
            }

            return mask;
        }
        
        public int[,] GetMoveUnitP0Mask(EnvironmentState state)
        {
            var mask = new int[m_Config.Height, m_Config.Width];

            foreach (var entity in state.GetCurrentPlayerStats().Entities)
            {
                if (!entity.Moved)
                {
                    var coordinates = entity.Coordinates;

                    mask[coordinates.Row, coordinates.Col] = 1;
                }
            }
            
            return mask;
        }
        
        public int[,] GetMoveUnitP1Mask(EnvironmentState state, OffsetCoordinates fromCoordinates, int range = 4)
        {
            var mask = new int[m_Config.Height, m_Config.Width];
            var unitToMove = state.GetUnit(fromCoordinates);

            ClearVisits();
            
            var fringes = new List<OffsetCoordinates> { fromCoordinates };

            SetVisited(fromCoordinates);

            for (int i = 0; i < range; i++)
            {
                var outsideFringes = new List<OffsetCoordinates>();

                foreach (var coordinates in fringes)
                {
                    if (state.GetPlayer(coordinates) == state.CurrentPlayer)
                    {
                        foreach (var neighbor in coordinates.IterateOddRNeighbors())
                        {
                            if (IsValid(neighbor) && IsLand(neighbor))
                            {
                                if (!WasVisited(neighbor) && neighbor != fromCoordinates)
                                {
                                    if (state.GetPlayer(neighbor) == state.CurrentPlayer)
                                    {
                                        outsideFringes.Add(neighbor);
                                        SetVisited(neighbor);
                                    }

                                    if (state.GetPlayer(neighbor) == state.CurrentPlayer)
                                    {
                                        var placedUnit = state.GetUnit(neighbor);
                                        
                                        if (placedUnit == Unit.None)
                                        {
                                            mask[neighbor.Row, neighbor.Col] = 1;
                                        }
                                        else if (CanMergeUnits(placedUnit, unitToMove))
                                        {
                                            mask[neighbor.Row, neighbor.Col] = 1;
                                        }
                                    }
                                    else if (state.GetPlayer(neighbor) == Player.None)
                                    {
                                        mask[neighbor.Row, neighbor.Col] = 1;
                                    }
                                    else // opponent
                                    {
                                        if (CanAttack(state, neighbor, unitToMove, state.GetPlayer(neighbor)))
                                        {
                                            mask[neighbor.Row, neighbor.Col] = 1;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                
                fringes = outsideFringes;
            }

            return mask;
        }

        private static void ConductUnitMaintenance(EnvironmentState state)
        {
            var totalMaintenance = 0;

            foreach (var entity in state.GetCurrentPlayerStats().Entities)
            {
                totalMaintenance += GetUnitMaintenance(entity.Unit);
            }

            var currentPlayerStats = state.GetCurrentPlayerStats();
            currentPlayerStats.GoldAmount -= totalMaintenance;

            if (currentPlayerStats.GoldAmount < 0)
            {
                currentPlayerStats.GoldAmount = 0;

                foreach (var entity in state.GetCurrentPlayerStats().Entities)
                {
                    state.SetEntityLUTIndex(entity.Coordinates, -1);
                    state.SetUnit(entity.Coordinates, Unit.None);
                }

                state.GetCurrentPlayerStats().Entities.Clear();
            }
        }

        private void CalculateIncoming(EnvironmentState state)
        {
            ClearVisits();
            
            var incomingGoldAmount = 0;

            var fortCoordinates = state.GetCurrentPlayerStats().FortCoordinates;
            
            var stack = new Stack<OffsetCoordinates>();
            
            stack.Push(fortCoordinates);
            SetVisited(fortCoordinates);
            
            while (stack.Count > 0)
            {
                var coordinates = stack.Pop();

                if (state.GetPlayer(coordinates) == state.CurrentPlayer)
                {
                    incomingGoldAmount++;
                    
                    foreach (var neighbor in coordinates.IterateOddRNeighbors())
                    {
                        if (IsValid(neighbor) && IsLand(neighbor) && !WasVisited(neighbor))
                        {
                            stack.Push(neighbor);
                            SetVisited(neighbor);
                        }
                    }
                }
            }

            state.GetCurrentPlayerStats().GoldAmount += incomingGoldAmount;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private EnvironmentState CopyState(EnvironmentState state)
        {
            var result = CreateEmptyState();

            Array.Copy(state.Units, result.Units, state.Units.Length);
            Array.Copy(state.Players, result.Players, state.Players.Length);
            Array.Copy(state.EntitiesLookUpTable, result.EntitiesLookUpTable, state.EntitiesLookUpTable.Length);

            result.CurrentPlayer = state.CurrentPlayer;
            
            result.Player1Stats.GoldAmount = state.Player1Stats.GoldAmount;
            result.Player1Stats.FortCoordinates = state.Player1Stats.FortCoordinates;
            result.Player1Stats.Entities = state.Player1Stats.Entities.ToList();

            result.Player2Stats.GoldAmount = state.Player2Stats.GoldAmount;
            result.Player2Stats.FortCoordinates = state.Player2Stats.FortCoordinates;
            result.Player2Stats.Entities = state.Player2Stats.Entities.ToList();

            result.Turn = state.Turn;

            return result;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private EnvironmentState CreateEmptyState()
        {
            return new EnvironmentState
            {
                Units = new Unit[m_Config.Height, m_Config.Width],
                Players = new Player[m_Config.Height, m_Config.Width],
                EntitiesLookUpTable = new int[m_Config.Height, m_Config.Width],
                
                CurrentPlayer = Player.None,

                Player1Stats = new PlayerStats(),
                Player2Stats = new PlayerStats(),

                Turn = 0
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsValid(OffsetCoordinates coordinates)
        {
            return coordinates.Col >= 0
                   && coordinates.Col < m_Config.Width
                   && coordinates.Row >= 0
                   && coordinates.Row < m_Config.Height;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsLand(OffsetCoordinates coordinates)
        {
            return m_Cells[coordinates.Row, coordinates.Col] == CellType.Land;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int CoordinatesToIndex(OffsetCoordinates coordinates)
        {
            return coordinates.Col + m_Config.Width * coordinates.Row - (coordinates.Row >> 1);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public OffsetCoordinates IndexToCoordinate(int index) 
        {
            var doubleWidth = (2 * m_Config.Width) - 1;
    
            var pairIndex = index / doubleWidth;
            var remainder = index % doubleWidth;

            int row, col;

            if (remainder < m_Config.Width) 
            {
                row = pairIndex * 2;
                col = remainder;
            } 
            else 
            {
                row = (pairIndex * 2) + 1;
                col = remainder - m_Config.Width;
            }

            return new OffsetCoordinates(col, row);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool CanMergeUnits(Unit unit0, Unit unit1)
        {
            if (!CanMergeUnit(unit0) || !CanMergeUnit(unit1))
            {
                return false;
            }

            return (unit0, unit1) switch
            {
                (Unit.Peasant, Unit.Peasant) => true,
                (Unit.Peasant, Unit.Spearman) => true,
                (Unit.Peasant, Unit.Knight) => true,
                (Unit.Spearman, Unit.Peasant) => true,
                (Unit.Spearman, Unit.Spearman) => true,
                (Unit.Knight, Unit.Peasant) => true,
                (Unit.Spearman, Unit.Knight) => false,
                (Unit.Knight, Unit.Spearman) => false,
                (Unit.Knight, Unit.Knight) => false,
                (_, Unit.Baron) => false,
                (Unit.Baron, _) => false,
                _ => false
            };
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Unit MergeUnits(Unit unit0, Unit unit1)
        {
            return (unit0, unit1) switch
            {
                (Unit.Peasant, Unit.Peasant) => Unit.Spearman,
                (Unit.Peasant, Unit.Spearman) => Unit.Knight,
                (Unit.Spearman, Unit.Peasant) => Unit.Knight,
                (Unit.Peasant, Unit.Knight) => Unit.Baron,
                (Unit.Knight, Unit.Peasant) => Unit.Baron,
                (Unit.Spearman, Unit.Spearman) => Unit.Knight,
                (Unit.Spearman, Unit.Knight) => Unit.Baron,
                (Unit.Knight, Unit.Spearman) => Unit.Baron,
                _ => throw new InvalidOperationException($"Invalid merge. Can't merge {unit0} and {unit1}")
            };
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool CanMergeUnit(Unit unit)
        {
            return unit is Unit.Peasant or Unit.Spearman or Unit.Knight or Unit.Baron;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int UnitToStrength(Unit unit)
        {
            return unit switch
            {
                Unit.Peasant => 1,
                Unit.Spearman => 2,
                Unit.Knight => 3,
                Unit.Baron => 4,
                _ => throw new ArgumentOutOfRangeException(nameof(unit), unit, null)
            };
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int UnitToDefence(Unit unit)
        {
            return unit switch
            {
                Unit.Fort => 1,
                Unit.Tower => 2,
                Unit.Peasant => 1,
                Unit.Spearman => 2,
                Unit.Knight => 3,
                Unit.Baron => 4,
                _ => 0
            };
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GetUnitCost(Unit unit)
        {
            return unit switch
            {
                Unit.Tower => 15,
                Unit.Peasant => 10,
                Unit.Spearman => 20,
                Unit.Knight => 30,
                Unit.Baron => 40,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GetUnitMaintenance(Unit unit)
        {
            return unit switch
            {
                Unit.Peasant => 2,
                Unit.Spearman => 6,
                Unit.Knight => 18,
                Unit.Baron => 64,
                _ => 0
            };
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Fill<T>(T[,] array, T value)
        {
            var height = array.GetLength(0);
            var width = array.GetLength(1);

            for (int row = 0; row < height; row++)
            {
                for (int col = 0; col < width; col++)
                {
                    array[row, col] = value;
                }
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ClearVisits()
        {
            Array.Clear(m_VisitedBuffer, 0, m_VisitedBuffer.Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SetVisited(OffsetCoordinates coordinates)
        {
            m_VisitedBuffer[coordinates.Row, coordinates.Col] = true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool WasVisited(OffsetCoordinates coordinates)
        {
            return m_VisitedBuffer[coordinates.Row, coordinates.Col];
        }
    }
}
