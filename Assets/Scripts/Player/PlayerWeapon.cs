using System.Collections;
using NUnit.Framework;
using UnityEngine;

public class PlayerWeapon : MonoBehaviour
{
    private bool shooting = false;
    [SerializeField] private new GameObject camera;
    [SerializeField] private GameObject playerHandObj;
    [SerializeField] private Animator animator;
    [SerializeField] private AudioSource audioSource;
    private PlayerHand playerHand;
    [SerializeField] private GameObject player;
    [SerializeField] private int currWeaponSlot = 0;
    [SerializeField] private LayerMask shootRaycastMask = ~0;

    public bool Shooting => shooting;
    public Weapon[] weapons;
    public Weapon CurrWeapon {get; private set;}
    private GameObject currWeaponPrefab;
    private PlayerController playerController;

    void Awake()
    {
        playerHand = playerHandObj.GetComponent<PlayerHand>();

        Assert.IsNotEmpty(weapons);
        SetUp();
        SetWeapon(currWeaponSlot);
    }

    void Start()
    {
        playerController = player.GetComponent<PlayerController>();
        playerController.UpdateAmmoText(CurrWeapon);   
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
        else if (Input.GetKeyDown(KeyCode.R) && !shooting && CurrWeapon.CurrBarrelAmmo != CurrWeapon.Base.MaxBarrelAmmo)
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
                currWeaponPrefab.SetActive(false);
                SetWeapon(1);
                break;
            case 1:
                currWeaponPrefab.SetActive(false);
                SetWeapon(0);
                break;
            default:
                Debug.Log("Weapon error: wrong value passed @ PlayerWeapon.cs");
                return;
        }

        playerController.UpdateAmmoText(CurrWeapon);   
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
        currWeaponPrefab.SetActive(false);
        SetLayerRecursive(currWeaponPrefab, LayerMask.NameToLayer("Weapon"));
        if(playerController != null) 
        {
            playerController.UpdateAmmoText(CurrWeapon);     
        }
        audioSource.resource = CurrWeapon.Base.FireSound;
        StartCoroutine(SwapDelay());
    }

    IEnumerator SwapDelay()
    {
        animator.SetTrigger("Swap");
        yield return new WaitForSeconds(.1f);
        currWeaponPrefab.SetActive(true);
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

    void SetLayerRecursive(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
            SetLayerRecursive(child.gameObject, layer);
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
            animator.SetTrigger("Fire");
            audioSource.Play();

            if (Physics.Raycast(origin, direction, out RaycastHit hit, 50f, shootRaycastMask))
            {
                Debug.Log(hit.collider.name);
                if (hit.collider.CompareTag("Enemy"))
                {
                    EnemyBehavior enemyBehavior = hit.collider.GetComponent<EnemyBehavior>();

                    if (enemyBehavior != null)
                    {
                        enemyBehavior.TakeDamage(CurrWeapon.Damage, gameObject);
                        enemyBehavior.Engage(player.transform, true);
                    }
                } 
                else if (hit.collider.TryGetComponent(out CarryableBehavior explosive) && explosive.IsExplodable)
                {
                    explosive.Explode();
                    Debug.Log("Explode!");
                }
                
            }
            CurrWeapon.DepleteAmmo(1);
            playerController.UpdateAmmoText(CurrWeapon); 
            yield return new WaitForSeconds(CurrWeapon.ShootDelay);
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
        animator.SetTrigger("Reload");
        playerController.UpdateAmmoText("Reloading...");  
        yield return new WaitForSeconds(1);
        Reload();
        shooting = false;
        playerController.UpdateAmmoText(CurrWeapon);  
    }
}
