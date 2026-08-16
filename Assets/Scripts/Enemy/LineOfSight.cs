using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineOfSight : MonoBehaviour
{
    [SerializeField] private GameObject target;
    [SerializeField] private EnemyBehavior parent;
    [SerializeField] private float losAngle = 70.0f;
    [SerializeField] private float followDelay = 3f;
    [SerializeField] private float goHomeDistance = 30f;


    private Collider player_collider;
    private SphereCollider detection_collider;
    private Coroutine detect_player;
    private Coroutine lose_target;

    private readonly HashSet<Collider> playerCollidersInRange = new HashSet<Collider>();

    void Awake()
    {
        detection_collider = GetComponent<SphereCollider>();
    }

    void Update()
    {
        if (parent.showLoS)
        {
           DrawLineOfSight(); 
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag != "Player")
        {
            return;
        }

        bool wasEmpty = playerCollidersInRange.Count == 0;
        playerCollidersInRange.Add(other);

        Collider rootCollider = other.transform.root.GetComponent<Collider>();
        player_collider = rootCollider != null ? rootCollider : other;
        target = other.transform.root.gameObject;

        if (!wasEmpty)
        {
            return;
        }

        if (lose_target != null)
        {
            StopCoroutine(lose_target);
            lose_target = null;
        }

        if (detect_player == null)
        {
            detect_player = StartCoroutine(DetectPlayer());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag != "Player")
        {
            return;
        }

        playerCollidersInRange.Remove(other);

        if (playerCollidersInRange.Count > 0)
        {
            return;
        }

        lose_target = StartCoroutine(LoseTargetDelay());
    }

    IEnumerator DetectPlayer()
    {
        while (true)
        {
            yield return new WaitForSeconds(.1f);

            Vector3[] points = GetBoundingPoints(player_collider.bounds);

            int points_hidden = 0;

            foreach (Vector3 point in points)
            {
                Vector3 target_direction = point - transform.position;
                float target_distance = target_direction.magnitude;
                float target_angle = Vector3.Angle(target_direction.normalized, transform.forward);
                bool covered = IsPointCovered(target_direction.normalized, target_distance);

                if (parent.showLoS)
                {     
                    Color ray_color = (covered || target_angle > losAngle) ? Color.red : Color.green;

                    Debug.DrawRay(transform.position, target_direction, ray_color, 1.0f);
                }

                if (covered || target_angle > losAngle)
                {
                    ++points_hidden;
                }
            }

            if (points_hidden >= points.Length)
            {
                //Debug.Log("Player hidden");
                parent.Disengage();
            }
            else
            {
                //Debug.Log("Player visible");
                float distance = (parent.GetStartPoint() - parent.transform.position).magnitude;

                bool tooFar = distance > goHomeDistance;

                if (!tooFar) // engage (chase/strafe/melee/shoot - EnemyBehavior decides which) if visible and not too far from start
                {
                    parent.Engage(target.transform);
                }
                else // too far from home, give up and head back
                {
                    parent.Disengage();
                    parent.GoToStartPoint();
                }
            }
        }
    }

    IEnumerator LoseTargetDelay()
    {
        yield return new WaitForSeconds(followDelay);

        target = null;

        if (detect_player != null)
        {
            StopCoroutine(detect_player);
            detect_player = null;
        }

        parent.Disengage();
        parent.GoToStartPoint();
    }

    private Vector3[] GetBoundingPoints(Bounds bounds)
    {
        Vector3[] bounding_points =
        {
            bounds.min,
            bounds.max,
            new Vector3( bounds.min.x, bounds.min.y, bounds.max.z ),
            new Vector3( bounds.min.x, bounds.max.y, bounds.min.z ),
            new Vector3( bounds.max.x, bounds.min.y, bounds.min.z ),
            new Vector3( bounds.min.x, bounds.max.y, bounds.max.z ),
            new Vector3( bounds.max.x, bounds.min.y, bounds.max.z ),
            new Vector3( bounds.max.x, bounds.max.y, bounds.min.z )
        };
        return bounding_points;
    }

    private bool IsPointCovered(Vector3 target_direction, float target_distance)
    {
        RaycastHit[] hits = Physics.RaycastAll(transform.position, target_direction.normalized, target_distance);

        foreach (RaycastHit hit in hits)
        {
            if (parent.showLoS)
            {
                Debug.DrawLine(transform.position, hit.point, Color.yellow, 1.0f);
            }
            if (hit.transform.gameObject.layer == LayerMask.NameToLayer("Cover"))
            {
                float cover_distance = Vector3.Distance(transform.position, hit.point);

                if (cover_distance < target_distance)
                {
                    //Debug.Log("Point covered");
                    return true;
                }
            }
        }

        return false;
    }

    private void DrawLineOfSight()
    {
        float view_distance = detection_collider.radius;

        Vector3 left_boundary = Quaternion.Euler(0, -losAngle, 0) * transform.forward;

        Vector3 right_boundary = Quaternion.Euler(0, losAngle, 0) * transform.forward;

        Debug.DrawRay(transform.position, transform.forward * view_distance, Color.blue);

        Debug.DrawRay(transform.position, left_boundary * view_distance, Color.cyan);

        Debug.DrawRay(transform.position, right_boundary * view_distance, Color.cyan);
    }
}