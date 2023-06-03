using System.Collections;
using UnityEngine;

public class BalistaBehaviour : MonoBehaviour
{
    [SerializeField] private float projectileSpeed;

    [SerializeField] private float reloadTime;

    [SerializeField] private GameObject projectilePref;

    VillageBehaviour villageBehaviour;
    Transform target;
    Rigidbody targetRb;
    Coroutine reloadTimeCoroutine;

    private void Awake()
    {
        villageBehaviour = FindObjectOfType<VillageBehaviour>();
        target = GameObject.FindGameObjectWithTag("Player").transform;
        targetRb = target.gameObject.GetComponent<Rigidbody>();

        if (villageBehaviour != null)
        {
            villageBehaviour.OnVillageEnter += OnVillageEnter;
            villageBehaviour.OnVillageExit += OnVillageExit;
        }
    }
    private void OnDestroy()
    {
        if (villageBehaviour != null)
        {
            villageBehaviour.OnVillageEnter -= OnVillageEnter;
            villageBehaviour.OnVillageExit -= OnVillageExit;
        }

        StopCoroutine(reloadTimeCoroutine);
    }
    public void OnVillageEnter()
    {
        reloadTimeCoroutine = StartCoroutine(ReloadTime(reloadTime));
    }
    public void OnVillageExit()
    {
        StopCoroutine(reloadTimeCoroutine);
    }
    public IEnumerator ReloadTime(float reloadTime)
    {
        while (true)
        {
            yield return new WaitForSeconds(reloadTime);
            ShootTarget();
        }
    }
    private void ShootTarget()
    {
        var newProjectile = Instantiate(projectilePref, transform.position, Quaternion.LookRotation(target.position, Vector3.up));
        var projectileRb = newProjectile.GetComponent<Rigidbody>();
        if (InterceptionDirection(target.position, transform.position, targetRb.velocity, projectileSpeed, out var direction))
        {
            projectileRb.velocity = direction * projectileSpeed;
        }
        else
        {
            projectileRb.velocity = (target.transform.position - transform.position).normalized * projectileSpeed;
        }
    }
    public bool InterceptionDirection(Vector3 a, Vector3 b, Vector3 vA, float sB, out Vector3 result)
    {
        var aToB = b - a;
        var dC = aToB.magnitude;
        var alpha = Vector3.Angle(aToB, vA) * Mathf.Deg2Rad;
        var sA = vA.magnitude;
        var r = sA / sB;
        if (Meth.SolveQuadratic(1-r*r, 2*r*dC*Mathf.Cos(alpha), -(dC*dC), out var root1, out var root2) == 0)
        {
            result = Vector3.zero;
            return false;
        }
        var dA = Mathf.Max(root1, root2);
        var t = dA / sB;
        var c = a + vA * t;
        result = (c - b).normalized;
        return true;
    }
}
public class Meth
{
    public static int SolveQuadratic(float a, float b, float c, out float root1, out float root2)
    {
        var discriminant = b * b - 4 * a * c;
        if (discriminant < 0)
        {
            root1 = Mathf.Infinity;
            root2 = -root1;
            return 0;
        }

        root1 = (-b + Mathf.Sqrt(discriminant)) / (2 * a);
        root2 = (-b - Mathf.Sqrt(discriminant)) / (2 * a);
        return discriminant > 0 ? 2 : 1;
    }
}