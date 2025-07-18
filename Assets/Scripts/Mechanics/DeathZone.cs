using System.Collections;
using System.Collections.Generic;
using Platformer.Gameplay;
using UnityEngine;
using Platformer.Core;
using Platformer.Model;
using static Platformer.Core.Simulation;
using UnityEngine.SceneManagement;

namespace Platformer.Mechanics
{
    /// <summary>
    /// DeathZone components mark a collider which will schedule a
    /// PlayerEnteredDeathZone event when the player enters the trigger.
    /// </summary>
    public class DeathZone : MonoBehaviour
    {
        Combat combat;
        PlatformerModel model = Simulation.GetModel<PlatformerModel>();
        GameController gameController;

        private void Start()
        {
            combat = GameObject.FindGameObjectWithTag("Player").GetComponent<Combat>();
            gameController = GameObject.FindObjectOfType<GameController>();
        }

        void OnTriggerEnter2D(Collider2D collider)
        {
            var p = collider.gameObject.GetComponent<PlayerController>();
            var e = collider.gameObject.GetComponent<EnemyController>();

            if (p != null)
            {
                Debug.Log("DeathZone: Player entered death zone.");

                model.virtualCamera.m_Follow = null;
                model.virtualCamera.m_LookAt = null;
                p.loseUnsavedPoints();

                if (p.audioSource && p.ouchAudio)
                {
                    Debug.Log("DeathZone: Playing ouch audio.");
                    p.audioSource.PlayOneShot(p.ouchAudio);
                }

                gameController.enabled = false;

                if (p.health != null)
                {
                    Debug.Log("DeathZone: Calling TakeDamage with lethal flag. IsAlive = " + p.health.IsAlive);
                    p.health.ForceDeath();
                }
                else
                {
                    Debug.LogError("DeathZone: Player has no Health component!");
                }
            }

            if (e != null)
            {
                Debug.Log("DeathZone: Enemy fell into pit and was destroyed.");
                Destroy(collider.gameObject);
            }
        }
    }
}