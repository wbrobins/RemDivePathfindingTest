using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class GroundMovement : EnemyMovement
{
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

    public override bool IsJumping => jumpRoutine != null;

    private Vector3 startPoint;

    void Update()
    {
        if (jumpRoutine == null && agent.enabled && agent.isOnNavMesh && agent.isOnOffMeshLink)
        {
            jumpRoutine = StartCoroutine(JumpAcrossLink());
        }
    }

    public override void Initialize(EnemyBehavior enemy)
    {
        base.Initialize(enemy);

        startPoint = transform.position;

        agent.autoTraverseOffMeshLink = false;
        agent.updateRotation = false;
        agent.speed = enemy.Enemy.Speed;
        strafeSpeed = enemy.Enemy.StrafeSpeed;
        strafeChangeInterval = enemy.Enemy.StrafeChangeInterval;
    }


    public override void MoveTo(Vector3 position)
    {
        if(agent.enabled == true && agent.isOnNavMesh && !IsJumping)
        {
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

        // Continue facing the player while retreating
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

        if (IsJumping) return;

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
            returnToStartAfterJump = true;   // Disengage only calls this once, so remember it
            return;
        }

        agent.stoppingDistance = 0f;
        agent.updateRotation = true;
        agent.SetDestination(startPoint);
    }

    public override void StopMovement()
    {
        if(agent.enabled && agent.isOnNavMesh)
        {
            agent.ResetPath();
        }
    }

    public override void OnKnockbackEnd()
    {
        if (!agent.enabled)
        {
            agent.enabled = true;
        }

        agent.Warp(transform.position);
    }

    private IEnumerator JumpAcrossLink()
    {
        OffMeshLinkData link = agent.currentOffMeshLinkData;
        Vector3 start = transform.position;
        Vector3 end = link.endPos + Vector3.up * agent.baseOffset;

        float duration = Mathf.Max(0.1f, Vector3.Distance(start, end) / jumpSpeed);
        float t = 0f;

        while (t < 1f)
        {
            // knockback (or anything else) disabled the agent mid-jump: bail out
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
