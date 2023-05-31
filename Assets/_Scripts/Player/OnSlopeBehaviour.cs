using UnityEngine;

public class OnSlopeBehaviour : MonoBehaviour
{
    PlayerController playerController;
    Rigidbody rb;

    [SerializeField] private bool onSlopePosition;
    [SerializeField] private bool onSlopeRotation;

    [SerializeField] private float rayCastLenght = 1f;
    [SerializeField] private LayerMask _layerMask;
    [SerializeField] private float smoothTimeTargetRotation;
    [SerializeField] private float playerForceToGround;

    private Vector3 averageNormal;
    private Quaternion targetRotation;
    private int baseRayCastCount = 0;
    private bool baseRayCastDetection = false;

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
        rb = GetComponent<Rigidbody>();
    }
    private void FixedUpdate()
    {
        if (playerController._state != PlayerController.State.Grounded) return;
        if (onSlopePosition) OnSlopePosition();
        if (onSlopeRotation) OnSlopeRotation();
    }
    private void OnSlopePosition()
    {
        if (!Physics.Raycast(transform.position, -transform.up))
        {
            Quaternion targetRotation = Quaternion.FromToRotation(transform.up, new Vector3(rb.rotation.eulerAngles.x, rb.rotation.eulerAngles.y, 0f));
            targetRotation = Quaternion.Slerp(rb.rotation, targetRotation, Time.deltaTime * 100);
            rb.MoveRotation(targetRotation);
            return;
        }
        if (!Physics.Raycast(transform.position, -transform.up, .6f))
        {
            rb.AddForce(-transform.up * playerForceToGround);
        }
    }
    private void OnSlopeRotation()
    {
        baseRayCastDetection = false;

        baseRayCastCount = 0;

        //BaseRayCasts(transform.position, -transform.up, Vector3.zero, Vector3.zero); //Downwards

        BaseRayCasts(transform.position, Vector3.zero, transform.forward, Vector3.zero); // Fowards
        BaseRayCasts(transform.position, Vector3.zero, transform.forward, Vector3.zero); // Fowards
        BaseRayCasts(transform.position, Vector3.zero, transform.forward, Vector3.zero); // Fowards

        BaseRayCasts(transform.position, -transform.up, transform.forward, Vector3.zero); // Fowards
        BaseRayCasts(transform.position, -transform.up, transform.forward, Vector3.zero); // Fowards
        BaseRayCasts(transform.position, -transform.up, transform.right, Vector3.zero); // Right
        BaseRayCasts(transform.position, -transform.up, -transform.right, Vector3.zero); // Left

        BaseRayCasts(transform.position, -transform.up, transform.forward, transform.right); //FowardRight
        BaseRayCasts(transform.position, -transform.up, transform.forward, -transform.right); //FowardLeft

        BaseRayCasts(transform.position, -transform.up, -transform.forward, transform.right); //BackRight
        BaseRayCasts(transform.position, -transform.up, -transform.forward, -transform.right); //BackLeft

        if (baseRayCastDetection)
        {
            averageNormal /= baseRayCastCount;

            targetRotation = Quaternion.FromToRotation(transform.up, averageNormal) * transform.rotation;
            targetRotation = Quaternion.Slerp(rb.rotation, targetRotation, Time.deltaTime * smoothTimeTargetRotation);
            rb.MoveRotation(Quaternion.Euler(targetRotation.eulerAngles.x, rb.rotation.eulerAngles.y, targetRotation.eulerAngles.z));
        }
    }
    private void BaseRayCasts(Vector3 origin, Vector3 heightDir, Vector3 firstDir, Vector3 secondDir)
    {
        RaycastHit hit;
        if (Physics.Raycast(origin, heightDir + (firstDir + secondDir).normalized, out hit, rayCastLenght, _layerMask))
        {
            baseRayCastDetection = true;
            baseRayCastCount++;
            averageNormal += hit.normal;
        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.black;
        Gizmos.DrawRay(transform.position, (transform.forward) * rayCastLenght);

        Gizmos.color = Color.black;
        Gizmos.DrawRay(transform.position, (-transform.up) * rayCastLenght);
        Gizmos.DrawRay(transform.position, (-transform.up + (transform.forward).normalized) * rayCastLenght);
        Gizmos.DrawRay(transform.position, (-transform.up + (transform.right).normalized) * rayCastLenght);
        Gizmos.DrawRay(transform.position, (-transform.up + (-transform.right).normalized) * rayCastLenght);

        Gizmos.color = Color.black;
        Gizmos.DrawRay(transform.position, (-transform.up + (transform.forward + transform.right).normalized) * rayCastLenght);
        Gizmos.DrawRay(transform.position, (-transform.up + (transform.forward + -transform.right).normalized) * rayCastLenght);
        Gizmos.DrawRay(transform.position, (-transform.up + (-transform.forward + transform.right).normalized) * rayCastLenght);
        Gizmos.DrawRay(transform.position, (-transform.up + (-transform.forward + -transform.right).normalized) * rayCastLenght);
    }
}
