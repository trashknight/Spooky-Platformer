using UnityEngine;
using System.Collections;
using Platformer.Gameplay;
using static Platformer.Core.Simulation;
using Platformer.Mechanics;
using Cinemachine;

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
    [Range(0f, 1f)] public float bottleSpawnAscentProgress = 0.5f;

    public Transform player;
    public Animator childAnimator;

    // New: Reference to Impulse Source
    public CinemachineImpulseSource impulseSource;

    void Start()
    {
        currentHealth = maxHealth;
        audioSource = GetComponent<AudioSource>();
        bossAttack = GetComponent<BossAttackBehavior>();

        if (childAnimator == null)
        {
            Transform child = transform.Find("Spider Idle");
            if (child != null)
                childAnimator = child.GetComponent<Animator>();
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

        if (hitSound != null && audioSource != null)
            audioSource.PlayOneShot(hitSound);

        if (player != null)
        {
            Combat combat = player.GetComponent<Combat>();
            if (combat != null && combat.biteAudio != null)
                audioSource.PlayOneShot(combat.biteAudio);
        }

        if (hitParticles != null && hitEffectPoint != null)
            Instantiate(hitParticles, hitEffectPoint.position, Quaternion.identity);

        if (currentHealth > 0)
            StartCoroutine(FlashOnHit());
        else
            Die();
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
        Transform damageZone = transform.Find("DamageCollider");
        if (damageZone != null)
        {
            damageZone.gameObject.SetActive(false);
            Debug.Log("DamageCollider disabled during death sequence.");
        }

        if (spriteRenderer != null && !spriteRenderer.enabled)
        {
            spriteRenderer.enabled = true;
            Debug.Log("Main sprite renderer re-enabled for death sequence.");
        }

        // Roar direction logic
        if (player != null)
        {
            Vector3 currentScale = transform.localScale;
            float originalScaleX = Mathf.Abs(currentScale.x);

            if (player.position.x < transform.position.x)
            {
                currentScale.x = originalScaleX;
                childAnimator?.SetTrigger("FinalRoarLeft");
            }
            else
            {
                currentScale.x = -originalScaleX;
                childAnimator?.SetTrigger("FinalRoarRight");
            }

            transform.localScale = currentScale;
        }
        else
        {
            Debug.LogError("Player Transform not assigned. Defaulting to FinalRoarRight.");
            childAnimator?.SetTrigger("FinalRoarRight");
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }

        childAnimator?.Rebind();
        childAnimator?.Update(0f);
        childAnimator.enabled = true;

        if (audioSource != null && finalRoarSound != null)
            audioSource.PlayOneShot(finalRoarSound);

        if (player != null)
        {
            Combat combat = player.GetComponent<Combat>();
            if (combat != null && combat.biteAudio != null)
                audioSource.PlayOneShot(combat.biteAudio);
        }

        // ✅ Trigger screen shake
        if (impulseSource != null)
            impulseSource.GenerateImpulse();

        if (deathParticles != null && hitEffectPoint != null)
            Instantiate(deathParticles, hitEffectPoint.position, Quaternion.identity);

        yield return new WaitForSeconds(2.0f);

        if (childAnimator != null)
            childAnimator.enabled = false;

        if (spriteRenderer != null && neutralSprite != null)
        {
            spriteRenderer.sprite = neutralSprite;
        }
        else if (spriteRenderer != null && bossAttack != null && bossAttack.riseSprite != null)
        {
            spriteRenderer.sprite = bossAttack.riseSprite;
        }

        Vector3 flyTarget = new Vector3(transform.position.x, transform.position.y + 20f, 0);
        Vector3 startAscentPos = transform.position;
        float t = 0f;
        float flyDuration = 1.5f;
        bool bottleSpawned = false;

        while (t < flyDuration)
        {
            transform.position = Vector3.Lerp(startAscentPos, flyTarget, t / flyDuration);
            t += Time.deltaTime;

            if (!bottleSpawned && t >= flyDuration * bottleSpawnAscentProgress)
            {
                if (bottlePrefab != null)
                {
                    Vector3 bottleSpawnPosition = new Vector3(transform.position.x, bottleDropSpawnY, transform.position.z);
                    Instantiate(bottlePrefab, bottleSpawnPosition, Quaternion.identity);
                    bottleSpawned = true;
                }
            }

            yield return null;
        }

        transform.position = flyTarget;

        if (!bottleSpawned && bottlePrefab != null)
        {
            Vector3 bottleSpawnPosition = new Vector3(transform.position.x, bottleDropSpawnY, transform.position.z);
            Instantiate(bottlePrefab, bottleSpawnPosition, Quaternion.identity);
        }

        gameObject.SetActive(false);
        Debug.Log("Spider GameObject deactivated. Boss fight sequence complete.");
    }
}