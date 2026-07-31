using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "Weapons/Create new weapon")]
public class WeaponBase : ScriptableObject
{
    [SerializeField] string weaponName;
    [SerializeField] float shootDelay;
    [SerializeField] int damage;
    [SerializeField] GameObject basePrefab;


    public string Name => weaponName;
    public float ShootDelay => shootDelay;
    public int Damage => damage;
    public GameObject BasePrefab => basePrefab;
}
