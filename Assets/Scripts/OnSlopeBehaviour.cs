using UnityEngine;
public class OnSlopeBehaviour : MonoBehaviour
{
    [SerializeField] private Transform[] feet;

    [SerializeField] private LayerMask playerLayer;
    private Vector3 averageNormal;
    private Quaternion targetRotation;
    private Vector3 targetPosition;
    private Vector3 smoothTargetPosition;
    private Vector3 currentVelocityTargetPosition = Vector3.zero;

    [SerializeField] private float smoothTimeTargetPosition;

    private void Update()
    {
        for (int i = 0; i < feet.Length; i++)
        {
            RaycastHit hit;
            Physics.Raycast(feet[i].position, -feet[i].up, out hit, Mathf.Infinity, playerLayer);
            averageNormal += hit.normal;
        }
        averageNormal /= feet.Length;

        targetRotation = Quaternion.FromToRotation(transform.up, averageNormal) * transform.rotation;

        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * smoothTimeTargetPosition);
    }
    private void ShootRayCast(Transform feet, out RaycastHit hit)
    {
        Physics.Raycast(feet.position, -feet.up, out hit, Mathf.Infinity, playerLayer);
        Debug.DrawRay(feet.position, -feet.up);
    }
}
