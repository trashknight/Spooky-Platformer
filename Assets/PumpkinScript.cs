using System.Collections;
using UnityEngine;

public class PumpkinScript : MonoBehaviour
{
    public int spawnId = 0;
    public bool wasHitByPlayer = false;

    public GameObject pumpkinHeadDeathVFX;
    public float vfxDuration = 2f;
    public Transform vfxTransform;

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
        // Used when manually triggering destruction (e.g., return to body)
        Destroy(gameObject);
    }

    void OnDestroy()
    {
        // This will now be called even if Enemy.cs calls Destroy()
        if (wasHitByPlayer && pumpkinHeadDeathVFX != null)
        {
            Instantiate(pumpkinHeadDeathVFX, vfxTransform.position, Quaternion.identity);
        }
    }
}