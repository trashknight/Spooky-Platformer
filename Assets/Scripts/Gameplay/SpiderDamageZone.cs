using UnityEngine;
using Platformer.Mechanics;

public class SpiderDamageZone : MonoBehaviour
{
    public int contactDamage = 1;
    public AudioClip hurtPlayerSound;

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryApplyDamage(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        TryApplyDamage(other);
    }

    void TryApplyDamage(Collider2D other)
    {
        Health playerHealth = other.GetComponent<Health>();
        if (playerHealth != null && playerHealth.IsAlive && !playerHealth.isInvincible)
        {
            playerHealth.TakeDamage(contactDamage, false);

            if (hurtPlayerSound != null)
            {
                AudioSource.PlayClipAtPoint(hurtPlayerSound, transform.position);
            }
        }
    }
}