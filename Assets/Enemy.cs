using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public int maxHealth = 1;
    int currentHealth;

    public GameObject deathVFX;
    public float deathVFXDuration = 2f;
    public Transform deathVFXTransform;

    [Header("Audio")]
    public AudioClip deathSound;
    public float deathSoundVolume = 1.0f;
    public float maxSoundDistance = 8f;

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        TakeDamage(damage, false);
    }

    public void TakeDamage(int damage, bool causedByPlayer)
    {
        currentHealth -= damage;

        // Play hurt animation here (if any)

        if (currentHealth <= 0)
        {
            Die(causedByPlayer);
        }
    }

    void Die(bool causedByPlayer)
    {
        Debug.Log("Enemy died of having too much sex!");

        // ✅ Play boosted death sound if player is close or caused it
        Transform player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (deathSound != null && player != null)
        {
            float distance = Vector2.Distance(transform.position, player.position);
            if (causedByPlayer || distance <= maxSoundDistance)
            {
                GameObject tempAudio = new GameObject("TempEnemyDeathSound");
                tempAudio.transform.position = transform.position;

                AudioSource src = tempAudio.AddComponent<AudioSource>();
                src.clip = deathSound;
                src.volume = deathSoundVolume;
                src.spatialBlend = 0f; // 2D sound
                src.Play();

                Destroy(tempAudio, deathSound.length);
            }
        }

        // Notify pumpkin head
        var shootProjectile = GetComponent<ShootProjectile>();
        if (shootProjectile != null)
        {
            int id = shootProjectile.SpawnID;
            foreach (var pumpkin in FindObjectsOfType<PumpkinScript>())
            {
                if (pumpkin.spawnId == id)
                {
                    pumpkin.wasHitByPlayer = true;
                    Debug.Log("Pumpkin head notified of ghost death.");
                    break;
                }
            }
        }

        // Death VFX
        if (deathVFX != null && deathVFXTransform != null)
        {
            GameObject death = Instantiate(deathVFX, deathVFXTransform.position, deathVFXTransform.rotation);
            Destroy(death, deathVFXDuration);
        }

        Destroy(gameObject);
    }
}