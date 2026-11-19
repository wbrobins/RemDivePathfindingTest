using UnityEngine;

public enum FireMode { Hitscan, Projectile }

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

    [Header("Firing")]
    [SerializeField] FireMode fireMode = FireMode.Hitscan;

    [Header("Projectile (ignored for Hitscan)")]
    [SerializeField] Projectile projectilePrefab;
    [SerializeField, Min(0.1f)] float projectileSpeed = 40f;
    [SerializeField, Min(0f)] float projectileGravityScale = 0f;
    [SerializeField, Min(0.1f)] float projectileLifetime = 5f;

    [Header("Effects")]
    [SerializeField] GameObject bulletHolePrefab;

    [Header("Held Pose")]
    [SerializeField] Vector3 heldPosition;
    [SerializeField] Vector3 heldEulerRotation;


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
    public FireMode Mode => fireMode;
    public Projectile ProjectilePrefab => projectilePrefab;
    public float ProjectileSpeed => projectileSpeed;
    public float ProjectileGravityScale => projectileGravityScale;
    public float ProjectileLifetime => projectileLifetime;
    public Vector3 HeldPosition => heldPosition;
    public Vector3 HeldEulerRotation => heldEulerRotation;
}
