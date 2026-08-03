using UnityEngine;

public class CarryableBehavior : MonoBehaviour
{
    [SerializeField] private CarryableBase carryableBase;
    private Rigidbody rb;
    private Collider carryableCollider;

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

    public void Throw()
    {
        
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

    private void OnDrawGizmos()
    {
        if (carryableBase != null && carryableBase.Explodable)
        {
          Gizmos.DrawWireSphere(transform.position, carryableBase.ExplodeRange);  
        }
    }
}
