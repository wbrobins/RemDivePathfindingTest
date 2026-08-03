using UnityEngine;


[CreateAssetMenu(fileName = "New carryable", menuName = "Carryables/Create new carryable")]
public class CarryableBase : ScriptableObject
{
    [SerializeField] string carryableName;
    [SerializeField] private Vector3 holdPositionOffset;
    [SerializeField] private Vector3 holdRotationOffset;
    [SerializeField] private Vector3 holdScale = Vector3.one;
    [SerializeField] private int throwDamage;
    [SerializeField] bool explodable;
    [SerializeField] float explodeRange;
    [SerializeField] int explodeDamage;
    [SerializeField] private float explosionForce;
    [SerializeField] private float explosionUpwardsModifier;
    [SerializeField] GameObject basePrefab;

    public string Name => carryableName;
    public Vector3 HoldPositionOffset => holdPositionOffset;
    public Vector3 HoldRotationOffset => holdRotationOffset;
    public Vector3 HoldScale => holdScale;
    public int ThrowDamage => throwDamage;
    public bool Explodable => explodable;
    public float ExplodeRange => explodeRange;
    public int ExplodeDamage => explodeDamage;
    public float ExplosionForce => explosionForce;
    public float ExplosionUpwardsModifier => explosionUpwardsModifier; 
    public GameObject BasePrefab => basePrefab;
}
