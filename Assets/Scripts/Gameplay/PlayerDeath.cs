using System.Collections;
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
                GameManager.Instance.showMenu = false; // ✅ Prevent menu after reload

                player.health.IsAlive = false;
                player.health.Die();
                model.virtualCamera.m_Follow = null;
                model.virtualCamera.m_LookAt = null;
                player.controlEnabled = false;
                player.loseUnsavedPoints();

                if (player.audioSource && player.ouchAudio)
                    player.audioSource.PlayOneShot(player.ouchAudio);

                player.animator.SetTrigger("hurt");
                player.animator.SetBool("dead", true);

                if (gameController)
                    gameController.enabled = false;

                // ✅ START coroutine for fade + reload
                if (combat != null)
                {
                    player.StartCoroutine(DeathFadeThenReload(combat));
                }
            }
        }

        // ✅ ADD THIS COROUTINE BELOW Execute()
        IEnumerator DeathFadeThenReload(Combat combat)
        {
            Debug.Log("PlayerDeath: Starting fade to black...");
            yield return combat.FadeToBlack(1.5f);

            Debug.Log("PlayerDeath: Fade complete. Reloading scene...");
            if (GameResetter.Instance != null)
            {
                GameResetter.Instance.StartCoroutine(GameResetter.Instance.FadeAndReloadSequence());
            }
            else
            {
                Debug.LogError("PlayerDeath: GameResetter is null!");
            }
        }
    }
}