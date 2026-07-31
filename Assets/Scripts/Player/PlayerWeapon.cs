using System.Collections;
using NUnit.Framework;
using UnityEngine;

public class PlayerWeapon : MonoBehaviour
{
    private bool shooting = false;
    [SerializeField] private new GameObject camera;
    [SerializeField] private int currWeaponSlot = 0;

    public Weapon[] weapons;
    public Weapon CurrWeapon {get; private set;}
    private GameObject currWeaponPrefab;

    void Awake()
    {
        Assert.IsNotEmpty(weapons);
        SetUp();
        SetWeapon(currWeaponSlot);
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
        else if (Input.GetKeyDown(KeyCode.F))
        {
            CycleWeapon();
        }
    }

    void SetUp()
    {
        foreach(Weapon weapon in weapons)
        {
            weapon.SetShootDelay(weapon.Base.ShootDelay);
            weapon.SetDamage(weapon.Base.Damage);
        }
    }

    void CycleWeapon()
    {
        switch (currWeaponSlot)
        {
            case 0:
                SetWeapon(1);
                break;
            case 1:
                SetWeapon(0);
                break;
            default:
                Debug.Log("Weapon error: wrong value passed @ PlayerWeapon.cs");
                return;
        }
    }

    void SetWeapon(int slot)
    {
        if(CurrWeapon != null)
        {
            Destroy(currWeaponPrefab);
        }
        CurrWeapon = weapons[slot];
        currWeaponSlot = slot;
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

        yield return new WaitForSeconds(CurrWeapon.ShootDelay);
        shooting = false;
    }
}
