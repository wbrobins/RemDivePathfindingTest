using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    private PlayerController player;
    [SerializeField] private bool touched = false;

    void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player" && !touched)
        {
            if(player == null)
            {
                player = other.GetComponent<PlayerController>();
            }

            player.UpdateCheckpoint(transform.position);
            touched = true;
        }
    }
}
