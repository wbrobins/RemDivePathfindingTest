using System.Collections;
using NUnit.Framework;
using UnityEngine;

public class PlayerWeapon : MonoBehaviour
{
    private bool shooting = false;
    [SerializeField] private new GameObject camera;
    [SerializeField] private GameObject playerHandObj;
    private PlayerHand playerHand;
    [SerializeField] private int currWeaponSlot = 0;

    public bool Shooting => shooting;
    public Weapon[] weapons;
    public Weapon CurrWeapon {get; private set;}
    private GameObject currWeaponPrefab;

    void Awake()
    {
        playerHand = playerHandObj.GetComponent<PlayerHand>();

        Assert.IsNotEmpty(weapons);
        SetUp();
        SetWeapon(currWeaponSlot);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && !shooting && !playerHand.Carrying)
        {
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
        else if (Input.GetKeyDown(KeyCode.R) && !shooting)
        {
            StartCoroutine(ReloadRoutine());
        }
    }

    void SetUp()
    {
        foreach(Weapon weapon in weapons)
        {
            weapon.SetShootDelay(weapon.Base.ShootDelay);
            weapon.SetDamage(weapon.Base.Damage);
            weapon.SetAmmo(weapon.Base.MaxBarrelAmmo, weapon.Base.MaxTotalAmmo);
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

    void Reload()
    {
        Debug.Log("Reloading...");
        if(CurrWeapon.TotalAmmo >= CurrWeapon.Base.MaxBarrelAmmo)
        {
            CurrWeapon.SetAmmo(CurrWeapon.Base.MaxBarrelAmmo, CurrWeapon.TotalAmmo - (CurrWeapon.Base.MaxBarrelAmmo - CurrWeapon.CurrBarrelAmmo));
        }
        else if(CurrWeapon.TotalAmmo < CurrWeapon.Base.MaxBarrelAmmo)
        {
            CurrWeapon.SetAmmo(CurrWeapon.TotalAmmo, 0);
        }
    }

    IEnumerator ShootRoutine()
    {
        //shoot
        if (CurrWeapon.CurrBarrelAmmo > 0)
        {
            shooting = true;

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
                        enemyBehavior.TakeDamage(CurrWeapon.Damage, gameObject);
                    }
                } 
                else if (hit.collider.TryGetComponent(out CarryableBehavior explosive) && explosive.IsExplodable)
                {
                    explosive.Explode();
                }
                
            }
            CurrWeapon.DepleteAmmo(1);
            yield return new WaitForSeconds(CurrWeapon.ShootDelay);
            shooting = false;
        }
        //reload
        else if(CurrWeapon.CurrBarrelAmmo == 0 && CurrWeapon.TotalAmmo > 0)
        {
            shooting = true;
            yield return new WaitForSeconds(1);
            Reload();
            shooting = false;
        }
        else
        {
            Debug.Log("Out of ammo");
        }
    }

    IEnumerator ReloadRoutine()
    {
        shooting = true;
        yield return new WaitForSeconds(1);
        Reload();
        shooting = false;
    }
}
