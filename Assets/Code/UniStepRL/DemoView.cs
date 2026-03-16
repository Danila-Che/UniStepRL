using System;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UniStepRL
{
    public class DemoView : MonoBehaviour
    {
        [SerializeField] private TMP_Text m_ActionText;
        [SerializeField] private TMP_Text m_CurrentStepText;
        [SerializeField] private Slider m_Slider;
        
        [SerializeField] private TMP_Text m_MapSizeText;
        [SerializeField] private TMP_Text m_MapSeedText;

        [SerializeField] private TMP_Text m_EntityLUT;

        [Header("Player 1")]
        [SerializeField] private TMP_Text m_Player1GoldText;
        [SerializeField] private TMP_Text m_Player1TotalCityTilesText;
        [SerializeField] private TMP_Text m_Player1MaintenanceText;
        
        [Header("Player 2")]
        [SerializeField] private TMP_Text m_Player2GoldText;
        [SerializeField] private TMP_Text m_Player2TotalCityTilesText;
        [SerializeField] private TMP_Text m_Player2MaintenanceText;

        [Header("Progress")]
        [SerializeField] private GameObject m_SelfPlayProgressPanel;
        [SerializeField] private Slider m_SelfPlayProgressBar;

        public void ShowSelfPlayProgress()
        {
            m_SelfPlayProgressPanel.SetActive(true);
        }

        public void UpdateSelfPlayProgress(int time, int timeLimit)
        {
            m_SelfPlayProgressBar.minValue = 0;
            m_SelfPlayProgressBar.maxValue = timeLimit;
            m_SelfPlayProgressBar.value = time;
        }

        public void HideSelfPlayProgress()
        {
            m_SelfPlayProgressPanel.SetActive(false);
        }
        
        public void SetTimeLimit(int timeLimit)
        {
            m_Slider.maxValue = timeLimit;
        }

        public void SetCurrentStep(int step)
        {
            m_CurrentStepText.SetText(step.ToString("000"));
            m_Slider.SetValueWithoutNotify(step);
        }

        public void SetNoneAction()
        {
            m_ActionText.SetText("none");
        }

        public void SetAction(Player player, string playerType, EnvironmentAction action)
        {
            switch (action.Type)
            {
                case ActionType.BuildTower:
                    m_ActionText.SetText($"{player} {playerType} build tower: {action.BuildTowerP0}");
                    break;
                
                case ActionType.PlaceUnit:
                    m_ActionText.SetText($"{player} {playerType} place unit: {action.PlaceUnitP0} -> {action.PlaceUnitP1}");
                    break;
                
                case ActionType.MoveUnit:
                    m_ActionText.SetText($"{player} {playerType} move unit: {action.MoveUnitP0} -> {action.MoveUnitP1}");
                    break;
                
                case ActionType.EndTurn:
                    m_ActionText.SetText($"{player} {playerType} end turn");
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void SetMapSize(int width, int height)
        {
            m_MapSizeText.SetText($"{width}x{height}");
        }

        public void SetMapSeed(int seed)
        {
            m_MapSeedText.SetText(seed.ToString());
        }

        public void SetGold(Player player, int gold)
        {
            switch (player)
            {
                case Player.Player1:
                    m_Player1GoldText.SetText(gold.ToString());
                    break;
                
                case Player.Player2:
                    m_Player2GoldText.SetText(gold.ToString());
                    break;
            }
        }
        
        public void SetTotalCityTiles(Player player, int totalCityTiles)
        {
            switch (player)
            {
                case Player.Player1:
                    m_Player1TotalCityTilesText.SetText(totalCityTiles.ToString());
                    break;
                
                case Player.Player2:
                    m_Player2TotalCityTilesText.SetText(totalCityTiles.ToString());
                    break;
            }
        }

        public void SetMaintenance(Player player, int maintenance)
        {
            switch (player)
            {
                case Player.Player1:
                    m_Player1MaintenanceText.SetText(maintenance.ToString());
                    break;
                
                case Player.Player2:
                    m_Player2MaintenanceText.SetText(maintenance.ToString());
                    break;
            }
        }

        public void SetEntityLUT(int[,] lut)
        {
            var height = lut.GetLength(0);
            var width = lut.GetLength(1);
            var builder = new StringBuilder();

            for (int row = 0; row < height; row++)
            {
                if ((row & 1) == 1)
                {
                    builder.Append(' ');
                }

                for (int col = 0; col < width; col++)
                {
                    var index = lut[height - 1 - row, col];
                    builder.Append(index == -1 ? "*" : index.ToString());
                    builder.Append(' ');
                }

                builder.AppendLine();
            }

            m_EntityLUT.SetText(builder.ToString());
        }
    }
}
