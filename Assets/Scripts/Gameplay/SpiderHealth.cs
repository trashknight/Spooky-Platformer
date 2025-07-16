using UnityEngine;
using System.Collections;
using Platformer.Gameplay;      
using static Platformer.Core.Simulation; 
using Platformer.Mechanics;     

public class SpiderHealth : MonoBehaviour
{
    public int maxHealth = 10;
    private int currentHealth;

    public AudioClip hitSound;
    public AudioClip finalRoarSound;
    private AudioSource audioSource;

    public SpriteRenderer spriteRenderer;     
    public Sprite hitFlashSprite;
    public Sprite neutralSprite;              
    public float flashDuration = 0.1f;

    public GameObject hitParticles;
    public GameObject deathParticles;
    public Transform hitEffectPoint;

    private BossAttackBehavior bossAttack;    

    public GameObject bottlePrefab;           

    public float bottleDropSpawnY = 16.0f;    
    
    // --- NEW VARIABLE: Bottle Spawn Ascent Progress ---
    [Range(0f, 1f)] // Restricts value in Inspector to between 0 and 1
    public float bottleSpawnAscentProgress = 0.5f; // Default to halfway (50%)
    // ----------------------------------------------------

    public Transform player;                  
    public Animator childAnimator;            

    void Start()
    {
        currentHealth = maxHealth;
        audioSource = GetComponent<AudioSource>();
        bossAttack = GetComponent<BossAttackBehavior>();

        if (childAnimator == null)
        {
            Transform child = transform.Find("Spider Idle");
            if (child != null)
            {
                childAnimator = child.GetComponent<Animator>();
            }
        }
        
        if (player == null) Debug.LogWarning("Player Transform not assigned to SpiderHealth! Cannot determine roar direction.");
        if (childAnimator == null) Debug.LogWarning("Child Animator not assigned or found for SpiderHealth! Roar animations may not play.");
        if (neutralSprite == null) Debug.LogWarning("Neutral Sprite not assigned to SpiderHealth! Spider may not revert visually after roar.");
        if (bottlePrefab == null) Debug.LogWarning("Bottle Prefab not assigned to SpiderHealth! Bottle will not drop.");
    }

    public void TakeDamage(int amount)
    {
        if (currentHealth <= 0) return;

        currentHealth -= amount;
        Debug.Log($"Spider took {amount} damage. Current health: {currentHealth}");

        if (hitSound != null)
            audioSource.PlayOneShot(hitSound);

        if (hitParticles != null && hitEffectPoint != null)
            Instantiate(hitParticles, hitEffectPoint.position, Quaternion.identity);

        if (currentHealth > 0)
        {
            StartCoroutine(FlashOnHit());
        }
        else
        {
            Die();
        }
    }

    IEnumerator FlashOnHit()
    {
        if (bossAttack != null && bossAttack.isSpitting)
            yield break;

        if (spriteRenderer != null && hitFlashSprite != null)
        {
            Sprite original = spriteRenderer.sprite;
            spriteRenderer.sprite = hitFlashSprite;
            yield return new WaitForSeconds(flashDuration);
            spriteRenderer.sprite = original;
        }
    }

    void Die()
    {
        Debug.Log("Spider defeated! Initiating death sequence.");

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        if (bossAttack != null)
        {
            bossAttack.StopAttack();
            bossAttack.enabled = false;
        }

        StartCoroutine(DeathSequence());
    }

