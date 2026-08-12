using UnityEngine;

public class Enemy
{
    [SerializeField] private EnemyBase enemyBase;
    [SerializeField] private string enemy_id;
    [SerializeField] int hp;
    [SerializeField] int maxHp;
    [SerializeField] float shootDelay;
    [SerializeField] float projectileSpeed;
    [SerializeField] float stopDistance;
    [SerializeField] float minRange;


    public EnemyBase Base => enemyBase;
    public string EnemyID => enemy_id;
    public int HP => hp;
    public int MaxHP => maxHp;
    public float ShootDelay => shootDelay;
    public float ProjectileSpeed => projectileSpeed;
    public float StopDistance => stopDistance;
    public float MinRange => minRange;
    

    public Enemy(EnemyBase enemyBase)
    {
        this.enemyBase = enemyBase;
        hp = enemyBase.HP;
        maxHp = enemyBase.HP;
        shootDelay = enemyBase.ShootDelay;
        projectileSpeed = enemyBase.ProjectileSpeed;
        stopDistance = enemyBase.StopDistance;
        minRange = enemyBase.MinRange;
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

