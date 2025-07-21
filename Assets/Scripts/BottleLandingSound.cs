using UnityEngine;

public class BottleLandingSound : MonoBehaviour
{
    public AudioClip landingClip;
    private bool hasLanded = false;

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasLanded) return;

        if (collision.CompareTag("BottleLandingZone")) // Your landing zone needs this tag!
        {
            AudioSource.PlayClipAtPoint(landingClip, transform.position);
            hasLanded = true;
        }
    }
}