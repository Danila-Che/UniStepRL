using System;
using System.Collections.Generic;
using UnityEngine;

namespace UniStepRL
{
    public class EnvironmentRenderer : MonoBehaviour
    {
        private const float k_OuterRadius = 0.5f;
        private const float k_InnerRadius = k_OuterRadius * 0.866025404f; // sqrt(3)/2
        
        [Header("Grid")]
        [Min(0.0f)]
        [SerializeField] private float m_HexSize;
        [SerializeField] private CellController m_LandCellPrefab;
        [SerializeField] private CellController m_WaterCellPrefab;
        [Space]
        [SerializeField] private Material m_NeutralMaterial;
        [SerializeField] private Material m_Player1Material;
        [SerializeField] private Material m_Player2Material;
        [SerializeField] private Material m_WildMaterial;

        [Header("Units")]
        [SerializeField] private GameObject m_FortPrefab;
        [SerializeField] private GameObject m_TowerPrefab;
        [Space]
        [SerializeField] private GameObject m_PeasantPrefab;
        [SerializeField] private GameObject m_SpearmanPrefab;
        [SerializeField] private GameObject m_KnightPrefab;
        [SerializeField] private GameObject m_BaronPrefab;
        [Space]
        [SerializeField] private GameObject m_GravePrefab;
        [SerializeField] private GameObject m_TreePrefab;
        [SerializeField] private GameObject m_WildTreePrefab;
        
        private CellController[,] m_Cells;

        private float m_XOffset;
        private float m_ZOffset;

        public void SeedState(EnvironmentConfiguration config, SimpleEnvironment environment)
        {
            var edgeWorldPosition = ToWorldPositionPointy(new OffsetCoordinates(config.Width - 1, config.Height - 1));
            m_XOffset = -0.5f * edgeWorldPosition.x;
            m_ZOffset = -0.5f * edgeWorldPosition.z;
            
            m_Cells = new CellController[config.Height + 2, config.Width + 2];
            var water = new HashSet<OffsetCoordinates>();

            for (int row = 0; row < config.Height; row++)
            {
                for (int col = 0; col < config.Width; col++)
                {
                    var coordinates = new OffsetCoordinates(col, row);

                    if (environment.GetCellType(coordinates) == CellType.Land)
                    {
                        var worldPosition = ToWorldPositionPointy(coordinates);

                        var cellController = Instantiate(m_LandCellPrefab, worldPosition, Quaternion.identity);
                        m_Cells[row, col] = cellController;
                        cellController.name = $"Land_Hex_{coordinates.Col}_{coordinates.Row}";

                        foreach (var neighbor in coordinates.IterateOddRNeighbors())
                        {
                            if (!environment.IsValid(neighbor) || environment.GetCellType(neighbor) == CellType.Water)
                            {
                                water.Add(neighbor);
                            }
                        }
                    }
                    else
                    {
                        water.Add(coordinates);
                    }
                }
            }

            foreach (var coordinates in water)
            {
                var worldPosition = ToWorldPositionPointy(coordinates);

                var cellController = Instantiate(m_WaterCellPrefab, worldPosition, Quaternion.identity);
                cellController.name = $"Water_Hex_{coordinates.Col}_{coordinates.Row}";
            }
        }

        public void UpdateState(EnvironmentConfiguration config, EnvironmentState state)
        {
            for (int row = 0; row < config.Height; row++)
            {
                for (int col = 0; col < config.Width; col++)
                {
                    var cellController = m_Cells[row, col];

                    if (cellController != null)
                    {
                        var coordinates = new OffsetCoordinates(col, row);
                        cellController.SetMaterial(GetMaterial(state.GetPlayer(coordinates)));
                        cellController.RemoveUnit();
                        cellController.PlaceUnit(GetPrefab(state.GetUnit(coordinates)));
                    }
                }
            }
        }

        private Material GetMaterial(Player player)
        {
            return player switch
            {
                Player.None => m_NeutralMaterial,
                Player.Player1 => m_Player1Material,
                Player.Player2 => m_Player2Material,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
        
        private GameObject GetPrefab(Unit unit)
        {
            return unit switch
            {
                Unit.None => null,
                Unit.Fort => m_FortPrefab,
                Unit.Tower => m_TowerPrefab,
                Unit.Peasant => m_PeasantPrefab,
                Unit.Spearman => m_SpearmanPrefab,
                Unit.Knight => m_KnightPrefab,
                Unit.Baron => m_BaronPrefab,
                // Unit.Grave => m_GravePrefab,
                // Unit.Tree => m_TreePrefab,
                // Unit.WildTree => m_WildTreePrefab,
                _ => throw new ArgumentOutOfRangeException(nameof(unit), unit, null)
            };
        }

        private Vector3 ToWorldPositionPointy(OffsetCoordinates coordinates, float yPosition = 0.0f)
        {
            var col = coordinates.Col;
            var row = coordinates.Row;

            if (row < 0)
            {
                col += 1;
            }
            
            var x = (col + 0.5f * row - row/2) * m_HexSize * 2.0f * k_InnerRadius;
            var z = row * m_HexSize * 1.5f * k_OuterRadius;
            
            return new Vector3(x + m_XOffset, yPosition, z + m_ZOffset);
        }
    }
}
