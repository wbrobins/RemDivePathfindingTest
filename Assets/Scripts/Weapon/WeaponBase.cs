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


    public string Name => weaponName;
    public float ShootDelay => shootDelay;
    public int Damage => damage;
    public int MaxBarrelAmmo => maxBarrelAmmo;
    public int MaxTotalAmmo => maxTotalAmmo;
    public GameObject BasePrefab => basePrefab;
}
