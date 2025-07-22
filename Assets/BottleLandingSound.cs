using UnityEngine;
using System.Collections;

public class BottleLandingSound : MonoBehaviour
{
    public AudioClip landingSound;
    public float landingDelay = 0.6f; // You can adjust this in the Inspector

    private AudioSource audioSource;
    private Coroutine landingCoroutine;
    private bool hasPlayed = false;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            Debug.LogWarning("BottleLandingSound: No AudioSource found on this GameObject.");
            return;
        }

        landingCoroutine = StartCoroutine(PlayLandingSoundAfterDelay());
    }

    IEnumerator PlayLandingSoundAfterDelay()
    {
        yield return new WaitForSeconds(landingDelay);

        if (!hasPlayed)
        {
            audioSource.PlayOneShot(landingSound);
            hasPlayed = true;
        }
    }

    public void CancelLandingSound()
    {
        if (landingCoroutine != null)
        {
            StopCoroutine(landingCoroutine);
            landingCoroutine = null;
        }
    }
}