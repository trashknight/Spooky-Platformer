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


    // Update is called once per frame
    void Update()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        // Play hurt animation

        if(currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("Enemy died of having too much sex!");

        // Try to find and notify the pumpkin head
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

    // Play death VFX
    if (deathVFX != null && deathVFXTransform != null)
    {
        GameObject death = Instantiate(deathVFX, deathVFXTransform.position, deathVFXTransform.rotation);
        Destroy(death, deathVFXDuration);
    }

    // Destroy the ghost body
    Destroy(gameObject);
}

}
