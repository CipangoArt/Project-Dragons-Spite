using UnityEngine;
public class OnSlopeBehaviour : MonoBehaviour
{
    [SerializeField] private Transform[] feet;

    [SerializeField] private LayerMask _layerMask;
    [SerializeField] private float playerHeight;
    private Vector3 averageNormal;
    private Quaternion targetRotation;

    [SerializeField] private float smoothTimeTargetPosition;
    private void FixedUpdate()
    {
        OnSlopePosition();
    }
    private void Update()
    {
        OnSlopeRotation();
    }
    private void OnSlopePosition()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, -transform.up, out hit, Mathf.Infinity, _layerMask))
        {
            transform.position = new Vector3(transform.position.x, hit.point.y + playerHeight, transform.position.z);
        }
    }
    private void OnSlopeRotation()
    {
        for (int i = 0; i < feet.Length; i++)
        {
            RaycastHit hit;
            Physics.Raycast(feet[i].position, -feet[i].up, out hit, Mathf.Infinity, _layerMask);
            Debug.DrawRay(feet[i].position, -feet[i].up);
            averageNormal += hit.normal;
        }
        averageNormal /= feet.Length;

        targetRotation = Quaternion.FromToRotation(transform.up, averageNormal) * transform.rotation;

        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * smoothTimeTargetPosition);
    }
}
