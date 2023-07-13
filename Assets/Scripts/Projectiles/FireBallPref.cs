using UnityEngine;
using Cinemachine;

public class FireBallPref : MonoBehaviour
{
    private HealthManager healthManager;
    [SerializeField] private Rigidbody rb;
    public CinemachineImpulseSource impulseSource;

    [SerializeField] private GameObject VFX_to_Debug;

    [SerializeField] private float maxLifeSpan;
    private float lifeSpanCount;
    [SerializeField] private int damage;
    [SerializeField] private float fireBallForce;
    [SerializeField] private float AOERadious;
    public float initialVelocity;

    private void Awake()
    {
        impulseSource = GetComponent<CinemachineImpulseSource>();
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
    private void OnTriggerEnter(Collider other)
    {
        if (!other.gameObject.CompareTag("Player") && !other.gameObject.CompareTag("Ignore"))
        {
            Explode();
        }
    }
    void Explode()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, AOERadious);
        for (int i = 0; i < hitColliders.Length; i++)
        {
            if (hitColliders[i].gameObject.CompareTag("Destructable") || hitColliders[i].gameObject.CompareTag("Balista") || hitColliders[i].gameObject.CompareTag("Villager"))
            {
                healthManager = hitColliders[i].GetComponent<HealthManager>();
                healthManager.TakeDamage(damage);
            }
        }
        ThirdPersonCameraSystem.instance.CameraShake(ThirdPersonCameraSystem.instance.impulseSource);
        Instantiate(VFX_to_Debug, transform.position, VFX_to_Debug.transform.rotation);
        AudioManager.instance.PlaySound("event:/Player/ExplosionsX");
        Destroy(gameObject);
    }
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, AOERadious);
    }
}
