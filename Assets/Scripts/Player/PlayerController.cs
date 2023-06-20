using UnityEngine;

[RequireComponent(typeof(Rigidbody))]

public class PlayerController : MonoBehaviour
{
    [SerializeField] private GameObject turboVF;
    [SerializeField] private GameObject turboVF2;
    [SerializeField] private GameObject initialTurboPref;
    [SerializeField] public Animator animBody;
    [SerializeField] public Animator animWings;
    private Rigidbody rb;
    private PlayerInput playerInput;

    [SerializeField] private GameObject sphere;

    public State _state;

    [Header("Speed")]
    [SerializeField] private float maxSpeed;
    [SerializeField] private float maxTargetSpeed;
    public float currentSpeed;
    public float defaultTargetSpeed = 750f;
    [SerializeField] private float multiplierTargetSpeed;
    [SerializeField] private float modifiedTargetSpeed;

    [Header("Multipliers")]
    [SerializeField] private float accelerationMultiplier = 1f;
    [SerializeField] private float deccelarationMultiplier = 1f;
    [SerializeField] private float groundedPitchVelocityModifier = 2f;
    [SerializeField] private float airbornPitchVelocityModifier = 2f;
    public float turboTargetMultiplier = 2f;

    [Header("Rotation")]
    [SerializeField] private float turnSmoothTime = 1f;

    [Header("Airborn Rotation")]
    [SerializeField] private float pitchMaxAngle;
    [SerializeField] private float pitchMinAngle;

    [SerializeField] private float rollTurnSmoothTime = 1f;
    [SerializeField] private float pitchTurnSmoothTime = 1f;
    [SerializeField] private float yawTurnSmoothTime = 1f;

    [Header("Input")]
    private Vector3 _cameraRelativeDir;

    [Header("Booleans")]
    [SerializeField] private bool detectIsGrounded = false;
    [SerializeField] private bool pitchRestriction = false;


    [Header("RayCastHits")]
    [SerializeField] private float heightToAirborne = 2f;
    [SerializeField] private float heightToGrounded = 2f;

    [SerializeField] private LayerMask _layerMask;
    private float lastVelocity;

    float upHillMinThreshold = -25f;
    float downHillMinThreshold = 25f;
    float upHillMaxThreshold;
    float downHillMaxThreshold;

    float incrementVelocity;
    float decrementVelocity;

    public float CurrentSpeed
    {
        get { return currentSpeed; }
        set { Mathf.Clamp(value, 0, maxSpeed); }
    }

