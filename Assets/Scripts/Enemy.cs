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


    public EnemyBase Base => enemyBase;
    public string EnemyID => enemy_id;
    public int HP => hp;
    public int MaxHP => maxHp;
    public float ShootDelay => shootDelay;
    public float ProjectileSpeed => projectileSpeed;
    public float StopDistance => stopDistance;
    

    public Enemy(string id)
    {
        enemy_id = id;
        enemyBase = Resources.Load<EnemyBase>($"Data/EnemyBases/" + id);
        hp = Base.HP;
        maxHp = Base.HP;
        shootDelay = Base.ShootDelay;
        projectileSpeed = Base.ProjectileSpeed;
        stopDistance = Base.StopDistance;
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

