using UnityEngine;

public class BottleLandingZone : MonoBehaviour
{
    public string bottleTag = "Bottle";
    public float preciseLandingY = -2.0f;

    public AudioClip landingClip;
    [Range(0f, 1f)] public float volume = 1.0f;

    private bool bottleLanded = false;
    private AudioSource audioSource;

    void Start()
    {
        // You can add an AudioSource to the LandingZone object, or this creates one.
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 0f; // Make sure it's 2D
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!bottleLanded && other.CompareTag(bottleTag))
        {
            Rigidbody2D bottleRb = other.GetComponent<Rigidbody2D>();
            if (bottleRb != null)
            {
                Debug.Log($"Bottle entered landing zone. Stopping physics for {other.name}.");
                bottleRb.velocity = Vector2.zero;
                bottleRb.angularVelocity = 0f;
                bottleRb.bodyType = RigidbodyType2D.Static;

                Vector3 currentBottlePos = other.transform.position;
                other.transform.position = new Vector3(currentBottlePos.x, preciseLandingY, currentBottlePos.z);
                Debug.Log($"Bottle snapped to precise landing Y: {other.transform.position}.");

                bottleLanded = true;

                if (landingClip != null && audioSource != null)
                {
                    audioSource.PlayOneShot(landingClip, volume);
                }
            }
            else
            {
                Debug.LogWarning("BottleLandingZone: Object with bottle tag entered but has no Rigidbody2D!");
            }
        }
    }
}