using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Platformer.Mechanics;
using UnityEngine.EventSystems; // Required for clearing selection

namespace Platformer.UI
{
    public class MainUIController : MonoBehaviour
    {
        public GameObject[] panels;
        public MetaGameController metaGameController;

        void Start()
        {
            metaGameController = FindObjectOfType<MetaGameController>();
            if (metaGameController == null)
                Debug.LogError("MainUIController: MetaGameController not found in scene!");
            Debug.Log($"MainUIController: Start() called in scene '{SceneManager.GetActiveScene().name}'.");
        }

        void OnEnable()
        {
            Debug.Log("MainUIController OnEnable: This script's GameObject (Start Menu Canvas) is now active! Calling SetActivePanel(0).");
            SetActivePanel(0);
        }

        public void SetActivePanel(int index)
        {
            Debug.Log($"MainUIController: SetActivePanel({index}) called. Panels array length: {panels.Length}");
            if (index >= 0 && index < panels.Length)
            {
                for (var i = 0; i < panels.Length; i++)
                {
                    var active = i == index;
                    var g = panels[i];

                    if (g == null)
                    {
                        Debug.LogWarning($"MainUIController: Panel at index {i} is NULL in the panels array! Please assign all panels.");
                        continue;
                    }

                    if (g.activeSelf != active) g.SetActive(active);
                    Debug.Log($"MainUIController: Panel {g.name} set active: {active}.");
                }
            }
            else
            {
                Debug.LogWarning($"MainUIController: SetActivePanel({index}) called with invalid index. No panel activated.");
            }
        }

        public void ToggleCreditsPanel()
        {
            Debug.Log("MainUIController: ToggleCreditsPanel() called.");

            // Find currently active panel
            for (int i = 0; i < panels.Length; i++)
            {
                if (panels[i] != null && panels[i].activeSelf)
                {
                    if (i == 1)
                    {
                        Debug.Log("Credits panel is currently active — switching to Main Menu.");
                        SetActivePanel(0);
                    }
                    else
                    {
                        Debug.Log("Switching to Credits panel.");
                        SetActivePanel(1);
                    }

                    // ✅ FIX: Deselect UI so hover state works again
                    EventSystem.current.SetSelectedGameObject(null);
                    return;
                }
            }

            // No panel was active? Default to showing credits
            Debug.Log("No panels were active — defaulting to Credits panel.");
            SetActivePanel(1);

            // Just in case
            EventSystem.current.SetSelectedGameObject(null);
        }

        public void StartGame()
        {
            Debug.Log("MainUIController: StartGame() called from button. Requesting MetaGameController to hide Main Menu and trigger initial fade.");

            if (metaGameController != null)
            {
                metaGameController.ToggleMainMenu(show: false);
            }
            else
            {
                Debug.LogError("MainUIController: MetaGameController not assigned or found! Cannot start game.");
            }

            PlayerController player = FindObjectOfType<PlayerController>();
            if (player != null)
            {
                Combat playerCombat = player.GetComponent<Combat>();
                if (playerCombat != null)
                {
                    playerCombat.TriggerInitialGameFade();
                }
                else
                {
                    Debug.LogError("MainUIController: Player's Combat component not found! Cannot trigger initial game fade.");
                }
            }
            else
            {
                Debug.LogError("MainUIController: PlayerController not found! Cannot trigger initial game fade.");
            }
        }
    }
}