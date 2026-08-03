using UnityEngine;

public class PlayerHand : MonoBehaviour
{
    private bool carrying = false;
    [SerializeField] private new GameObject camera;
    [SerializeField] private GameObject playerWeapon;

    [SerializeField] private CarryableBehavior currentCarryable;

    public bool Carrying => carrying;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (!carrying)
            {
                Vector3 origin = camera.transform.position;
                Vector3 direction = camera.transform.forward;

                Debug.DrawRay(origin, direction * 10f, Color.yellow, 1f);

                if(Physics.Raycast(origin, direction, out RaycastHit hit, 10f))
                {
                    if(hit.collider.TryGetComponent(out CarryableBehavior carryable))
                    {
                        currentCarryable = carryable;
                        currentCarryable.Pickup(transform);
                        carrying = true;
                    }
                } 
            }
            else if (carrying)
            {
                currentCarryable.Drop();
                currentCarryable = null;
                carrying = false;
            }
        }
    }
}
