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
    [SerializeField] int pelletsPerShot;
    [SerializeField] float spreadAngle;

    public WeaponBase Base => weaponBase;
    public string WeaponID => weapon_id;
    public float ShootDelay => shootDelay;
    public int Damage => damage;
    public int CurrBarrelAmmo => currBarrelAmmo;
    public int TotalAmmo => totalAmmo;
    public int PelletsPerShot => pelletsPerShot;
    public float SpreadAngle => spreadAngle;

    public Weapon(WeaponBase weaponBase)
    {
        this.weaponBase = weaponBase;
        shootDelay = weaponBase.ShootDelay;
        damage = weaponBase.Damage;
        currBarrelAmmo = weaponBase.MaxBarrelAmmo;
        totalAmmo = weaponBase.MaxTotalAmmo;
        pelletsPerShot = weaponBase.PelletsPerShot;
        spreadAngle = weaponBase.SpreadAngle;
    }

    public Weapon(WeaponBase weaponBase, float nShootDelay, int nDamage) : this(weaponBase)
    {
        shootDelay = nShootDelay;
        damage = nDamage;
    }

    public void SetDamage(int nDamage)
    {
        damage = nDamage;
    }

    public void SetShootDelay(float nShootDelay)
    {
        shootDelay = nShootDelay;
    }

    public void SetSpread(int nPellets, float nSpreadAngle)
    {
        pelletsPerShot = Mathf.Max(1, nPellets);
        spreadAngle = Mathf.Max(0f, nSpreadAngle);
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

    public Vector3 GetSpreadDirection(Vector3 forward)
    {
        if (spreadAngle <= 0f) return forward;

        Vector2 offset = Random.insideUnitCircle * spreadAngle;
        Quaternion rot = Quaternion.LookRotation(forward);
        return rot * Quaternion.Euler(offset.y, offset.x, 0f) * Vector3.forward;
    }
}
