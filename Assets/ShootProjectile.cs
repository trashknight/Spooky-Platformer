using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootProjectile : MonoBehaviour
{
    public GameObject projectile;
    public Transform spawnpoint;
    public int SpawnID;
    public Animator animator;

    void Start()
    {
        // sets a random number for the id of the spawner
        SpawnID = Random.Range(1, 500);
    }

    void Shoot() {
        GameObject Enemy = (GameObject) Instantiate(projectile, spawnpoint.position, Quaternion.identity);
        Enemy.SendMessage("SetSpawnId", SpawnID);
    }

    public void RespawnPumpkin() {
        animator.SetTrigger("RespawnPumpkin");
        Debug.Log("Respawning pumpkin");
    }
}
