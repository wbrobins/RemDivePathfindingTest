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

    public Weapon(WeaponBase weaponBase)
    {
        this.weaponBase = weaponBase;
        shootDelay = weaponBase.ShootDelay;
        damage = weaponBase.Damage;
        currBarrelAmmo = weaponBase.MaxBarrelAmmo;
        totalAmmo = weaponBase.MaxTotalAmmo;
    }

    public Weapon(WeaponBase weaponBase, float nShootDelay, int nDamage)
    {
        this.weaponBase = weaponBase;
        shootDelay = nShootDelay;
        damage = nDamage;
        currBarrelAmmo = weaponBase.MaxBarrelAmmo;
        totalAmmo = weaponBase.MaxTotalAmmo;
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
