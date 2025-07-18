using System.Collections;
using Platformer.Core;
using Platformer.Model;
using Platformer.Mechanics;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Platformer.Gameplay
{
    public class PlayerDeath : Simulation.Event<PlayerDeath>
    {
        PlatformerModel model = Simulation.GetModel<PlatformerModel>();
        Combat combat = GameObject.FindGameObjectWithTag("Player").GetComponent<Combat>();
        GameController gameController = GameObject.FindObjectOfType<GameController>();

        public override void Execute()
        {
            Debug.Log("PlayerDeath.Execute() started.");

            combat = GameObject.FindGameObjectWithTag("Player").GetComponent<Combat>();
            var player = model.player;

            if (!player.health.IsAlive)
            {
                Debug.Log("PlayerDeath.Execute(): Player is marked dead. Continuing death sequence...");

                // Ensure menu won't show on reload
                GameManager.Instance.showMenu = false;

                // Start blackout early in case the menu flashes
                combat.SetBlackoutInstant(); // NEW: Force blackout immediately

                // Standard death logic
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

                if (combat != null)
                {
                    Debug.Log("PlayerDeath.Execute(): Starting fade coroutine.");
                    player.StartCoroutine(DeathFadeThenReload(combat));
                }
                else
                {
                    Debug.LogError("PlayerDeath.Execute(): Combat reference is null!");
                }
            }
            else
            {
                Debug.Log("PlayerDeath.Execute(): Called while still alive — ignoring.");
            }
        }

        IEnumerator DeathFadeThenReload(Combat combat)
        {
            Debug.Log("PlayerDeath.DeathFadeThenReload() coroutine started.");
            yield return combat.FadeToBlack(1.5f); // Smooth fade to cover reset
            Debug.Log("Fade complete. Now reloading via GameResetter.");

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