using UnityEngine;

public class ExplosiveBarrel : MonoBehaviour
{
    public GameObject barrel;

    [SerializeField] private float explosionRange = 5.0f;
    [SerializeField] private int explosionDamage = 5;
    [SerializeField] private float explosionForce = 4f;
    [SerializeField] private float explosionUpwardsModifier = 3f;

    private void Awake()
    {
        barrel.SetActive(true);
    }

    public void Explode()
    {
        barrel.SetActive(false);

        Collider[] enemies = Physics.OverlapSphere(transform.position, explosionRange);

        foreach (Collider enemy in enemies)
        {
            if (enemy.CompareTag("Enemy"))
            {
                Debug.Log("enemy found");
                EnemyBehavior enemyBehavior = enemy.GetComponent<EnemyBehavior>();

                if (enemyBehavior != null)
                {
                    enemyBehavior.ApplyKnockback(transform.position, explosionForce, explosionRange, explosionUpwardsModifier);
                    enemyBehavior.TakeDamage(explosionDamage);
                }
            }
        }

        Destroy(gameObject);
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, explosionRange);
    }
}