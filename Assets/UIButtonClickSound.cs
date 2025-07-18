using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UIButtonClickSound : MonoBehaviour
{
    public AudioSource clickSoundSource;

    void Start()
    {
        GetComponent<Button>().onClick.AddListener(PlayClickSound);
    }

    void PlayClickSound()
    {
        if (clickSoundSource != null)
            clickSoundSource.Play();
    }
}