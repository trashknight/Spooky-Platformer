using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootProjectile : MonoBehaviour
{
    public GameObject projectile;
    public Transform spawnpoint;
    // Start is called before the first frame update

    void Shoot() {
        GameObject Enemy = (GameObject) Instantiate(projectile, spawnpoint.position, Quaternion.identity);
    }
}
