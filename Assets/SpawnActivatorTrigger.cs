using System.Collections;
using UnityEngine;
using Platformer.Mechanics;

public class SpawnActivatorTrigger : MonoBehaviour
{
    public int spawnPointId;
    public int checkpointId;
    public Transform VFXpoint;
    public SpriteRenderer spriteRenderer;
    public Sprite activatedSprite;
    public GameObject activationVFX;
    public float activationDuration = 2f;
    public Transform activationVFXTransform;

    GameManager gameManager;
    private bool hasActivatedOnce = false;

    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();

        // If this checkpoint was already activated in this run
        if (gameManager.activatedCheckpoints.Contains(checkpointId))
        {
            spriteRenderer.sprite = activatedSprite;
            hasActivatedOnce = true;

            // Play effects if this is the active spawn point on respawn
            if (gameManager.spawnPointId == spawnPointId)
            {
                PlayCheckpointEffects();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasActivatedOnce) return;

        var playerHealth = other.gameObject.GetComponent<Health>();
        var controller = other.gameObject.GetComponent<PlayerController>();

        if (playerHealth != null && controller != null)
        {
            bool isNewCheckpoint = (spawnPointId != gameManager.spawnPointId);

            controller.spawnPoint = this.transform;
            playerHealth.respawnVFXTransform = VFXpoint;
            spriteRenderer.sprite = activatedSprite;

            PlayCheckpointEffects(controller);

            if (isNewCheckpoint)
            {
                gameManager.spawnPointId = spawnPointId;
                gameManager.SaveCollectedTokensAtCheckpoint();
                gameManager.savedScore += gameManager.unsavedScore;
                gameManager.unsavedScore = 0;

                playerHealth.Reset();

                if (!gameManager.activatedCheckpoints.Contains(checkpointId))
                    gameManager.activatedCheckpoints.Add(checkpointId);

                hasActivatedOnce = true;

                Debug.Log("SpawnActivatorTrigger: First-time checkpoint activated. Tokens saved. Health restored.");
            }
            else
            {
                Debug.Log("SpawnActivatorTrigger: Respawned at existing checkpoint. Tokens not saved again.");
            }
        }
    }

    void PlayCheckpointEffects(PlayerController controller = null)
    {
        if (activationVFX != null && activationVFXTransform != null)
        {
            GameObject activation = Instantiate(activationVFX, activationVFXTransform.position, activationVFXTransform.rotation);
            Destroy(activation, activationDuration);
        }

        if (controller == null)
            controller = FindObjectOfType<PlayerController>();

        if (controller != null && controller.audioSource && controller.spawnpointAudio)
        {
            controller.audioSource.PlayOneShot(controller.spawnpointAudio);
        }
    }
}