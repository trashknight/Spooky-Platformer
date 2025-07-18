using Platformer.Core;
using Platformer.Mechanics;
using Platformer.Model;
using UnityEngine;

namespace Platformer.Gameplay
{
    /// <summary>
    /// Fired when the player is spawned after dying.
    /// </summary>
    public class PlayerSpawn : Simulation.Event<PlayerSpawn>
    {
        PlatformerModel model = Simulation.GetModel<PlatformerModel>();

        public override void Execute()
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

            if (playerObj == null)
            {
                Debug.LogError("PlayerSpawn: No GameObject with tag 'Player' found.");
                return;
            }

            var player = model.player;
            var controller = playerObj.GetComponent<PlayerController>();
            var combat = playerObj.GetComponent<Combat>();
            var sprite = playerObj.GetComponent<SpriteRenderer>();
            var gameManager = GameManager.Instance;

            // ✅ Reassign checkpoint spawnPoint based on saved spawnPointId
            SpawnActivatorTrigger[] checkpoints = GameObject.FindObjectsOfType<SpawnActivatorTrigger>();
            foreach (var checkpoint in checkpoints)
            {
                if (checkpoint.spawnPointId == gameManager.spawnPointId)
                {
                    if (checkpoint.VFXpoint != null)
                    {
                        controller.spawnPoint = checkpoint.VFXpoint;
                        Debug.Log($"PlayerSpawn: Found spawn marker for spawnPointId {gameManager.spawnPointId}.");
                    }
                    else
                    {
                        controller.spawnPoint = checkpoint.transform;
                        Debug.LogWarning($"PlayerSpawn: No VFXpoint assigned for checkpoint {gameManager.spawnPointId}, using root transform.");
                    }
                    break;
                }
            }

            if (controller.spawnPoint == null)
            {
                Debug.LogWarning("PlayerSpawn: No matching checkpoint found — falling back to initial spawn point.");
                controller.spawnPoint = playerObj.transform; // fallback
            }

            // Disable combat during respawn
            combat.attackEnabled = false;

            // Move player to the correct spawn point
            player.Teleport(controller.spawnPoint.position);
            player.animator.SetBool("Spawning", true);
            player.health.IsAlive = true;
            player.collider2d.enabled = true;
            player.controlEnabled = false;

            if (player.audioSource && player.respawnAudio)
                player.audioSource.PlayOneShot(player.respawnAudio);

            player.health.Reset();
            combat.facingRight = true;
            player.jumpState = PlayerController.JumpState.Grounded;

            model.virtualCamera.m_Follow = player.transform;
            model.virtualCamera.m_LookAt = player.transform;

            // Fade in after player is moved and camera is tracking
            combat.FadeInAfterRespawn(0.05f);

            // Re-enable control after short delay
            Simulation.Schedule<EnablePlayerInput>(2.1f);
        }
    }
}