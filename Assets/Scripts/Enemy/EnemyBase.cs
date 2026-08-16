using UnityEngine;


[CreateAssetMenu(fileName = "New Enemy", menuName = "Enemies/Create new enemy")]
public class EnemyBase : ScriptableObject
{
    public enum CombatType
    {
        Ranged,
        Melee
    }

    public enum MoveType
    {
        Ground,
        Air
    }
    [SerializeField] string enemyName;
    [SerializeField] int hp;
    [SerializeField] float speed = 5;
    [SerializeField] float knockbackRecoveryDelay = .4f;
    [SerializeField] GameObject basePrefab;

    [Header("Combat Type")]
    [SerializeField] private CombatType combatType = CombatType.Ranged;
    [SerializeField] private MoveType moveType = MoveType.Ground;

    [Header("Ranged Attack")]
    [SerializeField] float shootDelay;
    [SerializeField] float projectileSpeed;
    [SerializeField] float stopDistance;
    [SerializeField] float minRange = 5f;

    [Header("Ranged Strafing")]
    [Tooltip("How fast the enemy sidesteps while holding position and shooting")]
    [SerializeField] float strafeSpeed = 5f;
    [SerializeField] float strafeChangeInterval = 1.5f;

    [Header("Melee Attack")]
    [SerializeField] float meleeRange = 2f;
    [SerializeField] int meleeDamage = 10;
    [SerializeField] float meleeCooldown = 1.5f;

    

    

    public string Name => enemyName;
    public int HP => hp;
    public float Speed => speed;
    public CombatType ECombatType => combatType;
    public MoveType EMoveType => moveType;
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
    public GameObject BasePrefab => basePrefab;
}
