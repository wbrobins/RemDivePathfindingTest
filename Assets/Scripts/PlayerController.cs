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

    public float speed = 5.0f;
    public float jumpForce = 5.0f;
    public float shootDelay = 2.0f;
    public bool grounded = false;

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

        if (Input.GetMouseButtonDown(0) && !shooting)
        {
            shooting = true;
            StartCoroutine(ShootRoutine());
        }
    }

    void FixedUpdate()
    {
        if (jumpPressed && grounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
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
        Vector3 fwd = camera.transform.TransformDirection(Vector3.forward);

        if (Physics.Raycast(transform.position, fwd, 10, LayerMask.GetMask("Enemy")))
            print("Enemy hit");


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
