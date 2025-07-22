using UnityEngine;

public class TestSoundPlayer : MonoBehaviour
{
    public AudioClip soundToPlay;
    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            Debug.Log("TestSoundPlayer: No AudioSource found, so one was added.");
        }

        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f; // 2D sound
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            if (soundToPlay != null)
            {
                audioSource.PlayOneShot(soundToPlay);
                Debug.Log("TestSoundPlayer: Played sound.");
            }
            else
            {
                Debug.LogWarning("TestSoundPlayer: No sound assigned.");
            }
        }
    }
}