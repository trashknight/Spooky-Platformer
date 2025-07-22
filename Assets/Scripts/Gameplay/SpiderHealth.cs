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
    public AudioClip riseSound; // 🎵 NEW: For ascent sounds
    public float riseVolume = 0.6f; // 🔊 Adjust in Inspector

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

        if (player == null) Debug.LogWarning("SpiderHealth: Player Transform not assigned.");
        if (childAnimator == null) Debug.LogWarning("SpiderHealth: Child Animator not found.");
        if (neutralSprite == null) Debug.LogWarning("SpiderHealth: Neutral Sprite not assigned.");
        if (bottlePrefab == null) Debug.LogWarning("SpiderHealth: Bottle Prefab not assigned.");
    }

    public void TakeDamage(int amount)
    {
        if (currentHealth <= 0) return;

        currentHealth -= amount;
        Debug.Log($"Spider took {amount} damage. Current health: {currentHealth}");

        if (hitSound != null)
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
            damageZone.gameObject.SetActive(false);

        if (spriteRenderer != null && !spriteRenderer.enabled)
            spriteRenderer.enabled = true;

        // Roar direction
        if (player != null)
        {
            Vector3 scale = transform.localScale;
            float absX = Mathf.Abs(scale.x);

            if (player.position.x < transform.position.x)
            {
                scale.x = absX;
                childAnimator?.SetTrigger("FinalRoarLeft");
            }
            else
            {
                scale.x = -absX;
                childAnimator?.SetTrigger("FinalRoarRight");
            }

            transform.localScale = scale;
        }
        else
        {
            Debug.LogError("SpiderHealth: No player assigned, defaulting to FinalRoarRight.");
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            childAnimator?.SetTrigger("FinalRoarRight");
        }

        childAnimator?.Rebind();
        childAnimator?.Update(0f);
        childAnimator.enabled = true;

        if (finalRoarSound != null)
            audioSource.PlayOneShot(finalRoarSound);

        if (player != null)
        {
            Combat combat = player.GetComponent<Combat>();
            if (combat != null && combat.biteAudio != null)
                audioSource.PlayOneShot(combat.biteAudio);
        }

        if (impulseSource != null)
            impulseSource.GenerateImpulse();

        if (deathParticles != null && hitEffectPoint != null)
            Instantiate(deathParticles, hitEffectPoint.position, Quaternion.identity);

        yield return new WaitForSeconds(1.8f);

        if (riseSound != null)
            audioSource.PlayOneShot(riseSound, riseVolume);

        yield return new WaitForSeconds(0.2f);

        if (childAnimator != null)
            childAnimator.enabled = false;

        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = (neutralSprite != null) ? neutralSprite :
                                     (bossAttack != null && bossAttack.riseSprite != null) ? bossAttack.riseSprite :
                                     spriteRenderer.sprite;

        }

        // Final ascent + sound
        Vector3 flyTarget = transform.position + Vector3.up * 20f;
        Vector3 start = transform.position;
        float t = 0f;
        float flyDuration = 1.5f;
        bool bottleSpawned = false;

        while (t < flyDuration)
        {
            transform.position = Vector3.Lerp(start, flyTarget, t / flyDuration);
            t += Time.deltaTime;

            if (!bottleSpawned && t >= flyDuration * bottleSpawnAscentProgress)
            {
                if (bottlePrefab != null)
                {
                    Vector3 bottlePos = new Vector3(transform.position.x, bottleDropSpawnY, transform.position.z);
                    Instantiate(bottlePrefab, bottlePos, Quaternion.identity);
                    bottleSpawned = true;
                }
            }

            yield return null;
        }

        transform.position = flyTarget;

        if (!bottleSpawned && bottlePrefab != null)
        {
            Vector3 bottlePos = new Vector3(transform.position.x, bottleDropSpawnY, transform.position.z);
            Instantiate(bottlePrefab, bottlePos, Quaternion.identity);
        }

        gameObject.SetActive(false);
        Debug.Log("Spider boss fully deactivated.");
    }
}