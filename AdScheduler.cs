using UnityEngine;

public class AdScheduler : MonoBehaviour
{
    [SerializeField] private int m_Timer = 15;

    private void ShowInterstitial()
    {
        if (MonetizationManager.Instance != null)
        {
            MonetizationManager.Instance.ShowInterstitial();
        }

        ScheduleInterstitial();
    }

    private void ScheduleInterstitial()
    {
        if (m_Timer > 0)
        {
            Invoke(nameof(ShowInterstitial), m_Timer);
        }
    }

    private void Awake()
    {
        ScheduleInterstitial();
    }
}
