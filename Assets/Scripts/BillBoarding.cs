using UnityEngine;

public class BillBoarding : MonoBehaviour
{
    private Vector3 originalRotation;
    private void Awake()
    {
        originalRotation = transform.rotation.eulerAngles;
    }
    private void LateUpdate()
    {
        transform.forward = Camera.main.transform.forward;
        Vector3 rotation = transform.rotation.eulerAngles;
        rotation.x = originalRotation.x;
        rotation.z = originalRotation.z;
        transform.rotation = Quaternion.Euler(rotation);
    }
}
