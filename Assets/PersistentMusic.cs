using UnityEngine;

public class PersistentMusic : MonoBehaviour
{
    private static PersistentMusic instance;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject); // Prevent duplicate music objects
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject); // Persist across deaths
    }
}