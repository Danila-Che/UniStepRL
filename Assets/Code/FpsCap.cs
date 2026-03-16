using UnityEngine;

public class FpsCap : MonoBehaviour
{
    [SerializeField] private int m_TargetFrameRate = 60;
    
    public void OnEnable()
    {
        QualitySettings.vSyncCount = 0;

        Application.targetFrameRate = m_TargetFrameRate;
    }
}
