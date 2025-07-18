using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; } 

    public int unsavedScore = 0;
    public int savedScore = 0;
    public int spawnPointId;
    public bool showMenu = true;

    // Token tracking
    public HashSet<int> collectedTokenIndices = new HashSet<int>(); // Tokens permanently collected (saved)
    public List<int> collectedThisRun = new List<int>(); // Tokens collected since last checkpoint

    public bool ShouldDestroyToken(int tokenIndex)
    {
        // If the token was collected and saved in a past run, it should be destroyed
        if (collectedTokenIndices.Contains(tokenIndex)) return true;

        // If the token was collected in this run *after* a checkpoint, it will respawn
        return false;
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("GameManager: Duplicate instance found, destroying this one.", this);
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); 
            Debug.Log("GameManager: Singleton instance set and will persist.", this);
        }
    }

    public void SaveCollectedTokensAtCheckpoint()
    {
        foreach (var index in collectedThisRun)
        {
            collectedTokenIndices.Add(index);
            Debug.Log($"GameManager: Token {index} saved at checkpoint.");
        }
        collectedThisRun.Clear();
    }

    public void ClearUncommittedTokens()
    {
        Debug.Log("GameManager: Clearing uncommitted tokens after death.");
        collectedThisRun.Clear();
    }
}