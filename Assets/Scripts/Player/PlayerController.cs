using System.Collections;
using UnityEngine;

public enum MovementState
{
    Walking,
    Sprinting,
    Crouching,
    Jumping,
    Sliding,
    JumpSliding,
    Grappling
}

public class PlayerController : MonoBehaviour
{
    private Rigidbody rb;
    private CapsuleCollider pCollider;

    [SerializeField] private new GameObject camera;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float standingHeight = 1.0f;
    [SerializeField] private float slidingHeight = -1.0f;
    [SerializeField] private float cameraTransitionSpeed = 10f;
    private float targetCameraHeight;

    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float sprintSpeed = 10f;
    [SerializeField] private float crouchSpeed = 2.5f;
    [SerializeField] private float crouchHeight = -0.5f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float shootDelay = 2.0f;
    [SerializeField] private float slideSpeed = 15f;
    [SerializeField] private float slideDuration = .75f;
    [SerializeField] private float slideJumpMultiplier = 1.2f;

    [Header("Air Control")]
    [Tooltip("Higher = more responsive air steering, lower = more momentum preserved")]
    [SerializeField] private float airControlStrength = 2f;

    [Header("Ground Check")]
    [Tooltip("Empty transform positioned at the player's feet")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.3f;
    [SerializeField] private LayerMask groundCheckMask = ~0;
    [Tooltip("How long after a jump impulse the ground check is ignored, so landing detection can't instantly cancel the jump")]
    [SerializeField] private float jumpGraceDuration = 0.15f;
    [Tooltip("Small constant downward push while grounded, keeps the player glued to slopes instead of gravity slowly accumulating")]
    [SerializeField] private float groundStickForce = 3f;
    private float jumpGraceEndTime;
    private Vector3 groundNormal = Vector3.up;

    [Header("Grapple")]
    [SerializeField] private float grappleLookRange = 25f;
    [SerializeField] private float grappleSpeed = 20f;
    [Tooltip("Distance from the grapple point at which the player auto-detaches")]
    [SerializeField] private float grappleArriveDistance = 1.5f;
    [Tooltip("Momentum carried forward if E is pressed the instant the grapple starts")]
    [SerializeField] private float minGrappleExitSpeed = 5f;
    [Tooltip("Momentum carried forward if the grapple completes (or is cancelled right at the point)")]
    [SerializeField] private float maxGrappleExitSpeed = 20f;
    [SerializeField] private LayerMask grappleLookMask = ~0;
    private const string GrappleTag = "Grapple";

    private bool jumpPressed;
    private bool grounded;
    private bool shooting = false;
    private float slideTimer;
    private Vector3 slideDirection;

    private bool lookingAtGrapplePoint;
    private Transform lookedAtGrapplePoint;
    private Transform grappleTarget;
    private Vector3 grappleStartPosition;
    private float grappleTotalDistance;

    private MovementState currState = MovementState.Walking;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        pCollider = GetComponent<CapsuleCollider>();

        rb.interpolation = RigidbodyInterpolation.Interpolate;

        targetCameraHeight = standingHeight;
    }

    void Update()
    {
        if (currState == MovementState.Sliding && !Input.GetKey(KeyCode.LeftControl))
        {
            ExitSlide();
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            jumpPressed = true;
        }

        if (Input.GetKeyDown(KeyCode.LeftControl) && currState == MovementState.Sprinting)
        {
            EnterSlide();
        }

        CheckGrappleLookAt();

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (currState == MovementState.Grappling)
            {
                ExitGrapple();
            }
            else if (lookingAtGrapplePoint)
            {
                StartGrapple(lookedAtGrapplePoint);
            }
        }

        UpdateMovementState();
        UpdateCameraHeight();

