using UnityEngine;

public class BottleLandingSound : MonoBehaviour
{
    public AudioClip landingClip;
    [Range(0f, 1f)] public float volume = 1.0f;
    private bool hasLanded = false;
    private AudioSource audioSource;

    void Start()
    {
        // Use or create an AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 0f; // Make it 2D
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasLanded) return;

        if (collision.CompareTag("BottleLandingZone"))
        {
            if (landingClip != null && audioSource != null)
            {
                audioSource.PlayOneShot(landingClip, volume);
                hasLanded = true;
            }
        }
    }
}