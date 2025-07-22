using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Collider2D))]
public class EnemyImmunityZone : MonoBehaviour
{
    public static readonly HashSet<Collider2D> EnemiesInZones = new HashSet<Collider2D>();
    private int enemiesLayer;
    private Collider2D zoneCollider;

    void Awake()
    {
        enemiesLayer = LayerMask.NameToLayer("Enemies");
        if (enemiesLayer == -1)
        {
            Debug.LogError("EnemyImmunityZone: 'Enemies' layer is not defined in the project.");
        }

        zoneCollider = GetComponent<Collider2D>();
        if (!zoneCollider.isTrigger)
        {
            Debug.LogWarning("EnemyImmunityZone: Collider2D must be set as Trigger.");
        }
    }

    void Start()
    {
        // Create a buffer to hold overlapping colliders
        Collider2D[] buffer = new Collider2D[20]; // Adjust size if you expect more than 20 enemies
        ContactFilter2D filter = new ContactFilter2D
        {
            useLayerMask = true,
            layerMask = 1 << enemiesLayer,
            useTriggers = false
        };

        int count = zoneCollider.OverlapCollider(filter, buffer);

        for (int i = 0; i < count; i++)
        {
            Collider2D enemy = buffer[i];
            if (enemy != null && !EnemiesInZones.Contains(enemy))
            {
                EnemiesInZones.Add(enemy);
                Debug.Log($"EnemyImmunityZone: {enemy.name} STARTED inside zone.");
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == enemiesLayer)
        {
            EnemiesInZones.Add(other);
            Debug.Log($"EnemyImmunityZone: {other.name} ENTERED zone.");
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.layer == enemiesLayer)
        {
            EnemiesInZones.Remove(other);
            Debug.Log($"EnemyImmunityZone: {other.name} EXITED zone.");
        }
    }

    void OnDisable()
    {
        EnemiesInZones.Clear();
    }
}