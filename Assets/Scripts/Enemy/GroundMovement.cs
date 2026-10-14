using System.Collections;
using UnityEditor.ShaderGraph;
using UnityEngine;
using UnityEngine.AI;

public class GroundMovement : EnemyMovement
{
    public enum MoveState { Idle, Chase, Strafe, Retreat, ReturnHome, Jump, Knockback }

    [SerializeField] private NavMeshAgent agent;

    [Header("Ranged Strafing")]
    [Tooltip("How fast the enemy sidesteps while holding position and shooting")]
    [SerializeField] private float strafeSpeed;
    [Tooltip("How often a new random strafe direction is picked")]
    [SerializeField] private float strafeChangeInterval;
    private float nextStrafeSwitchTime;
    private float lastStrafeTickTime = -1f;
    private int strafeDirection = 1;

    [Header("Jumping")]
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private float jumpSpeed = 6f;   // horizontal speed during the jump
    private Coroutine jumpRoutine;
    private bool returnToStartAfterJump;

    [Header("Animation")]
    [Tooltip("Animator with a 2D Freeform Directional blend tree driven by MoveX / MoveY. Apply Root Motion must be off.")]
    [SerializeField] private Animator animator;
    [Tooltip("Smoothing time for the blend tree parameters")]
    [SerializeField] private float animDamp = 0.1f;
    [Tooltip("Degrees per second when turning toward the travel direction while returning to the start point")]
    [SerializeField] private float turnSpeed = 540f;

    private static readonly int MoveXHash = Animator.StringToHash("MoveX");
    private static readonly int MoveYHash = Animator.StringToHash("MoveY");
    private static readonly int KnockedHash = Animator.StringToHash("Knocked");

    public MoveState State { get; private set; } = MoveState.Idle;

    public override bool IsJumping => jumpRoutine != null;

    private Vector3 startPoint;
    private Vector3 lastPosition;
    private Vector3 worldVelocity;   // measured from real movement, so it covers agent, jump and knockback
    private float idleTimer;

    void Update()
    {
        Vector3 delta = transform.position - lastPosition;
        lastPosition = transform.position;
        worldVelocity = Time.deltaTime > 0f ? delta / Time.deltaTime : Vector3.zero;
        worldVelocity.y = 0f;

        if (jumpRoutine == null && agent.enabled && agent.isOnNavMesh && agent.isOnOffMeshLink)
        {
            jumpRoutine = StartCoroutine(JumpAcrossLink());
        }

        UpdateState();
        UpdateFacing();
    }

    void LateUpdate()
    {
        UpdateAnimator();
    }

    public override void Initialize(EnemyBehavior enemy)
    {
        base.Initialize(enemy);

        startPoint = transform.position;
        lastPosition = transform.position;

        agent.autoTraverseOffMeshLink = false;
        agent.updateRotation = false;   
        agent.speed = enemy.Enemy.Speed;
        strafeSpeed = enemy.Enemy.StrafeSpeed;
        strafeChangeInterval = enemy.Enemy.StrafeChangeInterval;
    }

   

    private void SetState(MoveState next)
    {
        if (State == MoveState.Knockback) return;                 
        if (IsJumping && next != MoveState.Jump) return;          
        State = next;
    }

    private void UpdateState()
    {
        if (State == MoveState.Knockback) return;

        if (IsJumping)
        {
            SetState(MoveState.Jump);
            return;
        }

        if (State == MoveState.Jump)
        {
            State = MoveState.Idle;   // just landed; the next command moves us on
        }

        if (worldVelocity.sqrMagnitude < 0.01f)
        {
            idleTimer += Time.deltaTime;
            if (idleTimer > 0.15f)
            {
                SetState(MoveState.Idle);
            }
        }
        else
        {
            idleTimer = 0f;
        }
    }

