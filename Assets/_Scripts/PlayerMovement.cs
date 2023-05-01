using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Animator))]

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private GameObject glideGameObject;// glide test

    public static State _state;
    [SerializeField] State _stateSF;// Just to view static variable via inspector

    [SerializeField] Cinemachine.CinemachineFreeLook _cinemachineFreeLook;

    [SerializeField] private LayerMask _layerMask;

    [Header("Pounce")]
    [SerializeField] private float pounceForce;
    [SerializeField] private float glideForce;

    [Header("Speed")]
    [SerializeField] private float multiplierTargetSpeed;

    [SerializeField] private float modifiedTargetSpeed;

    [SerializeField] private float maxSpeed;

    [Header("Speed Multipliers")]

    [Header("Running Multipliers")]
    [SerializeField] private float currentSpeed;
    [SerializeField] private float targetSpeed;
    [SerializeField] private float accelerationMultiplier;
    [SerializeField] private float deccelarationMultiplier;

    [Header("Turbo Multipliers")]
    [SerializeField] private float turboCurrentMultiplier;
    [SerializeField] private float turboTargetMultiplier;
    [SerializeField] private float turboAccelerationMultiplier;
    [SerializeField] private float turboDeccelarationMultiplier;

    [Header("UpHill Multipliers")]
    [SerializeField] private float upHillCurrentMultiplier;
    [SerializeField] private float upHillTargetMultiplier;
    [SerializeField] private float upHillAccelerationMultiplier;
    [SerializeField] private float upHillDeccelarationMultiplier;

    [Header("DownHill Multipliers")]
    [SerializeField] private float downHillCurrentMultiplier;
    [SerializeField] private float downHillTargetMultiplier;
    [SerializeField] private float downHillAccelerationMultiplier;
    [SerializeField] private float downHillDeccelarationMultiplier;

    [Header("Rotation")]
    [SerializeField] private float turnSmoothTime;

    [Header("Airborn Speed")]
    [SerializeField] private float goingDownAirbornSpeedMultiplier;
    [SerializeField] private float goingDownAirbornSpeedMax;

    [SerializeField] private float goingUpAirbornSpeedMultiplier;
    [SerializeField] private float goingUpAirbornSpeedMax;

    [Header("Airborn Rotation")]
    [SerializeField] private float rollTurnSmoothTime;
    [SerializeField] private float pitchTurnSmoothTime;
    [SerializeField] private float yawTurnSmoothTime;

    [Header("Input")]
    private Vector2 _input;
    private Vector3 _direction;
    private Vector3 cameraRelativeMovement;
    private Vector3 lastCameraRelativeMovement;

    [Header("FOV")]
    [SerializeField] private float fov = 50f;
    [SerializeField] private float fovNormal = 50f;
    [SerializeField] private float fovTurbo = 75f;
    [SerializeField] private float fovSmoothTime = 1f;

    [Header("Booleans")]
    private bool isTurboing = false;
    private bool isBreaking = false;
    private bool detectIsGrounded = false;

    private Animator _animator;
    private Rigidbody _rigidbody;

    //OnSlopeBehaviour
    [SerializeField] private float smoothTimeTargetRotation;

    [SerializeField] private float playerForceToGround;

    [Header("Activables")]
    [SerializeField] private bool onSlopePosition;
    [SerializeField] private bool onSlopeRotation;

    [Header("RayCastHits")]
    [SerializeField] private float rayCastLenght = 1f;

    [SerializeField] private float heightToAirborne = 2f;

    private Vector3 averageNormal;
    private Quaternion targetRotation;
    private int baseRayCastCount = 0;
    private bool baseRayCastDetection = false;

    public enum State
    {
        Grounded,
        Airborne,
        OpenWings
    }
    private void Update()
    {
        GetComponent<Renderer>().material.SetColor(
            "_Color", PlayerMovement._state == PlayerMovement.State.Grounded ? Color.black : Color.blue
        );
        _stateSF = _state;
    }
    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _animator = GetComponent<Animator>();

        PlayerInputActions playerInputActions = new PlayerInputActions();
        playerInputActions.Gameplay.Enable();
        playerInputActions.Gameplay.Move.performed += Move;
        playerInputActions.Gameplay.Move.canceled += Move;
        playerInputActions.Gameplay.Pounce.performed += Pounce;
        playerInputActions.Gameplay.Pounce.canceled += Pounce;
        playerInputActions.Gameplay.Turbo.performed += Turbo;
        playerInputActions.Gameplay.Turbo.canceled += Turbo;
    }
    private void FixedUpdate()
    {
        if (onSlopePosition && PlayerMovement._state == PlayerMovement.State.Grounded) OnSlopePosition();
        if (onSlopeRotation && PlayerMovement._state == PlayerMovement.State.Grounded) OnSlopeRotation();

        if (detectIsGrounded) Grounded();

        if (_state == State.Grounded)
        {
            _rigidbody.useGravity = false;

            if (!Physics.Raycast(transform.position, -transform.up, heightToAirborne))
            {
                detectIsGrounded = false;
                Invoke("OffGround", .5f);
                _state = State.Airborne;
                return;
            }

            //!Inputing
            if (_input.magnitude == 0 && !isTurboing)
            {
                currentSpeed = Mathf.Lerp(currentSpeed, 0f, deccelarationMultiplier * Time.deltaTime);
                turboCurrentMultiplier = Mathf.Lerp(turboCurrentMultiplier, 0f, turboDeccelarationMultiplier * Time.deltaTime);
                upHillCurrentMultiplier = Mathf.Lerp(upHillCurrentMultiplier, 0f, upHillDeccelarationMultiplier * Time.deltaTime);

                _rigidbody.velocity = new Vector3(transform.forward.x * currentSpeed * Time.deltaTime, transform.forward.y * currentSpeed * Time.deltaTime, transform.forward.z * currentSpeed * Time.deltaTime);
                fov = Mathf.Lerp(fov, fovNormal, fovSmoothTime * Time.deltaTime);
                //_cinemachineFreeLook.m_Lens.FieldOfView = fov;
                return;
            }

            ApplyTurbo();
            ApplyGroundRotation();
            ApplyGroundMovement();
            return;
        }
        if (_state == State.Airborne)
        {
            _rigidbody.useGravity = true;

            cameraRelativeMovement = ConvertToCameraSpace(_direction);

            

            ////Yaw
            //float targetAngleYaw = Mathf.Atan2(cameraRelativeMovement.x, cameraRelativeMovement.z) * Mathf.Rad2Deg;
            //float angleYaw = Mathf.LerpAngle(_rigidbody.rotation.eulerAngles.y, targetAngleYaw, yawTurnSmoothTime * Time.deltaTime);

            //Pitch And Roll
            Vector3 direction = _rigidbody.velocity.normalized;
            Quaternion rotation = Quaternion.LookRotation(direction);
            Quaternion smooth = Quaternion.Lerp(_rigidbody.rotation, rotation, Time.deltaTime * 2);

            ////Yaw
            //if (_direction.magnitude != 0)
            //{
            //    Debug.Log("_direction.magnitude = 0");
            //    // Modify the smooth quaternion to rotate around y-axis by angleYaw degrees
            //    Vector3 euler = smooth.eulerAngles;
            //    euler.y = angleYaw;
            //    smooth = Quaternion.Euler(euler);
            //}

            _rigidbody.MoveRotation(smooth);

            return;
        }
        if (_state == State.OpenWings)
        {
            _rigidbody.useGravity = true;
            _rigidbody.AddForce(Vector3.up * glideForce);
            ApplyTurbo();
            ApplyAirbornRotation();
            ApplyAirbornMovement();
        }
    }

    //Variable Modifiers
    Vector3 ConvertToCameraSpace(Vector3 vectorToRotate)
    {
        float currentYValue = vectorToRotate.y;
        Vector3 cameraFoward = Camera.main.transform.forward;
        Vector3 cameraRight = Camera.main.transform.right;

        cameraFoward.y = 0;
        cameraRight.y = 0;

        cameraFoward = cameraFoward.normalized;
        cameraRight = cameraRight.normalized;

        Vector3 cameraFowardZProduct = vectorToRotate.z * cameraFoward;
        Vector3 cameraRightXProduct = vectorToRotate.x * cameraRight;

        Vector3 vectorRotatedToCameraSpace = cameraFowardZProduct + cameraRightXProduct;
        vectorRotatedToCameraSpace.y = currentYValue;
        return vectorRotatedToCameraSpace;
    }
    public float HillVelocityModifier(float angle)
    {
        angle = Mathf.Clamp(angle, -90f, 90f);

        float t = (angle + 90f) / 180f;
        float y = Mathf.Lerp(0f, 2f, t);
        return y;
    }
    public void ApplyTurbo()
    {
        float t = currentSpeed / (targetSpeed * turboTargetMultiplier);
        float y = Mathf.Lerp(fovNormal, fovTurbo, t);

        fov = Mathf.Lerp(fov, y, fovSmoothTime * Time.deltaTime);
        //_cinemachineFreeLook.m_Lens.FieldOfView = fov;

        //if (isTurboing)
        //{
        //    fov = Mathf.Lerp(fov, fovTurbo, fovSmoothTime * Time.deltaTime);
        //    _cinemachineFreeLook.m_Lens.FieldOfView = fov;
        //    isTurboing = true;
        //}
        //else
        //{
        //    fov = Mathf.Lerp(fov, fovNormal, fovSmoothTime * Time.deltaTime);
        //    _cinemachineFreeLook.m_Lens.FieldOfView = fov;
        //}
    }

    //Ground Movement
    private void ApplyGroundRotation()
    {
        if (_input.sqrMagnitude == 0) return;

        cameraRelativeMovement = ConvertToCameraSpace(_direction);
        float targetAngle = Mathf.Atan2(cameraRelativeMovement.x, cameraRelativeMovement.z) * Mathf.Rad2Deg;
        float angle = Mathf.LerpAngle(transform.eulerAngles.y, targetAngle, turnSmoothTime * Time.deltaTime);

        //Break
        if (cameraRelativeMovement.x == Mathf.Abs(lastCameraRelativeMovement.x) * -1 || cameraRelativeMovement.z == lastCameraRelativeMovement.z * -1 && !isTurboing)
        {
            Debug.Log("yeah boi");
            isBreaking = true;
            currentSpeed = Mathf.Lerp(currentSpeed, 0, deccelarationMultiplier * Time.deltaTime);
            _rigidbody.velocity = new Vector3(transform.forward.x * currentSpeed * Time.deltaTime, transform.forward.y * currentSpeed * Time.deltaTime, transform.forward.z * currentSpeed * Time.deltaTime);
        }
        //Rotate Y
        else
        {
            isBreaking = false;
            _rigidbody.MoveRotation(Quaternion.Euler(_rigidbody.rotation.eulerAngles.x, angle, _rigidbody.rotation.eulerAngles.z));
        }
        lastCameraRelativeMovement = cameraRelativeMovement;
    }
    private void ApplyGroundMovement()
    {
        if (isBreaking == true) return;

        multiplierTargetSpeed = 0f;

        Vector3 eulerAngles = transform.rotation.normalized.eulerAngles;
        float xRotation = eulerAngles.x > 180f ? eulerAngles.x - 360f : eulerAngles.x;

        multiplierTargetSpeed += HillVelocityModifier(xRotation);

        if (isTurboing) multiplierTargetSpeed += turboTargetMultiplier;

        if (_input.magnitude != 0)
        {
            //If Accelerating
            if (modifiedTargetSpeed >= currentSpeed)
            {
                currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed * multiplierTargetSpeed, accelerationMultiplier * Time.deltaTime);
            }
            //If Decelerating
            else if (modifiedTargetSpeed < currentSpeed)
            {
                currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed * multiplierTargetSpeed, deccelarationMultiplier * Time.deltaTime);
            }

            _rigidbody.velocity = new Vector3(transform.forward.x * Time.deltaTime, transform.forward.y * Time.deltaTime, transform.forward.z * Time.deltaTime) * currentSpeed;
            Debug.Log(multiplierTargetSpeed);
        }
        modifiedTargetSpeed = targetSpeed * multiplierTargetSpeed;
    }

    //Airborne Movement
    private void ApplyAirbornRotation()
    {
        cameraRelativeMovement = ConvertToCameraSpace(_direction);

        //AD
        //Yaw
        if (_direction.x != 0f)
        {

            float targetAngleRoll = Mathf.Atan2(-_direction.x, _direction.y) * Mathf.Rad2Deg;
            targetAngleRoll *= .5f;
            float angleRoll = Mathf.LerpAngle(_rigidbody.rotation.eulerAngles.z, targetAngleRoll, rollTurnSmoothTime * Time.deltaTime);

            float targetAngleYaw = Mathf.Atan2(cameraRelativeMovement.x, cameraRelativeMovement.z) * Mathf.Rad2Deg;
            float angleYaw = Mathf.LerpAngle(_rigidbody.rotation.eulerAngles.y, targetAngleYaw, yawTurnSmoothTime * Time.deltaTime);

            _rigidbody.MoveRotation(Quaternion.Euler(_rigidbody.rotation.eulerAngles.x, angleYaw, angleRoll));
        }
        else
        {
            float angleRoll = Mathf.LerpAngle(_rigidbody.rotation.eulerAngles.z, 0f, rollTurnSmoothTime * Time.deltaTime);
            _rigidbody.MoveRotation(Quaternion.Euler(_rigidbody.rotation.eulerAngles.x, _rigidbody.rotation.eulerAngles.y, angleRoll));
        }

        //WS
        //Pitch
        if (_direction.z != 0f)
        {
            float targetAnglePitch = Mathf.Atan2(_direction.z, _direction.y) * Mathf.Rad2Deg;

            if (targetAnglePitch > 0)
            {
                float anglePitch = Mathf.LerpAngle(_rigidbody.rotation.eulerAngles.x, targetAnglePitch * 1f, pitchTurnSmoothTime * Time.deltaTime);

                _rigidbody.MoveRotation(Quaternion.Euler(anglePitch, _rigidbody.rotation.eulerAngles.y, _rigidbody.rotation.eulerAngles.z));
            }
            if (targetAnglePitch < 0)
            {
                float anglePitch = Mathf.LerpAngle(_rigidbody.rotation.eulerAngles.x, targetAnglePitch * .5f, pitchTurnSmoothTime * Time.deltaTime);

                _rigidbody.MoveRotation(Quaternion.Euler(anglePitch, _rigidbody.rotation.eulerAngles.y, _rigidbody.rotation.eulerAngles.z));
            }
        }
    }
    private void ApplyAirbornMovement()
    {
        _rigidbody.velocity = new Vector3(transform.forward.x * currentSpeed * Time.deltaTime, transform.forward.y * currentSpeed * Time.deltaTime, transform.forward.z * currentSpeed * Time.deltaTime);
    }

    //isGrounded
    private void OffGround() { detectIsGrounded = true; }
    public void Grounded()
    {
        if (_state == State.Airborne || _state == State.OpenWings)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, Vector3.down, out hit, 1f, _layerMask))
            {
                _state = State.Grounded;
            }
        }
    }

    //Input
    public void Pounce(InputAction.CallbackContext context)
    {
        if (context.performed && _state == State.Grounded)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, -transform.up, out hit, _layerMask))
            {
                detectIsGrounded = false;
                Invoke("OffGround", .5f);
                _state = State.Airborne;

                Vector3 ponceDir = (transform.forward + hit.normal) / 2;
                _rigidbody.AddForce(ponceDir * pounceForce);
            }
        }
        else if (context.performed && _state == State.Airborne)
        {
            glideGameObject.SetActive(true);
            _state = State.OpenWings;
        }
        if (context.canceled && _state == State.OpenWings || _state == State.Grounded)
        {
            glideGameObject.SetActive(false);
            _state = State.Airborne;
        }
    }
    public void Move(InputAction.CallbackContext context)
    {
        _input = context.ReadValue<Vector2>();
        _direction = new Vector3(_input.x, 0f, _input.y);
    }
    public void Turbo(InputAction.CallbackContext context)
    {
        if (context.performed) { isTurboing = true; }
        if (context.canceled) { isTurboing = false; }
    }

    //OnSlopeBehaviour
    private void OnSlopePosition()
    {
        if (!Physics.Raycast(transform.position, -transform.up))
        {
            Quaternion targetRotation = Quaternion.FromToRotation(transform.up, new Vector3(_rigidbody.rotation.eulerAngles.x, _rigidbody.rotation.eulerAngles.y, 0f));
            //targetRotation = Quaternion.Slerp(_rigidbody.rotation, targetRotation, Time.deltaTime * 100);
            _rigidbody.MoveRotation(targetRotation);
            return;
        }
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
        baseRayCastDetection = false;

        baseRayCastCount = 0;

        BaseRayCasts(transform.position, transform.forward, Vector3.zero, Vector3.zero); // Fowards
        BaseRayCasts(transform.position, transform.forward, Vector3.zero, Vector3.zero); // Fowards
        BaseRayCasts(transform.position, transform.forward, Vector3.zero, Vector3.zero); // Fowards

        BaseRayCasts(transform.position, -transform.up, Vector3.zero, Vector3.zero); //Downwards
        BaseRayCasts(transform.position, -transform.up, Vector3.forward, Vector3.zero); // Fowards
        BaseRayCasts(transform.position, -transform.up, Vector3.right, Vector3.zero); // Right
        BaseRayCasts(transform.position, -transform.up, Vector3.left, Vector3.zero); // Left

        BaseRayCasts(transform.position, -transform.up, Vector3.forward, Vector3.right);
        BaseRayCasts(transform.position, -transform.up, Vector3.forward, Vector3.left);
        BaseRayCasts(transform.position, -transform.up, Vector3.back, Vector3.right);
        BaseRayCasts(transform.position, -transform.up, Vector3.back, Vector3.left);

        if (baseRayCastDetection)
        {
            averageNormal /= baseRayCastCount;

            targetRotation = Quaternion.FromToRotation(transform.up, averageNormal) * transform.rotation;
            targetRotation = Quaternion.Slerp(_rigidbody.rotation, targetRotation, Time.deltaTime * smoothTimeTargetRotation);
            _rigidbody.MoveRotation(targetRotation);
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
        Gizmos.DrawRay(transform.position, (transform.forward + (Vector3.zero + Vector3.zero).normalized) * rayCastLenght);

        Gizmos.color = Color.black;
        Gizmos.DrawRay(transform.position, (-transform.up + (Vector3.zero + Vector3.zero).normalized) * rayCastLenght);
        Gizmos.DrawRay(transform.position, (-transform.up + (Vector3.forward + Vector3.zero).normalized) * rayCastLenght);
        Gizmos.DrawRay(transform.position, (-transform.up + (Vector3.right + Vector3.zero).normalized) * rayCastLenght);
        Gizmos.DrawRay(transform.position, (-transform.up + (Vector3.left + Vector3.zero).normalized) * rayCastLenght);

        Gizmos.color = Color.black;
        Gizmos.DrawRay(transform.position, (-transform.up + (Vector3.forward + Vector3.right).normalized) * rayCastLenght);
        Gizmos.DrawRay(transform.position, (-transform.up + (Vector3.forward + Vector3.left).normalized) * rayCastLenght);
        Gizmos.DrawRay(transform.position, (-transform.up + (Vector3.back + Vector3.right).normalized) * rayCastLenght);
        Gizmos.DrawRay(transform.position, (-transform.up + (Vector3.back + Vector3.left).normalized) * rayCastLenght);
    }
}