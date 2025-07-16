using UnityEngine;

public class BottleLandingZone : MonoBehaviour
{
    // Tag of the bottle prefab. Ensure your bottle prefab has this tag!
    public string bottleTag = "Bottle"; 

    // The precise Y-coordinate where the bottle should stop and rest.
    // Adjust this very carefully in the Inspector until the bottle looks perfectly on the ground.
    public float preciseLandingY = -2.0f; // Example: adjust this to your ground level

    // Flag to ensure this zone only stops the bottle once.
    private bool bottleLanded = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the object entering this trigger has the bottle's tag
        if (!bottleLanded && other.CompareTag(bottleTag))
        {
            Rigidbody2D bottleRb = other.GetComponent<Rigidbody2D>();
            if (bottleRb != null)
            {
                Debug.Log($"Bottle entered landing zone. Stopping physics for {other.name}.");
                bottleRb.velocity = Vector2.zero;       // Stop any current linear movement
                bottleRb.angularVelocity = 0f;          // Stop any current angular rotation
                bottleRb.bodyType = RigidbodyType2D.Static; // Make it static: completely removes it from physics simulation.
                                                            // It will no longer move, fall, or collide with anything physically.

                // Manually set its Y position to the precise landing spot
                Vector3 currentBottlePos = other.transform.position;
                other.transform.position = new Vector3(currentBottlePos.x, preciseLandingY, currentBottlePos.z);
                Debug.Log($"Bottle snapped to precise landing Y: {other.transform.position}.");

                bottleLanded = true; // Mark that the bottle has landed

                // Optional: Disable this landing zone's collider after the first bottle lands,
                // so it doesn't trigger multiple times if you have other falling objects.
                // GetComponent<Collider2D>().enabled = false;
            }
            else
            {
                Debug.LogWarning("BottleLandingZone: Object with bottle tag entered but has no Rigidbody2D!");
            }
        }
    }
}