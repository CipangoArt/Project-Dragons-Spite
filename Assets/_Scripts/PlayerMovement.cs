using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Animator))]

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private GameObject sphere;// glide test

    public static State _state;//State of which Dragon is in
    [SerializeField] State _stateSF;// Just to view static variable via inspector

    [SerializeField] Cinemachine.CinemachineFreeLook _cinemachineFreeLook;

    [SerializeField] private LayerMask _layerMask;

    [Header("Pounce")]
    [SerializeField] private float pounceForce;
    [SerializeField] private float glideForce;

    [Header("Speed")]
    [SerializeField] private float currentSpeed;
    [SerializeField] private float targetSpeed = 750f;
    [SerializeField] private float multiplierTargetSpeed;
    [SerializeField] private float modifiedTargetSpeed;

    [Header("Multipliers")]
    [SerializeField] private float accelerationMultiplier = 1f;
    [SerializeField] private float deccelarationMultiplier = 1f;
    [SerializeField] private float groundedPitchVelocityModifier = 2f;
    [SerializeField] private float airbornPitchVelocityModifier = 2f;
    [SerializeField] private float turboTargetMultiplier = 2f;

    [Header("Rotation")]
    [SerializeField] private float turnSmoothTime = 1f;

    [Header("Airborn Rotation")]
    [SerializeField] private float rollTurnSmoothTime = 1f;
    [SerializeField] private float pitchTurnSmoothTime = 1f;
    [SerializeField] private float yawTurnSmoothTime = 1f;

    [Header("Input")]
    private Vector2 _input;
    private Vector3 _direction;
    private Vector3 _cameraRelativeDir;

    [Header("FOV")]
    [SerializeField] private float fov = 50f;
    [SerializeField] private float fovNormal = 50f;
    [SerializeField] private float fovMax = 75f;
    [SerializeField] private float fovSmoothTime = 1f;

    [Header("Booleans")]
    private bool isTurboing = false;
    [SerializeField] private bool detectIsGrounded = false;
    [SerializeField] private bool pitchRestriction = false;

    [Header("Attack")]
    [SerializeField] private float timeToCombo;

    private Animator anim;
    private Rigidbody rb;

    //OnSlopeBehaviour
    [SerializeField] private float smoothTimeTargetRotation;

    [SerializeField] private float playerForceToGround;

    [Header("Activables")]
    [SerializeField] private bool onSlopePosition;
    [SerializeField] private bool onSlopeRotation;

    [Header("Fuel")]
    [SerializeField] private Image fuelBar;
    [SerializeField] private bool isFuelDepleted;
    [SerializeField] private float currentFuel;
    [SerializeField] private float maxFuel;
    [SerializeField] private float fuelDepletingSpeed;
    [SerializeField] private float fuelGainSpeed;


    [Header("RayCastHits")]
    [SerializeField] private float rayCastLenght = 1f;

    [SerializeField] private float heightToAirborne = 2f;

    [SerializeField] private float heightToGrounded = 2f;

    private Vector3 averageNormal;
    private Quaternion targetRotation;
    private int baseRayCastCount = 0;
    private bool baseRayCastDetection = false;

    private float lastVelocity;

    public enum State
    {
        Grounded,
        Airborne,
        Gliding
    }
    private void Update()
    {
        Fuel();
        sphere.GetComponent<Renderer>().material.SetColor(
            "_Color", PlayerMovement._state == PlayerMovement.State.Grounded ? Color.black : Color.blue
        );
        _stateSF = _state;
    }
    private void Awake()
    {
        currentFuel = maxFuel;
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();

        PlayerInputActions playerInputActions = new PlayerInputActions();
        playerInputActions.Gameplay.Enable();
        playerInputActions.Gameplay.Move.performed += Move;
        playerInputActions.Gameplay.Move.canceled += Move;
        playerInputActions.Gameplay.Pounce.performed += Pounce;
        playerInputActions.Gameplay.Pounce.canceled += Pounce;
        playerInputActions.Gameplay.Turbo.performed += Turbo;
        playerInputActions.Gameplay.Turbo.canceled += Turbo;
        playerInputActions.Gameplay.MeleeAttack.performed += MeleeAttack;
        playerInputActions.Gameplay.MeleeAttack.canceled += MeleeAttack;
    }
    private void FixedUpdate()
    {
        float yeah = currentSpeed / (targetSpeed * turboTargetMultiplier);
        anim.SetFloat("Running", yeah * 2);
        switch (_state)
        {
            case State.Grounded:
                rb.useGravity = false;

                OnSlopePosition();
                OnSlopeRotation();

                //OffGround -> Airborne
                if (!Physics.Raycast(transform.position, -transform.up, heightToAirborne))
                {
                    GoAirborne();
                    return;
                }

                //!Inputing
                if (_input.magnitude == 0 && !isTurboing)
                {
                    anim.SetBool("IsMoving", false);
                    //Degrade Speed
                    currentSpeed = Mathf.Lerp(currentSpeed, 0f, deccelarationMultiplier * Time.deltaTime);
                    rb.velocity = transform.forward * currentSpeed * Time.deltaTime;
                    return;
                }

                Fov();
                ApplyGroundRotation();
                ApplyGroundMovement();
                break;

            case State.Airborne:
                rb.useGravity = true;
                if (detectIsGrounded) Grounded();

                _cameraRelativeDir = ConvertToCameraSpace(_direction);

                //Pitch
                Vector3 direction = rb.velocity.normalized;
                Quaternion rotation = Quaternion.LookRotation(direction);
                Quaternion smooth = Quaternion.Lerp(rb.rotation, rotation, Time.deltaTime * 2);
                lastVelocity = rb.velocity.magnitude;
                Fov();
                rb.MoveRotation(smooth);
                break;

            case State.Gliding:
                if (detectIsGrounded) Grounded();
                rb.useGravity = false;
                Fov();
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

        //
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
    public void Fov()
    {
        float t = currentSpeed / (targetSpeed * turboTargetMultiplier);
        float y = Mathf.Lerp(fovNormal, fovMax, t);

        fov = Mathf.Lerp(fov, y, fovSmoothTime * Time.deltaTime);
    }
    private void Fuel()
    {
        if (currentFuel <= .1f)
        {
            isFuelDepleted = true;
        }
        if (isFuelDepleted) isTurboing = false;
        if (!isTurboing || isFuelDepleted)
        {
            //increment
            if (currentFuel >= maxFuel)
            {
                currentFuel = maxFuel;
                isFuelDepleted = false;
            }
            currentFuel += fuelGainSpeed * Time.deltaTime;
        }
        if (isTurboing)
        {
            currentFuel -= fuelDepletingSpeed * Time.deltaTime;
        }
        fuelBar.fillAmount = currentFuel / maxFuel;
    }

    //Ground Movement
    private void ApplyGroundRotation()
    {
        if (_input.sqrMagnitude == 0) return;

        //Yaw Rotation
        _cameraRelativeDir = ConvertToCameraSpace(_direction);
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
        if (isTurboing) multiplierTargetSpeed += turboTargetMultiplier;

        anim.SetBool("IsMoving", true);
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

        Vector3 velocity = transform.forward * currentSpeed * Time.deltaTime;

        rb.velocity = velocity;
        modifiedTargetSpeed = targetSpeed * multiplierTargetSpeed;
    }

    //Airborne Movement
    private void ApplyGlidingRotation()
    {
        _cameraRelativeDir = ConvertToCameraSpace(_direction);

        //Yaw
        if (_direction.x != 0f)
        {
            //Roll
            float targetAngleRoll = Mathf.Atan2(-_direction.x, _direction.y) * Mathf.Rad2Deg;
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

        if (rb.velocity.magnitude < 15f)
        {
            pitchRestriction = true;
            float targetAnglePitch = Mathf.Atan2(1, -1) * Mathf.Rad2Deg;
            float targetAnglePitchModified = targetAnglePitch * .5f;
            float anglePitch = Mathf.LerpAngle(rb.rotation.eulerAngles.x, targetAnglePitchModified, pitchTurnSmoothTime * Time.deltaTime);

            if (rb.rotation.eulerAngles.x < targetAnglePitchModified * 1.2) pitchRestriction = false;

            rb.MoveRotation(Quaternion.Euler(anglePitch, rb.rotation.eulerAngles.y, rb.rotation.eulerAngles.z));
            return;
        }
        //Pitch
        if (_direction.z != 0f && pitchRestriction == false)
        {
            float targetAnglePitch = Mathf.Atan2(_direction.z, _direction.y) * Mathf.Rad2Deg;

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
    }
    private void ApplyGlidingMovement()
    {
        multiplierTargetSpeed = 0f;

        Vector3 eulerAngles = transform.rotation.normalized.eulerAngles;
        float xRotation = eulerAngles.x > 180f ? eulerAngles.x - 360f : eulerAngles.x;

        multiplierTargetSpeed += PitchMultiplier(xRotation, airbornPitchVelocityModifier);

        if (isTurboing) multiplierTargetSpeed += turboTargetMultiplier;

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

        Vector3 velocity = transform.forward * currentSpeed * Time.deltaTime;

        rb.velocity = velocity;
        modifiedTargetSpeed = targetSpeed * multiplierTargetSpeed;
    }

    //isGrounded
    private void OffGround() { detectIsGrounded = true; }
    public void Grounded()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, heightToGrounded, _layerMask))
        {
            GoGrounded();
        }
    }

    //Transitions
    private void GoAirborne()
    {
        if (_state == State.Grounded)
        {
            detectIsGrounded = false;
            Invoke("OffGround", .5f);
        }
        _state = State.Airborne;
    }
    private void GoGliding()
    {
        _state = State.Gliding;
    }
    private void GoGrounded()
    {
        _state = State.Grounded;
    }

    //Input
    public void Pounce(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (_state == State.Grounded)
            {
                RaycastHit hit;
                if (Physics.Raycast(transform.position, -transform.up, out hit, _layerMask))
                {
                    GoAirborne();
                    Vector3 ponceDir = (transform.forward + hit.normal) / 2;
                    rb.AddForce(ponceDir * pounceForce);
                }
            }
            else if (_state == State.Airborne)
            {
                GoGliding();
            }
        }
        if (context.canceled && _state == State.Gliding)
        {
            GoAirborne();
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
    public void MeleeAttack(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            MeleeAttack();
        }
    }

    //OnSlopeBehaviour
    private void OnSlopePosition()
    {
        if (!Physics.Raycast(transform.position, -transform.up))
        {
            Quaternion targetRotation = Quaternion.FromToRotation(transform.up, new Vector3(rb.rotation.eulerAngles.x, rb.rotation.eulerAngles.y, 0f));
            //targetRotation = Quaternion.Slerp(_rigidbody.rotation, targetRotation, Time.deltaTime * 100);
            rb.MoveRotation(targetRotation);
            return;
        }
        if (!Physics.Raycast(transform.position, -transform.up, .6f))
        {
            rb.AddForce(-transform.up * playerForceToGround);
        }
        else
        {
            targetRotation = Quaternion.FromToRotation(transform.up, new Vector3(rb.rotation.x, rb.rotation.y, 0f));
            targetRotation = Quaternion.Slerp(rb.rotation, targetRotation, Time.deltaTime * 10);
            rb.MoveRotation(targetRotation);
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
            rb.MoveRotation(targetRotation);
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

    float count;
    float maxCount;

    private void MeleeAttack()
    {
        //anim.SetTrigger(Attack);

        //StartCoroutine(ComboAttack(timeToCombo));
    }
    IEnumerator ComboAttack(float time)
    {
        yield return new WaitForSeconds(time);
    }
    private void FireBall()
    {

    }
    private void TurboAttack()
    {

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