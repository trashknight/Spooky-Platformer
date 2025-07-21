using UnityEngine;

public class BottlePreLandingSoundZone : MonoBehaviour
{
    public string bottleTag = "Bottle";
    public AudioClip landingClip;

    [Tooltip("Use values >1.0 to make the sound louder than default.")]
    public float volume = 1.0f;

    private bool hasPlayed = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (hasPlayed) return;

        if (other.CompareTag(bottleTag))
        {
            // Create a temp object at the zone's location
            GameObject tempAudio = new GameObject("TempBottleSound");
            tempAudio.transform.position = transform.position;

            AudioSource source = tempAudio.AddComponent<AudioSource>();
            source.clip = landingClip;
            source.volume = volume;
            source.spatialBlend = 0f; // 2D sound
            source.Play();

            Destroy(tempAudio, landingClip.length);
            hasPlayed = true;

            Debug.Log("BottlePreLandingSoundZone: Played boosted landing sound.");
        }
    }
}