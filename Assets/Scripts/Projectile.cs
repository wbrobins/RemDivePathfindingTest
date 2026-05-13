using System.Collections;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float despawnDelay = 5.0f;

    void Start()
    {
        StartCoroutine(DespawnRoutine());
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
        if (other.CompareTag("Player") || other.CompareTag("Wall"))
        {
            Destroy(gameObject);
        }
    }
}
