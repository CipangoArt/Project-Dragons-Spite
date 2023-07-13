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

    private bool destructionTriggered=false;
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
        if (!destructionTriggered)
        {
            currentHealth -= damageAmount;
            if (currentHealth <= 0)
            {
                villageBehaviour.LostDestructables();
                timeManager.GainTime(timeGainOnDestroyed);
                gaugeManager.GainGauge(gaugeGainOnDestroyed);
                HouseExplosion();
                destructionTriggered = true;
                Destroy(gameObject, 8f);
                return;
            }
            else HouseFracture();
        }
       
    }
    private void HouseFracture()
    {
        gameObject.GetComponent<FractureHandler>().DamageHouse();
    }
    private void HouseExplosion()
    {
        gameObject.GetComponent<FractureHandler>().FractureHouse();
    }
}
