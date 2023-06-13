using UnityEngine;
using UnityEngine.UI;

public class GaugeManager : MonoBehaviour
{
    [SerializeField] private Image gaugeBar;
    [SerializeField] private bool isGaugeDepleted;
    [SerializeField] public float currentGauge;
    [SerializeField] private float maxGauge;
    [SerializeField] private float gaugeDepletingSpeed;
    [SerializeField] private float gaugeGainSpeed;

    private float CurrentGauge
    {
        get { return currentGauge; }
        set { currentGauge = Mathf.Clamp(value, 0, maxGauge); }
    }
    private void Awake()
    {
        currentGauge = maxGauge;
    }
    private void Update()
    {
        GaugeBar();
    }
    private void GaugeBar()
    {
        gaugeBar.fillAmount = currentGauge / maxGauge;
    }
    public void GainGauge(float gaugeGained)
    {
        currentGauge += gaugeGained;
    }
    public void LoseGauge(float gaugeLost)
    {
        currentGauge -= gaugeLost;
    }
    public void LoseFuelGradually(float gaugeLoseSpeed)
    {
        currentGauge -= Time.deltaTime * gaugeLoseSpeed;
    }
}
