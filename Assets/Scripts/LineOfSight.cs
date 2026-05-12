using System.Collections;
using UnityEngine;

public class LineOfSight : MonoBehaviour
{
    [SerializeField] private GameObject target;
    [SerializeField] private EnemyBehavior parent;
    [SerializeField] private float losAngle = 70.0f;
    [SerializeField] private float followDelay = 3f;


    private Collider player_collider;
    private SphereCollider detection_collider;
    private Coroutine detect_player;
    private Coroutine lose_target;

    void Awake()
    {
        detection_collider = GetComponent<SphereCollider>();
    }

    void Update()
    {
        DrawLineOfSight();
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            if(lose_target != null)
            {
                StopCoroutine(lose_target);
                lose_target = null;
            }

            target = other.gameObject;
            player_collider=other;

            if(detect_player == null)
            {
                detect_player = StartCoroutine(DetectPlayer());
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.tag == "Player")
        {
            lose_target = StartCoroutine(LoseTargetDelay());
        }
    }

    IEnumerator DetectPlayer()
    {
        while (true)
        {
            yield return new WaitForSeconds(.1f);

            Vector3[] points = GetBoundingPoints(player_collider.bounds);

            int points_hidden = 0;

            foreach(Vector3 point in points)
            {
                Vector3 target_direction = point - transform.position;
                float target_distance = target_direction.magnitude;
                float target_angle = Vector3.Angle(target_direction.normalized, transform.forward);
                bool covered = IsPointCovered(target_direction.normalized, target_distance);

                Color ray_color = (covered || target_angle > losAngle) ? Color.red : Color.green;

                Debug.DrawRay(transform.position, target_direction, ray_color, 1.0f);

                if (covered || target_angle > losAngle)
                {
                    ++points_hidden;
                }
            }

            if(points_hidden >= points.Length)
            {
                Debug.Log("Player hidden");
            }
            else
            {
                Debug.Log("Player visible");
                parent.FollowTarget(target.transform.position); //follow player if visible
            }
        }
    }

    IEnumerator LoseTargetDelay()
    {
        yield return new WaitForSeconds(followDelay);

        target = null;

        if(detect_player != null)
        {
            StopCoroutine(detect_player);
            detect_player = null;
        }

        parent.GoToStartPoint();
    }

    private Vector3[] GetBoundingPoints( Bounds bounds )
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

    private bool IsPointCovered( Vector3 target_direction, float target_distance )
    {
        RaycastHit[] hits = Physics.RaycastAll(transform.position, target_direction.normalized, target_distance );

        foreach ( RaycastHit hit in hits )
        {
            Debug.DrawLine(transform.position, hit.point, Color.yellow, 1.0f);
            if ( hit.transform.gameObject.layer == LayerMask.NameToLayer( "Cover" ) )
            {
                float cover_distance = Vector3.Distance(transform.position, hit.point );

                if ( cover_distance < target_distance)
                {
                    Debug.Log("Point covered");
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

        Debug.DrawRay(transform.position, left_boundary*view_distance, Color.cyan);

        Debug.DrawRay(transform.position, right_boundary*view_distance, Color.cyan);
    }
}
