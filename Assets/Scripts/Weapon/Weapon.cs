using UnityEngine;

public class Weapon
{
    [SerializeField] WeaponBase weaponBase;
    [SerializeField] private string weapon_id;
    [SerializeField] float shootDelay;

    public WeaponBase Base => weaponBase;
    public string WeaponID => weapon_id;
    public float ShootDelay => shootDelay;

    public Weapon(string id)
    {
        weapon_id = id;
        weaponBase = Resources.Load<WeaponBase>($"Data/WeaponBases/" + id);
        shootDelay = Base.ShootDelay;
    }
}
