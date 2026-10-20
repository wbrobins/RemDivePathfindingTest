using UnityEngine;

public class LookDetector : MonoBehaviour
{
    [Tooltip("Raycast range. Set this to at least the largest LookDistance on any LookTarget.")]
    [SerializeField] private float maxLookDistance = 25f;
    [SerializeField] private LayerMask lookMask = ~0;
    [SerializeField] private PlayerHand playerHand;

    private LookTarget current;

    private void Update()
    {
        LookTarget hitTarget = null;

        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, maxLookDistance, lookMask, QueryTriggerInteraction.Ignore))
        {
            LookTarget candidate = hit.collider.GetComponentInParent<LookTarget>();

            if (candidate != null && hit.distance <= candidate.LookDistance)
            {
                hitTarget = candidate;
            }
        }

        if (hitTarget != current)
        {
            if (current != null) current.ShowMessage(false);
            current = hitTarget;
        }

        if (current != null) current.ShowMessage(!playerHand.Carrying);
    }
}