using UnityEngine;

public class AerialMovement : EnemyMovement
{
    [Header("Movement")]
    [SerializeField] private float aerialSpeed = 6f;
    [SerializeField] private float acceleration = 8f;
    [SerializeField] private float rotationSpeed = 5f;

    [Header("Obstacle Avoidance")]
    [SerializeField] private float detectionRadius = 1.5f;
    [SerializeField] private float obstacleAvoidanceDistance = 3f;
    [SerializeField] private LayerMask obstacleMask;
    [SerializeField] private float avoidanceStrength = 2f;

    [Header("Strafing")]
    [SerializeField] private float strafeSpeed = 4f;
    [SerializeField] private float strafeChangeInterval = 2f;

    private Vector3 desiredDirection;
    private Vector3 currentVelocity;

    private Transform currentTarget;

    private bool strafing;
    private int strafeDirection = 1;
    private float nextStrafeSwitchTime;

    private Vector3 startPoint;

    public override void Initialize(EnemyBehavior enemy)
    {
        base.Initialize(enemy);

        startPoint = transform.position;
    }

    private void Update()
    {
        if (desiredDirection.sqrMagnitude < 0.001f)
        {
            return;
        }

        Vector3 finalDirection =
            GetAvoidanceDirection(desiredDirection);

        Vector3 targetVelocity =
            finalDirection * aerialSpeed;

        currentVelocity = Vector3.MoveTowards(
            currentVelocity,
            targetVelocity,
            acceleration * Time.deltaTime
        );

        transform.position +=
            currentVelocity * Time.deltaTime;

        if (strafing && currentTarget != null)
        {
            FaceTarget(currentTarget);
        }
        else
        {
            FaceMovementDirection(currentVelocity);
        }
    }

    public override void MoveTo(Vector3 position)
    {
        Vector3 direction =
            position - transform.position;

        if (direction.sqrMagnitude < 0.01f)
        {
            desiredDirection = Vector3.zero;
            return;
        }

        strafing = false;
        currentTarget = null;

        desiredDirection = direction.normalized;
    }

    public override void RetreatFrom(Transform target)
    {
        if (target == null)
        {
            return;
        }

        Vector3 direction =
            transform.position - target.position;

        if (direction.sqrMagnitude < 0.001f)
        {
            return;
        }

        strafing = false;
        currentTarget = null;

        desiredDirection = direction.normalized;
    }

    public override void Strafe(Transform target)
    {
        if (target == null)
        {
            return;
        }

        currentTarget = target;
        strafing = true;

        if (Time.time >= nextStrafeSwitchTime)
        {
            nextStrafeSwitchTime =
                Time.time + strafeChangeInterval;

            strafeDirection =
                Random.value < 0.5f ? -1 : 1;
        }

        Vector3 toTarget =
            target.position - transform.position;

        if (toTarget.sqrMagnitude < 0.001f)
        {
            desiredDirection = Vector3.zero;
            return;
        }

        float distance = toTarget.magnitude;

        Vector3 directionToTarget =
            toTarget.normalized;

        Vector3 strafe =
            Vector3.Cross(
                directionToTarget,
                Vector3.up
            ) * strafeDirection;

        Vector3 distanceCorrection = Vector3.zero;

        if (distance > enemy.Enemy.StopDistance)
        {
            distanceCorrection = directionToTarget;
        }
        else if (distance < enemy.Enemy.MinRange)
        {
            distanceCorrection = -directionToTarget;
        }

        Vector3 direction =
            strafe +
            distanceCorrection;

        if (direction.sqrMagnitude > 0.001f)
        {
            desiredDirection =
                direction.normalized;
        }
    }

    private Vector3 GetAvoidanceDirection(Vector3 movementDirection)
    {
        if (!Physics.SphereCast(
            transform.position,
            detectionRadius,
            movementDirection,
            out RaycastHit hit,
            obstacleAvoidanceDistance,
            obstacleMask,
            QueryTriggerInteraction.Ignore))
        {
            return movementDirection;
        }

        Vector3 avoidance =
            hit.normal * avoidanceStrength;

        Vector3 result =
            movementDirection + avoidance;

        if (result.sqrMagnitude < 0.001f)
        {
            return hit.normal;
        }

        return result.normalized;
    }

    private void FaceTarget(Transform target)
    {
        Vector3 direction =
            target.position - transform.position;

        if (direction.sqrMagnitude < 0.001f)
        {
            return;
        }

        Quaternion targetRotation =
            Quaternion.LookRotation(direction);

        transform.rotation =
            Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
    }

    private void FaceMovementDirection(Vector3 direction)
    {
        if (direction.sqrMagnitude < 0.001f)
        {
            return;
        }

        Quaternion targetRotation =
            Quaternion.LookRotation(direction);

        transform.rotation =
            Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
    }

    public override void GoToStartPoint()
    {
        MoveTo(startPoint);
    }

    public override void StopMovement()
    {
        desiredDirection = Vector3.zero;
        currentVelocity = Vector3.zero;
        currentTarget = null;
        strafing = false;
    }

    public override void OnKnockbackEnd()
    {
        currentVelocity = Vector3.zero;
        desiredDirection = Vector3.zero;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(
            transform.position,
            detectionRadius
        );

        Gizmos.DrawRay(
            transform.position,
            transform.forward *
            obstacleAvoidanceDistance
        );
    }
}