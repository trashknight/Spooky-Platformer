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
        Combat combat = GameObject.FindGameObjectWithTag("Player").GetComponent<Combat>();
        SpriteRenderer sprite = GameObject.FindGameObjectWithTag("Player").GetComponent<SpriteRenderer>();
        GameManager gameManager = GameObject.FindObjectOfType<GameManager>();

        public override void Execute()
        {
            var player = model.player;
            player.Teleport(player.spawnPoint.transform.position);
            player.animator.SetBool("Spawning", true);
            player.health.IsAlive = true;
            player.collider2d.enabled = true;
            player.controlEnabled = false;
            if (player.audioSource && player.respawnAudio)
                player.audioSource.PlayOneShot(player.respawnAudio);
            player.health.Reset();
            //sprite.flipX = false;
            combat.facingRight = true;
            player.jumpState = PlayerController.JumpState.Grounded;
            model.virtualCamera.m_Follow = player.transform;
            model.virtualCamera.m_LookAt = player.transform;
            Simulation.Schedule<EnablePlayerInput>(2.1f);
        }
    }
}