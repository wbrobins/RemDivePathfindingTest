using UnityEngine;

public class LookDetector : MonoBehaviour
{
    [SerializeField] private float lookDistance = 10f;
    [SerializeField] private LayerMask lookMask = ~0;
    [SerializeField] private PlayerHand playerHand;   
    

    private LookTarget current;

    private void Update()
    {
        LookTarget hitTarget = null;

        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, lookDistance, lookMask, QueryTriggerInteraction.Ignore))
        {
            hitTarget = hit.collider.GetComponentInParent<LookTarget>();
        }

        if (hitTarget != current)
        {
            if (current != null) current.ShowMessage(false);
            current = hitTarget;
        }

        if (current != null) current.ShowMessage(!playerHand.Carrying);
    }
}