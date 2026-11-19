using System;
using System.Collections.Generic;
using UnityEngine;

public enum Team { Player, Enemy }

public class Projectile : MonoBehaviour
{
    [Header("Explosion")]
    [SerializeField] private bool explodeOnImpact = false;
    [Tooltip("Also explode when the lifetime runs out (grenades, timed fuses)")]
    [SerializeField] private bool explodeOnTimeout = false;
    [SerializeField, Min(0.1f)] private float explosionRadius = 4f;
    [Tooltip("Fraction of damage dealt at the very edge of the radius. 1 = no falloff")]
    [SerializeField, Range(0f, 1f)] private float edgeDamageMultiplier = 0.25f;
    [Tooltip("Spawned at the blast point. Give it its own auto-destroy (e.g. a particle system with Stop Action: Destroy)")]
    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private float explosionForce = 15f;
    [SerializeField] private float upwardsModifier = 1f;

    private Team team;
    private GameObject owner;
    private int damage;
    private Vector3 velocity;
    private float gravityScale;
    private float lifeLeft;
    private LayerMask mask;
    private Action<RaycastHit> onWorldHit;

    public void Init(Team team, GameObject owner, int damage, Vector3 direction, float speed,
                     float gravityScale, float lifetime, LayerMask mask,
                     Action<RaycastHit> onWorldHit = null)
    {
        this.team = team;
        this.owner = owner;
        this.damage = damage;
        this.gravityScale = gravityScale;
        this.mask = mask;
        this.onWorldHit = onWorldHit;
        lifeLeft = lifetime;
        velocity = direction.normalized * speed;
        transform.forward = velocity;
    }

    void Update()
    {
        float dt = Time.deltaTime;
        velocity += Physics.gravity * gravityScale * dt;
        Vector3 step = velocity * dt;
        float dist = step.magnitude;

        if (dist > 0f && Physics.Raycast(transform.position, step / dist, out RaycastHit hit, dist, mask, QueryTriggerInteraction.Ignore))
        {
            Resolve(hit);
            Destroy(gameObject);
            return;
        }

        transform.position += step;
        if (velocity.sqrMagnitude > 0.001f) transform.forward = velocity;

        lifeLeft -= dt;
        if (lifeLeft <= 0f)
        {
            if (explodeOnTimeout) Explode(transform.position);
            Destroy(gameObject);
        }
    }

    void Resolve(RaycastHit hit)
    {
        if (explodeOnImpact)
        {
            //nudge off the surface so the blast center isn't inside the wall
            Explode(hit.point + hit.normal * 0.1f);
            return;
        }

        DirectHit(hit.collider, hit);
    }

    void DirectHit(Collider c, RaycastHit hit)
    {
        if (team == Team.Player)
        {
            if (c.CompareTag("Enemy"))
            {
                EnemyBehavior enemy = c.GetComponent<EnemyBehavior>();
                if (enemy != null)
                {
                    enemy.TakeDamage(damage, owner);
                    enemy.Engage(owner.transform, true);
                    Crosshair.Instance.ShowHit();
                }
            }
            else if (c.TryGetComponent(out CarryableBehavior explosive) && explosive.IsExplodable)
            {
                explosive.Explode();
                Crosshair.Instance.ShowHit();
            }
            else onWorldHit?.Invoke(hit);
        }
        else
        {
            if (c.CompareTag("Player"))
            {
                PlayerController p = c.GetComponentInParent<PlayerController>();
                if (p != null) p.TakeDamage(damage);
            }
            else onWorldHit?.Invoke(hit);
        }
    }

    void Explode(Vector3 center)
    {
        if (explosionPrefab != null)
            Instantiate(explosionPrefab, center, Quaternion.identity);

        //one colliders-per-target problem: a single enemy/player can have several colliders, so track who's already been hit
        var hitEnemies = new HashSet<EnemyBehavior>();
        var hitExplosives = new HashSet<CarryableBehavior>();
        bool playerHit = false;
        bool anyHitMarker = false;

        Collider[] cols = Physics.OverlapSphere(center, explosionRadius, mask, QueryTriggerInteraction.Ignore);
        foreach (Collider c in cols)
        {
            if (c.CompareTag("Enemy") && team == Team.Player)
            {
                EnemyBehavior enemy = c.GetComponentInParent<EnemyBehavior>();
                if (enemy == null || !hitEnemies.Add(enemy)) continue;

                enemy.TakeDamage(ScaledDamage(center, c), owner);
                enemy.Engage(owner.transform, true);

                //damage can kill/deactivate the enemy, and a coroutine can't start on an inactive object
                if (explosionForce > 0f && enemy.gameObject.activeInHierarchy)
                    enemy.ApplyKnockback(center, explosionForce, explosionRadius, upwardsModifier);

                anyHitMarker = true;
            }
            else if (c.CompareTag("Player") && team == Team.Enemy)
            {
                if (playerHit) continue;
                PlayerController p = c.GetComponentInParent<PlayerController>();
                if (p == null) continue;

                playerHit = true;
                p.TakeDamage(ScaledDamage(center, c));
            }
            else if (c.TryGetComponent(out CarryableBehavior explosive) && explosive.IsExplodable)
            {
                if (!hitExplosives.Add(explosive)) continue;
                explosive.Explode();
                if (team == Team.Player) anyHitMarker = true;
            }
        }

        if (anyHitMarker) Crosshair.Instance.ShowHit();
    }

    int ScaledDamage(Vector3 center, Collider target)
    {
        float dist = Vector3.Distance(center, target.ClosestPoint(center));
        float scale = Mathf.Lerp(1f, edgeDamageMultiplier, Mathf.Clamp01(dist / explosionRadius));
        return Mathf.Max(1, Mathf.RoundToInt(damage * scale));
    }
}