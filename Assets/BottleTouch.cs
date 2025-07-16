using UnityEngine;
using Platformer.Gameplay;      
using static Platformer.Core.Simulation; 
using Platformer.Mechanics;     // Needed to find VictoryZone class

public class BottleTouch : MonoBehaviour
{
    public AudioClip collectSound;
    private AudioSource audioSource;

    private bool hasBeenCollected = false;

    // The hasLanded flag and OnCollisionEnter2D are no longer needed here.

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            Debug.LogWarning("BottleTouch: No AudioSource found on this GameObject. Collect sound will not play.");
        }
    }

    // OnCollisionEnter2D method removed - landing is now handled by BottleLandingZone.

    void OnTriggerEnter2D(Collider2D other)
    {
        if (hasBeenCollected) return;

        // Player collection logic:
        if (other.CompareTag("Player")) // Assuming player has "Player" tag
        {
            hasBeenCollected = true; 
            Debug.Log("Bottle collected by Player! Scheduling Victory Event.");

            if (collectSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(collectSound);
            }
            else if (collectSound == null)
            {
                 Debug.LogWarning("BottleTouch: Collect Sound not assigned.");
            }

            // Find an existing VictoryZone in the scene and schedule the victory event
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

            // After collection, hide the bottle sprite and disable its colliders/physics for interaction
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
                rb.isKinematic = true; // Make it kinematic (stops all physics influence)
            }
            // The bottle GameObject remains in the scene, hidden by the victory canvas.
        }
    }
}