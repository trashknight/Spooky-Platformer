using UnityEngine;
using UnityEngine.SceneManagement;
using Platformer.Mechanics;
using Platformer.UI;
using UnityEngine.EventSystems;
using System.Collections;

namespace Platformer.Core
{
    public class GameResetter : MonoBehaviour
    {
        public static GameResetter Instance { get; private set; }

        [Tooltip("The duration of the blackout fade when restarting the scene.")]
        public float fadeDuration = 1.5f;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("GameResetter.Awake(): Duplicate instance found, destroying this one.");
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                Debug.Log("GameResetter.Awake(): Singleton instance set.");
            }

            SceneManager.sceneLoaded += OnSceneLoaded;
            Debug.Log("GameResetter.Awake(): Subscribed to sceneLoaded.");
        }

        void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            Debug.Log("GameResetter.OnDestroy(): Unsubscribed from sceneLoaded.");
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Debug.Log($"GameResetter.OnSceneLoaded(): Scene '{scene.name}' loaded.");

            EventSystem currentEventSystem = FindObjectOfType<EventSystem>();
            if (currentEventSystem == null)
                Debug.LogError("GameResetter.OnSceneLoaded(): EventSystem not found!");

            MetaGameController metaController = MetaGameController.Instance;
            if (metaController != null && !metaController.gameObject.activeInHierarchy)
            {
                metaController.gameObject.SetActive(true);
                Debug.Log("GameResetter.OnSceneLoaded(): Re-enabled MetaGameController.");
            }

            if (currentEventSystem != null && !currentEventSystem.gameObject.activeInHierarchy)
            {
                currentEventSystem.gameObject.SetActive(true);
                Debug.Log("GameResetter.OnSceneLoaded(): Re-enabled EventSystem.");
            }
        }

        public void StartFullGameReset()
        {
            Debug.Log("GameResetter: Starting full game reset.");

            if (GameManager.Instance != null)
            {
                GameManager.Instance.unsavedScore = 0;
                GameManager.Instance.savedScore = 0;
                GameManager.Instance.spawnPointId = 0;
                GameManager.Instance.showMenu = true;
                Debug.Log("GameResetter: GameManager reset.");
            }
            else
            {
                Debug.LogError("GameResetter: GameManager not found!");
            }

            CleanUpDynamicObjects();

            EventSystem currentEventSystemBeforeReload = FindObjectOfType<EventSystem>();
            if (currentEventSystemBeforeReload != null)
            {
                currentEventSystemBeforeReload.gameObject.SetActive(false);
                Debug.Log("GameResetter: Disabled EventSystem before reload.");
            }

            Time.timeScale = 0;
            Debug.Log("GameResetter: Paused game before fade and reload.");

            StartCoroutine(FadeAndReloadSequence());
        }

        private void CleanUpDynamicObjects()
        {
            PoisonProjectile[] poisonBalls = FindObjectsOfType<PoisonProjectile>();
            foreach (PoisonProjectile ball in poisonBalls) Destroy(ball.gameObject);
            Debug.Log($"GameResetter: Destroyed {poisonBalls.Length} PoisonProjectile(s).");

            PumpkinScript[] pumpkinHeads = FindObjectsOfType<PumpkinScript>();
            foreach (PumpkinScript head in pumpkinHeads) Destroy(head.gameObject);
            Debug.Log($"GameResetter: Destroyed {pumpkinHeads.Length} PumpkinScript(s).");

            // Add additional dynamic cleanup here as needed
        }

        IEnumerator FadeAndReloadSequence()
        {
            PlayerController player = FindObjectOfType<PlayerController>();
            Combat playerCombat = null;

            if (player != null)
            {
                playerCombat = player.GetComponent<Combat>();
                if (playerCombat != null)
                {
                    Debug.Log("GameResetter: Starting fade to black.");
                    yield return StartCoroutine(playerCombat.FadeToBlack(fadeDuration));
                }
                else
                {
                    Debug.LogError("GameResetter: No Combat on player.");
                }
            }
            else
            {
                Debug.LogError("GameResetter: No PlayerController found.");
            }

            Debug.Log("GameResetter: Fade complete. Reloading scene...");

            // ✅ NEW: Clean up persistent objects to ensure clean state
            if (MetaGameController.Instance != null)
            {
                Destroy(MetaGameController.Instance.gameObject);
                Debug.Log("GameResetter: Destroyed MetaGameController before scene reload.");
            }

            if (Instance != null)
            {
                Destroy(gameObject); // Destroys this GameResetter
                Debug.Log("GameResetter: Destroyed GameResetter before scene reload.");
            }

            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void TestClickReceived()
        {
            Debug.Log("GameResetter: Victory screen click detected.");
        }
    }
}