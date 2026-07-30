using System.Collections;
using UnityEngine;

public class PlayerWeapon : MonoBehaviour
{
    private bool shooting = false;
    [SerializeField] private new GameObject camera;
    [SerializeField] private float shootDelay = 0.5f;

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && !shooting)
        {
            shooting = true;
            StartCoroutine(ShootRoutine());
        }
    }

    IEnumerator ShootRoutine()
    {
        //raycast logic here
        Vector3 origin = transform.position;
        Vector3 direction = camera.transform.forward;

        Debug.DrawRay(origin, direction * 50f, Color.red, 1f);

        if (Physics.Raycast(origin, direction, out RaycastHit hit, 50f))
        {
            if (hit.collider.CompareTag("Enemy"))
            {
                EnemyBehavior enemyBehavior = hit.collider.GetComponent<EnemyBehavior>();

                if (enemyBehavior != null)
                {
                    enemyBehavior.TakeDamage(1);
                }
            } 
            else if (hit.collider.CompareTag("Explodable"))
            {
                ExplosiveBarrel explosiveBarrel = hit.collider.GetComponent<ExplosiveBarrel>();

                if(explosiveBarrel != null)
                {
                    explosiveBarrel.Explode();
                }
            }
        }

        yield return new WaitForSeconds(shootDelay);
        shooting = false;
    }
}
