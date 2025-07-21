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

    private void OnTriggerEnter2D(Collider2D other)
    {
        gameManager = FindObjectOfType<GameManager>();
        var playerHealth = other.gameObject.GetComponent<Health>();
        var controller = other.gameObject.GetComponent<PlayerController>();

        if (playerHealth != null && controller != null)
        {
            bool isNewCheckpoint = (spawnPointId != gameManager.spawnPointId);

            // ✅ Set new spawn point
            controller.spawnPoint = this.transform;

            // ✅ Save tokens
            gameManager.SaveCollectedTokensAtCheckpoint();

            // ✅ Score
            gameManager.spawnPointId = spawnPointId;
            gameManager.savedScore += gameManager.unsavedScore;
            gameManager.unsavedScore = 0;

            // ✅ Restore health only if this is a new checkpoint
            if (isNewCheckpoint)
            {
                playerHealth.Reset();
                Debug.Log("SpawnActivatorTrigger: New checkpoint activated — health restored.");
            }
            else
            {
                Debug.Log("SpawnActivatorTrigger: Checkpoint already active — health not restored.");
            }

            // ✅ Visuals
            playerHealth.respawnVFXTransform = VFXpoint;
            spriteRenderer.sprite = activatedSprite;

            if (controller.audioSource && controller.spawnpointAudio)
                controller.audioSource.PlayOneShot(controller.spawnpointAudio);

            GameObject activation = Instantiate(activationVFX, activationVFXTransform.position, activationVFXTransform.rotation);
            Destroy(activation, activationDuration);
        }
    }
}