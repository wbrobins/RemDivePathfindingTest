using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemyBehavior : MonoBehaviour
{
    public NavMeshAgent agent;
    public GameObject player;
    public bool following;
    private Vector3 startPoint;
    private Transform firePoint;
    [SerializeField] private GameObject projectilePrefab;

    [Header("Knockback")]
    [Tooltip("How long physics fully controls the enemy after an explosion before the NavMeshAgent takes back over")]
    [SerializeField] private float knockbackRecoveryDelay = 0.4f;

    private Rigidbody rb;
    private Coroutine knockbackRoutine;

    public string enemyId;

    public Enemy Enemy { get; private set; }


    void Awake()
    {
        SetUp(new Enemy(enemyId));
        rb = GetComponent<Rigidbody>();
    }

    public void SetUp(Enemy enemy)
    {
        Enemy = enemy;
        agent.stoppingDistance = Enemy.StopDistance;
    }

    void Start()
    {
        //init
        player = GameObject.Find("Player");
        firePoint = transform.Find("FirePoint");

        //get current position as start
        startPoint = transform.position;

        if (rb != null)
        {
            rb.isKinematic = true;
        }
    }

    public Vector3 GetStartPoint()
    {
        return startPoint;
    }

    public void GoToStartPoint()
    {
        agent.stoppingDistance = 0;
        agent.SetDestination(startPoint);
    }

    public void FollowTarget(Vector3 position)
    {
        if(agent.enabled == true)
        {
            agent.stoppingDistance = Enemy.StopDistance;
            agent.SetDestination(position);
            transform.LookAt(position); 
        }
    }

    public IEnumerator ShootTarget(Transform target)
    {
        while (true)
        {
            yield return new WaitForSeconds(Enemy.ShootDelay);

            GameObject projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);

            Vector3 aimPoint = target.position + Vector3.up * 1.0f;
            Vector3 direction = (aimPoint - firePoint.position).normalized;

            Rigidbody projectileRb = projectile.GetComponent<Rigidbody>();
            projectileRb.linearVelocity = direction * Enemy.ProjectileSpeed;
        }
    }

    public void TakeDamage(int damage)
    {
        Enemy.Hit(damage);
        Debug.Log("Damage done: " + damage);
        if (Enemy.HP == 0)
        {
            Destroy(gameObject);
            Debug.Log("Enemy killed");
        }
    }

    public void ApplyKnockback(Vector3 explosionOrigin, float explosionForce, float explosionRadius, float upwardsModifier = 1f)
    {
        Debug.Log("ApplyKnockback called");
        if (rb == null)
        {
            Debug.Log("No rigidbody!");
            return;
        }

        if (knockbackRoutine != null)
        {
            StopCoroutine(knockbackRoutine);
        }

        Debug.Log("Starting knockback coroutine");
        knockbackRoutine = StartCoroutine(KnockbackRoutine(explosionOrigin, explosionForce, explosionRadius, upwardsModifier));
    }

    private IEnumerator KnockbackRoutine(Vector3 origin, float force, float radius, float upwardsModifier)
    {
        Debug.Log("Coroutine started");
        // Hand full control to physics for the duration of the knockback.
        agent.enabled = false;
        rb.isKinematic = false;
        Debug.Log("kinematic disabled");

        rb.linearVelocity = Vector3.zero;
        rb.AddExplosionForce(force, origin, radius, upwardsModifier, ForceMode.Impulse);

        yield return new WaitForSeconds(knockbackRecoveryDelay);

        rb.linearVelocity = Vector3.zero;

        // gameObject may have been destroyed by damage during the knockback window
        if (this == null || !gameObject.activeInHierarchy)
        {
            yield break;
        }

        agent.enabled = true;
        rb.isKinematic = true;
        agent.Warp(transform.position); // resync the agent's internal state to the post-knockback position
        GoToStartPoint();

        knockbackRoutine = null;
    }
}