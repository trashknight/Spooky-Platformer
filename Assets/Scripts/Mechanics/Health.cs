using System.Collections;
using Platformer.Gameplay;
using UnityEngine;
using static Platformer.Core.Simulation;

namespace Platformer.Mechanics
{
    /// <summary>
    /// Represebts the current vital statistics of some game entity.
    /// </summary>
    public class Health : MonoBehaviour
    {
        /// <summary>
        /// The maximum hit points for the entity.
        /// </summary>
        public int maxHP = 3;

        public GameObject respawnVFX;
        public float respawnDuration = 2f;
        public Transform respawnVFXTransform;
        public GameObject life1, life2, life3;
        public bool isInvincible = false;
        public float invincibilityDurationSeconds = 1.5f;
        public float invincibilityDeltaTime = 0.10f;
        public SpriteRenderer sprite;
        private PlayerController player;
        private AudioSource audiosrc;
        public Animator animator;
        private Combat combat;
        public Transform[] spawnpoints;
        

        /// <summary>
        /// Indicates if the entity should be considered 'alive'.
        /// </summary>
        public bool IsAlive = true;


        int currentHP;

        private void Start() {
            player = GetComponent<PlayerController>();
            audiosrc = GetComponent<AudioSource>();
            animator = GetComponent<Animator>();
            combat = GetComponent<Combat>();
        }

        /// <summary>
        /// Increment the HP of the entity.
        /// </summary>
        public void Increment()
        {
            currentHP = Mathf.Clamp(currentHP + 1, 0, maxHP);
            UpdateLives();
        }

        public void Reset() {
            currentHP = maxHP;
            UpdateLives();
            isInvincible = false;
            combat.EnableAttack();
        }

        /// <summary>
        /// Decrement the HP of the entity. Will trigger a HealthIsZero event when
        /// current HP reaches 0.
        /// </summary>
        public void Decrement()
        {
            // Should trigger ouhch and invincibility frames
            if (!isInvincible) {
                if (audiosrc && player.ouchAudio)
                    audiosrc.PlayOneShot(player.ouchAudio);
                animator.SetTrigger("hurt");
                currentHP = Mathf.Clamp(currentHP - 1, 0, maxHP);
                UpdateLives();
                if (currentHP == 0)
                {
                    Debug.Log("Health is zero");
                    isInvincible = true;
                    var ev = Schedule<HealthIsZero>();
                    ev.health = this;
                } else {
                    StartCoroutine(BecomeTemporarilyInvincible());
                }
            }
            
        }

        private IEnumerator BecomeTemporarilyInvincible() {
            isInvincible = true;
            for (float i = 0; i < invincibilityDurationSeconds; i += invincibilityDeltaTime)
                {
                    if (sprite.enabled) {
                        sprite.enabled = false;
                    } else {
                        sprite.enabled = true;
                    }
                    yield return new WaitForSeconds(invincibilityDeltaTime);
                }
            sprite.enabled = true;
            isInvincible = false;
            combat.EnableAttack();
        }

        /// <summary>
        /// Decrement the HP of the entitiy until HP reaches 0.
        /// </summary>
        public void Die()
        {
            while (currentHP > 0) Decrement();
        }

        void Awake()
        {
            currentHP = maxHP;
        }

        void MakeRespawnVFX () {
            GameObject respawn = Instantiate(respawnVFX, respawnVFXTransform.position, respawnVFXTransform.rotation);
            Destroy(respawn, respawnDuration);
        }

        void UpdateLives() {

        switch (currentHP) {
            case 3:
                life1.gameObject.SetActive(true);
                life2.gameObject.SetActive(true);
                life3.gameObject.SetActive(true);
                break;
            case 2:
                life1.gameObject.SetActive(true);
                life2.gameObject.SetActive(true);
                life3.gameObject.SetActive(false);
                break;
            case 1:
                life1.gameObject.SetActive(true);
                life2.gameObject.SetActive(false);
                life3.gameObject.SetActive(false);
                break;
            case 0:
                life1.gameObject.SetActive(false);
                life2.gameObject.SetActive(false);
                life3.gameObject.SetActive(false);
                break;
        } 
        }
    }
}