        if (Input.GetMouseButtonDown(0) && !shooting)
        {
            shooting = true;
            StartCoroutine(ShootRoutine());
        }
    }

    void FixedUpdate()
    {
        CheckGrounded();
        HandleMovement();
        HandleJump();
        HandleGrapple();
    }

    void CheckGrounded()
    {
        if (Time.time < jumpGraceEndTime)
        {
            grounded = false;
            return;
        }

        Collider[] hits = Physics.OverlapSphere(groundCheck.position, groundCheckRadius, groundCheckMask);

        grounded = false;
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Ground") || hit.CompareTag("Wall"))
            {
                grounded = true;
                break;
            }
        }

        if (grounded)
        {
            if (Physics.Raycast(groundCheck.position + Vector3.up * 0.1f, Vector3.down, out RaycastHit slopeHit, groundCheckRadius + 0.3f, groundCheckMask))
            {
                groundNormal = slopeHit.normal;
            }
            else
            {
                groundNormal = Vector3.up;
            }
        }
    }

    void UpdateCameraHeight()
    {
        Vector3 cameraPosition = cameraTransform.localPosition;

        cameraPosition.y = Mathf.Lerp(
            cameraPosition.y,
            targetCameraHeight,
            Time.deltaTime * cameraTransitionSpeed
        );

        cameraTransform.localPosition = cameraPosition;
    }

    void UpdateMovementState()
    {
        if (currState == MovementState.Sliding)
        {
            return;
        }

        if (currState == MovementState.Grappling)
        {
            return;
        }

        if (currState == MovementState.JumpSliding)
        {
            if (grounded)
            {
                if (Input.GetKey(KeyCode.LeftControl))
                {
                    EnterSlide();
                }
                else
                {
                    targetCameraHeight = standingHeight;
                    currState = Input.GetKey(KeyCode.LeftShift) ? MovementState.Sprinting : MovementState.Walking;
                }
            }
            return;
        }

        if (!grounded)
        {
            currState = MovementState.Jumping;
        }
        else if (Input.GetKey(KeyCode.LeftControl))
        {
            currState = MovementState.Crouching;
            targetCameraHeight = crouchHeight;
        }
        else if (Input.GetKey(KeyCode.LeftShift))
        {
            currState = MovementState.Sprinting;
            targetCameraHeight = standingHeight;
        }
        else
        {
            currState = MovementState.Walking;
            targetCameraHeight = standingHeight;
        }
    }

    void HandleMovement()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        Vector3 forward = camera.transform.forward;
        Vector3 right = camera.transform.right;

        forward.y = 0;
        right.y = 0;

        forward.Normalize();
        right.Normalize();

        Vector3 moveDirection = (right * x + forward * z).normalized;

        switch (currState)
        {
            case MovementState.Walking:
                SetGroundVelocity(moveDirection, walkSpeed);
                break;
            case MovementState.Sprinting:
                SetGroundVelocity(moveDirection, sprintSpeed);
                break;
            case MovementState.Crouching:
                SetGroundVelocity(moveDirection, crouchSpeed);
                break;
            case MovementState.Sliding:
                HandleSlide();
                break;
            case MovementState.Grappling:
                break;
            case MovementState.Jumping:
                break;
            case MovementState.JumpSliding:
                ApplyAirControl(moveDirection, walkSpeed);
                break;
        }
    }

    void SetGroundVelocity(Vector3 moveDirection, float moveSpeed)
    {
        Vector3 slopeDirection = Vector3.ProjectOnPlane(moveDirection, groundNormal).normalized;
        rb.linearVelocity = slopeDirection * moveSpeed - groundNormal * groundStickForce;
    }

    void ApplyAirControl(Vector3 moveDirection, float moveSpeed)
    {
        Vector3 currentHorizontal = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        Vector3 desiredHorizontal = moveDirection * moveSpeed;

        Vector3 newHorizontal = Vector3.Lerp(
            currentHorizontal,
            desiredHorizontal,
            Time.fixedDeltaTime * airControlStrength
        );

        rb.linearVelocity = new Vector3(newHorizontal.x, rb.linearVelocity.y, newHorizontal.z);
    }

    void HandleJump()
    {
        if (jumpPressed && grounded)
        {
            if (currState == MovementState.Sliding)
            {
                Vector3 launchDirection = GetCameraFlatForward();

                Vector3 slideMomentum = launchDirection * slideSpeed * slideJumpMultiplier;
                rb.linearVelocity = new Vector3(slideMomentum.x, rb.linearVelocity.y, slideMomentum.z);
                rb.AddForce(Vector3.up * jumpForce * slideJumpMultiplier, ForceMode.Impulse);
                EnterJumpSlide();
            }
            else
            {
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            }

            grounded = false;
            jumpGraceEndTime = Time.time + jumpGraceDuration;
        }

        jumpPressed = false;
    }

    Vector3 GetCameraFlatForward()
    {
        Vector3 forward = camera.transform.forward;
        forward.y = 0f;

        if (forward.sqrMagnitude < 0.0001f)
        {
            return slideDirection;
        }

        return forward.normalized;
    }

    void EnterSlide()
    {
        currState = MovementState.Sliding;

        slideTimer = slideDuration;

        targetCameraHeight = slidingHeight;

        slideDirection = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z).normalized;
    }

    void ExitSlide()
    {
        targetCameraHeight = standingHeight;

        currState = Input.GetKey(KeyCode.LeftShift) ? MovementState.Sprinting : MovementState.Walking;
    }

    void HandleSlide()
    {
        Vector3 downhillDirection = Vector3.ProjectOnPlane(Vector3.down, groundNormal).normalized;

        bool goingDownhill =
            grounded &&
            downhillDirection.sqrMagnitude > 0.001f &&
            Vector3.Dot(slideDirection, downhillDirection) > 0f;

        if (!goingDownhill)
        {
            slideTimer -= Time.fixedDeltaTime;
        }

        rb.linearVelocity = slideDirection * slideSpeed
                        + Vector3.up * rb.linearVelocity.y;

        if (slideTimer <= 0)
        {
            ExitSlide();
        }
    }

    void EnterJumpSlide()
    {
        currState = MovementState.JumpSliding;

        targetCameraHeight = slidingHeight;
    }

    void CheckGrappleLookAt()
    {
        lookingAtGrapplePoint = false;
        lookedAtGrapplePoint = null;

        if (Physics.Raycast(camera.transform.position, camera.transform.forward, out RaycastHit hit, grappleLookRange, grappleLookMask))
        {
            if (hit.collider.CompareTag(GrappleTag))
            {
                lookingAtGrapplePoint = true;
                lookedAtGrapplePoint = hit.collider.transform;
            }
        }
    }

    void StartGrapple(Transform target)
    {
        grappleTarget = target;
        grappleStartPosition = transform.position;
        grappleTotalDistance = Mathf.Max(Vector3.Distance(grappleStartPosition, target.position), 0.01f);

        targetCameraHeight = standingHeight;
        rb.useGravity = false;

        currState = MovementState.Grappling;
    }

    void HandleGrapple()
    {
        if (currState != MovementState.Grappling)
        {
            return;
        }

        if (grappleTarget == null)
        {
            ExitGrapple();
            return;
        }

        Vector3 toTarget = grappleTarget.position - transform.position;

        if (toTarget.magnitude <= grappleArriveDistance)
        {
            ExitGrapple();
            return;
        }

        rb.linearVelocity = toTarget.normalized * grappleSpeed;
    }

    void ExitGrapple()
    {
        if (currState != MovementState.Grappling)
        {
            return;
        }

        float traveled = Vector3.Distance(grappleStartPosition, transform.position);
        float progress = Mathf.Clamp01(traveled / grappleTotalDistance);

        Vector3 direction = grappleTarget != null
            ? (grappleTarget.position - grappleStartPosition).normalized
            : rb.linearVelocity.normalized;

        float exitSpeed = Mathf.Lerp(minGrappleExitSpeed, maxGrappleExitSpeed, progress);

        rb.useGravity = true;
        rb.linearVelocity = direction * exitSpeed;

        grappleTarget = null;

        currState = MovementState.Jumping;
    }

    IEnumerator ShootRoutine()
    {
        //raycast logic here
        Vector3 origin = camera.transform.position;
        Vector3 direction = camera.transform.forward;

        Debug.DrawRay(origin, direction * 10f, Color.red, 1f);

        if (Physics.Raycast(origin, direction, out RaycastHit hit, 10f))
        {
            if (hit.collider.CompareTag("Enemy"))
            {
                EnemyBehavior enemyBehavior = hit.collider.GetComponent<EnemyBehavior>();

                if (enemyBehavior != null)
                {
                    enemyBehavior.TakeDamage(1);
                }
            }
        }

        yield return new WaitForSeconds(shootDelay);
        shooting = false;
    }

}