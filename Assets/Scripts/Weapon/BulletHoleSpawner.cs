using System.Collections.Generic;
using UnityEngine;

public class BulletHoleSpawner : MonoBehaviour
{
    [SerializeField] private int maxHoles = 100;
    [SerializeField] private float surfaceOffset = .01f;
    [SerializeField] private float lifetime = 20f; //0 = never fade out

    private readonly Queue<GameObject> holes = new Queue<GameObject>();

    public void Spawn(GameObject prefab, RaycastHit hit)
    {
        if (prefab == null) return;

        //remove oldest holes
        while (holes.Count >= maxHoles)
        {
            GameObject oldest = holes.Dequeue();
            if (oldest != null) Destroy(oldest);
        }

        Vector3 pos = hit.point + hit.normal * surfaceOffset;
        //quad's visible face points along -Z, so look *into* the surface
        Quaternion rot = Quaternion.LookRotation(-hit.normal) * Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));

        GameObject hole = Instantiate(prefab, pos, rot);

        //only stick to moving objects; parenting to scaled static geometry distorts the decal
        if (hit.rigidbody != null)
            hole.transform.SetParent(hit.collider.transform, true);

        holes.Enqueue(hole);
        if (lifetime > 0f) Destroy(hole, lifetime);
    }
}
