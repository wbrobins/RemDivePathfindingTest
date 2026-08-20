using UnityEngine;

public class AerialMovement : EnemyMovement
{
    [Header("Aerial Controls")]
    [SerializeField] private float aerialSpeed = 6f;
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private float detectionRadius = 1.5f;
    [SerializeField] private float obstacleAvoidanceDistance = 3f;
    [SerializeField] private LayerMask obstacleMask;

    [Header("Aerial Strafing")]
    [SerializeField] private float strafeSpeed = 4f;
    [SerializeField] private float strafeChangeInterval = 2f;

    private float nextStrafeSwitchTime;
    private int strafeDirection = 1;

    private Vector3 startPoint;

    public override void Initialize(EnemyBehavior enemy)
    {
        base.Initialize(enemy);

        startPoint = transform.position;
    }

    public override void MoveTo(Vector3 position)
    {
        Vector3 toTarget = position - transform.position;

        if(toTarget.sqrMagnitude < .01f)
        {
            return;
        }

        Vector3 moveDirection = toTarget.normalized;

        if(Physics.SphereCast(transform.position, detectionRadius, moveDirection, out RaycastHit hit, obstacleAvoidanceDistance, obstacleMask, QueryTriggerInteraction.Ignore))
        {
            Vector3 avoidanceDirection = hit.normal;

            moveDirection = (moveDirection + avoidanceDirection).normalized;
        }

        transform.position += moveDirection * aerialSpeed * Time.deltaTime;
        
        FaceMovementDirection(moveDirection);
    }

    public override void RetreatFrom(Transform target)
    {
        Vector3 awayFromTarget = transform.position - target.position;

        if(awayFromTarget.sqrMagnitude < .001f){return;}

        awayFromTarget.Normalize();

        Vector3 retreatPosition = transform.position + awayFromTarget * 5f;

        MoveTo(retreatPosition);

        
    }

    public override void Strafe(Transform target)
    {
        if(target == null) {return;}

        if(Time.time >= nextStrafeSwitchTime)
        {
            nextStrafeSwitchTime = Time.time + strafeChangeInterval;

            strafeDirection = Random.value < .5f ? -1 : 1;
        }

        Vector3 toTarget = target.position - transform.position;

        if(toTarget.sqrMagnitude < .001f){return;}

        float distance = toTarget.magnitude;

        Vector3 directionToTarget = toTarget.normalized;

        Vector3 strafeDirectionVector = Vector3.Cross(directionToTarget, Vector3.up) * strafeDirection;

        Vector3 distanceCorrection = Vector3.zero;

        if(distance > enemy.Enemy.StopDistance)
        {
            distanceCorrection = directionToTarget;
        }
        else if (distance < enemy.Enemy.MinRange)
        {
            distanceCorrection = -directionToTarget;
        }

        Vector3 desiredDirection = strafeDirectionVector + distanceCorrection;

        if (desiredDirection.sqrMagnitude < .001f) { return; }

        desiredDirection.Normalize();

        Vector3 desiredPosition = transform.position + desiredDirection * strafeSpeed;

        MoveTo(desiredPosition);

        FaceTarget(target);
    }

    public override void GoToStartPoint()
    {
        MoveTo(startPoint);
    }

    public override void StopMovement()
    {
        
    }
    
    private void FaceTarget(Transform target)
    {
        Vector3 lookDirection = target.position - transform.position;

        if(lookDirection.sqrMagnitude < .001f){return;}

        Quaternion targetRotation = Quaternion.LookRotation(lookDirection);

        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
    }

    private void FaceMovementDirection(Vector3 direction)
    {
        if(direction.sqrMagnitude < .001f) {return;}

        Quaternion targetRotation = Quaternion.LookRotation(direction);

        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        Gizmos.DrawRay(transform.position, transform.forward * obstacleAvoidanceDistance);
    }
}
