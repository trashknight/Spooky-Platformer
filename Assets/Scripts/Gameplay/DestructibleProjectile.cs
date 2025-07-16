using UnityEngine;

public class DestructibleProjectile : MonoBehaviour
{
    public GameObject destroyEffect;
    public AudioClip destroySound;

    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void DestroyProjectile()
    {
        Debug.Log("DestroyProjectile called!");

        if (destroyEffect != null)
        {
            GameObject effect = Instantiate(destroyEffect, transform.position, Quaternion.identity);
            ParticleSystem ps = effect.GetComponent<ParticleSystem>();
            if (ps != null)
                ps.Play();

            Destroy(effect, ps.main.duration + ps.main.startLifetime.constantMax);
        }

        if (destroySound != null)
            AudioSource.PlayClipAtPoint(destroySound, transform.position);

        Destroy(gameObject);
    }
}