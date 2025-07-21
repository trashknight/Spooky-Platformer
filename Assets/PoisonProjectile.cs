using UnityEngine;
using Platformer.Mechanics;

public class PoisonProjectile : MonoBehaviour
{
    public float speed = 5f;
    public float lifetime = 5f;
    private Vector2 direction;

    public AudioClip poisonHitSound; // Assign in Inspector
    [Range(0f, 5f)] public float poisonHitVolume = 1.0f;

    private AudioSource audioSource;

    void Start()
    {
        Destroy(gameObject, lifetime);
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        transform.Translate(direction * speed * Time.deltaTime);
    }

    public void SetDirection(Vector2 dir)
    {
        direction = dir.normalized;

        // Flip sprite if going RIGHT (default faces left)
        Vector3 scale = transform.localScale;
        scale.x = dir.x > 0 ? -Mathf.Abs(scale.x) : Mathf.Abs(scale.x);
        transform.localScale = scale;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        Health playerHealth = collision.GetComponent<Health>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(1, false);

            // ✅ Play sound from separate object before destroying this one
            if (poisonHitSound != null)
            {
                GameObject tempAudio = new GameObject("TempPoisonHitSound");
                tempAudio.transform.position = transform.position;

                AudioSource tempSource = tempAudio.AddComponent<AudioSource>();
                tempSource.clip = poisonHitSound;
                tempSource.volume = poisonHitVolume;
                tempSource.spatialBlend = 0f; // 2D sound
                tempSource.Play();

                Destroy(tempAudio, poisonHitSound.length);
            }

            Destroy(gameObject);
        }
    }
}