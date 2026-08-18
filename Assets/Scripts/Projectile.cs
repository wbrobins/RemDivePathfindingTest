using System.Collections;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float despawnDelay = 5.0f;
    [SerializeField] private int damage = 1;

    void Start()
    {
        StartCoroutine(DespawnRoutine());
    }

    public void SetDamage(int eDamage)
    {
        damage = eDamage;
    }

    public void SetDespawnDelay(float delay)
    {
        despawnDelay = delay;
    }

    IEnumerator DespawnRoutine()
    {
        yield return new WaitForSeconds(despawnDelay);
        Destroy(gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponentInParent<PlayerController>();
            player.TakeDamage(damage);
            Destroy(gameObject);
        } 
        else if (other.CompareTag("Wall"))
        {
            Destroy(gameObject);
        }
    }
}
