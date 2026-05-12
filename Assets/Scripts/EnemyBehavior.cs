using UnityEngine;
using UnityEngine.AI;

public class EnemyBehavior : MonoBehaviour
{
    public NavMeshAgent agent;
    public GameObject player;
    public bool following;
    private Vector3 startPoint;
    [SerializeField] private float stopDistance = 2.5f;

    void Awake()
    {
        agent.stoppingDistance = stopDistance;
    }

    void Start()
    {
        //init
        player = GameObject.Find("Player");

        //get current position as start
        startPoint = transform.position;
    }

    public void GoToStartPoint()
    {
        agent.SetDestination(startPoint);
    }

    public void FollowTarget(Vector3 position)
    {
        agent.SetDestination(position);
        transform.LookAt(position);
    }
}
