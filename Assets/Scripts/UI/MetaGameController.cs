using Platformer.Mechanics;
using Platformer.UI;
using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using Platformer.Core;

namespace Platformer.UI
{
    public class MetaGameController : MonoBehaviour
    {
        public static MetaGameController Instance { get; private set; }

        public MainUIController mainMenu;
        public Canvas[] gamePlayCanvasii;
        public GameController gameController;

        GameManager gameManager;
        public bool showMainCanvas;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("MetaGameController.Awake(): Duplicate instance found, destroying this one. (Normal for persistent singletons)", this);
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                Debug.Log("MetaGameController.Awake(): Singleton instance set and will persist.");
            }

            gameManager = GameManager.Instance;
            if (gameManager == null)
                Debug.LogError("MetaGameController.Awake(): GameManager instance not found! This is critical.");

            if (gameManager != null)
            {
                this.showMainCanvas = gameManager.showMenu;
                Debug.Log($"MetaGameController.Awake(): Initializing showMainCanvas from GameManager.showMenu: {this.showMainCanvas}");
            }
            else
            {
                this.showMainCanvas = true;
            }

            if (this.showMainCanvas)
            {
                Time.timeScale = 0;
                Debug.Log("MetaGameController.Awake(): Setting Time.timeScale to 0 because showMainCanvas is true (initial/reset menu state).");
            }
        }

        void OnEnable()
        {
            Debug.Log("MetaGameController.OnEnable(): Re-finding scene-specific references and applying UI state.");
            StartCoroutine(FindAndApplyUIDelayed());
        }

        IEnumerator FindAndApplyUIDelayed()
        {
            yield return null;

            mainMenu = FindObjectOfType<MainUIController>();
            if (mainMenu == null)
                Debug.LogError("MetaGameController.FindAndApplyUIDelayed: MainUIController (Start Menu Canvas) not found! Check if it's active in scene.");

            gameController = GameController.Instance;
            if (gameController == null)
                Debug.LogError("MetaGameController.FindAndApplyUIDelayed: GameController instance not found! This is critical.");

            GameObject gameCanvasObject = GameObject.FindGameObjectWithTag("GameplayCanvas");
            if (gameCanvasObject != null)
            {
                Canvas mainGameCanvas = gameCanvasObject.GetComponent<Canvas>();
                if (mainGameCanvas != null)
                {
                    gamePlayCanvasii = new Canvas[] { mainGameCanvas };
                    Debug.Log($"MetaGameController.FindAndApplyUIDelayed: Found and assigned 'GameplayCanvas' with tag '{gameCanvasObject.tag}'.");
                }
                else
                {
                    Debug.LogError("MetaGameController.FindAndApplyUIDelayed: 'GameplayCanvas' GameObject found but has no Canvas component!");
                    gamePlayCanvasii = new Canvas[0];
                }
            }
            else
            {
                Debug.LogWarning("MetaGameController.FindAndApplyUIDelayed: 'GameplayCanvas' with tag 'GameplayCanvas' not found.");
                gamePlayCanvasii = new Canvas[0];
            }

            if (gameManager != null)
            {
                this.showMainCanvas = gameManager.showMenu;
                Debug.Log($"MetaGameController.FindAndApplyUIDelayed(): Re-read showMainCanvas from GameManager.showMenu: {this.showMainCanvas} after delay.");
            }
            else
            {
                this.showMainCanvas = true;
            }

            Debug.Log($"MetaGameController.FindAndApplyUIDelayed(): Applying UI state based on showMainCanvas={this.showMainCanvas}.");
            _ToggleMainMenu(this.showMainCanvas);
        }

        public void ToggleMainMenu(bool show)
        {
            Debug.Log($"MetaGameController: ToggleMainMenu called. Desired show={show}, current showMainCanvas={this.showMainCanvas}");
            if (this.showMainCanvas != show)
            {
                _ToggleMainMenu(show);
                gameManager.showMenu = show;
                Debug.Log($"MetaGameController: ToggleMainMenu: State changed. New showMainCanvas={this.showMainCanvas}. GameManager.showMenu={gameManager.showMenu}.");
            }
            else
            {
                Debug.Log("MetaGameController: ToggleMainMenu: State not changing, already in desired state.");
            }
        }

        void _ToggleMainMenu(bool show)
        {
            Debug.Log($"MetaGameController:_ToggleMainMenu called with show={show}. Current Time.timeScale: {Time.timeScale}.");

            if (show)
            {
                Time.timeScale = 0;

                if (mainMenu != null)
                    mainMenu.gameObject.SetActive(true);

                foreach (var i in gamePlayCanvasii)
                {
                    if (i != null)
                        i.gameObject.SetActive(false);
                }

                // ✅ Fix: Reset pointer state so Play button highlights without needing a click
                EventSystem.current.SetSelectedGameObject(null);
                Cursor.visible = false;
                Cursor.visible = true;
            }
            else
            {
                Time.timeScale = 1;

                if (mainMenu != null)
                    mainMenu.gameObject.SetActive(false);

                foreach (var i in gamePlayCanvasii)
                {
                    if (i != null)
                        i.gameObject.SetActive(true);
                }
            }

            this.showMainCanvas = show;
        }

        void Update()
        {
            if (Input.GetButtonDown("Menu"))
            {
                var model = Simulation.GetModel<Platformer.Model.PlatformerModel>();
                if (model != null && model.player != null && model.player.controlEnabled)
                {
                    ToggleMainMenu(show: !showMainCanvas);
                }
                else
                {
                    Debug.Log("Pause input ignored: player control is currently disabled.");
                }
            }
        }
    }
}