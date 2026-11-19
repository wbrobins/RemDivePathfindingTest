using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyBehavior : MonoBehaviour
{
    public static readonly HashSet<EnemyBehavior> Active = new HashSet<EnemyBehavior>();
    public static event Action<EnemyBehavior> OnEnemyRemoved;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics()
    {
        Active.Clear();
        OnEnemyRemoved = null;
    }

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
    [SerializeField] private HealthBar hpBar;
    [SerializeField] private float speed;
    [SerializeField] private LayerMask projectileMask;
    [SerializeField] private float projectileLifetime = 5f;
 
    [Header("Combat Type")]
    [SerializeField] private CombatType combatType = CombatType.Ranged;

    [Header("Ranged Attack Visuals")]
    [SerializeField] private Transform chargeOrb;
    [SerializeField] private float windupTime = 3f;
    [SerializeField] private float orbMaxScale = 0.4f;

 
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
    [SerializeField] private GameObject model;

    [Tooltip("If true, this enemy continuously engages the player regardless of detection range")]
    public bool aggroOnSpawn;

    private bool countedAsCombatant;
    private bool isDead;
 
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

    void SetCombatant(bool value)
    {
        if (countedAsCombatant == value) return;
        countedAsCombatant = value;
        Debug.Log($"{name} combatant -> {value} (count will be changing)", this);

        if (value) MusicManager.Instance?.RegisterCombatant();
        else MusicManager.Instance?.UnregisterCombatant();
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

        hpBar.SetHealth(Enemy.HP, Enemy.MaxHP);

        //PH
        HideCharge();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            showLoS = !showLoS;
        }

        if (aggroOnSpawn && player != null && knockbackRoutine == null)
        {
            Engage(player.transform, HasLineOfSight(player.transform));
        }
    }

    private bool HasLineOfSight(Transform target)
    {
        Vector3 from = transform.position + Vector3.up;
        Vector3 to = target.position + Vector3.up;
        return !Physics.Linecast(from, to, LayerMask.GetMask("Cover"));
    }
    
    public CombatType GetCombatType()
    {
        return combatType;
    }

    public void Engage(Transform target, bool canSeeTarget)
    {
        if (isDead) return;

        currentTarget = target;
        following = true;
        SetCombatant(true);

        float distance = Vector3.Distance(transform.position, target.position);

        if (combatType == CombatType.Melee)
        {
            EngageMelee(target, distance, canSeeTarget);
        }
        else
        {
            EngageRanged(target, distance, canSeeTarget);
        }
    }

    public void Disengage()
    {
        following = false;
        SetCombatant(false);
        currentTarget = null;
        StopShooting();
        movement.GoToStartPoint();
    }

    void EngageMelee(Transform target, float distance, bool canSeeTarget)
    {
        FaceTarget(target);

        if (distance > meleeRange)
        {
            movement.MoveTo(target.position);
        }

        if (canSeeTarget && distance <= meleeRange && Time.time >= lastMeleeTime + meleeCooldown)
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

    void EngageRanged(Transform target, float distance, bool canSeeTarget)
    {
        FaceTarget(target);

        if (!canSeeTarget)
        {
            StopShooting();
            movement.MoveTo(target.position);
            return;
        }

        if (shootRoutine == null)
        {
            shootRoutine = StartCoroutine(ShootTarget());
        }

        if (distance > Enemy.StopDistance)
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
        }
    }

    private void FaceTarget(Transform target)
    {
        Vector3 lookDirection = target.position - transform.position;
        lookDirection.y = 0f;

        if (lookDirection.sqrMagnitude > 0.001f)
        {
            transform.rotation = Quaternion.LookRotation(lookDirection);
        }
    }

    void StopShooting()
    {
        if(shootRoutine != null)
        {
            StopCoroutine(shootRoutine);
            shootRoutine = null;
        }
        HideCharge();
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
            //subtract the windup so total time between shots still equals ShootDelay
            yield return new WaitForSeconds(Mathf.Max(0f, Enemy.ShootDelay - windupTime));

            for (float t = 0f; t < windupTime; t += Time.deltaTime)
            {
                if (!following || currentTarget == null)
                {
                    HideCharge();
                    shootRoutine = null;
                    yield break;
                }
                SetCharge(t / windupTime);
                yield return null;
            }
            HideCharge();

            if (!following || currentTarget == null)
            {
                shootRoutine = null;
                yield break;
            }

            Vector3 aimPoint = currentTarget.position + Vector3.up * 1.0f;
            Vector3 direction = (aimPoint - firePoint.position).normalized;

            GameObject projectileInstance = Instantiate(projectilePrefab, firePoint.position, Quaternion.LookRotation(direction));
            Projectile projectile = projectileInstance.GetComponent<Projectile>();
            projectile.Init(Team.Enemy, gameObject, Enemy.RangedDamage, direction,
                            Enemy.ProjectileSpeed, 0f, projectileLifetime, projectileMask);
        }

        shootRoutine = null;
    }

    public void TakeDamage(int damage, GameObject source)
    {
        if (isDead) return;

        Enemy.Hit(damage);
        model.GetComponent<DamageFlash>()?.TriggerFlash();
        hpBar.SetHealth(Enemy.HP, Enemy.MaxHP);
        Debug.Log("Damage done: " + damage + " from " + source.name);

        if (source.CompareTag("Player"))
        {
            Engage(source.transform, true);
        }

        if (Enemy.HP == 0)
        {
            isDead = true;
            SetCombatant(false);
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
        agent.enabled = false;
        rb.isKinematic = false;
        movement.OnKnockbackStart();  

        rb.linearVelocity = Vector3.zero;
        rb.AddExplosionForce(force, origin, radius, upwardsModifier, ForceMode.Impulse);

        yield return new WaitForSeconds(knockbackRecoveryDelay);

        rb.linearVelocity = Vector3.zero;

        if (this == null || !gameObject.activeInHierarchy)
        {
            yield break;
        }
        
        agent.enabled = true;
        yield return new WaitUntil(() => agent.isOnNavMesh); 
        agent.Warp(transform.position);
        rb.isKinematic = true;
        
        Debug.Log("Knockback successfully ended");
        movement.OnKnockbackEnd();  
        GoToStartPoint();

        knockbackRoutine = null;
    }

    void OnEnable()  => Active.Add(this);

    void OnDisable()
    {
        SetCombatant(false);
        Active.Remove(this);
        OnEnemyRemoved?.Invoke(this);
    }

    //PLACEHOLDER ANIM FUNCTIONS
    void SetCharge(float k)
    {
        if (chargeOrb == null) return;
        chargeOrb.gameObject.SetActive(k > 0f);
        chargeOrb.localScale = Vector3.one * orbMaxScale * Mathf.Clamp01(k);
    }

    void HideCharge()
    {
        if (chargeOrb != null) chargeOrb.gameObject.SetActive(false);
    }
}