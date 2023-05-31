using UnityEngine;
using UnityEngine.UI;

public class Gauge : MonoBehaviour
{
    PlayerInput playerInput;

    [SerializeField] private Image gaugeBar;
    [SerializeField] private bool isGaugeDepleted;
    [SerializeField] private float currentGauge;
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
        playerInput = GetComponent<PlayerInput>();
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

        currentGauge = Mathf.Clamp(currentGauge, 0, maxGauge);
    }
    public void LoseGauge(float gaugeLost)
    {
        currentGauge -= gaugeLost;
        currentGauge = Mathf.Clamp(currentGauge, 0, maxGauge);
    }
    public void LoseFuelGradually(float gaugeLoseSpeed)
    {
        currentGauge -= Time.deltaTime * gaugeLoseSpeed;
    }
}
