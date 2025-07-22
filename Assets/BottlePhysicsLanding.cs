using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(AudioSource))]
public class BottlePhysicsLanding : MonoBehaviour
{
    public AudioClip landingSound;
    public float landingVolume = 1f;
    public LayerMask groundLayer;
    public float minVelocityToLand = 1f;
    public bool hasLanded = false;

    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) Debug.LogWarning("BottlePhysicsLanding: No AudioSource found!");
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f; // 2D
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (hasLanded) return;

        // Check if the collision is with ground and has enough impact
        if (((1 << collision.gameObject.layer) & groundLayer) != 0)
        {
            float impactVelocity = Mathf.Abs(collision.relativeVelocity.y);
            if (impactVelocity >= minVelocityToLand)
            {
                hasLanded = true;
                if (landingSound != null && audioSource != null)
                {
                    audioSource.PlayOneShot(landingSound, landingVolume);
                    Debug.Log("BottlePhysicsLanding: Played landing sound.");
                }
            }
        }
    }
}