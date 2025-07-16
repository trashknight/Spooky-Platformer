using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; // Needed for Debug.Log in case of scene name.

namespace Platformer.UI
{
    /// <summary>
    /// A simple controller for switching between UI panels.
    /// This script is attached to your Start Menu Canvas.
    /// </summary>
    public class MainUIController : MonoBehaviour
    {
        // Array of GameObjects representing different panels within this MainUI (e.g., Play Panel, Credits Panel).
        // Assign these in the Inspector.
        public GameObject[] panels; 
        // Reference to the MetaGameController (will be found in Start).
        public MetaGameController metaGameController;

        // Called when the script instance is being loaded.
        private void Start() 
        {
            // Attempt to find MetaGameController if not assigned in Inspector.
            metaGameController = FindObjectOfType<MetaGameController>();
            if (metaGameController == null) Debug.LogError("MainUIController: MetaGameController not found in scene!");
            Debug.Log($"MainUIController: Start() called in scene '{SceneManager.GetActiveScene().name}'.");
        }
            
        // Called when the object becomes enabled and active.
        void OnEnable()
        {
            Debug.Log("MainUIController OnEnable: This script's GameObject (Start Menu Canvas) is now active! Calling SetActivePanel(0).");
            // Set the first panel active when this MainUIController's GameObject becomes enabled.
            SetActivePanel(0); 
        }

        /// <summary>
        /// Sets a specific panel active and deactivates all others in the 'panels' array.
        /// </summary>
        /// <param name="index">The index of the panel to activate.</param>
        public void SetActivePanel(int index)
        {
            Debug.Log($"MainUIController: SetActivePanel({index}) called. Panels array length: {panels.Length}");
            // Validate the index to prevent errors (e.g., index out of bounds)
            if (index >= 0 && index < panels.Length) 
            {
                for (var i = 0; i < panels.Length; i++)
                {
                    var active = i == index; // Determine if the current panel in loop should be active
                    var g = panels[i]; // Get the panel GameObject from the array

                    if (g == null) {
                        Debug.LogWarning($"MainUIController: Panel at index {i} is NULL in the panels array! Please assign all panels.");
                        continue; // Skip null entries in the array
                    }
                    if (g.activeSelf != active) g.SetActive(active); // Set active state only if it needs to change
                    Debug.Log($"MainUIController: Panel {g.name} set active: {active}.");
                }
            } else {
                Debug.LogWarning($"MainUIController: SetActivePanel({index}) called with invalid index. No panel activated.");
            }
        }

        /// <summary>
        /// Public method called by a UI button to start the game (e.g., the Play Button).
        /// </summary>
        public void StartGame() {
            Debug.Log("MainUIController: StartGame() called from button. Requesting MetaGameController to hide Main Menu.");
            // This calls MetaGameController to hide the main menu and show gameplay UI.
            // 'show: false' will cause MetaGameController to hide the menu.
            if (metaGameController != null)
            {
                metaGameController.ToggleMainMenu(show: false); 
            } else {
                Debug.LogError("MainUIController: MetaGameController not assigned or found! Cannot start game.");
            }
        }

        // You might have other public methods here for credits, options, etc., called by other UI buttons.
    }
}
