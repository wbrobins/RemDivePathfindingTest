using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private Rigidbody rb;
    private new GameObject camera;
    private bool jumpPressed = false;
    private bool shooting = false;
    private bool sprinting = false;
    private float sprintSpeed;
    private float baseSpeed;

    public float speed = 5.0f;
    public float jumpForce = 5.0f;
    public float shootDelay = 2.0f;
    public bool grounded = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        camera = GameObject.Find("Camera");
        baseSpeed = speed;
        sprintSpeed = speed*2;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            jumpPressed = true;
        }

        if (Input.GetMouseButtonDown(0) && !shooting)
        {
            shooting = true;
            StartCoroutine(ShootRoutine());
        }

        if (Input.GetKey(KeyCode.LeftShift))
        {
            sprinting = true;
        }
        else
        {
            sprinting = false;
        }
    }

    void FixedUpdate()
    {
        if (sprinting)
        {
            speed = sprintSpeed;
        }
        else
        {
            speed = baseSpeed;
        }
        
        if (jumpPressed && grounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            //Debug.Log("Jump!");
            grounded = false;
        }

        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        Vector3 forward = camera.transform.forward;
        Vector3 right = camera.transform.right;

        forward.y = 0;
        right.y = 0;

        forward.Normalize();
        right.Normalize();

        Vector3 moveDirection = (right * x + forward * z).normalized;

        rb.linearVelocity = new Vector3(
            moveDirection.x * speed,
            rb.linearVelocity.y,
            moveDirection.z * speed
        );

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
