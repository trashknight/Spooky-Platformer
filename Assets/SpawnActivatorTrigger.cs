using System.Collections;
using UnityEngine;
using Platformer.Mechanics;

public class SpawnActivatorTrigger : MonoBehaviour
{
    public int spawnPointId;
    public Transform VFXpoint;
    public SpriteRenderer spriteRenderer;
    public Sprite activatedSprite;
    public GameObject activationVFX;
    public float activationDuration = 2f;
    public Transform activationVFXTransform;

    GameManager gameManager;
    private bool hasActivatedOnce = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasActivatedOnce) return; // Don't trigger twice during same frame

        var playerHealth = other.gameObject.GetComponent<Health>();
        var controller = other.gameObject.GetComponent<PlayerController>();

        if (playerHealth != null && controller != null)
        {
            gameManager = FindObjectOfType<GameManager>();
            bool isNewCheckpoint = (spawnPointId != gameManager.spawnPointId);

            // Always assign the player's spawn point and play visuals/sounds
            controller.spawnPoint = this.transform;
            playerHealth.respawnVFXTransform = VFXpoint;
            spriteRenderer.sprite = activatedSprite;

            if (controller.audioSource && controller.spawnpointAudio)
                controller.audioSource.PlayOneShot(controller.spawnpointAudio);

            GameObject activation = Instantiate(activationVFX, activationVFXTransform.position, activationVFXTransform.rotation);
            Destroy(activation, activationDuration);

            // Only perform checkpoint save logic once per checkpoint
            if (isNewCheckpoint)
            {
                gameManager.SaveCollectedTokensAtCheckpoint();
                gameManager.spawnPointId = spawnPointId;
                gameManager.savedScore += gameManager.unsavedScore;
                gameManager.unsavedScore = 0;

                playerHealth.Reset();
                Debug.Log("SpawnActivatorTrigger: First-time checkpoint activated. Tokens saved. Health restored.");

                hasActivatedOnce = true;
            }
            else
            {
                Debug.Log("SpawnActivatorTrigger: Respawned at existing checkpoint. Tokens not saved again.");
            }
        }
    }
}