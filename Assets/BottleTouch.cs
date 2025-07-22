using UnityEngine;
using Platformer.Gameplay;      
using static Platformer.Core.Simulation; 
using Platformer.Mechanics;     // Needed to find VictoryZone class

public class BottleTouch : MonoBehaviour
{
    public AudioClip collectSound;
    private AudioSource audioSource;

    private bool hasBeenCollected = false;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            Debug.LogWarning("BottleTouch: No AudioSource found on this GameObject. Collect sound will not play.");
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (hasBeenCollected) return;

        if (other.CompareTag("Player"))
        {
            hasBeenCollected = true; 
            Debug.Log("Bottle collected by Player! Scheduling Victory Event.");

            // Cancel pending landing sound
            BottleLandingSound landingSound = GetComponent<BottleLandingSound>();
            if (landingSound != null)
            {
                landingSound.CancelLandingSound();
            }

            // Play collect sound
            if (collectSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(collectSound);
            }
            else if (collectSound == null)
            {
                Debug.LogWarning("BottleTouch: Collect Sound not assigned.");
            }

            // Trigger the victory zone event
            VictoryZone existingVictoryZone = FindObjectOfType<VictoryZone>();
            if (existingVictoryZone != null)
            {
                var ev = Schedule<PlayerEnteredVictoryZone>();
                ev.victoryZone = existingVictoryZone;
                Debug.Log("PlayerEnteredVictoryZone event scheduled via existing VictoryZone.");
            }
            else
            {
                Debug.LogError("BottleTouch: Could not find an existing VictoryZone in scene to schedule event! Victory screen may not activate.");
            }

            // Visually and physically hide the bottle
            GetComponent<SpriteRenderer>().enabled = false; 
            
            Collider2D[] colliders = GetComponents<Collider2D>();
            foreach (Collider2D col in colliders)
            {
                col.enabled = false;
            }
            
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.velocity = Vector2.zero;
                rb.angularVelocity = 0f;
                rb.isKinematic = true;
            }

            // The bottle GameObject remains in the scene, hidden under the victory canvas
        }
    }
}