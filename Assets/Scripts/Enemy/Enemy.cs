using UnityEngine;

public class Enemy
{
    public enum CombatType
    {
        Ranged,
        Melee
    }

    [SerializeField] private EnemyBase enemyBase;
    [SerializeField] private string enemy_id;
    [SerializeField] int hp;
    [SerializeField] int maxHp;
    [SerializeField] private CombatType combatType = CombatType.Ranged;
    [SerializeField] float shootDelay;
    [SerializeField] float projectileSpeed;
    [SerializeField] float stopDistance;
    [SerializeField] float minRange;
    [SerializeField] float meleeRange;
    [SerializeField] int meleeDamage;
    [SerializeField] float meleeCooldown;
    [SerializeField] float strafeSpeed;
    [SerializeField] float strafeChangeInterval;
    [SerializeField] float knockbackRecoveryDelay;


    public EnemyBase Base => enemyBase;
    public string EnemyID => enemy_id;
    public int HP => hp;
    public int MaxHP => maxHp;
    public CombatType ECombatType => combatType;
    public float ShootDelay => shootDelay;
    public float ProjectileSpeed => projectileSpeed;
    public float StopDistance => stopDistance;
    public float MinRange => minRange;
    public float MeleeRange => meleeRange;
    public int MeleeDamage => meleeDamage;
    public float MeleeCooldown => meleeCooldown;
    public float StrafeSpeed => strafeSpeed;
    public float StrafeChangeInterval => strafeChangeInterval;
    public float KnockbackRecoveryDelay => knockbackRecoveryDelay;

    public Enemy(EnemyBase enemyBase)
    {
        this.enemyBase = enemyBase;
        hp = enemyBase.HP;
        maxHp = enemyBase.HP;
        shootDelay = enemyBase.ShootDelay;
        projectileSpeed = enemyBase.ProjectileSpeed;
        stopDistance = enemyBase.StopDistance;
        minRange = enemyBase.MinRange;
        meleeRange = enemyBase.MeleeRange;
        meleeDamage = enemyBase.MeleeDamage;
        meleeCooldown = enemyBase.MeleeCooldown;
        strafeSpeed = enemyBase.StrafeSpeed;
        strafeChangeInterval = enemyBase.StrafeChangeInterval;
        knockbackRecoveryDelay = enemyBase.KnockbackRecoveryDelay;
        combatType = (CombatType)enemyBase.ECombatType;
    }

    public void Hit(int damage)
    {
        hp = Mathf.Max(hp-damage,0);
    }

    public void ResetHP()
    {
        hp = maxHp;
    }
}