    IEnumerator DeathSequence()
    {
        if (spriteRenderer != null && !spriteRenderer.enabled)
        {
            spriteRenderer.enabled = true;
            Debug.Log("Main sprite renderer re-enabled for death sequence.");
        }

        // --- Roar Direction Logic (Preserved) ---
        if (player != null)
        {
            Vector3 currentScale = transform.localScale;
            float originalScaleXAbsolute = Mathf.Abs(currentScale.x); 

            Debug.Log($"Player X: {player.position.x}, Boss X: {transform.position.x}. Deciding death roar direction.");

            if (player.position.x < transform.position.x)
            {
                Debug.Log("Player is LEFT of Boss. Setting parent scale to LEFT (positive) and triggering FinalRoarLeft.");
                currentScale.x = originalScaleXAbsolute;
                
                if (childAnimator != null)
                {
                    childAnimator.enabled = true;
                    childAnimator.Rebind();
                    childAnimator.Update(0f);
                    childAnimator.SetTrigger("FinalRoarLeft");
                    Debug.Log("Animator trigger FinalRoarLeft set.");
                }
            }
            else
            {
                Debug.Log("Player is RIGHT of Boss. Setting parent scale to RIGHT (negative) and triggering FinalRoarRight.");
                currentScale.x = -originalScaleXAbsolute;

                if (childAnimator != null)
                {
                    childAnimator.enabled = true;
                    childAnimator.Rebind();
                    childAnimator.Update(0f);
                    childAnimator.SetTrigger("FinalRoarRight");
                    Debug.Log("Animator trigger FinalRoarRight set.");
                }
            }
            transform.localScale = currentScale;
            Debug.Log($"Spider (parent) final scale.x for death roar: {transform.localScale.x} ({(transform.localScale.x > 0 ? "Left" : "Right")}).");
        }
        else
        {
            Debug.LogError("Player Transform not assigned in SpiderHealth script. Cannot determine direction for death roar. Defaulting to FinalRoarRight.");
            if (childAnimator != null)
            {
                childAnimator.enabled = true;
                childAnimator.Rebind();
                childAnimator.Update(0f);
                childAnimator.SetTrigger("FinalRoarRight");
                transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            }
        }
        // --- End Roar Direction Logic ---

        if (audioSource != null && finalRoarSound != null)
            audioSource.PlayOneShot(finalRoarSound);
        else
            Debug.LogWarning("Final roar sound or audio source not assigned for death sequence.");

        if (deathParticles != null && hitEffectPoint != null)
            Instantiate(deathParticles, hitEffectPoint.position, Quaternion.identity);
        else
            Debug.LogWarning("Death particles or hit effect point not assigned for death sequence.");

        Debug.Log("Waiting for death roar animation to complete before ascent...");
        yield return new WaitForSeconds(2.0f);
        Debug.Log("Death roar animation wait complete. Preparing for ascent.");

        // --- FIX 1: Roar ending on roar frame instead of reverting ---
        if (childAnimator != null)
        {
            childAnimator.enabled = false;
            Debug.Log("Child Animator disabled after death roar for ascent.");
        }

        if (spriteRenderer != null && neutralSprite != null) 
        {
            spriteRenderer.sprite = neutralSprite; 
            Debug.Log("Main sprite renderer set to neutral sprite for ascent.");
        }
        else if (spriteRenderer != null && bossAttack != null && bossAttack.riseSprite != null)
        {
            spriteRenderer.sprite = bossAttack.riseSprite;
            Debug.Log("Main sprite renderer set to rise sprite from BossAttackBehavior for ascent.");
        }
        else
        {
            Debug.LogWarning("Could not set sprite for ascent. Neither neutralSprite nor BossAttackBehavior's riseSprite are assigned or available.");
        }
        // --- END FIX 1 ---


        Vector3 flyTarget = new Vector3(transform.position.x, transform.position.y + 20f, 0); 
        Vector3 startAscentPos = transform.position;
        float t = 0f;
        float flyDuration = 1.5f;

        // Flag to ensure bottle only spawns once inside the loop
        bool bottleSpawnedDuringAscent = false; 

        Debug.Log($"Starting ascent from {startAscentPos} to {flyTarget}.");
        while (t < flyDuration)
        {
            transform.position = Vector3.Lerp(startAscentPos, flyTarget, t / flyDuration);
            t += Time.deltaTime;

            // --- Bottle Spawn Timing using bottleSpawnAscentProgress ---
            if (!bottleSpawnedDuringAscent && t >= flyDuration * bottleSpawnAscentProgress) // Now uses the new variable!
            {
                if (bottlePrefab != null)
                {
                    Vector3 bottleSpawnPosition = new Vector3(transform.position.x, bottleDropSpawnY, transform.position.z);
                    
                    Debug.Log($"[SpiderHealth DEBUG] Bottle spawned at calculated position: {bottleSpawnPosition.ToString()}. bottleDropSpawnY setting: {bottleDropSpawnY}. Spider's Y at spawn: {transform.position.y}.");
                    Instantiate(bottlePrefab, bottleSpawnPosition, Quaternion.identity);
                    Debug.Log($"Bottle spawned at custom Y: {bottleSpawnPosition} while spider is mid-ascent.");
                    bottleSpawnedDuringAscent = true; 
                }
                else
                {
                    Debug.LogWarning("Bottle prefab not assigned. Cannot drop bottle.");
                }
            }
            // --- End Bottle Spawn Timing ---

            yield return null; 
        }

        transform.position = flyTarget;
        Debug.Log("Spider has completed ascent and is off-screen.");

        // Fallback: Ensure bottle is spawned even if loop timing somehow prevented it (unlikely but safe)
        if (!bottleSpawnedDuringAscent && bottlePrefab != null)
        {
            Vector3 bottleSpawnPosition = new Vector3(transform.position.x, bottleDropSpawnY, transform.position.z);
            Instantiate(bottlePrefab, bottleSpawnPosition, Quaternion.identity);
            Debug.Log($"Bottle spawned as fallback after ascent at: {bottleSpawnPosition}.");
        }

        gameObject.SetActive(false); 
        Debug.Log("Spider GameObject deactivated. Boss fight sequence complete.");
    }
}