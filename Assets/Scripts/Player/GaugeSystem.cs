using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GaugeSystem : MonoBehaviour
{
    [SerializeField] private Image gaugeBar;
    [SerializeField] public float currentGauge;
    [SerializeField] private float maxGauge;

    public float CurrentGauge
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
        UpdateGaugeBar();
    }
    private void UpdateGaugeBar()
    {
        gaugeBar.fillAmount = currentGauge / maxGauge;
    }
    public void GainGauge(float gaugeGained)
    {
        CurrentGauge += gaugeGained;
    }
    public void LoseGauge(float gaugeLost)
    {
        CurrentGauge -= gaugeLost;
    }
    public IEnumerator GradualLoseGauge(float gaugeLoseSpeed)
    {
        while (CurrentGauge > 0)
        {
            CurrentGauge -= Time.deltaTime * gaugeLoseSpeed;
            yield return new WaitForSeconds(Time.deltaTime);
        }

    }
}
