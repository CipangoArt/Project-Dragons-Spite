using UnityEngine;
using UnityEngine.InputSystem;
using FIMSpace.FSpine;
using FIMSpace.GroundFitter;

public class NewMovement : MonoBehaviour
{
    [Header("Switch")]
    [SerializeField] FSpineAnimator fSpine;
    [SerializeField] DragonMovement DMovement;
    [SerializeField] FGroundFitter_Input fitterInput;

    [Header("Collsion")]
    [SerializeField] CapsuleCollider capsuleCollider;

    [Header("Input")]
    [SerializeField] PlayerInput playerInput;

    Vector2 move;
    Vector2 lookDelta;

    bool flap;

    float zoom;

    [Header("Ground")]
    [SerializeField] float groundDistance;
    [SerializeField] LayerMask layerMask;

    RaycastHit groundHit;

    bool ground;

    //States
    public enum State
    {
        Ground,
        Air
    }

    public State state;

    [Header("Apply")]
    [SerializeField] Rigidbody RB;
    float t;
    Vector3 velocity;

    [Header("Gravity")]
    [SerializeField] float gravity;

    [Header("Physics")]
    [SerializeField] float moveDeadzone;

    Vector3 moveVelocity;

    bool moving;

    float speed;

    float speedPercent;

    float verticalVelocity;

    struct CollisionInformation
    {
        public RaycastHit hitInfo;

        public bool collided;

        public bool cast;

        public bool check;
    }

    [Header("Snap")]
    [SerializeField] float height;
    [SerializeField] float stopDeadzone;

    [Header("Move")]
    [SerializeField] float baseSpeed;
    [SerializeField] float accelaration;
    [SerializeField] float maxSpeed;

    [Header("Friction")]
    [SerializeField] float friction;
    [SerializeField] float moveFriction;

    [Header("Jump")]
    [SerializeField] int jumps;
    [SerializeField] float baseJumpHeight;
    [SerializeField] float fullJumpHeight;
    [SerializeField] float jumpTimer;
    [SerializeField] float coyoteTime;

    Vector3 jumpNormal;

    bool canJump;

    bool jumping;

    bool jump;

    int currentJumps;

    float currentJumpTimer;

    float currentCoyoteTime;

    [Header("Air Control")]
    [SerializeField] float airControl;

    [Header("Transform")]
    [SerializeField] Transform cameraTransform;

    void Start()
    {
        playerInput.onActionTriggered += callbackContext =>
        {
            switch (callbackContext.action.name)
            {
                case "Move":
                    move = callbackContext.ReadValue<Vector2>();
                    break;
                case "Look Delta":
                    lookDelta = callbackContext.ReadValue<Vector2>();
                    break;
                case "Jump":
                    flap = callbackContext.ReadValue<float>() == 1;
                    break;
                case "Zoom":
                    zoom = callbackContext.ReadValue<float>();
                    break;
            }
        };
    }


    void Ground()
    {
        ground = Physics.Raycast(transform.position, -transform.up, out groundHit, Mathf.Infinity, layerMask, QueryTriggerInteraction.Ignore) && groundHit.distance <= groundDistance;
    }

    void SetState()
    {
        state = ground ? State.Ground : State.Air;
    }

    void Apply()
    {
        RB.velocity += velocity;
    }

    void Gravity()
    {
        velocity -= Vector3.up * gravity;
    }

    void InitPhysics()
    {
        velocity = Vector3.zero;

        verticalVelocity = Vector3.Dot(RB.velocity, transform.up);

        moveVelocity = RB.velocity - (transform.up * verticalVelocity);

        speed = moveVelocity.magnitude;

        speedPercent = Mathf.Clamp(speed / maxSpeed, 0, 1);

        moving = move.magnitude > 0 && speed > moveDeadzone;

    }

    CollisionInformation CollisionCheck(Vector3 goal)
    {
        CollisionInformation returning = new CollisionInformation();

        if (Physics.SphereCast(
            transform.position,
            capsuleCollider.radius - Physics.defaultContactOffset,
            Vector3.Normalize(goal - transform.position),
            out RaycastHit checkHit,
            Vector3.Distance(transform.position, goal),
            3,
            QueryTriggerInteraction.Ignore))
        {
            returning.hitInfo = checkHit;

            returning.collided = true;

            returning.cast = true;
        }

        if (Physics.CheckSphere(
            goal,
            capsuleCollider.radius - Physics.defaultContactOffset,
            3,
            QueryTriggerInteraction.Ignore))
        {
            returning.collided = true;

            returning.check = true;
        }
        return returning;
    }

    void Snap()
    {
        if (ground && !moving && speed <= stopDeadzone)
            velocity -= RB.velocity;

        Vector3 snapGoal = groundHit.point + (groundHit.normal * height);
        if (CollisionCheck(snapGoal).collided) return;

        transform.position = snapGoal;
    }

