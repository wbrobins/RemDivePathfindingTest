using UnityEngine;

public class Carryable
{
    [SerializeField] private CarryableBase carryableBase;
    [SerializeField] private int throwDamage;
    [SerializeField] private bool explodable;
    [SerializeField] private float explodeRange;
    [SerializeField] private int explodeDamage;
    [SerializeField] private float explosionForce;
    [SerializeField] private float explosionUpwardsModifier;


    public CarryableBase Base => carryableBase;
    public int ThrowDamage => throwDamage;
    public bool Explodable => explodable;
    public float ExplodeRange => explodeRange;
    public int ExplodeDamage => explodeDamage;
    public float ExplosionForce => explosionForce;
    public float ExplosionUpwardsModifier => explosionUpwardsModifier; 


    public Carryable(CarryableBase carryableBase)
    {
        this.carryableBase = carryableBase;
        throwDamage = carryableBase.ThrowDamage;
        explodable = carryableBase.Explodable;
        explodeRange = carryableBase.ExplodeRange;
        explodeDamage = carryableBase.ExplodeDamage;
        explosionForce = carryableBase.ExplosionForce;
        explosionUpwardsModifier = carryableBase.ExplosionUpwardsModifier;
    }
}
