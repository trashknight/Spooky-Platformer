using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Platformer.Mechanics;

public class SpawnActivatorTrigger : MonoBehaviour
{
    // Start is called before the first frame update
    public Transform spawnpoint;
    public Transform VFXpoint;
    

    private void OnTriggerEnter2D(Collider2D other) {
        // add any other effects for getting the checkpoint
        var player = other.gameObject.GetComponent<Health>();
        if (player != null) {
            player.spawnPoint = spawnpoint;
            player.respawnVFXTransform = VFXpoint;
        }
    }
}
