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
    [SerializeField] private float currentSpeed;
    [SerializeField] private float groundTargetSpeed;
    [SerializeField] private float turboTargetSpeed;
    [SerializeField] private float maxSpeed;
    [SerializeField] private float maxTurboSpeed;
    [SerializeField] private float goingDownGroundSpeedTargetMultiplier;
    [SerializeField] private float goingDownGroundSpeedMultiplier;

    [Header("Transition")]
    [SerializeField] private float acceleration = 1f;
    private float _accelerationSmoothVelocity;

    [SerializeField] private float deccelaration = 1f;
    private float _deccelarationSmoothVelocity;

    [SerializeField] private float turboAcceleration = 1f;
    private float _turboAccelerationSmoothVelocity;

    [Header("Rotation")]
    [SerializeField] private float turnSmoothTime;
    private float _turnSmoothVelocity;

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

    [Header("FOV")]
    [SerializeField] private float fov = 40f;
    [SerializeField] private float fovNormal = 40f;
    [SerializeField] private float fovTurbo = 50f;
    [SerializeField] private float fovSmoothTime = 1f;
    private float fovCurrentVelocity;

    [Header("Booleans")]
    private bool isTurboing = false;
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
        if (detectIsGrounded)
        {
            Grounded();
        }
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
                currentSpeed = Mathf.SmoothDamp(currentSpeed, 0f, ref _deccelarationSmoothVelocity, deccelaration);
                _rigidbody.velocity = new Vector3(transform.forward.x * currentSpeed * Time.deltaTime, transform.forward.y * currentSpeed * Time.deltaTime, transform.forward.z * currentSpeed * Time.deltaTime);
                fov = Mathf.SmoothDamp(fov, fovNormal, ref fovCurrentVelocity, fovSmoothTime);
                _cinemachineFreeLook.m_Lens.FieldOfView = fov;
                return;
            }

            ApplyGroundTurbo();
            ApplyGroundRotation();
            ApplyGroundMovement();
            return;
        }
        if (_state == State.Airborne)
        {
            _rigidbody.useGravity = true;
            ApplyAirbornRotation();
        }
        if (_state == State.OpenWings)
        {
            _rigidbody.useGravity = true;
            _rigidbody.AddForce(Vector3.up * glideForce);
            ApplyAirbornRotation();
            ApplyAirbornMovement();
            ApplyAirbornTurbo();
        }
    }
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

    //Ground Movement
    private void ApplyGroundRotation()
    {
        if (_input.sqrMagnitude == 0) return;

        cameraRelativeMovement = ConvertToCameraSpace(_direction);

        float targetAngle = Mathf.Atan2(cameraRelativeMovement.x, cameraRelativeMovement.z) * Mathf.Rad2Deg;
        float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref _turnSmoothVelocity, turnSmoothTime);

        _rigidbody.MoveRotation(Quaternion.Euler(_rigidbody.rotation.eulerAngles.x, angle, _rigidbody.rotation.eulerAngles.z));
    }
    private void ApplyGroundMovement()
    {
        if (transform.rotation.x < -15f)
        {
            currentSpeed = Mathf.SmoothDamp(currentSpeed, groundTargetSpeed, ref _accelerationSmoothVelocity, acceleration);
            _rigidbody.velocity = new Vector3(transform.forward.x * currentSpeed * 2f * Time.deltaTime, transform.forward.y * currentSpeed * 2f * Time.deltaTime, transform.forward.z * currentSpeed * 2f * Time.deltaTime);
            return;
        }
        if (_input.magnitude != 0 && !isTurboing)
        {
            currentSpeed = Mathf.SmoothDamp(currentSpeed, groundTargetSpeed, ref _accelerationSmoothVelocity, acceleration);
            _rigidbody.velocity = new Vector3(transform.forward.x * currentSpeed * Time.deltaTime, transform.forward.y * currentSpeed * Time.deltaTime, transform.forward.z * currentSpeed * Time.deltaTime);
        }
    }
    public void ApplyGroundTurbo()
    {
        if (_input.magnitude != 0 && isTurboing)
        {
            fov = Mathf.SmoothDamp(fov, fovTurbo, ref fovCurrentVelocity, fovSmoothTime);
            _cinemachineFreeLook.m_Lens.FieldOfView = fov;
            isTurboing = true;

            currentSpeed = Mathf.SmoothDamp(currentSpeed, turboTargetSpeed, ref _turboAccelerationSmoothVelocity, turboAcceleration);
            _rigidbody.velocity = new Vector3(transform.forward.x * currentSpeed * Time.deltaTime, transform.forward.y * currentSpeed * Time.deltaTime, transform.forward.z * currentSpeed * Time.deltaTime);
        }
    }

    //Airborne Movement
    private void ApplyAirbornRotation()
    {
        cameraRelativeMovement = ConvertToCameraSpace(_direction);

        //Defaut
        if ((_state == State.Airborne || _state == State.OpenWings) && _input.magnitude == 0f)
        {
            //Roll
            float angleRoll = Mathf.LerpAngle(_rigidbody.rotation.eulerAngles.z, 0f, rollTurnSmoothTime * Time.deltaTime);
            _rigidbody.MoveRotation(Quaternion.Euler(_rigidbody.rotation.eulerAngles.x, _rigidbody.rotation.eulerAngles.y, angleRoll));
        }

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
        if (!isTurboing)
        {
            _rigidbody.velocity = new Vector3(transform.forward.x * currentSpeed * Time.deltaTime, transform.forward.y * currentSpeed * Time.deltaTime, transform.forward.z * currentSpeed * Time.deltaTime);
        }
    }
    public void ApplyAirbornTurbo()
    {
        if (isTurboing)
        {
            fov = Mathf.SmoothDamp(fov, fovTurbo, ref fovCurrentVelocity, fovSmoothTime);
            _cinemachineFreeLook.m_Lens.FieldOfView = fov;
            isTurboing = true;


            _rigidbody.velocity = new Vector3(transform.forward.x * turboTargetSpeed * Time.deltaTime, transform.forward.y * turboTargetSpeed * Time.deltaTime, transform.forward.z * turboTargetSpeed * Time.deltaTime);
            _animator.SetFloat("Velocity", currentSpeed);
        }
        else
        {
            fov = Mathf.SmoothDamp(fov, fovNormal, ref fovCurrentVelocity, fovSmoothTime);
            _cinemachineFreeLook.m_Lens.FieldOfView = fov;
        }
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
        if (context.canceled && _state == State.OpenWings)
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