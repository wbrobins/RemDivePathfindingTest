using UnityEngine;

[System.Serializable]
public class Weapon
{
    [SerializeField] WeaponBase weaponBase;
    [SerializeField] private string weapon_id;
    [SerializeField] float shootDelay;
    [SerializeField] int damage;
    [SerializeField] int currBarrelAmmo;
    [SerializeField] int totalAmmo;

    public WeaponBase Base => weaponBase;
    public string WeaponID => weapon_id;
    public float ShootDelay => shootDelay;
    public int Damage => damage;
    public int CurrBarrelAmmo => currBarrelAmmo;
    public int TotalAmmo => totalAmmo;

    public Weapon(string id)
    {
        weapon_id = id;
        weaponBase = Resources.Load<WeaponBase>($"Data/WeaponBases/" + id);
        shootDelay = Base.ShootDelay;
        damage = Base.Damage;
        currBarrelAmmo = Base.MaxBarrelAmmo;
        totalAmmo = Base.MaxTotalAmmo;
    }

    public Weapon(string id, float nShootDelay, int nDamage)
    {
        weapon_id = id;
        weaponBase = Resources.Load<WeaponBase>($"Data/WeaponBases/" + id);
        shootDelay = nShootDelay;
        damage = nDamage;
        currBarrelAmmo = Base.MaxBarrelAmmo;
        totalAmmo = Base.MaxTotalAmmo;
    }

    public void SetDamage(int nDamage)
    {
        damage = nDamage;
    }

    public void SetShootDelay(float nShootDelay)
    {
        shootDelay = nShootDelay;
    }

    public void SetAmmo(int nCurrAmmo, int nTotalAmmo)
    {
        currBarrelAmmo = nCurrAmmo;
        totalAmmo = nTotalAmmo;
    }

    public void DepleteAmmo(int nAmmo)
    {
        currBarrelAmmo -= nAmmo;
    }
}
