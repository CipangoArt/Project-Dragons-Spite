using UnityEngine;
using UnityEngine.Events;
using System;

public class HealthManager : MonoBehaviour
{
    [SerializeField] private int maxHealth = 5;
    [SerializeField] private int currentHealth;

    [SerializeField] private float timeGainOnDestroyed;
    [SerializeField] private float gaugeGainOnDestroyed;

    public VillageBehaviour villageBehaviour;
    public GaugeSystem gaugeManager;
    public TimeSystem timeManager;

    private int CurrentHealth
    {
        get { return currentHealth; }
        set { currentHealth = Mathf.Clamp(value, 0, maxHealth); }
    }

    private void Awake()
    {
        GameObject gameObject = GameObject.FindGameObjectWithTag("Player");
        gaugeManager = gameObject.GetComponent<GaugeSystem>();
        timeManager = gameObject.GetComponent<TimeSystem>();
        currentHealth = maxHealth;
    }
    public void TakeDamage(int damageAmount)
    {
        currentHealth -= damageAmount;

        if (currentHealth <= 0)
        {
            villageBehaviour.LostDestructables();
            timeManager.GainTime(timeGainOnDestroyed);
            gaugeManager.GainGauge(gaugeGainOnDestroyed);
            Destroy(gameObject);
        }
    }
}
