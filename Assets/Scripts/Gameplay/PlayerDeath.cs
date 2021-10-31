using System.Collections;
using System.Collections.Generic;
using Platformer.Core;
using Platformer.Model;
using Platformer.Mechanics;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Platformer.Gameplay
{
    /// <summary>
    /// Fired when the player has died.
    /// </summary>
    /// <typeparam name="PlayerDeath"></typeparam>
    public class PlayerDeath : Simulation.Event<PlayerDeath>
    {
        
        PlatformerModel model = Simulation.GetModel<PlatformerModel>();
        Combat combat = GameObject.FindGameObjectWithTag("Player").GetComponent<Combat>();
        GameController gameController = GameObject.FindObjectOfType<GameController>();

        public override void Execute()
        {
            combat = GameObject.FindGameObjectWithTag("Player").GetComponent<Combat>();
            Debug.Log("Executing player death");
            combat.attackEnabled = false;
            var player = model.player;
            if (player.health.IsAlive)
            {
                player.health.IsAlive = false;
                player.health.Die();
                model.virtualCamera.m_Follow = null;
                model.virtualCamera.m_LookAt = null;
                // player.collider.enabled = false;
                player.controlEnabled = false;
                player.loseUnsavedPoints();

                if (player.audioSource && player.ouchAudio)
                    player.audioSource.PlayOneShot(player.ouchAudio);
                player.animator.SetTrigger("hurt");
                player.animator.SetBool("dead", true);
                if (gameController)
                    gameController.enabled = false;
                //Simulation.Schedule<PlayerSpawn>(2);
                //SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                combat.ReloadScene(1.5f);
            }
        }
    }
}