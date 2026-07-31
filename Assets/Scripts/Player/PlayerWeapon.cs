using System.Collections;
using NUnit.Framework;
using UnityEngine;

public class PlayerWeapon : MonoBehaviour
{
    private bool shooting = false;
    [SerializeField] private new GameObject camera;
    [SerializeField] private float shootDelay = .1f;

    public string[] weapons;
    public Weapon CurrWeapon {get; private set;}
    private GameObject currWeaponPrefab;

    void Awake()
    {
        Assert.IsNotEmpty(weapons);
        SetWeapon(0);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && !shooting)
        {
            shooting = true;
            StartCoroutine(ShootRoutine());
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SetWeapon(0);
        } 
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SetWeapon(1);
        }
    }

    void SetWeapon(int slot)
    {
        if(CurrWeapon != null)
        {
            Destroy(currWeaponPrefab);
        }
        CurrWeapon = new Weapon(weapons[slot]);
        shootDelay = CurrWeapon.Base.ShootDelay;
        Debug.Log("Current Weapon: " + CurrWeapon.Base.Name);
        currWeaponPrefab = Instantiate(CurrWeapon.Base.BasePrefab, transform.position, camera.transform.rotation, transform);
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
                    enemyBehavior.TakeDamage(CurrWeapon.Damage);
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
