using Platformer.Mechanics;
using Platformer.UI;
using UnityEngine;
using System.Collections; // Needed for IEnumerator if you plan to use coroutines later
using UnityEngine.SceneManagement; // Needed for SceneManager.GetActiveScene().name in logs

namespace Platformer.UI
{
    /// <summary>
    /// The MetaGameController is responsible for switching control between the high level
    /// contexts of the application, e.g., the Main Menu and Gameplay systems.
    /// </summary>
    public class MetaGameController : MonoBehaviour
    {
        /// <summary>
        /// The main UI object which is used for the menu (your Start Menu Canvas).
        /// </summary>
        public MainUIController mainMenu; // Assign your Start Menu Canvas GameObject here

        /// <summary>
        /// A list of Canvas objects which are used during gameplay (when the main UI is turned off).
        /// </summary>
        public Canvas[] gamePlayCanvasii; // Assign your gameplay HUDs/Canvases here

        /// <summary>
        /// The Game Controller (GameObject with GameController.cs) instance.
        /// </summary>
        public GameController gameController; // Assign the GameObject with GameController.cs here

        // Reference to the persistent GameManager singleton.
        GameManager gameManager; 

        // Tracks the current active state of the main menu (true if shown, false if hidden).
        public bool showMainCanvas; 

        void Awake() 
        {
            // Get singleton instances reliably.
            gameManager = GameManager.Instance; 
            if (gameManager == null) Debug.LogError("MetaGameController: GameManager instance not found in scene!");

            gameController = GameController.Instance; 
            if (gameController == null) Debug.LogError("MetaGameController: GameController instance not found in scene!");

            // Set initial UI state based on GameManager.showMenu preference when the scene loads.
            // This should correctly show the menu if 'gameManager.showMenu' is true (e.g., after a restart).
            Debug.Log($"MetaGameController Awake in Scene: '{SceneManager.GetActiveScene().name}'. GameManager.showMenu is {gameManager?.showMenu.ToString() ?? "NULL GM"}");
            _ToggleMainMenu(gameManager.showMenu); // Call private helper to set initial active states
            this.showMainCanvas = gameManager.showMenu; // Sync internal state
            Debug.Log($"MetaGameController Awake: After _ToggleMainMenu, mainMenu.gameObject.activeSelf is {(mainMenu != null && mainMenu.gameObject != null ? mainMenu.gameObject.activeSelf.ToString() : "mainMenu is NULL or Missing GameObject")}");
        }

        void OnEnable() 
        {
            // OnEnable can be called multiple times if the GameObject is toggled.
            // Initial state setting is handled in Awake to prevent re-toggling.
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
            }
        }

        /// <summary>
        /// Internal method to perform the actual UI toggling and time scale adjustment.
        /// </summary>
        /// <param name="show">True to show the main menu (pause game), false to hide it (resume game).</param>
        void _ToggleMainMenu(bool show)
        {
            Debug.Log($"MetaGameController:_ToggleMainMenu called with show={show}. mainMenu reference is {(mainMenu == null ? "NULL" : "NOT NULL")}.");
            if (show) // Transition to Main Menu State (Game is paused, menu UI is active)
            {
                Time.timeScale = 0; // Pause the game
                if (mainMenu != null) mainMenu.gameObject.SetActive(true); // Activate Main Menu Canvas
                Debug.Log($"MetaGameController:_ToggleMainMenu: mainMenu.gameObject.SetActive(true) called. Result: {(mainMenu != null && mainMenu.gameObject != null ? mainMenu.gameObject.activeSelf.ToString() : "mainMenu is NULL or Missing GameObject")}");
                
                // Deactivate all gameplay canvases (HUD, score, etc.)
                foreach (var i in gamePlayCanvasii) {
                    if (i != null) i.gameObject.SetActive(false); // Ensure Canvas is not null before accessing its GameObject
                }
            }
            else // Transition to Gameplay State (Game is running, gameplay UI is active)
            {
                Time.timeScale = 1; // Resume the game
                if (mainMenu != null) mainMenu.gameObject.SetActive(false); // Deactivate Main Menu Canvas
                Debug.Log($"MetaGameController:_ToggleMainMenu: mainMenu.gameObject.SetActive(false) called.");
                
                // Activate all gameplay canvases (HUD, score, etc.)
                foreach (var i in gamePlayCanvasii) {
                    if (i != null) i.gameObject.SetActive(true); // Ensure Canvas is not null
                }
            }
            this.showMainCanvas = show; // Sync internal state variable
        }

        void Update()
        {
            // Input for toggling menu (e.g., Pause button)
            if (Input.GetButtonDown("Menu")) // Make sure "Menu" button is set in Input Manager
            {
                ToggleMainMenu(show: !showMainCanvas); // Toggle the menu state
            }
        }

        // The RestartCurrentGame method is not used in the scene reload strategy.
        // It would be part of an in-scene reset system.
        // public void RestartCurrentGame() { /* ... */ } 
    }
}