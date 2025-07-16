using UnityEngine;
using Platformer.Mechanics;

public class PoisonProjectile : MonoBehaviour
{
    public float speed = 5f;
    public float lifetime = 5f;
    private Vector2 direction;

    public AudioClip poisonHitSound; // Assign in Inspector
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
            playerHealth.Decrement(); // Deal damage

            if (poisonHitSound != null && audioSource != null)
                audioSource.PlayOneShot(poisonHitSound);

            Destroy(gameObject);
        }
    }
}