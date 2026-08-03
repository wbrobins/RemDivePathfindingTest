using System.Collections;
using UnityEngine;

public class CarryableBehavior : MonoBehaviour
{
    [SerializeField] private CarryableBase carryableBase;

    private Rigidbody rb;
    private Collider carryableCollider;
    private bool thrown = false;

    public bool IsExplodable => Carryable.Base.Explodable;
    
    public Carryable Carryable { get; private set; }

    void Awake()
    {
        SetUp(new Carryable(carryableBase));
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        carryableCollider = GetComponent<Collider>();
    }

    public void SetUp(Carryable carryable)
    {
        Carryable = carryable;
    }

    public void Pickup(Transform nTransform)
    {
        transform.SetParent(nTransform);

        transform.localPosition = Carryable.Base.HoldPositionOffset;
        transform.localRotation = Quaternion.Euler(Carryable.Base.HoldRotationOffset);

        rb.isKinematic = true;
        rb.useGravity = false;
        carryableCollider.enabled = false;
    }

    public void Drop()
    {
        transform.SetParent(null);

        transform.localScale = Vector3.one;

        rb.isKinematic = false;
        rb.useGravity = true;
        carryableCollider.enabled = true;
    }

    public void Throw(Vector3 direction, float throwForce)
    {
        thrown = true;

        transform.SetParent(null);
        transform.localScale = Vector3.one;


        rb.isKinematic = false;
        rb.useGravity = true;
        carryableCollider.enabled = true;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.linearVelocity = direction * throwForce;

        StartCoroutine(ThrowRoutine());
    }

    public void Explode()
    {
        Collider[] enemies = Physics.OverlapSphere(transform.position, Carryable.ExplodeRange);

        foreach (Collider enemy in enemies)
        {
            if (enemy.CompareTag("Enemy"))
            {
                Debug.Log("enemy found");
                EnemyBehavior enemyBehavior = enemy.GetComponent<EnemyBehavior>();

                if (enemyBehavior != null)
                {
                    enemyBehavior.ApplyKnockback(transform.position, Carryable.ExplosionForce, Carryable.ExplodeRange,Carryable.ExplosionUpwardsModifier);
                    enemyBehavior.TakeDamage(Carryable.ExplodeDamage);
                }
            }
        }

        Destroy(gameObject);
    }

    IEnumerator ThrowRoutine()
    {
        yield return new WaitForSeconds(5f);
        thrown = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (thrown)
        {
            if (collision.transform.CompareTag("Enemy"))
            {
                EnemyBehavior enemy = collision.transform.GetComponent<EnemyBehavior>();

                if (enemy != null)
                {
                    enemy.TakeDamage(Carryable.ThrowDamage);
                }  

                if (carryableBase.Explodable)
                {
                    Explode();
                }
                }
        }
    }

    private void OnDrawGizmos()
    {
        if (carryableBase != null && carryableBase.Explodable)
        {
          Gizmos.DrawWireSphere(transform.position, carryableBase.ExplodeRange);  
        }
    }
}