    void Rotate()
    {
        Vector3 normal = ground ? groundHit.normal : Vector3.up;

        if (!ground)
        {
            Vector3 snapGoal = transform.position - (transform.up * height) + (normal * height);

            if (!CollisionCheck(snapGoal).collided)
                transform.position = snapGoal;
        }

        transform.rotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(Vector3.forward, normal), normal);
    }

    private void Move()
    {
        if (move.magnitude == 0) return;
        Vector3 moveFowardNormal = Vector3.ProjectOnPlane(cameraTransform.forward, transform.up).normalized;

        Vector3 moveRightNormal = Vector3.ProjectOnPlane(cameraTransform.right, transform.up).normalized;

        velocity += ((moveFowardNormal * move.y) + moveRightNormal * move.x) *
            (moving ? accelaration * (1 - speedPercent) : baseSpeed * (1 - Mathf.Max(Vector3.Dot(Vector3.forward, Vector3.up), 0)));
    }

    void Friction()
    {
        Vector3 frictionMask = velocity * (1 - Mathf.Max(Vector3.Dot(velocity, Vector3.up), 0));

        Vector3 frictionVelocity = moving ?
            (RB.velocity
            - (Vector3.Project(RB.velocity, frictionMask)
            * Mathf.Max(Vector3.Dot(RB.velocity.normalized, frictionMask.normalized), 0)))
            * moveFriction
            : RB.velocity * friction;

        velocity += moving ?
            -frictionVelocity
            + (velocity.normalized
            * frictionVelocity.magnitude
            * Mathf.Abs(Vector3.Dot(Vector3.Cross(moveVelocity, transform.up).normalized, velocity.normalized)))
            : -frictionVelocity;
    }

    void Jump()
    {
        if (ground)
        {
            canJump = true;
            jumping = false;

            currentJumps = jumps;

            currentCoyoteTime = coyoteTime;
        }
        else
        {
            if (currentCoyoteTime <= 0 && currentJumps == jumps)
            {
                currentJumps = 1;
            }
            if (currentJumps > 0 && !flap)
                canJump = true;
        }
        if (flap)
        {
            if (canJump)
            {
                jumpNormal = transform.up;

                float jumpForce = Mathf.Sqrt(2 * (gravity / Time.fixedDeltaTime) * (ground ? (baseJumpHeight - groundDistance) : baseJumpHeight));

                velocity += jumpNormal * (ground ? jumpForce : Mathf.Max(jumpForce - Mathf.Max(verticalVelocity, 0), 0));

                canJump = false;

                jumping = true;

                currentJumps -= 1;

                currentJumpTimer = jumpTimer;

                if (ground)
                {
                    Vector3 snapGoal = groundHit.point + (transform.up * (groundDistance + Physics.defaultContactOffset));

                    if (CollisionCheck(snapGoal).collided) return;

                    transform.position = snapGoal;
                }
            }
            else if (jumping && currentJumpTimer > 0)
            {
                velocity += jumpNormal
                    * ((Time.fixedDeltaTime
                    * (Mathf.Sqrt(2 * (gravity / Time.fixedDeltaTime) * fullJumpHeight)
                    - Mathf.Sqrt(2 * (gravity / Time.fixedDeltaTime) * baseJumpHeight)))
                    / jumpTimer);

                if (currentJumpTimer <= 0)
                    jumping = false;
            }

        }
        else
        {
            jumping = false;
        }
    }

    void Count()
    {
        if (currentJumpTimer > 0)
            currentJumpTimer -= Time.fixedDeltaTime;
        if (currentJumpTimer > 0)
            currentCoyoteTime -= Time.fixedDeltaTime;
    }

    void AirControl()
    {
        if (!moving) return;

        velocity -= (moveVelocity
            - (Vector3.Project(moveVelocity, Vector3.ProjectOnPlane(velocity, transform.up))
            * Mathf.Max(Vector3.Dot(moveVelocity.normalized, Vector3.ProjectOnPlane(velocity, transform.up).normalized), 0)))
            * airControl;
    }

    private void FixedUpdate()
    {
        Count();
        Ground();
        SetState();
        InitPhysics();
        if (state == State.Air)
        {
            fSpine.enabled = false;
            fitterInput.enabled = false;
            fSpine.enabled = false;
            

            

            

           

            /* switch (state)
             {
                 case State.Ground:
                     Snap();
                     Rotate();
                     Move();
                     Friction();
                     Jump();
                     Apply();
                     break;
                 case State.Air:*/
            Rotate();
            Move();
            Gravity();
            AirControl();
            Jump();
            Apply();
            //  break;
        }
        else
        {
            fSpine.enabled = true;
            fitterInput.enabled = true;
            fSpine.enabled = true;
        }
    }

}




