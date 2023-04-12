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

    private Vector3 previousNormals;
    private Quaternion fromTo;

    public enum State
    {
        Grounded,
        Airborne,
        OpenWings
    }
    private void Update()
    {
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
        if (detectIsGrounded)
        {
            Grounded();
        }
        if (_state == State.Grounded)
        {
            _rigidbody.useGravity = false;
            ApplyGroundTurbo();
            //!Inputing
            if (_input.magnitude == 0 && !isTurboing)
            {
                currentSpeed = Mathf.SmoothDamp(currentSpeed, 0f, ref _deccelarationSmoothVelocity, deccelaration);
                _rigidbody.velocity = new Vector3(transform.forward.x * currentSpeed * Time.deltaTime, transform.forward.y * currentSpeed * Time.deltaTime, transform.forward.z * currentSpeed * Time.deltaTime);
                return;
            }
            ApplyGroundRotation();
            ApplyGroundMovement();
            return;
        }
        if (_state == State.Airborne)
        {
            _rigidbody.useGravity = true;
            ApplyAirbornRotation();
            ApplyAirbornMovement();
            ApplyAirbornTurbo();
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
        RaycastHit hit;
        if (Physics.Raycast(transform.position, -transform.up, out hit))
        {
            fromTo = Quaternion.FromToRotation(previousNormals, hit.normal);
            previousNormals = hit.normal;
        }
        if (_input.magnitude != 0 && !isTurboing)
        {
            currentSpeed = Mathf.SmoothDamp(currentSpeed, groundTargetSpeed, ref _accelerationSmoothVelocity, acceleration);
            _rigidbody.velocity = new Vector3(transform.forward.x * currentSpeed * Time.deltaTime, transform.forward.y * currentSpeed * Time.deltaTime, transform.forward.z * currentSpeed * Time.deltaTime);
            _rigidbody.velocity = fromTo * _rigidbody.velocity;
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
            _rigidbody.velocity = fromTo * _rigidbody.velocity;
        }
        else
        {
            fov = Mathf.SmoothDamp(fov, fovNormal, ref fovCurrentVelocity, fovSmoothTime);
            _cinemachineFreeLook.m_Lens.FieldOfView = fov;
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
            float angleRoll = Mathf.SmoothDampAngle(transform.eulerAngles.z, 0f, ref _turnSmoothVelocity, rollTurnSmoothTime);
            _rigidbody.MoveRotation(Quaternion.Euler(_rigidbody.rotation.eulerAngles.x, _rigidbody.rotation.eulerAngles.y, angleRoll));
        }

        //Yaw
        if (_direction.x != 0f)
        {
            float targetAngleYaw = Mathf.Atan2(cameraRelativeMovement.x, cameraRelativeMovement.z) * Mathf.Rad2Deg;
            float angleYaw = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngleYaw, ref _turnSmoothVelocity, yawTurnSmoothTime);

            float targetAngleRoll = Mathf.Atan2(cameraRelativeMovement.x, cameraRelativeMovement.y) * Mathf.Rad2Deg;
            float angleRoll = Mathf.SmoothDampAngle(transform.eulerAngles.z, targetAngleRoll, ref _turnSmoothVelocity, rollTurnSmoothTime);

            _rigidbody.MoveRotation(Quaternion.Euler(_rigidbody.rotation.eulerAngles.x, angleYaw, angleRoll));
        }

        //Pitch
        if (_direction.z != 0f)
        {
            float targetAnglePitch = Mathf.Atan2(-cameraRelativeMovement.z, -cameraRelativeMovement.y) * Mathf.Rad2Deg;
            if (targetAnglePitch > 0)
            {
                float anglePitch = Mathf.SmoothDampAngle(transform.eulerAngles.x, targetAnglePitch * 1f, ref _turnSmoothVelocity, pitchTurnSmoothTime);
                Debug.Log(transform.eulerAngles.x + "" + targetAnglePitch);
                _rigidbody.MoveRotation(Quaternion.Euler(anglePitch, _rigidbody.rotation.eulerAngles.y, _rigidbody.rotation.eulerAngles.z));
            }
            if (targetAnglePitch < 0)
            {
                float anglePitch = Mathf.SmoothDampAngle(transform.eulerAngles.x, targetAnglePitch * 0.5f, ref _turnSmoothVelocity, pitchTurnSmoothTime);
                Debug.Log(transform.eulerAngles.x + "" + targetAnglePitch);
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
            Debug.DrawRay(transform.position, Vector3.down * 10f);
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
}