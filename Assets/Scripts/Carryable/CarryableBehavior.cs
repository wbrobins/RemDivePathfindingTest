using System.Collections;
using UnityEngine;

public class CarryableBehavior : MonoBehaviour
{
    [SerializeField] private CarryableBase carryableBase;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] audioClips;
    [SerializeField] private float dropOffset = 1.5f;
    [SerializeField] private float chainDelay = 0.1f; 
    [SerializeField] private ParticleSystem particles;

    private Rigidbody rb;
    private Collider carryableCollider;
    private bool thrown = false;
    private bool exploded = false;


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

    public void Drop(Vector3 direction)
    {
        transform.SetParent(null);
        transform.localScale = Vector3.one;

        transform.position += direction.normalized * dropOffset;

        rb.isKinematic = false;
        rb.useGravity = true;
        carryableCollider.enabled = true;
    }

    public void Throw(Vector3 direction, float throwForce)
    {
        thrown = true;
        if(audioClips.Length >= 2) audioSource.PlayOneShot(audioClips[1]);

        transform.SetParent(null);
        transform.localScale = Vector3.one;

        transform.position += Vector3.up;


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
        if (exploded) return;   //stops infinite loops
        exploded = true;

        Collider[] hits = Physics.OverlapSphere(transform.position, Carryable.ExplodeRange);

        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Enemy"))
            {
                Debug.Log("enemy found");
                EnemyBehavior enemyBehavior = hit.GetComponent<EnemyBehavior>();

                if (enemyBehavior != null)
                {
                    enemyBehavior.ApplyKnockback(transform.position, Carryable.ExplosionForce, Carryable.ExplodeRange,Carryable.ExplosionUpwardsModifier);
                    enemyBehavior.TakeDamage(Carryable.ExplodeDamage, gameObject);
                }
            }
            else if(hit.CompareTag("Explodable"))
            {
                CarryableBehavior other = hit.GetComponentInParent<CarryableBehavior>();

                if (other != null && other != this && other.IsExplodable)
                {
                    float dist = Vector3.Distance(transform.position, other.transform.position);
                    float delay = chainDelay + (dist / Carryable.ExplodeRange) * chainDelay;
                    other.TriggerChain(delay);
                }
            }
        }

        audioSource.Play();
        particles.Play();

        foreach (Renderer r in GetComponentsInChildren<Renderer>()) 
        {
            if (r is ParticleSystemRenderer) continue;
            r.enabled = false;
        }
        foreach (Collider c in GetComponentsInChildren<Collider>()) c.enabled = false;

        Destroy(gameObject, audioSource.clip.length);
    }


    public void TriggerChain(float delay)
    {
        if (!exploded) StartCoroutine(ChainRoutine(delay));
    }

    private IEnumerator ChainRoutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        Explode();
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
                    if(!carryableBase.Explodable && audioSource != null) audioSource.PlayOneShot(audioClips[0]);
                    enemy.TakeDamage(Carryable.ThrowDamage, gameObject);
                }  

                if (carryableBase.Explodable)
                {
                    Explode();
                }
            }
            else if (collision.transform.CompareTag("Ground") && Carryable.Explodable)
            {
                Explode();
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