    public enum State
    {
        Grounded,
        Airborne,
        Gliding
    }
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        playerInput = GetComponent<PlayerInput>();
    }
    private void FixedUpdate()
    {
        if (playerInput.isTurboing)
        {
            turboVF.SetActive(true);
            turboVF2.SetActive(true);
            //Instantiate(initialTurboPref, turboVF.transform.position, Quaternion.identity);
            //Instantiate(initialTurboPref, turboVF2.transform.position, Quaternion.identity);
        }
        else
        {
            turboVF.SetActive(false);
            turboVF2.SetActive(false);
        }
        switch (_state)
        {
            case State.Grounded:
                animBody.SetBool("Airborne", false);
                animBody.SetBool("Gliding", false);
                animBody.SetBool("Grounded", true);
                animWings.SetBool("Airborne", false);
                animWings.SetBool("Gliding", false);
                animWings.SetBool("Grounded", true);
                rb.useGravity = false;

                //float yeah = rb.velocity.magnitude / maxSpeed;
                //anim.SetFloat("Running", yeah);

                float yeah = currentSpeed / defaultTargetSpeed;
                animBody.SetFloat("Running", yeah);
                animWings.SetFloat("Running", yeah);

                //OffGround -> Airborne
                if (!Physics.Raycast(transform.position, -transform.up, heightToAirborne)) { GoAirborne(); return; }
                //!Inputing
                if (playerInput.direction.magnitude == 0 && !playerInput.isTurboing)
                {
                    //Degrade Speed
                    currentSpeed = Mathf.Lerp(currentSpeed, 0f, deccelarationMultiplier * Time.deltaTime);
                    rb.velocity = transform.forward * currentSpeed * Time.deltaTime;
                    return;
                }
                ApplyGroundRotation();
                ApplyGroundMovement();
                break;

            case State.Airborne:
                animBody.SetBool("Airborne", true);
                animBody.SetBool("Gliding", false);
                animBody.SetBool("Grounded", false);
                animWings.SetBool("Airborne", true);
                animWings.SetBool("Gliding", false);
                animWings.SetBool("Grounded", false);
                rb.useGravity = true;

                GoGrounded();
                ApplyAirborneRotation();
                break;

            case State.Gliding:
                animBody.SetBool("Airborne", false);
                animBody.SetBool("Gliding", true);
                animBody.SetBool("Grounded", false);
                animWings.SetBool("Airborne", false);
                animWings.SetBool("Gliding", true);
                animWings.SetBool("Grounded", false);
                rb.useGravity = false;

                GoGrounded();
                ApplyGlidingRotation();
                ApplyGlidingMovement();
                break;
            default:
                break;
        }
    }

    //Variable Modifiers
    Vector3 ConvertToCameraSpace(Vector3 vectorToRotate)
    {
        float currentYValue = vectorToRotate.y;
        Vector3 cameraFoward = Camera.main.transform.forward;
        Vector3 cameraRight = Camera.main.transform.right;

        //Reset camera Yaw
        cameraFoward.y = 0;
        cameraRight.y = 0;

        //Normalize
        cameraFoward = cameraFoward.normalized;
        cameraRight = cameraRight.normalized;

        Vector3 cameraFowardZProduct = vectorToRotate.z * cameraFoward;
        Vector3 cameraRightXProduct = vectorToRotate.x * cameraRight;

        Vector3 vectorRotatedToCameraSpace = cameraFowardZProduct + cameraRightXProduct;
        vectorRotatedToCameraSpace.y = currentYValue;
        return vectorRotatedToCameraSpace;
    }
    Vector3 ConvertToDragonSpace(Vector3 vectorToRotate)
    {
        float currentYValue = vectorToRotate.y;
        Vector3 cameraFoward = rb.transform.forward;
        Vector3 cameraRight = rb.transform.right;

        //Reset camera Yaw
        cameraFoward.y = 0;
        cameraRight.y = 0;

        //Normalize
        cameraFoward = cameraFoward.normalized;
        cameraRight = cameraRight.normalized;

        Vector3 cameraFowardZProduct = vectorToRotate.z * cameraFoward;
        Vector3 cameraRightXProduct = vectorToRotate.x * cameraRight;

        Vector3 vectorRotatedToCameraSpace = cameraFowardZProduct + cameraRightXProduct;
        vectorRotatedToCameraSpace.y = currentYValue;
        return vectorRotatedToCameraSpace;
    }
    public float PitchMultiplier(float angle, float multiplier)
    {
        angle = Mathf.Clamp(angle, -90f, 90f);

        float t = (angle + 90f) / 180f;
        float y = Mathf.Lerp(0, multiplier, t);
        return y;
    }

    //Ground Movement
    private void ApplyGroundRotation()
    {
        if (playerInput.direction.magnitude == 0) return;

        //Yaw Rotation
        _cameraRelativeDir = ConvertToCameraSpace(playerInput.direction);
        float targetAngle = Mathf.Atan2(_cameraRelativeDir.x, _cameraRelativeDir.z) * Mathf.Rad2Deg;
        float angle = Mathf.LerpAngle(transform.eulerAngles.y, targetAngle, turnSmoothTime * Time.deltaTime);

        rb.MoveRotation(Quaternion.Euler(rb.rotation.eulerAngles.x, angle, rb.rotation.eulerAngles.z));
    }
    private void ApplyGroundMovement()
    {
        multiplierTargetSpeed = 0f;

        Vector3 eulerAngles = transform.rotation.normalized.eulerAngles;
        float xRotation = eulerAngles.x > 180f ? eulerAngles.x - 360f : eulerAngles.x;

        multiplierTargetSpeed += PitchMultiplier(xRotation, groundedPitchVelocityModifier);
        if (playerInput.isTurboing) multiplierTargetSpeed += turboTargetMultiplier;

        //If Accelerating
        if (modifiedTargetSpeed >= currentSpeed)
        {
            currentSpeed = Mathf.Lerp(currentSpeed, defaultTargetSpeed * multiplierTargetSpeed, accelerationMultiplier * Time.deltaTime);
        }
        //If Decelerating
        else if (modifiedTargetSpeed < currentSpeed)
        {
            currentSpeed = Mathf.Lerp(currentSpeed, defaultTargetSpeed * multiplierTargetSpeed, deccelarationMultiplier * Time.deltaTime);
        }

        Vector3 velocity = transform.forward * currentSpeed * Time.deltaTime;

        rb.velocity = Vector3.ClampMagnitude(velocity, maxSpeed);

        modifiedTargetSpeed = defaultTargetSpeed * multiplierTargetSpeed;
    }
    private void ApplyGroundMovementINCREMENT()
    {
        Vector3 eulerAngles = transform.rotation.normalized.eulerAngles;
        float xRotation = eulerAngles.x > 180f ? eulerAngles.x - 360f : eulerAngles.x;
        float multiplier = PitchMultiplier(xRotation, 2);

        DownHill(xRotation, multiplier);

        UpHill(xRotation, multiplier);

        if (playerInput.isTurboing)
        {
            currentSpeed = Mathf.Lerp(currentSpeed, maxSpeed, accelerationMultiplier * Time.deltaTime);
        }
        else
        {
            currentSpeed = Mathf.Lerp(currentSpeed, maxTargetSpeed, accelerationMultiplier * Time.deltaTime);
        }

        Vector3 velocity = transform.forward * currentSpeed * Time.deltaTime;

        rb.velocity = Vector3.ClampMagnitude(velocity, maxSpeed);
    }
    private void DownHill(float xRotation, float multiplier)
    {
        if (xRotation > downHillMinThreshold)
            maxTargetSpeed += Time.deltaTime * multiplier;
    }
    private void UpHill(float xRotation, float multiplier)
    {
        if (xRotation < upHillMinThreshold)
            maxTargetSpeed -= Time.deltaTime * multiplier;
        if (maxTargetSpeed <= defaultTargetSpeed)
        {
            defaultTargetSpeed = maxSpeed;
        }
    }

    //Gliding Movement
    private void ApplyGlidingRotation()
    {
        Vector3 _cameraRelativeDir = ConvertToDragonSpace(playerInput.direction);
        //Yaw
        if (playerInput.direction.x != 0f)
        {
            //Roll
            float targetAngleRoll = Mathf.Atan2(-playerInput.direction.x, playerInput.direction.z) * Mathf.Rad2Deg;
            targetAngleRoll *= .5f;
            float angleRoll = Mathf.LerpAngle(rb.rotation.eulerAngles.z, targetAngleRoll, rollTurnSmoothTime * Time.deltaTime);

            //Yaw
            float targetAngleYaw = Mathf.Atan2(_cameraRelativeDir.x, _cameraRelativeDir.z) * Mathf.Rad2Deg;
            float angleYaw = Mathf.LerpAngle(rb.rotation.eulerAngles.y, targetAngleYaw, yawTurnSmoothTime * Time.deltaTime);

            //Apply Rotation
            rb.MoveRotation(Quaternion.Euler(rb.rotation.eulerAngles.x, angleYaw, angleRoll));
        }
        else
        {
            //Defaut Roll
            float angleRoll = Mathf.LerpAngle(rb.rotation.eulerAngles.z, 0f, rollTurnSmoothTime * Time.deltaTime);
            rb.MoveRotation(Quaternion.Euler(rb.rotation.eulerAngles.x, rb.rotation.eulerAngles.y, angleRoll));
        }
        float targetAnglePitch;
        //Limit Height
        if (rb.velocity.magnitude < 15f)
        {
            pitchRestriction = true;
            targetAnglePitch = Mathf.Atan2(1, -1) * Mathf.Rad2Deg;
            float targetAnglePitchModified = targetAnglePitch * .5f;
            float anglePitch = Mathf.LerpAngle(rb.rotation.eulerAngles.x, targetAnglePitchModified, pitchTurnSmoothTime * Time.deltaTime);

            if (rb.rotation.eulerAngles.x < targetAnglePitchModified * 1.2) pitchRestriction = false;

            rb.MoveRotation(Quaternion.Euler(anglePitch, rb.rotation.eulerAngles.y, rb.rotation.eulerAngles.z));
            return;
        }

        //Pitch
        //if (playerInput.direction.z != 0f && pitchRestriction == false)
        //{
        //    float targetAnglePitch = Mathf.Atan2(playerInput.direction.y, playerInput.direction.z) * Mathf.Rad2Deg;
        //    float anglePitch = Mathf.LerpAngle(rb.rotation.eulerAngles.x, targetAnglePitch, pitchTurnSmoothTime * Time.deltaTime);

        //    Debug.Log(anglePitch);
        //    //anglePitch = Mathf.Clamp(anglePitch, pitchMinAngle, pitchMaxAngle);

        //    //Apply Rotation
        //    rb.MoveRotation(Quaternion.Euler(anglePitch, rb.rotation.eulerAngles.y, rb.rotation.eulerAngles.z));
        //}
        targetAnglePitch = Mathf.Atan2(playerInput.direction.z, playerInput.direction.y) * Mathf.Rad2Deg;

        //Up
        if (targetAnglePitch < 0)
        {
            float anglePitch = Mathf.LerpAngle(rb.rotation.eulerAngles.x, targetAnglePitch * .5f, pitchTurnSmoothTime * Time.deltaTime);

            rb.MoveRotation(Quaternion.Euler(anglePitch, rb.rotation.eulerAngles.y, rb.rotation.eulerAngles.z));
        }
        //Down
        else if (targetAnglePitch > 0)
        {
            float anglePitch = Mathf.LerpAngle(rb.rotation.eulerAngles.x, targetAnglePitch * 1f, pitchTurnSmoothTime * Time.deltaTime);

            rb.MoveRotation(Quaternion.Euler(anglePitch, rb.rotation.eulerAngles.y, rb.rotation.eulerAngles.z));
        }
    }
    private void ApplyGlidingMovement()
    {
        multiplierTargetSpeed = 0f;

        Vector3 eulerAngles = transform.rotation.normalized.eulerAngles;
        float xRotation = eulerAngles.x > 180f ? eulerAngles.x - 360f : eulerAngles.x;

        multiplierTargetSpeed += PitchMultiplier(xRotation, airbornPitchVelocityModifier);

        if (playerInput.isTurboing) multiplierTargetSpeed += turboTargetMultiplier;

        //If Accelerating
        if (modifiedTargetSpeed >= currentSpeed)
        {
            currentSpeed = Mathf.Lerp(currentSpeed, defaultTargetSpeed * multiplierTargetSpeed, accelerationMultiplier * Time.deltaTime);
        }
        //If Decelerating
        else if (modifiedTargetSpeed < currentSpeed)
        {
            currentSpeed = Mathf.Lerp(currentSpeed, defaultTargetSpeed * multiplierTargetSpeed, deccelarationMultiplier * Time.deltaTime);
        }

        Vector3 velocity = transform.forward * currentSpeed * Time.deltaTime;

        rb.velocity = Vector3.ClampMagnitude(velocity, maxSpeed);

        rb.velocity = velocity;

        modifiedTargetSpeed = defaultTargetSpeed * multiplierTargetSpeed;
    }

    //Airborne Movement
    private void ApplyAirborneRotation()
    {
        Vector3 direction = rb.velocity.normalized;
        Quaternion rotation = Quaternion.LookRotation(direction);
        Quaternion smooth = Quaternion.Lerp(rb.rotation, rotation, Time.deltaTime * 2);
        lastVelocity = rb.velocity.magnitude;
        rb.MoveRotation(smooth);
    }

    //Transitions
    private void OffGround() { detectIsGrounded = true; }
    public void GoGrounded()
    {
        if (!detectIsGrounded) return;
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, heightToGrounded, _layerMask))
        {
            _state = State.Grounded;
        }
    }
    public void GoAirborne()
    {
        if (_state == State.Grounded)
        {
            detectIsGrounded = false;
            Invoke("OffGround", .5f);
        }
        _state = State.Airborne;
    }
    public void GoGliding()
    {
        _state = State.Gliding;
    }
}