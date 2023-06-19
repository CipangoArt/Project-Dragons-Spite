using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoltPref : MonoBehaviour
{
    [SerializeField] private int damage;
    [SerializeField] private float maxLifeSpan;
    private float lifeSpanCount;
    
    private void Update()
    {
        lifeSpanCount += Time.deltaTime;
        if (lifeSpanCount >= maxLifeSpan)
        {
            Destroy(gameObject);
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.gameObject.name);
        if (other.gameObject.CompareTag("Player"))
        {
            other.GetComponent<GaugeManager>().LoseGauge(damage);
            Destroy(gameObject);
        }
        if (other.gameObject.CompareTag("Balista"))
        {
            return;
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
