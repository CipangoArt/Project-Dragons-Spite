using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireBallPref : MonoBehaviour
{
    [SerializeField] private HealthManager healthManager;

    [SerializeField] private Rigidbody rb;
    [SerializeField] private float fireBallForce;
    [SerializeField] GameObject VFX_to_Debug;

    public float initialVelocity;
    [SerializeField] private float maxLifeSpan;
    private float lifeSpanCount;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.gameObject.CompareTag("Player"))
        {
            if (other.gameObject.CompareTag("Structure"))
            {
                healthManager = other.GetComponent<HealthManager>();
                healthManager.TakeDamage(1);
            }
            Explode();
        }
    }
    private void FixedUpdate()
    {
        rb.velocity = transform.forward * (fireBallForce + initialVelocity);
    }
    private void Update()
    {
        lifeSpanCount += Time.deltaTime;
        if (lifeSpanCount >= maxLifeSpan)
        {
            Explode();
        }
    }
    void Explode()
    {
        Instantiate(VFX_to_Debug, transform.position, VFX_to_Debug.transform.rotation);
        Destroy(gameObject);
    }
}
