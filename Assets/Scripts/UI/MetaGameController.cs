using Platformer.Mechanics;
using Platformer.UI;
using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using Platformer.Core;

#if UNITY_STANDALONE_WIN
using System.Runtime.InteropServices;
#endif

namespace Platformer.UI
{
    public class MetaGameController : MonoBehaviour
    {
        public static MetaGameController Instance { get; private set; }

        public MainUIController mainMenu;
        public Canvas[] gamePlayCanvasii;
        public GameController gameController;

        public AudioClip clickSFX;
        private AudioSource audioSource;
        private bool initialized = false;

        GameManager gameManager;
        public bool showMainCanvas;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("MetaGameController.Awake(): Singleton instance set.");

            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();

            gameManager = GameManager.Instance;
            if (gameManager == null)
                Debug.LogError("MetaGameController.Awake(): GameManager not found!");

            showMainCanvas = gameManager != null ? gameManager.showMenu : true;

            if (showMainCanvas)
            {
                Time.timeScale = 0;
                Debug.Log("MetaGameController.Awake(): Game starts paused.");
            }
        }

        void OnEnable()
        {
            StartCoroutine(FindAndApplyUIDelayed());
        }

        IEnumerator FindAndApplyUIDelayed()
        {
            yield return null;

            mainMenu = FindObjectOfType<MainUIController>();
            gameController = GameController.Instance;

            GameObject gameCanvasObject = GameObject.FindGameObjectWithTag("GameplayCanvas");
            if (gameCanvasObject != null)
            {
                Canvas mainGameCanvas = gameCanvasObject.GetComponent<Canvas>();
                gamePlayCanvasii = mainGameCanvas != null ? new Canvas[] { mainGameCanvas } : new Canvas[0];
            }
            else
            {
                gamePlayCanvasii = new Canvas[0];
            }

            showMainCanvas = gameManager != null ? gameManager.showMenu : true;
            _ToggleMainMenu(showMainCanvas, playSound: false); // Don't play sound during init
            initialized = true;
        }

        public void ToggleMainMenu(bool show)
        {
            if (this.showMainCanvas == show)
            {
                Debug.Log("MetaGameController: ToggleMainMenu skipped (already in desired state).");
                return;
            }

            _ToggleMainMenu(show, playSound: true);
            if (gameManager != null) gameManager.showMenu = show;
        }

        void _ToggleMainMenu(bool show, bool playSound)
        {
            if (show)
            {
                Time.timeScale = 0;
                if (mainMenu != null) mainMenu.gameObject.SetActive(true);
                foreach (var c in gamePlayCanvasii) if (c != null) c.gameObject.SetActive(false);
                EventSystem.current.SetSelectedGameObject(null);
                Cursor.visible = false;
                Cursor.visible = true;
            }
            else
            {
                Time.timeScale = 1;
                if (mainMenu != null) mainMenu.gameObject.SetActive(false);
                foreach (var c in gamePlayCanvasii) if (c != null) c.gameObject.SetActive(true);
            }

            if (playSound && initialized && clickSFX != null)
            {
                Debug.Log("MetaGameController: Playing click sound.");
                audioSource.PlayOneShot(clickSFX);
            }

            this.showMainCanvas = show;
        }

        void Update()
        {
            var model = Simulation.GetModel<Platformer.Model.PlatformerModel>();
            if (Input.GetMouseButtonDown(0))
            {
                if (model != null && model.player != null && model.player.controlEnabled)
                {
                    ToggleMainMenu(!showMainCanvas);
                }
                else
                {
                    Debug.Log("Click ignored: player has no control.");
                }
            }

#if UNITY_STANDALONE_WIN
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                var hwnd = GetActiveWindow();
                ShowWindow(hwnd, SW_MINIMIZE);
            }
#endif
        }

#if UNITY_STANDALONE_WIN
        [DllImport("user32.dll")] private static extern System.IntPtr GetActiveWindow();
        [DllImport("user32.dll")] private static extern bool ShowWindow(System.IntPtr hWnd, int nCmdShow);
        private const int SW_MINIMIZE = 6;
#endif
    }
}