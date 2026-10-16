using UnityEngine;

public class DeathZone : MonoBehaviour
{
    private PlayerController player;

    void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            if(player == null)
            {
                player = other.GetComponent<PlayerController>();
            }
            player.TeleportToLastCheckpoint();
        }
    }
}
