using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Animator))]

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] Cinemachine.CinemachineFreeLook _cinemachineFreeLook;
    [SerializeField] private State _state;

    [Header("MaxSpeed")]
    [SerializeField] private float groundspeed;
    [SerializeField] private float turbospeed;
    private float speed;

    [Header("Gradient")]
    [SerializeField] private float acceleration;
    [SerializeField] private float deceleration;

    [Header("Rotation")]
    [SerializeField] private float turnSmoothTime;
    private float _turnSmoothVelocity;

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
    private bool isAttacking = false;
    private bool isTurboing = false;

    private Animator _animator;
    private Rigidbody rigidbody;

    [SerializeField] private float pushPower;

    enum State
    {
        Idle,
        Running
    }
    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();
        _animator = GetComponent<Animator>();
    }
    private void FixedUpdate()
    {
        ApplyMovement();
        Turbo();
    }
    private void Update()
    {
        ApplyRotation();
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
    private void ApplyRotation()
    {
        //If Not Detecting Moving Input, Return
        if (_input.sqrMagnitude == 0) return;
        cameraRelativeMovement = ConvertToCameraSpace(_direction);
        //Rotating Towards Moving Direction
        float targetAngle = Mathf.Atan2(cameraRelativeMovement.x, cameraRelativeMovement.z) * Mathf.Rad2Deg;
        //Smooth Rotation
        float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref _turnSmoothVelocity, turnSmoothTime);
        //Change Rotation
        transform.rotation = Quaternion.Euler(0.0f, angle, 0.0f);
    }
    private void ApplyMovement()
    {
        //Move
        if (_input.magnitude != 0 && !isTurboing)
        {
            rigidbody.velocity = new Vector3(transform.forward.x * groundspeed * Time.deltaTime, rigidbody.velocity.y, transform.forward.z * groundspeed * Time.deltaTime);
            _animator.SetFloat("Velocity", groundspeed);
        }
    }
    public void Move(InputAction.CallbackContext context)
    {
        _input = context.ReadValue<Vector2>();
        _direction = new Vector3(_input.x, 0f, _input.y);
    }
    public void Jump(InputAction.CallbackContext context)
    {
        if (context.started) { _animator.SetTrigger("Jump"); }
        if (context.canceled) { _animator.ResetTrigger("Jump"); }
    }
    public void Turbo(InputAction.CallbackContext context)
    {
        if (context.started) { isTurboing = true; Invoke("Turbo", 2f); }
        if (context.canceled) { isTurboing = false; }
    }
    public void Attack(InputAction.CallbackContext context)
    {
        if (context.performed && !isAttacking)
        {
            isAttacking = true;
            _animator.SetTrigger("Attack");
        }
    }
    public void Turbo()
    {
        if (_input.magnitude != 0 && isTurboing)
        {
            fov = Mathf.SmoothDamp(fov, fovTurbo, ref fovCurrentVelocity, fovSmoothTime);
            _cinemachineFreeLook.m_Lens.FieldOfView = fov;
            isTurboing = true;

            rigidbody.velocity = new Vector3(transform.forward.x * turbospeed * Time.deltaTime, rigidbody.velocity.y, transform.forward.z * turbospeed * Time.deltaTime);
            _animator.SetFloat("Velocity", groundspeed);
        }
        else
        {
            fov = Mathf.SmoothDamp(fov, fovNormal, ref fovCurrentVelocity, fovSmoothTime);
            _cinemachineFreeLook.m_Lens.FieldOfView = fov;
        }
    }
    public void OnAttackAnimationFinished()
    {
        isAttacking = false;
    }
}