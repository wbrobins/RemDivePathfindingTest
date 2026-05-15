using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemyBehavior : MonoBehaviour
{
    public NavMeshAgent agent;
    public GameObject player;
    public bool following;
    private Vector3 startPoint;
    private Transform firePoint;
    [SerializeField] private GameObject projectilePrefab;

    public string enemyId;

    public Enemy Enemy {get; private set;}

    
    void Awake()
    {
        SetUp(new Enemy(enemyId));
    }

    public void SetUp(Enemy enemy)
    {
        Enemy = enemy;
        agent.stoppingDistance = Enemy.StopDistance;
    }

    void Start()
    {
        //init
        player = GameObject.Find("Player");
        firePoint = transform.Find("FirePoint");

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
        agent.stoppingDistance = Enemy.StopDistance;
        agent.SetDestination(position);
        transform.LookAt(position);
    }

    public IEnumerator ShootTarget(Transform target)
    {
        while (true)
        {
            yield return new WaitForSeconds(Enemy.ShootDelay);

            GameObject projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);

            Vector3 aimPoint = target.position + Vector3.up * 1.0f;
            Vector3 direction = (aimPoint - firePoint.position).normalized;

            Rigidbody rb = projectile.GetComponent<Rigidbody>();
            rb.linearVelocity = direction * Enemy.ProjectileSpeed;
        }
    }

    public void TakeDamage(int damage)
    {
        Enemy.Hit(damage);
        Debug.Log("Damage done: " + damage);
        if (Enemy.HP == 0)
        {
            Destroy(gameObject);
            Debug.Log("Enemy killed");
        }
    }
}
