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
    public bool showLoS = true;
    private Transform currentTarget;
    private Vector3 startPoint;
    private Transform firePoint;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private EnemyBase enemyBase;
    [SerializeField] private float speed;
 
    [Header("Combat Type")]
    [SerializeField] private CombatType combatType = CombatType.Ranged;

 
    [Header("Melee Attack")]
    [SerializeField] private float meleeRange;
    [SerializeField] private int meleeDamage;
    [SerializeField] private float meleeCooldown;
    private float lastMeleeTime = -Mathf.Infinity;
 
    [Header("Knockback")]
    [Tooltip("How long physics fully controls the enemy after an explosion before the NavMeshAgent takes back over")]
    [SerializeField] private float knockbackRecoveryDelay;
 
    private Rigidbody rb;
    private Coroutine knockbackRoutine;
    private Coroutine shootRoutine;

    [SerializeField] private EnemyMovement movement;
 
    public Enemy Enemy { get; private set; }


    void Awake()
    {
        SetUp(new Enemy(enemyBase));
        rb = GetComponent<Rigidbody>();
    }

    public void SetUp(Enemy enemy)
    {
        Enemy = enemy;
        //agent.stoppingDistance = Enemy.StopDistance;
        meleeRange = enemy.MeleeRange;
        meleeDamage = enemy.MeleeDamage;
        meleeCooldown = enemy.MeleeCooldown;
        knockbackRecoveryDelay = enemy.KnockbackRecoveryDelay;
        speed = enemy.Speed;
        combatType = (CombatType)enemy.ECombatType;
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

        movement.Initialize(this);

        if(movement is AerialMovement)
        {
            rb.useGravity = false;
            if(agent != null)
            {
                agent.enabled = false;
            }
        }
        else
        {
            rb.useGravity = true;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            if (showLoS)
            {
                showLoS = false;
            }
            else
            {
                showLoS = true;
            }
        }
    }
    public CombatType GetCombatType()
    {
        return combatType;
    }

    public void Engage(Transform target)
    {
        //if (agent == null || !agent.enabled)
        //{
        //    return;
        //}

        //agent.speed = speed;
        currentTarget = target;
        following = true;

        float distance = Vector3.Distance(transform.position, target.position);

        if (combatType == CombatType.Melee)
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
        currentTarget = null;
        if(movement is GroundMovement)
        {
            agent.updateRotation = true; 
        }
        StopShooting();
        movement.GoToStartPoint();
    }

    void EngageMelee(Transform target, float distance)
    {
        if (distance > meleeRange)
        {
            movement.MoveTo(target.position);
        }

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
            PlayerController player = target.GetComponent<PlayerController>();
            player.TakeDamage(Enemy.MeleeDamage);
        }
    }

    void EngageRanged(Transform target, float distance)
    {
        if(movement is GroundMovement)
        {
            agent.updateRotation = false;
        }
        if(shootRoutine == null)
        {
            shootRoutine = StartCoroutine(ShootTarget());
        }

        if(distance > Enemy.StopDistance)
        {
            movement.MoveTo(target.position);
        }
        else if (distance < Enemy.MinRange)
        {
            movement.RetreatFrom(target);
        }
        else
        {
           movement.Strafe(target);
           transform.LookAt(target.position);
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
        StopShooting();
        movement.GoToStartPoint();
    }

    public void FollowTarget(Vector3 position)
    {
        movement.MoveTo(position);
    }

    public void RetreatFrom(Transform target)
    {
        movement.RetreatFrom(target);
    }


    public IEnumerator ShootTarget()
    {
        while (following && currentTarget != null)
        {
            yield return new WaitForSeconds(Enemy.ShootDelay);

            if (!following || currentTarget == null)
            {
                yield break;
            }

            GameObject projectileInstance = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
            Projectile projectile = projectileInstance.GetComponent<Projectile>();
            projectile.SetDamage(Enemy.RangedDamage);

            Vector3 aimPoint = currentTarget.position + Vector3.up * 1.0f;
            Vector3 direction = (aimPoint - firePoint.position).normalized;

            Rigidbody projectileRb = projectileInstance.GetComponent<Rigidbody>();
            projectileRb.linearVelocity = direction * Enemy.ProjectileSpeed;
        }

        shootRoutine = null;
    }

    public void TakeDamage(int damage, GameObject source)
    {
        Enemy.Hit(damage);
        Debug.Log("Damage done: " + damage + " from " + source.name);

        if (source.CompareTag("Player"))
        {
            Engage(source.transform);
        }

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

        yield return new WaitUntil(() => agent.isOnNavMesh);  // dont move until agent is back on navmesh
        agent.enabled = true;
        rb.isKinematic = true;
        agent.Warp(transform.position); // resync the agent's internal state to the post-knockback position
        GoToStartPoint();

        knockbackRoutine = null;
    }
}
