using System.Collections;
using UnityEngine;

public enum MovementState
{
    Walking,
    Sprinting,
    Jumping,
    Sliding
}

public class PlayerController : MonoBehaviour
{
    private Rigidbody rb;
    private new GameObject camera;
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float sprintSpeed = 10f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float shootDelay = 2.0f;

    private bool jumpPressed;
    private bool grounded;
    private bool shooting = false;

    private MovementState currState = MovementState.Walking;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        camera = GameObject.Find("Camera");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            jumpPressed = true;
        }

        UpdateMovementState();

        if (Input.GetMouseButtonDown(0) && !shooting)
        {
            shooting = true;
            StartCoroutine(ShootRoutine());
        }
    }

    void FixedUpdate()
    {
        HandleMovement();
        HandleJump();
    }

    void UpdateMovementState()
    {
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

        float moveSpeed = walkSpeed;

        switch (currState)
        {
            case MovementState.Walking:
                moveSpeed = walkSpeed;
                break;
            case MovementState.Sprinting:
                moveSpeed = sprintSpeed;
                break;
            case MovementState.Jumping:
                moveSpeed = walkSpeed;
                break;
        }

        Debug.Log(moveSpeed);
        rb.linearVelocity = new Vector3(
            moveDirection.x * moveSpeed,
            rb.linearVelocity.y,
            moveDirection.z * moveSpeed
        );
        Debug.Log(currState);
    }

    void HandleJump()
    {
        if (jumpPressed && grounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            //Debug.Log("Jump!");
            grounded = false;
        }

        jumpPressed = false;
    }

    IEnumerator ShootRoutine()
    {
        //raycast logic here
        Vector3 origin = camera.transform.position;
        Vector3 direction = camera.transform.forward;

        Debug.DrawRay(origin, direction*10f, Color.red, 1f);

        if (Physics.Raycast(origin, direction, out RaycastHit hit, 10f))
        {
            if (hit.collider.CompareTag("Enemy"))
            {
                EnemyBehavior enemyBehavior= hit.collider.GetComponent<EnemyBehavior>();

                if(enemyBehavior != null)
                {
                    enemyBehavior.TakeDamage(1);
                }
            }
            //Debug.Log(hit.collider.name);
            //Debug.Log(hit.collider.gameObject.layer);
            //Debug.Log(hit.collider.tag);
        }
        
        //Debug.Log("shooting");
        yield return new WaitForSeconds(shootDelay);
        shooting = false;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Ground") || collision.collider.CompareTag("Wall"))
        {
            grounded = true;
        }
    }
}
