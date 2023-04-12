using UnityEngine;

public class OnSlopeBehaviour : MonoBehaviour
{
    [SerializeField] private Transform feetFrontRight, feetFrontLeft;

    [SerializeField] private LayerMask _layerMask;

    [SerializeField] private float smoothTimeTargetRotation;

    [SerializeField] private float playerForceToGround;

    [Header("Booleans")]
    [SerializeField] private bool onSlopePosition;
    [SerializeField] private bool onSlopeRotation;

    [Header("RayCastHits")]
    private float rayCastLenght = 1f;
    private Vector3 targetPos;

    private Vector3 averageNormal;
    private Quaternion targetRotation;
    private Rigidbody _rigidbody;
    private int baseRayCastCount = 0;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }
    private void Update()
    {
        GetComponent<Renderer>().material.SetColor(
            "_Color", PlayerMovement._state == PlayerMovement.State.Grounded ? Color.black : Color.blue
        );
    }
    private void FixedUpdate()
    {
        if (onSlopePosition && PlayerMovement._state == PlayerMovement.State.Grounded) OnSlopePosition();
        if (onSlopeRotation && PlayerMovement._state == PlayerMovement.State.Grounded) OnSlopeRotation();
    }
    private void OnSlopePosition()
    {
        if (!Physics.Raycast(transform.position, -transform.up, .6f))
        {
            _rigidbody.AddForce(-transform.up * playerForceToGround);
        }
        else
        {
            targetRotation = Quaternion.FromToRotation(transform.up, new Vector3(_rigidbody.rotation.x, _rigidbody.rotation.y, 0f));
            targetRotation = Quaternion.Slerp(_rigidbody.rotation, targetRotation, Time.deltaTime * 10);
            _rigidbody.MoveRotation(targetRotation);
        }
    }
    private void OnSlopeRotation()
    {
        baseRayCastCount = 0;

        BaseRayCasts(transform.forward, Vector3.zero, Vector3.zero); // Fowards

        BaseRayCasts(-transform.up, Vector3.zero, Vector3.zero); //Downwards
        BaseRayCasts(-transform.up, Vector3.forward, Vector3.zero); // Fowards
        BaseRayCasts(-transform.up, Vector3.right, Vector3.zero); // Right
        BaseRayCasts(-transform.up, Vector3.left, Vector3.zero); // Left

        BaseRayCasts(-transform.up, Vector3.forward, Vector3.right); // Foward Right
        BaseRayCasts(-transform.up, Vector3.forward, Vector3.left); // Foward Left
        BaseRayCasts(-transform.up, Vector3.back, Vector3.right);
        BaseRayCasts(-transform.up, Vector3.back, Vector3.left);

        averageNormal /= 2;

        targetRotation = Quaternion.FromToRotation(transform.up, averageNormal) * transform.rotation;
        targetRotation = Quaternion.Slerp(_rigidbody.rotation, targetRotation, Time.deltaTime * smoothTimeTargetRotation);
        _rigidbody.MoveRotation(targetRotation);
    }
    private void BaseRayCasts(Vector3 heightDir, Vector3 firstDir, Vector3 secondDir)
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, heightDir + (firstDir + secondDir).normalized, out hit, rayCastLenght, _layerMask))
        {
            baseRayCastCount++;
            averageNormal += hit.normal;
            Debug.DrawRay(transform.position, heightDir + (firstDir + secondDir).normalized);
        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.DrawRay(transform.position, -transform.up * .6f);
    }
}
