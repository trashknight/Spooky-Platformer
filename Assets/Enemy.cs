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

        // Die you turd animation
        GameObject death = Instantiate(deathVFX, deathVFXTransform.position, deathVFXTransform.rotation);
        Destroy(death, deathVFXDuration);

        // Disable the enemy
        Destroy(gameObject);
    }

}
