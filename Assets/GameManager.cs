using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // --- NEW: Static Instance property ---
    // This is the correct way to make the GameManager accessible as a singleton
    public static GameManager Instance { get; private set; } 
    // ------------------------------------

    public int unsavedScore = 0;
    public int savedScore = 0;
    public int spawnPointId;
    public bool showMenu = true; // This tracks if the menu should be shown on initial scene load

    void Awake()
    {
        // --- MODIFIED: More robust singleton setup using 'Instance' ---
        // Check if an instance already exists and it's not this one.
        if (Instance != null && Instance != this)
        {
            // If a duplicate instance is found, destroy this new GameObject.
            Debug.LogWarning("GameManager: Duplicate instance found, destroying this one.", this);
            Destroy(gameObject);
        }
        else
        {
            // If no instance exists, or this is the first one, set it as the singleton instance.
            Instance = this;
            // Ensure this GameObject persists across scene loads.
            DontDestroyOnLoad(gameObject); 
            Debug.Log("GameManager: Singleton instance set and will persist.", this);
        }
        // --- END MODIFIED ---
    }

    // The SetUpSingleton() method is now incorporated directly into Awake() above,
    // so you can remove the private SetUpSingleton() method if it still exists.
    // private void SetUpSingleton() { /* ... */ } 
}