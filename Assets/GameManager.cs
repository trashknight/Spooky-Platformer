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

    // ✅ Track which checkpoints have been activated
    public HashSet<int> activatedCheckpoints = new HashSet<int>();

    // ✅ Token tracking
    public HashSet<int> collectedTokenIndices = new HashSet<int>(); // Tokens permanently collected (saved)
    public List<int> collectedThisRun = new List<int>();            // Tokens collected since last checkpoint

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

    public bool ShouldDestroyToken(int tokenIndex)
    {
        // Destroy if it was saved in a past run
        if (collectedTokenIndices.Contains(tokenIndex)) return true;

        // Otherwise allow respawn (e.g. was collected this run but not checkpointed)
        return false;
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

    public void ResetAllCheckpoints()
    {
        activatedCheckpoints.Clear();
        Debug.Log("GameManager: Activated checkpoints reset.");
    }

    public void ResetAllTokens()
    {
        collectedTokenIndices.Clear();
        collectedThisRun.Clear();
        Debug.Log("GameManager: All token data cleared.");
    }

    public void FullReset()
    {
        ResetAllCheckpoints();
        ResetAllTokens();
        unsavedScore = 0;
        savedScore = 0;
        spawnPointId = 0;
        showMenu = true;
        Debug.Log("GameManager: Full reset completed.");
    }
}