    private void UpdateFacing()
    {
        if (State != MoveState.ReturnHome) return;

        Vector3 v = agent.velocity;
        v.y = 0f;
        if (v.sqrMagnitude < 0.05f) return;

        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            Quaternion.LookRotation(v),
            turnSpeed * Time.deltaTime);
    }

    private void UpdateAnimator()
    {
        if (animator == null) return;

        animator.SetBool(KnockedHash, State == MoveState.Knockback);

        Vector2 move = Vector2.zero;

        // Jump / Knockback zero the locomotion blend tree. Trigger their own animations here later.
        if (State != MoveState.Jump && State != MoveState.Knockback)
        {
            Vector3 local = transform.InverseTransformDirection(worldVelocity);
            float refSpeed = State == MoveState.Strafe ? strafeSpeed : agent.speed;

            move = new Vector2(local.x, local.z) / Mathf.Max(0.01f, refSpeed);
            move = Vector2.ClampMagnitude(move, 1f);
        }

        animator.SetFloat(MoveXHash, move.x, animDamp, Time.deltaTime);
        animator.SetFloat(MoveYHash, move.y, animDamp, Time.deltaTime);
    }

    public override void MoveTo(Vector3 position)
    {
        if(agent.enabled == true && agent.isOnNavMesh && !IsJumping)
        {
            SetState(MoveState.Chase);

            if(enemy.GetCombatType() == EnemyBehavior.CombatType.Ranged)
            {
                agent.stoppingDistance = enemy.Enemy.StopDistance;
            }
            else if (enemy.GetCombatType() == EnemyBehavior.CombatType.Melee)
            {
                agent.stoppingDistance = enemy.Enemy.MeleeRange;
            }
            
            agent.SetDestination(position);
        }
    }

    public override void RetreatFrom(Transform target)
    {
        if (!agent.enabled || !agent.isOnNavMesh || IsJumping)
        {
            return;
        }

        SetState(MoveState.Retreat);

        Vector3 awayFromTarget = transform.position - target.position;
        awayFromTarget.y = 0f;

        if (awayFromTarget.sqrMagnitude < 0.001f){return;}

        awayFromTarget.Normalize();

        Vector3 retreatPosition = transform.position + awayFromTarget * 5f;

        if (NavMesh.SamplePosition(retreatPosition, out NavMeshHit navHit, 2f, NavMesh.AllAreas))
        {
            agent.stoppingDistance = 0f;
            agent.SetDestination(navHit.position);
        }

        //continue facing the player while retreating
        Vector3 lookDirection = target.position - transform.position;
        lookDirection.y = 0f;

        if (lookDirection.sqrMagnitude > 0.001f)
        {
            transform.rotation = Quaternion.LookRotation(lookDirection);
        }
    }

    public override void Strafe(Transform target)
    {
        float now = Time.time;
        float dt = lastStrafeTickTime < 0f ? 0f : now - lastStrafeTickTime;
        lastStrafeTickTime = now;

        
        if (IsJumping || !agent.enabled) return;

        SetState(MoveState.Strafe);

        
        if (now >= nextStrafeSwitchTime)
        {
            nextStrafeSwitchTime = now + strafeChangeInterval;
            strafeDirection = Random.value < 0.5f ? -1 : 1;
        }

        
        Vector3 lookDirection = target.position - transform.position;
        lookDirection.y = 0f;

        if (lookDirection.sqrMagnitude > 0.001f)
        {
            transform.rotation = Quaternion.LookRotation(lookDirection);
        }

        
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
            if(agent.isOnNavMesh)
            {
                agent.SetDestination(navHit.position);
            }
        }
    }

    public override void GoToStartPoint()
    {
        if(!agent.enabled || !agent.isOnNavMesh)
        {
            return;
        }

        if (IsJumping)
        {
            returnToStartAfterJump = true;  
            return;
        }

        SetState(MoveState.ReturnHome);

        agent.stoppingDistance = 0f;
        agent.SetDestination(startPoint);
    }

    public override void StopMovement()
    {
        if(agent.enabled && agent.isOnNavMesh)
        {
            agent.ResetPath();
        }
    }

    public override void OnKnockbackStart()
    {
        Debug.Log("Knockback started");
        State = MoveState.Knockback;
    }

    public override void OnKnockbackEnd()
    {
        if (agent.isOnNavMesh)
        {
            agent.Warp(transform.position);
        }
        
        Debug.Log("Knockback ended");
        State = MoveState.Idle;
        idleTimer = 0f;
    }

   

    private IEnumerator JumpAcrossLink()
    {
        animator.SetTrigger("Jump");
        OffMeshLinkData link = agent.currentOffMeshLinkData;
        Vector3 start = transform.position;
        Vector3 end = link.endPos + Vector3.up * agent.baseOffset;

        float duration = Mathf.Max(0.1f, Vector3.Distance(start, end) / jumpSpeed);
        float t = 0f;

        while (t < 1f)
        {
            //knockback disabled the agent mid-jump: bail out
            if (!agent.enabled || !agent.isOnOffMeshLink)
            {
                returnToStartAfterJump = false;
                jumpRoutine = null;
                yield break;
            }

            t += Time.deltaTime / duration;
            float arc = 4f * jumpHeight * t * (1f - t);
            transform.position = Vector3.Lerp(start, end, t) + Vector3.up * arc;
            yield return null;
        }
        
        transform.position = end;
        agent.CompleteOffMeshLink();
        jumpRoutine = null;

        if (returnToStartAfterJump)
        {
            returnToStartAfterJump = false;
            GoToStartPoint();
        }
    }

}