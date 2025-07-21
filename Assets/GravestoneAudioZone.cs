using UnityEngine;

public class GravestoneAudioZone : MonoBehaviour
{
    public float activationRadius = 6f;
    public AudioClip ambientSound;
    [Range(0f, 2f)] public float maxVolume = 1.0f;
    public float fadeSpeed = 1.0f; // Units per second

    private Transform player;
    private AudioSource audioSource;
    private bool playerInRange = false;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.clip = ambientSound;
        audioSource.loop = true;
        audioSource.volume = 0f;
        audioSource.spatialBlend = 0f;
        audioSource.playOnAwake = false;
    }

    void Update()
    {
        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);
        bool inRangeNow = distance <= activationRadius;

        // If player just entered range, start playing
        if (inRangeNow && !playerInRange)
        {
            playerInRange = true;
            if (!audioSource.isPlaying)
                audioSource.Play();
        }

        // If player just left range, update state
        if (!inRangeNow && playerInRange)
        {
            playerInRange = false;
        }

        // Handle volume fading
        float targetVolume = playerInRange ? maxVolume : 0f;
        audioSource.volume = Mathf.MoveTowards(audioSource.volume, targetVolume, fadeSpeed * Time.deltaTime);

        // Stop playing if volume reaches 0 and player is not in range
        if (!playerInRange && audioSource.volume <= 0.001f && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }
}