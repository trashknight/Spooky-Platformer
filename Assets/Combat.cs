using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // Needed for Image component

public class Combat : MonoBehaviour
{
    public Animator animator;       // Assign in Inspector
    public AudioSource audioSource; // Assign in Inspector
    public AudioClip biteAudio;     // Assign in Inspector

    public Transform attackPointR;  // Assign in Inspector
    public Transform attackPointL;  // Assign in Inspector
    public float attackRange = 0.5f;
    public LayerMask enemyLayers;   // Assign in Inspector

    public int attackDamage = 1;

    public float attackRate = 2f;
    float nextattackTime =0f;
    public bool attackEnabled = true;
    public bool facingRight = true;

    GameObject blackoutSquare; // This will hold the reference to your blackout UI element.

    private void Start() {
        // --- FIX START ---
        // Attempt to find the GameObject with the "Blackout Square" tag.
        blackoutSquare = GameObject.FindGameObjectWithTag("Blackout Square");

        // Check if the GameObject was found before trying to use it.
        if (blackoutSquare != null)
        {
            // The blackout square is typically a UI Image that starts transparent and fades in/out.
            // Setting it active might be what you want, but ensure its Image component's alpha is 0
            // if you want it to start transparent.
            blackoutSquare.SetActive(true); 
            StartCoroutine(ExecuteAfterTime(0.5f));
        }
        else
        {
            // Log an error if the GameObject wasn't found, so you know what's missing.
            Debug.LogError("Combat: Blackout Square GameObject with tag 'Blackout Square' not found in scene! Fading will not work.");
        }
        // --- FIX END ---
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time >= nextattackTime)
        {
            if ((Input.GetKeyDown(KeyCode.Space)) && attackEnabled)
            {
                // Play an attack animation yass kween you go gurl
                // Make sure 'animator' is assigned in the Inspector.
                if (animator != null) {
                    animator.SetTrigger("Sexy Attack 69");
                } else {
                    Debug.LogWarning("Combat: Animator not assigned. Cannot play attack animation.");
                }
                
                // the animation will call attack()
                nextattackTime = Time.time + 1f / attackRate;
            }
        }
    }

    public void Attack()
    {
        if (attackEnabled) {
            Debug.Log("Calling attack");
            // Make sure 'audioSource' and 'biteAudio' are assigned in the Inspector.
            if (audioSource != null && biteAudio != null) {
                audioSource.PlayOneShot(biteAudio);
            } else {
                Debug.LogWarning("Combat: AudioSource or Bite Audio not assigned. Cannot play bite sound.");
            }
            
            // Detect enemies that are gonna die
            Collider2D[] hitEnemies;
            if (facingRight) {
                // Make sure 'attackPointR' is assigned in the Inspector.
                if (attackPointR != null) {
                    hitEnemies = Physics2D.OverlapCircleAll(attackPointR.position, attackRange, enemyLayers);
                } else {
                    Debug.LogWarning("Combat: attackPointR not assigned. Cannot detect enemies.");
                    hitEnemies = new Collider2D[0]; // Return empty array to prevent null reference later
                }
            }
            else {
                // Make sure 'attackPointL' is assigned in the Inspector.
                if (attackPointL != null) {
                    hitEnemies = Physics2D.OverlapCircleAll(attackPointL.position, attackRange, enemyLayers);
                } else {
                    Debug.LogWarning("Combat: attackPointL not assigned. Cannot detect enemies.");
                    hitEnemies = new Collider2D[0]; // Return empty array
                }
            }

            // Damage those baddies
            foreach(Collider2D enemy in hitEnemies)
            {
                // if you've destroyed a pumpkin, make the corresponding pumpkineer grow a new one
                PumpkinScript pumpkin = enemy.GetComponent<PumpkinScript>();
                if (pumpkin) {
                    int spawnId = pumpkin.spawnId;
                    // Note: FindObjectsOfType is generally inefficient if called frequently.
                    // Consider caching references if this loop runs often or with many objects.
                    foreach (var pumpkineer in FindObjectsOfType(typeof(ShootProjectile)) as ShootProjectile[]) {
                        if (pumpkineer.SpawnID == spawnId) {
                            pumpkineer.RespawnPumpkin();
                        }
                    }
                }
                // First try basic enemy
                Enemy e = enemy.GetComponent<Enemy>();
                if (e != null)
                {
                    e.TakeDamage(attackDamage);
                }
                else
                {
                    // Try spider boss
                    SpiderHealth spider = enemy.GetComponent<SpiderHealth>();
                    if (spider != null)
                    {
                        spider.TakeDamage(attackDamage);
                    }
                    else
                    {
                        // Try destructible poison projectile
                        DestructibleProjectile projectile = enemy.GetComponent<DestructibleProjectile>();
                        if (projectile != null)
                        {
                            projectile.DestroyProjectile();
                        }
                    }
                }
                Debug.Log("sending damage");
            }
        }
    }    

    public void EnableAttack() {
        attackEnabled = true;
        //Debug.Log("Enabling Attack");
    }

    public void DisableAttack() {
        attackEnabled = false;
        //Debug.Log("Disabling Attack");
    }

    private void OnDrawGizmosSelected()
    {
        // Add null checks for Gizmos as well to prevent errors in editor if not assigned
        if (attackPointR != null)
        {
            Gizmos.DrawWireSphere(attackPointR.position, attackRange);
        }
        if (attackPointL != null)
        {
            Gizmos.DrawWireSphere(attackPointL.position, attackRange);
        }
    }

    public void ReloadScene(float time) {
        // add in fadeout
        // Make sure blackoutSquare is not null before starting coroutine.
        if (blackoutSquare != null) {
            StartCoroutine(FadeBlackoutSquare());
        } else {
            Debug.LogError("Combat: Cannot start fade. BlackoutSquare is null.");
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); // Fallback to immediate reload
        }
    }

    IEnumerator ExecuteAfterTime(float time)
    {
        yield return new WaitForSeconds(time);
        // Make sure blackoutSquare is not null before starting coroutine.
        if (blackoutSquare != null) {
            StartCoroutine(FadeBlackoutSquare(-4));
        } else {
            Debug.LogError("Combat: Cannot start fade. BlackoutSquare is null.");
        }
    }

    IEnumerator FadeBlackoutSquare(float fadeSpeed = 4) {
        // Make sure blackoutSquare and its Image component are not null.
        if (blackoutSquare == null) {
            Debug.LogError("Combat: BlackoutSquare is null in FadeBlackoutSquare. Cannot fade.");
            yield break; // Exit coroutine
        }
        Image blackoutImage = blackoutSquare.GetComponent<Image>();
        if (blackoutImage == null) {
            Debug.LogError("Combat: BlackoutSquare does not have an Image component. Cannot fade.");
            yield break; // Exit coroutine
        }

        Color objectColor = blackoutImage.color;
        float fadeAmount;
        bool go = true;
        while (go) {
            fadeAmount = objectColor.a + (fadeSpeed/3 * Time.deltaTime);

            objectColor = new Color(objectColor.r, objectColor.g, objectColor.b, fadeAmount);
            blackoutImage.color = objectColor; // Assign back to the image component

            if ((objectColor.a >= 1) && (fadeSpeed > 0)) { // Fading in to black
                go = false;
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
            if ((objectColor.a <= 0) && (fadeSpeed < 0)) { // Fading out from black
                go = false;
            }
            yield return null;
        }        
    }
}