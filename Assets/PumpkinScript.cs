using System.Collections;
using UnityEngine;

public class PumpkinScript : MonoBehaviour
{
    public int spawnId = 0;
    public bool wasHitByPlayer = false;

    public GameObject pumpkinHeadDeathVFX;
    public float vfxDuration = 2f;
    public Transform vfxTransform;

    [Header("Audio")]
    public AudioClip deathSound;
    public float deathSoundVolume = 1.0f;

    public void SetSpawnId(int id)
    {
        spawnId = id;
    }

    public void EnableDeathEffect()
    {
        wasHitByPlayer = true;
    }

    void SelfDestruct()
    {
        Destroy(gameObject);
    }

    void OnDestroy()
    {
        if (wasHitByPlayer)
        {
            // ✅ Play death sound with adjustable volume
            if (deathSound != null)
            {
                GameObject tempAudio = new GameObject("TempPumpkinSound");
                tempAudio.transform.position = transform.position;

                AudioSource source = tempAudio.AddComponent<AudioSource>();
                source.clip = deathSound;
                source.volume = deathSoundVolume;
                source.spatialBlend = 0f; // 2D sound
                source.Play();

                Destroy(tempAudio, deathSound.length);
            }

            // ✅ Spawn death particles
            if (pumpkinHeadDeathVFX != null && vfxTransform != null)
            {
                GameObject fx = Instantiate(pumpkinHeadDeathVFX, vfxTransform.position, Quaternion.identity);
                Destroy(fx, vfxDuration);
            }
        }
    }
}