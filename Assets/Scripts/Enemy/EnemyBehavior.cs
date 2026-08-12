using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemyBehavior : MonoBehaviour
{
    public enum CombatType
    {
        Ranged,
        Melee
    }
 
    public NavMeshAgent agent;
    public GameObject player;
    public bool following;
    private Vector3 startPoint;
    private Transform firePoint;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private EnemyBase enemyBase;
 
    [Header("Combat Type")]
    [SerializeField] private CombatType combatType = CombatType.Ranged;
 
    [Header("Melee Attack")]
    [SerializeField] private float meleeRange = 2f;
    [SerializeField] private int meleeDamage = 10;
    [SerializeField] private float meleeCooldown = 1.5f;
    private float lastMeleeTime = -Mathf.Infinity;
 
    [Header("Ranged Strafing")]
    [Tooltip("How fast the enemy sidesteps while holding position and shooting")]
    [SerializeField] private float strafeSpeed = 3f;
    [Tooltip("How often a new random strafe direction is picked")]
    [SerializeField] private float strafeChangeInterval = 1.5f;
    private float nextStrafeSwitchTime;
    private float lastStrafeTickTime = -1f;
    private int strafeDirection = 1;
 
    [Header("Knockback")]
    [Tooltip("How long physics fully controls the enemy after an explosion before the NavMeshAgent takes back over")]
    [SerializeField] private float knockbackRecoveryDelay = 0.4f;
 
    private Rigidbody rb;
    private Coroutine knockbackRoutine;
    private Coroutine shootRoutine;
 
    public Enemy Enemy { get; private set; }


    void Awake()
    {
        SetUp(new Enemy(enemyBase));
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

        agent.updateRotation = false;
    }

    public void Engage(Transform target)
    {
        if(agent == null || !agent.enabled)
        {
            return;
        }

        agent.updateRotation = false;

        following = true;
        float distance = Vector3.Distance(transform.position, target.position);

        if(combatType == CombatType.Melee)
        {
            EngageMelee(target, distance);
        }
        else
        {
            EngageRanged(target, distance);
        }
    }

    public void Disengage()
    {
        following = false;
        StopShooting();
    }

    void EngageMelee(Transform target, float distance)
    {
        FollowTarget(target.position);

        if(distance <= meleeRange && Time.time >= lastMeleeTime + meleeCooldown)
        {
            PerformMeleeAttack(target);
        }
    }

    void PerformMeleeAttack(Transform target)
    {
        lastMeleeTime = Time.time;
        //anim here

        if(Vector3.Distance(transform.position, target.position) <= meleeRange)
        {
            Debug.Log("Player hit by melee attack");
            //health logic here
        }
    }

    void EngageRanged(Transform target, float distance)
    {
        if(shootRoutine == null)
        {
            shootRoutine = StartCoroutine(ShootTarget(target));
        }

        if(distance > Enemy.StopDistance)
        {
            FollowTarget(target.position);
        }
        else if (distance < Enemy.MinRange)
        {
            RetreatFrom(target);
        }
        else
        {
           HandleStrafing(target); 
           transform.LookAt(target.position);
        }
    }

    void HandleStrafing(Transform target)
    {
        float now = Time.time;
        float dt = lastStrafeTickTime < 0f ? 0f : now - lastStrafeTickTime;
        lastStrafeTickTime = now;

        // Pick a new direction periodically
        if (now >= nextStrafeSwitchTime)
        {
            nextStrafeSwitchTime = now + strafeChangeInterval;
            strafeDirection = Random.value < 0.5f ? -1 : 1;
        }

        // Face the player
        Vector3 lookDirection = target.position - transform.position;
        lookDirection.y = 0f;

        if (lookDirection.sqrMagnitude > 0.001f)
        {
            transform.rotation = Quaternion.LookRotation(lookDirection);
        }

        // Move sideways relative to the player
        Vector3 toTarget = (transform.position - target.position).normalized;
        Vector3 strafeDir = Vector3.Cross(toTarget, Vector3.up) * strafeDirection;

        Vector3 desiredPosition =
            transform.position + strafeDir * strafeSpeed * dt;

        if (NavMesh.SamplePosition(
            desiredPosition,
            out NavMeshHit navHit,
            2f,
            NavMesh.AllAreas))
        {
            agent.stoppingDistance = 0f;
            agent.SetDestination(navHit.position);
        }
    }

    void StopShooting()
    {
        if(shootRoutine != null)
        {
            StopCoroutine(shootRoutine);
            shootRoutine = null;
        }
    }

    public Vector3 GetStartPoint()
    {
        return startPoint;
    }

    public void GoToStartPoint()
    {
        if(agent.enabled == true)
        {
            agent.stoppingDistance = 0;
            agent.SetDestination(startPoint);
            agent.updateRotation = true;
        }
        StopShooting();
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

    public void RetreatFrom(Transform target)
    {
        if (!agent.enabled)
        {
            return;
        }

        Vector3 awayFromTarget = transform.position - target.position;
        awayFromTarget.y = 0f;
        awayFromTarget.Normalize();

        Vector3 retreatPosition = transform.position + awayFromTarget * 5f;

        if (NavMesh.SamplePosition(retreatPosition, out NavMeshHit navHit, 2f, NavMesh.AllAreas))
        {
            agent.stoppingDistance = 0f;
            agent.SetDestination(navHit.position);
        }

        // Continue facing the player while retreating
        Vector3 lookDirection = target.position - transform.position;
        lookDirection.y = 0f;

        if (lookDirection.sqrMagnitude > 0.001f)
        {
            transform.rotation = Quaternion.LookRotation(lookDirection);
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
            StopShooting();
            Destroy(gameObject);
            //Debug.Log("Enemy killed");
        }
    }

    public void ApplyKnockback(Vector3 explosionOrigin, float explosionForce, float explosionRadius, float upwardsModifier = 1f)
    {
        //Debug.Log("ApplyKnockback called");
        if (rb == null)
        {
            Debug.Log("No rigidbody!");
            return;
        }

        if (knockbackRoutine != null)
        {
            StopCoroutine(knockbackRoutine);
        }

        //Debug.Log("Starting knockback coroutine");
        knockbackRoutine = StartCoroutine(KnockbackRoutine(explosionOrigin, explosionForce, explosionRadius, upwardsModifier));
    }

    private IEnumerator KnockbackRoutine(Vector3 origin, float force, float radius, float upwardsModifier)
    {
        //Debug.Log("Coroutine started");
        // Hand full control to physics for the duration of the knockback.
        agent.enabled = false;
        rb.isKinematic = false;
        //Debug.Log("kinematic disabled");

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
