using Platformer.Core;
using Platformer.Mechanics;
using Platformer.Model;
using UnityEngine;
using Cinemachine;
using System.Collections;

namespace Platformer.Gameplay
{
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

            Transform particleSpawnPoint = null;

            // --- Determine correct spawn point based on checkpoint ---
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

                    // Try to find VFXOnlyPoint as child of checkpoint's parent
                    if (checkpoint.VFXpoint != null && checkpoint.VFXpoint.parent != null)
                    {
                        Transform vfxChild = checkpoint.VFXpoint.parent.Find("VFXOnlyPoint");
                        if (vfxChild != null)
                        {
                            particleSpawnPoint = vfxChild;
                            Debug.Log("PlayerSpawn: Found VFXOnlyPoint under checkpoint parent.");
                        }
                    }

                    break;
                }
            }

            // --- Fallback to original spawn point ---
            if (controller.spawnPoint == null)
            {
                Debug.LogWarning("PlayerSpawn: No checkpoint matched. Falling back to original spawn point.");
                controller.spawnPoint = playerObj.transform;
            }

            if (particleSpawnPoint == null)
            {
                GameObject taggedVFX = GameObject.FindGameObjectWithTag("InitialSpawnVFX");
                if (taggedVFX != null)
                {
                    particleSpawnPoint = taggedVFX.transform;
                    Debug.Log("PlayerSpawn: Found fallback InitialSpawnVFX by tag.");
                }
                else
                {
                    particleSpawnPoint = controller.spawnPoint;
                    Debug.Log("PlayerSpawn: No tagged VFX fallback found. Using spawnPoint for particle.");
                }
            }

            // --- Disable camera damping temporarily ---
            CinemachineVirtualCamera vcam = model.virtualCamera;
            var transposer = vcam.GetCinemachineComponent<CinemachineFramingTransposer>();
            float originalXDamping = transposer.m_XDamping;
            float originalYDamping = transposer.m_YDamping;

            transposer.m_XDamping = 0f;
            transposer.m_YDamping = 0f;

            // --- Execute Spawn ---
            combat.attackEnabled = false;
            player.Teleport(controller.spawnPoint.position);

            if (player.respawnVFX != null && particleSpawnPoint != null)
            {
                Debug.Log($"Spawning particle at: {particleSpawnPoint.position}");
                GameObject.Instantiate(player.respawnVFX, particleSpawnPoint.position, Quaternion.identity);
            }

            player.animator.SetBool("Spawning", true);
            player.health.IsAlive = true;
            player.collider2d.enabled = true;
            player.controlEnabled = false;

            // --- Delayed dirt sound effect ---
            if (player.audioSource)
            {
                if (!player.hasSpawnedOnce && player.firstSpawnAudio != null)
                    player.StartCoroutine(PlaySpawnSoundDelayed(player.firstSpawnAudio, 0.1f));
                else if (player.respawnAudio != null)
                    player.StartCoroutine(PlaySpawnSoundDelayed(player.respawnAudio, 0.1f));

                player.hasSpawnedOnce = true;
            }

            player.health.Reset();
            combat.facingRight = true;
            player.jumpState = PlayerController.JumpState.Grounded;

            model.virtualCamera.m_Follow = player.transform;
            model.virtualCamera.m_LookAt = player.transform;

            // --- Re-enable damping after short delay ---
            player.StartCoroutine(RestoreCameraDamping(transposer, originalXDamping, originalYDamping, 0.1f));

            combat.FadeInAfterRespawn(0.05f);
            Simulation.Schedule<EnablePlayerInput>(2.1f);
        }

        private IEnumerator RestoreCameraDamping(CinemachineFramingTransposer transposer, float xDamp, float yDamp, float delay)
        {
            yield return new WaitForSeconds(delay);
            transposer.m_XDamping = xDamp;
            transposer.m_YDamping = yDamp;
            Debug.Log("Camera damping restored.");
        }

        private IEnumerator PlaySpawnSoundDelayed(AudioClip clip, float delay)
        {
            yield return new WaitForSeconds(delay);
            var player = model.player;
            if (player != null && player.audioSource && clip != null)
            {
                player.audioSource.PlayOneShot(clip);
                Debug.Log("Spawn sound played: " + clip.name);
            }
        }
    }
}