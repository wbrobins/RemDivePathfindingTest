using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

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

    [SerializeField] public int hp = 10;
    private int maximumHp;

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
    [SerializeField] private float slideSpeed = 15f;
    [SerializeField] private float slideDuration = .75f;
    [SerializeField] private float slideJumpMultiplier = 1.2f;
    private bool sprintToggled;
    private bool wantsSprint;

    [SerializeField] private Vector3 lastCheckpointPos;

    [Header("Ground Acceleration")]
    [Tooltip("Seconds to go from standstill to full speed of the current state")]
    [SerializeField] private float rampUpTime = 0.2f;
    [Tooltip("Seconds to go from full speed to standstill when no keys are held. Set to 0 for instant stop")]
    [SerializeField] private float rampDownTime = 0.1f;
    private Vector3 groundVelocity;

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

    [Header("Crouch Clearance")]
    [Tooltip("The CapsuleCollider on the head object (the one that holds the camera)")]
    [SerializeField] private CapsuleCollider headCollider;
    [SerializeField] private LayerMask ceilingCheckMask = ~0;
    [Tooltip("Shrinks the check capsule slightly so brushing a wall or ceiling doesn't count as blocked")]
    [SerializeField] private float clearanceMargin = 0.02f;
    private readonly Collider[] ceilingHits = new Collider[8];

    [Header("Slide Tuning")]
    [Tooltip("Slide start boost only applies if this much time has passed since the last boosted slide")]
    [SerializeField] private float slideBoostCooldown = 1.5f;
    [Tooltip("Flat-ground slide speed lost per second")]
    [SerializeField] private float slideDecay = 8f;
    [Tooltip("Slide won't decay below this")]
    [SerializeField] private float minSlideSpeed = 8f;
    [Tooltip("Degrees per second the slide direction can rotate toward your input")]
    [SerializeField] private float slideSteerRate = 90f;

    [Header("Slide Jump Tuning")]
    [Tooltip("Horizontal speed multiplier on slide jump (vertical still uses slideJumpMultiplier)")]
    [SerializeField] private float slideJumpHorizontalBoost = 1.1f;
    [SerializeField] private float maxSlideJumpSpeed = 18f;
    [Tooltip("Degrees per second you can steer while in the air after a slide jump (speed is preserved)")]
    [SerializeField] private float jumpSlideSteerRate = 120f;
    [Tooltip("Speed kept when landing a slide jump into another slide. Keep boost x retention below 1 so chains bleed speed")]
    [SerializeField] private float slideJumpLandingRetention = 0.85f;

    [Header("Gravity")]
    [SerializeField] private float riseGravityMultiplier = 1.2f;
    [SerializeField] private float fallGravityMultiplier = 2f;

    private float currentSlideSpeed;
    private float nextSlideBoostTime;

    [Header("Grapple")]
    [SerializeField] private float grappleLookRange = 25f;
    [Tooltip("Distance from the grapple point at which the player auto-detaches")]
    [SerializeField] private float grappleArriveDistance = 1.5f;
    [SerializeField] private float grappleSpeed = 20f;
    [Tooltip("Momentum carried if you detach right where you started the grapple")]
    [SerializeField] private float minGrappleExitSpeed = 5f;
    [Tooltip("Momentum carried once you've traveled Exit Max Distance from where you started")]
    [SerializeField] private float maxGrappleExitSpeed = 20f;
    [Tooltip("Distance traveled from the grapple start at which exit momentum reaches its max")]
    [SerializeField] private float grappleExitMaxDistance = 25f;
    private Vector3 grappleStartPosition;
    [SerializeField] private LayerMask grappleLookMask = ~0;

    [Header("UI Hooks")]
    [SerializeField] private TextMeshProUGUI ammoText;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI alertText;
    [SerializeField] private TextMeshProUGUI midAlertText;
    [SerializeField] private PlayerDamageFlash damageFlash;

    private const string GrappleTag = "Grapple";

    private bool jumpPressed;
    private bool grounded;
    private float slideTimer;
    private Vector3 slideDirection;

    private bool lookingAtGrapplePoint;
    private Transform lookedAtGrapplePoint;
    private Transform grappleTarget;
    private MovementState currState = MovementState.Walking;

    public MovementState CurrState => currState;
    public bool Grounded => grounded;
    public float HorizontalSpeed
    {
        get
        {
            Vector3 v = rb.linearVelocity;
            return new Vector2(v.x, v.z).magnitude;
        }
    }
    public float VerticalSpeed => rb.linearVelocity.y;

    void Start()
    {
        lastCheckpointPos = transform.position; 
        rb = GetComponent<Rigidbody>();
        pCollider = GetComponent<CapsuleCollider>();

        rb.interpolation = RigidbodyInterpolation.Interpolate;

        targetCameraHeight = standingHeight;
        healthText.text = "Health: " + hp.ToString(); 
        maximumHp = hp;
    }

    void Update()
    {
        if (PauseMenu.IsPaused) return;

        HandleSprintInput();

        if (currState == MovementState.Sliding && !Input.GetKey(KeyCode.LeftControl))
        {
            ExitSlide();
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            jumpPressed = true;
        }

        Vector3 flatVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);

        if (Input.GetKeyDown(KeyCode.LeftControl) && currState == MovementState.Sprinting && flatVelocity.sqrMagnitude > 1f)
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
    }

    void FixedUpdate()
    {
        CheckGrounded();
        HandleMovement();
        HandleJump();
        HandleGrapple();
        ApplyExtraGravity();
    }

    void HandleSprintInput()
    {
         if (!GameSettings.ToggleSprint)
        {
            sprintToggled = false;
            wantsSprint = Input.GetKey(KeyCode.LeftShift);
            return;
        }

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            sprintToggled = !sprintToggled;
        }

        wantsSprint = sprintToggled;
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

    public void TeleportToLastCheckpoint()
    {
        transform.position = lastCheckpointPos;
    }

    public void UpdateCheckpoint(Vector3 pos)
    {
        lastCheckpointPos = pos;
    }

    public void UpdateAlertText(string pText)
    {
        alertText.text = pText;
    }

    public void UpdateMiddleAlertText(string pText)
    {
        midAlertText.text = pText;
    }

    bool CanStandUp()
    {
        Transform head = headCollider.transform;

        //where the head object sits when standing
        Vector3 local = head.localPosition;
        local.y = standingHeight;
        Vector3 standingPos = head.parent != null ? head.parent.TransformPoint(local) : local;
        Vector3 offset = standingPos - head.position;

        //capsule axis and scale handling 
        Vector3 s = head.lossyScale;
        Vector3 axis;
        float axisScale, radialScale;
        switch (headCollider.direction)
        {
            case 0:
                axis = Vector3.right;
                axisScale = Mathf.Abs(s.x);
                radialScale = Mathf.Max(Mathf.Abs(s.y), Mathf.Abs(s.z));
                break;
            case 2:
                axis = Vector3.forward;
                axisScale = Mathf.Abs(s.z);
                radialScale = Mathf.Max(Mathf.Abs(s.x), Mathf.Abs(s.y));
                break;
            default:
                axis = Vector3.up;
                axisScale = Mathf.Abs(s.y);
                radialScale = Mathf.Max(Mathf.Abs(s.x), Mathf.Abs(s.z));
                break;
        }

        float worldRadius = headCollider.radius * radialScale;
        float halfSegment = Mathf.Max(0f, headCollider.height * axisScale * 0.5f - worldRadius);

        Vector3 center = head.TransformPoint(headCollider.center) + offset;
        Vector3 axisWorld = head.TransformDirection(axis);
        Vector3 p0 = center + axisWorld * halfSegment;
        Vector3 p1 = center - axisWorld * halfSegment;

        int count = Physics.OverlapCapsuleNonAlloc(p0, p1, Mathf.Max(0.01f, worldRadius - clearanceMargin), ceilingHits, ceilingCheckMask, QueryTriggerInteraction.Ignore);

        for (int i = 0; i < count; i++)
        {
            //ignore the player's own colliders
            if (!ceilingHits[i].transform.IsChildOf(transform))
            {
                return false;
            }
        }
        return true;
    }

    void UpdateCameraHeight()
    {
        Vector3 cameraPosition = cameraTransform.localPosition;

        cameraPosition.y = Mathf.Lerp(cameraPosition.y, targetCameraHeight, Time.deltaTime * cameraTransitionSpeed);

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
                    rb.linearVelocity = new Vector3(rb.linearVelocity.x * slideJumpLandingRetention, rb.linearVelocity.y, rb.linearVelocity.z * slideJumpLandingRetention);

                    EnterSlide();
                }
                else
                {
                    if (!CanStandUp())
                    {
                        currState = MovementState.Crouching;
                        targetCameraHeight = crouchHeight;
                    }
                    else
                    {
                       targetCameraHeight = standingHeight;
                       currState = wantsSprint ? MovementState.Sprinting : MovementState.Walking;
                    }
                }
            }
            return;
        }

        if (!grounded)
        {
            currState = MovementState.Jumping;
        }
        else if (Input.GetKey(KeyCode.LeftControl) || (currState == MovementState.Crouching && !CanStandUp()))
        {
            currState = MovementState.Crouching;
            targetCameraHeight = crouchHeight;
        }
        else if (wantsSprint)
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

        bool isGroundState = currState == MovementState.Walking || currState == MovementState.Sprinting || currState == MovementState.Crouching;

        if (!isGroundState)
        {
            groundVelocity = Vector3.ProjectOnPlane(rb.linearVelocity, groundNormal);
        }

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
                HandleSlide(moveDirection);
                break;
            case MovementState.Grappling:
                break;
            case MovementState.Jumping:
                break;
            case MovementState.JumpSliding:
                ApplyAirControl(moveDirection);
                break;
        }
    }

    void SetGroundVelocity(Vector3 moveDirection, float moveSpeed)
    {
        Vector3 slopeDirection = Vector3.ProjectOnPlane(moveDirection, groundNormal).normalized;
        Vector3 targetVelocity = slopeDirection * moveSpeed;

        bool hasInput = moveDirection.sqrMagnitude > 0.01f;
        float time = hasInput ? rampUpTime : rampDownTime;
        float rate = time > 0f ? moveSpeed / time : float.PositiveInfinity;

        groundVelocity = Vector3.MoveTowards(groundVelocity, targetVelocity, rate * Time.fixedDeltaTime);

        rb.linearVelocity = groundVelocity - groundNormal * groundStickForce;
    }

    void ApplyAirControl(Vector3 moveDirection)
    {
        Vector3 flat = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        if (moveDirection.sqrMagnitude < 0.01f || flat.sqrMagnitude < 0.01f) return;

        flat = Vector3.RotateTowards(flat, moveDirection * flat.magnitude, jumpSlideSteerRate * Mathf.Deg2Rad * Time.fixedDeltaTime, 0f);
        rb.linearVelocity = new Vector3(flat.x, rb.linearVelocity.y, flat.z);
    }

    void ApplyExtraGravity()
    {
        if (grounded || currState == MovementState.Grappling) return;
        float mult = rb.linearVelocity.y > 0f ? riseGravityMultiplier : fallGravityMultiplier;
        rb.AddForce(Physics.gravity * (mult - 1f), ForceMode.Acceleration);
    }

    void HandleJump()
    {
        if (jumpPressed && grounded && !(currState == MovementState.Crouching && !CanStandUp()))
        {
            if (currState == MovementState.Sliding)
            {
                float launchSpeed = Mathf.Min(currentSlideSpeed * slideJumpHorizontalBoost, maxSlideJumpSpeed);
                Vector3 slideMomentum = slideDirection * launchSpeed;   //keeps momentum direction
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

        Vector3 flat = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        slideDirection = flat.normalized;

        float entrySpeed = flat.magnitude;
        if (Time.time >= nextSlideBoostTime)
        {
            entrySpeed = Mathf.Max(entrySpeed, slideSpeed);
            nextSlideBoostTime = Time.time + slideBoostCooldown;
        }
        currentSlideSpeed = entrySpeed;
    }

    void ExitSlide()
    {
        if (!CanStandUp())
        {
            currState = MovementState.Crouching;
            targetCameraHeight = crouchHeight;
            return;
        }

        targetCameraHeight = standingHeight;
        currState = wantsSprint ? MovementState.Sprinting : MovementState.Walking;
    }

    void HandleSlide(Vector3 moveDirection)
    {
        Vector3 downhillDirection = Vector3.ProjectOnPlane(Vector3.down, groundNormal).normalized;
        bool goingDownhill = grounded && downhillDirection.sqrMagnitude > 0.001f && Vector3.Dot(slideDirection, downhillDirection) > 0f;

        if (!goingDownhill)
        {
            slideTimer -= Time.fixedDeltaTime;

            // Decay on flat/uphill only, so your ramp slide still carries momentum
            if (currentSlideSpeed > minSlideSpeed)
                currentSlideSpeed = Mathf.Max(minSlideSpeed, currentSlideSpeed - slideDecay * Time.fixedDeltaTime);
        }

        // Steer without changing speed
        if (moveDirection.sqrMagnitude > 0.01f)
        {
            slideDirection = Vector3.RotateTowards(slideDirection, moveDirection,
                slideSteerRate * Mathf.Deg2Rad * Time.fixedDeltaTime, 0f).normalized;
        }

        rb.linearVelocity = slideDirection * currentSlideSpeed + Vector3.up * rb.linearVelocity.y;

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

        Vector3 direction = grappleTarget != null ? (grappleTarget.position - transform.position).normalized : rb.linearVelocity.normalized;

        float traveled = Vector3.Distance(grappleStartPosition, transform.position);
        float t = Mathf.Clamp01(traveled / grappleExitMaxDistance);
        float exitSpeed = Mathf.Lerp(minGrappleExitSpeed, maxGrappleExitSpeed, t);

        rb.useGravity = true;
        rb.linearVelocity = direction * exitSpeed;

        grappleTarget = null;

        currState = MovementState.Jumping;
    }

    public void TakeDamage(int damage)
    {
        hp -= damage;
        damageFlash.Flash((float)damage/maximumHp * 2f);
        healthText.text = "Health: " + hp.ToString();
        if(hp <= 0)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    public GameObject GetCamera()
    {
        return camera;
    }

    public void UpdateAmmoText(Weapon weapon)
    {
        ammoText.text = "Ammo: " + weapon.CurrBarrelAmmo.ToString() + "/" + weapon.TotalAmmo.ToString();
    }

    public void UpdateAmmoText(string text)
    {
        ammoText.text = text;
    }
}