using UnityEngine;

public class DestructibleProjectile : MonoBehaviour
{
    public GameObject destroyEffect;
    public AudioClip destroySound;
    [Range(0f, 5f)] public float destroySoundVolume = 1.0f;

    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void DestroyProjectile()
    {
        Debug.Log("DestroyProjectile called!");

        // ✅ Visual effect
        if (destroyEffect != null)
        {
            GameObject effect = Instantiate(destroyEffect, transform.position, Quaternion.identity);
            ParticleSystem ps = effect.GetComponent<ParticleSystem>();
            if (ps != null)
                ps.Play();

            Destroy(effect, ps.main.duration + ps.main.startLifetime.constantMax);
        }

        // ✅ Custom sound logic
        if (destroySound != null)
        {
            GameObject tempAudio = new GameObject("TempProjectileSound");
            tempAudio.transform.position = transform.position;

            AudioSource src = tempAudio.AddComponent<AudioSource>();
            src.clip = destroySound;
            src.volume = destroySoundVolume;
            src.spatialBlend = 0f; // Make it fully 2D
            src.Play();

            Destroy(tempAudio, destroySound.length);
        }

        Destroy(gameObject);
    }
}