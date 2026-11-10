using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "Weapons/Create new weapon")]
public class WeaponBase : ScriptableObject
{
    [SerializeField] string weaponName;
    [SerializeField] float shootDelay;
    [SerializeField] int damage;
    [SerializeField] int maxBarrelAmmo;
    [SerializeField] int maxTotalAmmo;
    [SerializeField] GameObject basePrefab;
    [SerializeField] AudioClip fireSound;

    [Header("Spread")]
    [SerializeField, Min(1)] int pelletsPerShot = 1;
    [SerializeField, Range(0f, 45f)] float spreadAngle = 0f;

    [Header("Effects")]
    [SerializeField] GameObject bulletHolePrefab;


    public string Name => weaponName;
    public float ShootDelay => shootDelay;
    public int Damage => damage;
    public int MaxBarrelAmmo => maxBarrelAmmo;
    public int MaxTotalAmmo => maxTotalAmmo;
    public GameObject BasePrefab => basePrefab;
    public AudioClip FireSound => fireSound;
    public int PelletsPerShot => pelletsPerShot;
    public float SpreadAngle => spreadAngle;
    public GameObject BulletHolePrefab => bulletHolePrefab;
}
