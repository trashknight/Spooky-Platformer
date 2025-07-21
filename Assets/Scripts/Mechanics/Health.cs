using System.Collections;
using Platformer.Gameplay;
using UnityEngine;
using static Platformer.Core.Simulation;

namespace Platformer.Mechanics
{
    /// <summary>
    /// Represents the current vital statistics of some game entity.
    /// </summary>
    public class Health : MonoBehaviour
    {
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

        public bool IsAlive = true;
        int currentHP;

        private void Awake()
        {
            currentHP = maxHP;
        }

        private void Start()
        {
            player = GetComponent<PlayerController>();
            audiosrc = GetComponent<AudioSource>();
            animator = GetComponent<Animator>();
            combat = GetComponent<Combat>();
        }

        public void Reset()
        {
            Debug.Log("Health.Reset() called. Restoring HP to max.");
            currentHP = maxHP;
            IsAlive = true; // Ensure player can take damage again
            UpdateLives();
            isInvincible = false;
            combat.EnableAttack();
        }

        public void Increment()
        {
            currentHP = Mathf.Clamp(currentHP + 1, 0, maxHP);
            UpdateLives();
        }

        public void TakeDamage(int damageAmount, bool isLethal)
        {
            if (!IsAlive)
            {
                Debug.Log("TakeDamage() called but player is already dead.");
                return;
            }

            if (isInvincible && !isLethal)
            {
                Debug.Log("TakeDamage() ignored due to invincibility.");
                return;
            }

            Debug.Log($"TakeDamage() called. Damage: {damageAmount}, Lethal: {isLethal}, CurrentHP: {currentHP}");

            if (isLethal)
            {
                currentHP = 0;
                TriggerDeath();
            }
            else
            {
                currentHP -= damageAmount;
                currentHP = Mathf.Clamp(currentHP, 0, maxHP);
                UpdateLives();

                if (currentHP <= 0)
                {
                    TriggerDeath();
                }
                else
                {
                    StartCoroutine(BecomeTemporarilyInvincible());

                    if (animator != null)
                        animator.SetTrigger("hurt");

                    // ✅ Play sound ONLY if the player is still alive
                    if (audiosrc != null && player.ouchAudio != null)
                        audiosrc.PlayOneShot(player.ouchAudio);
                }
            }
        }

        private void TriggerDeath()
        {
            if (!IsAlive)
            {
                Debug.Log("TriggerDeath() called but IsAlive already false.");
                return;
            }

            Debug.Log("TriggerDeath() executed.");
            IsAlive = false;
            UpdateLives();

            Schedule<PlayerDeath>(); // ← This is the correct call
        }

        public void ForceDeath()
        {
            Debug.Log("Health.ForceDeath() called.");
    
            // Directly trigger death, even if IsAlive is already false.
            IsAlive = false;
            currentHP = 0;
            UpdateLives();

            var deathEvent = new PlayerDeath();
            deathEvent.Execute();
        }

        private IEnumerator BecomeTemporarilyInvincible()
        {
            isInvincible = true;
            for (float i = 0; i < invincibilityDurationSeconds; i += invincibilityDeltaTime)
            {
                sprite.enabled = !sprite.enabled;
                yield return new WaitForSeconds(invincibilityDeltaTime);
            }
            sprite.enabled = true;
            isInvincible = false;
        }

        void MakeRespawnVFX()
        {
            GameObject respawn = Instantiate(respawnVFX, respawnVFXTransform.position, respawnVFXTransform.rotation);
            Destroy(respawn, respawnDuration);
        }

        void UpdateLives()
        {
            switch (currentHP)
            {
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