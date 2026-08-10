using UnityEngine;

[RequireComponent(typeof(Collider))]
public class GrapplePoint : MonoBehaviour
{
    private void Reset()
    {
        gameObject.tag = "Grapple";
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 0.3f);
    }
}