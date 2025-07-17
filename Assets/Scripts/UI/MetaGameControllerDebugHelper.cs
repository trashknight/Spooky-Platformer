using UnityEngine;
using Platformer.UI; // Make sure this matches the namespace of your MetaGameController

public class MetaGameControllerDebugHelper : MonoBehaviour
{
    [Tooltip("This will show the persistent MetaGameController instance.")]
    public MetaGameController persistentMetaGameController;

    void Update()
    {
        // In Update, we constantly check for the singleton instance.
        // Once MetaGameController.Instance is set (in its Awake), this will grab it.
        if (MetaGameController.Instance != null)
        {
            persistentMetaGameController = MetaGameController.Instance;
        }
        else
        {
            // This warning means MetaGameController's Awake hasn't run yet or it was destroyed.
            // It should typically not be null after the game starts.
            Debug.LogWarning("MetaGameController.Instance is null in debug helper. Make sure MetaGameController exists in scene and its Awake runs.");
        }
    }
}