using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemyBehavior : MonoBehaviour
{
    public NavMeshAgent agent;
    public GameObject player;
    public bool following;
    private Vector3 startPoint;

    [SerializeField] private float stopDistance = 2.5f;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float shootDelay = 3.0f;
    [SerializeField] private float projectileSpeed = 10f;
    [SerializeField] private Transform firePoint;

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

    public Vector3 GetStartPoint()
    {
        return startPoint;
    }
    
    public void GoToStartPoint()
    {
        agent.stoppingDistance = 0;
        agent.SetDestination(startPoint);
    }

    public void FollowTarget(Vector3 position)
    {
        agent.stoppingDistance = stopDistance;
        agent.SetDestination(position);
        transform.LookAt(position);
    }

    public IEnumerator ShootTarget(Transform target)
    {
        while (true)
        {
            yield return new WaitForSeconds(shootDelay);

            GameObject projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);

            Vector3 aimPoint = target.position + Vector3.up * 1.0f;
            Vector3 direction = (aimPoint - firePoint.position).normalized;

            Rigidbody rb = projectile.GetComponent<Rigidbody>();
            rb.linearVelocity = direction * projectileSpeed;
        }
    }
}
