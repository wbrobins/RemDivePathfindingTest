using UnityEngine;


[CreateAssetMenu(fileName = "New Enemy", menuName = "Enemies/Create new enemy")]
public class EnemyBase : ScriptableObject
{
    [SerializeField] string enemyName;
    [SerializeField] Sprite sprite;
    [SerializeField] int hp;
    [SerializeField] float shootDelay;
    [SerializeField] float projectileSpeed;
    [SerializeField] float stopDistance;
    [SerializeField] float minRange;
    [SerializeField] GameObject basePrefab;

    public string Name => enemyName;
    public Sprite Sprite => sprite;
    public int HP => hp;
    public float ShootDelay => shootDelay;
    public float ProjectileSpeed => projectileSpeed;
    public float StopDistance => stopDistance;
    public float MinRange => minRange;
    public GameObject BasePrefab => basePrefab;
}
