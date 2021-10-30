using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Platformer.Mechanics;

public class SpawnActivatorTrigger : MonoBehaviour
{
    // Start is called before the first frame update
    public Transform spawnpoint;
    public Transform VFXpoint;
    public SpriteRenderer spriteRenderer;
    public Sprite activatedSprite;
    public GameObject activationVFX;
    public float activationDuration = 2f;
    public Transform activationVFXTransform;


    private void OnTriggerEnter2D(Collider2D other) {
        // add any other effects for getting the checkpoint
        var player = other.gameObject.GetComponent<Health>();
        if (player != null) {
            player.spawnPoint = spawnpoint;
            player.respawnVFXTransform = VFXpoint;
            spriteRenderer.sprite = activatedSprite;
            GameObject activation = Instantiate(activationVFX, activationVFXTransform.position, activationVFXTransform.rotation);
            Destroy(activation, activationDuration);
        }
    }
}
