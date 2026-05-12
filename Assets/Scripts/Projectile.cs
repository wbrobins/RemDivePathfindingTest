using System.Collections;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float despawnDelay = 5.0f;

    void Start()
    {
        StartCoroutine(DespawnRoutine());
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
            Destroy(gameObject);
        }
    }
}
