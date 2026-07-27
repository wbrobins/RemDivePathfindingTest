using System.Collections;
using UnityEngine;

public enum MovementState
{
    Walking,
    Sprinting,
    Jumping,
    Sliding,
    JumpSliding
}

public class PlayerController : MonoBehaviour
{
    private Rigidbody rb;

    [SerializeField] private new GameObject camera;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float standingHeight = 1.0f;
    [SerializeField] private float slidingHeight = -1.0f;
    [SerializeField] private float cameraTransitionSpeed = 10f;
    private float targetCameraHeight;

    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float sprintSpeed = 10f;
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

    private bool jumpPressed;
    private bool grounded;
    private bool shooting = false;
    private float slideTimer;
    private Vector3 slideDirection;

    private MovementState currState = MovementState.Walking;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
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
    }

    void CheckGrounded()
    {
        if (rb.linearVelocity.y > 0.1f)
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
        else if (Input.GetKey(KeyCode.LeftShift))
        {
            currState = MovementState.Sprinting;
        }
        else
        {
            currState = MovementState.Walking;
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
            case MovementState.Sliding:
                HandleSlide();
                break;
            case MovementState.Jumping:
            case MovementState.JumpSliding:
                ApplyAirControl(moveDirection, walkSpeed);
                break;
        }
    }

    void SetGroundVelocity(Vector3 moveDirection, float moveSpeed)
    {
        rb.linearVelocity = new Vector3(
            moveDirection.x * moveSpeed,
            rb.linearVelocity.y,
            moveDirection.z * moveSpeed
        );
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
        slideTimer -= Time.fixedDeltaTime;

        rb.linearVelocity = slideDirection * slideSpeed + Vector3.up * rb.linearVelocity.y;

        if (slideTimer <= 0)
        {
            ExitSlide();
        }
    }

    void EnterJumpSlide()
    {
        currState = MovementState.JumpSliding;

        targetCameraHeight = standingHeight;
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