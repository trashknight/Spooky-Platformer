using Platformer.Mechanics;
using Platformer.UI;
using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement; // Needed for SceneManager.GetActiveScene().name
using UnityEngine.EventSystems; // Required for EventSystem (for fallback)
using Platformer.Core;             // Needed for GameResetter

namespace Platformer.UI
{
    /// <summary>
    /// The MetaGameController is responsible for switching control between the high level
    /// contexts of the application, e.g., the Main Menu and Gameplay systems.
    /// </summary>
    public class MetaGameController : MonoBehaviour
    {
        public static MetaGameController Instance { get; private set; }

        // These public fields will now be assigned dynamically in OnEnable.
        // Ensure they are NOT assigned in the Inspector.
        public MainUIController mainMenu;
        public Canvas[] gamePlayCanvasii;
        public GameController gameController;

        GameManager gameManager;
        public bool showMainCanvas; // Will persist across scene loads

        void Awake()
        {
            // Singleton pattern: Ensure only one instance exists and persists.
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

            // GameManager is set to execute earlier via Script Execution Order, so it should be available.
            gameManager = GameManager.Instance;
            if (gameManager == null) Debug.LogError("MetaGameController.Awake(): GameManager instance not found! This is critical.");

            // Initialize showMainCanvas from GameManager's persistent state.
            if (gameManager != null) {
                this.showMainCanvas = gameManager.showMenu;
                Debug.Log($"MetaGameController.Awake(): Initializing showMainCanvas from GameManager.showMenu: {this.showMainCanvas}");
            } else {
                this.showMainCanvas = true; // Fallback: Assume menu should be shown if GameManager not ready.
            }

            // Ensure Time.timeScale is 0 if menu is meant to be shown (initial state or reset to menu)
            if (this.showMainCanvas)
            {
                Time.timeScale = 0;
                Debug.Log("MetaGameController.Awake(): Setting Time.timeScale to 0 because showMainCanvas is true (initial/reset menu state).");
            }
        }

        void OnEnable()
        {
            Debug.Log("MetaGameController.OnEnable(): Re-finding scene-specific references and applying UI state.");

            // Start a coroutine for delayed UI state application.
            StartCoroutine(FindAndApplyUIDelayed());
        }

        // Coroutine to perform finding and apply UI state after a slight delay
        IEnumerator FindAndApplyUIDelayed()
        {
            yield return null; // Wait one frame for scene objects to fully initialize their active states.
            // yield return new WaitForEndOfFrame(); // Could also use this for a longer wait if needed

            // 1. Find MainUIController (your Start Menu Canvas)
            mainMenu = FindObjectOfType<MainUIController>();
            if (mainMenu == null) Debug.LogError("MetaGameController.FindAndApplyUIDelayed: MainUIController (Start Menu Canvas) not found! Check if it's active in scene.");

            // 2. Find GameController (it's also a singleton, get its instance)
            gameController = GameController.Instance;
            if (gameController == null) Debug.LogError("MetaGameController.FindAndApplyUIDelayed: GameController instance not found! This is critical.");

            // 3. Find Gameplay Canvases (e.g., your Game Canvas) by tag
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
                    Debug.LogError("MetaGameController.FindAndApplyUIDelayed: 'GameplayCanvas' GameObject found but has no Canvas component! Cannot manage gameplay UI.");
                    gamePlayCanvasii = new Canvas[0];
                }
            }
            else
            {
                Debug.LogWarning("MetaGameController.FindAndApplyUIDelayed: Main 'GameplayCanvas' with tag 'GameplayCanvas' not found. Ensure your main gameplay HUD canvas has this tag.");
                gamePlayCanvasii = new Canvas[0];
            }

            // Now that references are found, apply the correct UI state.
            if (gameManager != null) { // Re-read GameManager.showMenu here as a final source of truth
                this.showMainCanvas = gameManager.showMenu;
                Debug.Log($"MetaGameController.FindAndApplyUIDelayed(): Re-read showMainCanvas from GameManager.showMenu: {this.showMainCanvas} after delay.");
            } else {
                this.showMainCanvas = true;
            }

            Debug.Log($"MetaGameController.FindAndApplyUIDelayed(): Applying UI state based on showMainCanvas={this.showMainCanvas}.");
            _ToggleMainMenu(this.showMainCanvas); // This sets Time.timeScale and manages UI visibility

            // Re-enable EventSystem is now handled by GameResetter.OnSceneLoaded.
            // Removed the GameResetter.Instance.ReEnableEventSystem() call from here.
        }


        /// <summary>
        /// Public method to toggle the main menu visibility. Called by MainUIController.
        /// </summary>
        /// <param name="show">True to show the main menu, false to hide it.</param>
        public void ToggleMainMenu(bool show)
        {
            Debug.Log($"MetaGameController: ToggleMainMenu called. Desired show={show}, current showMainCanvas={this.showMainCanvas}");
            if (this.showMainCanvas != show) // Only perform the toggle if the state is actually changing
            {
                _ToggleMainMenu(show); // Perform the actual UI activation/deactivation
                gameManager.showMenu = show; // Update GameManager's persistent preference for next scene loads/starts
                Debug.Log($"MetaGameController: ToggleMainMenu: State changed. New showMainCanvas={this.showMainCanvas}. GameManager.showMenu={gameManager.showMenu}.");
            } else {
                Debug.Log("MetaGameController: ToggleMainMenu: State not changing, already in desired state.");
            }
        }

        /// <summary>
        /// Internal method to perform the actual UI toggling and time scale adjustment.
        /// </summary>
        /// <param name="show">True to show the main menu (pause game), false to hide it (resume game).</param>
        void _ToggleMainMenu(bool show)
        {
            Debug.Log($"MetaGameController:_ToggleMainMenu called with show={show}. Current Time.timeScale: {Time.timeScale}.");
            if (show) // Transition to Main Menu State (Game is paused, menu UI is active)
            {
                Time.timeScale = 0; // Pause the game
                if (mainMenu != null) mainMenu.gameObject.SetActive(true); // Activate Main Menu Canvas
                Debug.Log($"MetaGameController:_ToggleMainMenu: mainMenu.gameObject.SetActive(true) called. Result: {(mainMenu != null && mainMenu.gameObject != null ? mainMenu.gameObject.activeSelf.ToString() : "mainMenu is NULL or Missing GameObject")}");

                foreach (var i in gamePlayCanvasii) {
                    if (i != null) i.gameObject.SetActive(false);
                }
            }
            else // Transition to Gameplay State (Game is running, gameplay UI is active)
            {
                Time.timeScale = 1; // Resume the game
                if (mainMenu != null) mainMenu.gameObject.SetActive(false);
                Debug.Log($"MetaGameController:_ToggleMainMenu: mainMenu.gameObject.SetActive(false) called.");

                foreach (var i in gamePlayCanvasii) {
                    if (i != null) i.gameObject.SetActive(true);
                }
            }
            this.showMainCanvas = show; // Sync internal state variable
        }

        void Update()
        {
            if (Input.GetButtonDown("Menu"))
            {
                ToggleMainMenu(show: !showMainCanvas);
            }
        }
    }
}