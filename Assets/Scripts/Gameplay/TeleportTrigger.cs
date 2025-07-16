using UnityEngine;

public class TeleportTrigger : MonoBehaviour
{
    public Transform teleportTarget;   // Where to send the player
    public GameObject player;          // The player GameObject

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject == player)
        {
            player.transform.position = teleportTarget.position;
        }
    }
}