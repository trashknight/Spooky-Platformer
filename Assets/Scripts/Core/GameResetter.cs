using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Platformer.Mechanics;
using Platformer.UI;
using UnityEngine.EventSystems;

namespace Platformer.Core
{
    public class GameResetter : MonoBehaviour
    {
        public static GameResetter Instance { get; private set; }

        [Tooltip("The duration of the blackout fade when restarting the scene.")]
        public float fadeDuration = 1.5f;

        [Header("Audio")]
        public AudioClip clickSound;

        private AudioSource audioSource;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f;
            audioSource.volume = 1f;

            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            EventSystem currentEventSystem = FindObjectOfType<EventSystem>();
            if (currentEventSystem == null)
                Debug.LogError("GameResetter.OnSceneLoaded(): EventSystem not found!");

            MetaGameController metaController = MetaGameController.Instance;
            if (metaController != null && !metaController.gameObject.activeInHierarchy)
                metaController.gameObject.SetActive(true);

            if (currentEventSystem != null && !currentEventSystem.gameObject.activeInHierarchy)
                currentEventSystem.gameObject.SetActive(true);
        }

        public void StartFullGameReset()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.unsavedScore = 0;
                GameManager.Instance.savedScore = 0;
                GameManager.Instance.spawnPointId = 0;
                GameManager.Instance.showMenu = true;
                GameManager.Instance.collectedTokenIndices.Clear();
                GameManager.Instance.collectedThisRun.Clear();
            }

            CleanUpDynamicObjects();

            EventSystem currentEventSystemBeforeReload = FindObjectOfType<EventSystem>();
            if (currentEventSystemBeforeReload != null)
                currentEventSystemBeforeReload.gameObject.SetActive(false);

            Time.timeScale = 0;
            StartCoroutine(FadeAndReloadSequence(true));
        }

        private void CleanUpDynamicObjects()
        {
            PoisonProjectile[] poisonBalls = FindObjectsOfType<PoisonProjectile>();
            foreach (PoisonProjectile ball in poisonBalls) Destroy(ball.gameObject);

            PumpkinScript[] pumpkinHeads = FindObjectsOfType<PumpkinScript>();
            foreach (PumpkinScript head in pumpkinHeads) Destroy(head.gameObject);
        }

        public IEnumerator FadeAndReloadSequence(bool isFullReset = false)
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                var combat = player.GetComponent<Combat>();
                if (combat != null)
                {
                    if (!combat.HasAlreadyFaded)
                        yield return combat.FadeToBlack(fadeDuration);
                }
                else
                {
                    yield return new WaitForSeconds(fadeDuration);
                }
            }
            else
            {
                yield return new WaitForSeconds(fadeDuration);
            }

            if (isFullReset)
            {
                var music = GameObject.FindObjectOfType<PersistentMusic>();
                if (music != null)
                    Destroy(music.gameObject);
            }

            if (MetaGameController.Instance != null)
                Destroy(MetaGameController.Instance.gameObject);

            if (Instance != null)
                Destroy(gameObject);

            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            yield return null;
        }

        public void TestClickReceived()
        {
            if (audioSource != null && clickSound != null)
                audioSource.PlayOneShot(clickSound);

            StartFullGameReset();
        }
    }
}