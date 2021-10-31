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
        private void Start() {
            combat = GameObject.FindGameObjectWithTag("Player").GetComponent<Combat>();
            gameController = GameObject.FindObjectOfType<GameController>();
            
        }
        void OnTriggerEnter2D(Collider2D collider)
        {
            var p = collider.gameObject.GetComponent<PlayerController>();
            var e = collider.gameObject.GetComponent<EnemyController>();
            if (p != null)
            {
                // var ev = Schedule<PlayerEnteredDeathZone>();
                // ev.deathzone = this;
                //SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                model.virtualCamera.m_Follow = null;
                model.virtualCamera.m_LookAt = null;
                p.loseUnsavedPoints();
                // p.health.Die();
                if (p.audioSource && p.ouchAudio)
                    p.audioSource.PlayOneShot(p.ouchAudio);
                gameController.enabled = false;
                combat.ReloadScene(0.5f);
            }
            if (e != null) {
                Destroy(collider.gameObject);
            }
        }
    }
}