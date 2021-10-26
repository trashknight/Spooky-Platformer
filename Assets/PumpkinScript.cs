using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PumpkinScript : MonoBehaviour
{
    public int spawnId = 0;
    public void SetSpawnId(int id) {
        spawnId = id;
    }

    void SelfDestruct() {
        Destroy(gameObject);
    }
}
