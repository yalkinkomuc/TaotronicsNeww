using UnityEngine;
using System.Collections.Generic;

public class UIInputBlocker : MonoBehaviour
{
    public static UIInputBlocker instance;

    [Header("UI Panels")]
    [Tooltip("UI panels that should disable gameplay input")]
    [SerializeField] private List<GameObject> uiPanels = new List<GameObject>();
    
    [Header("UIBlocker")]
    [SerializeField] private GameObject uiBlockerPanel;
    
    // Active UI panel count
    private int activePanelCount = 0;
    
    private void Awake()
    {
        // Singleton pattern
        if (instance == null)
        {
            instance = this;
            //DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        // Add tracker to each panel
        foreach (GameObject panel in uiPanels)
        {
            if (panel != null && panel.GetComponent<UIObjectTracker>() == null)
            {
                panel.AddComponent<UIObjectTracker>();
            }
        }
        
        // Reset input state
        activePanelCount = 0;
        EnableGameplayInput(true);
        
        // Check initial state
        CheckActivePanels();
    }
    
    // Check UI panels
    private void CheckActivePanels()
    {
        activePanelCount = 0;
        
        // Check active/inactive state for each panel
        foreach (GameObject panel in uiPanels)
        {
            if (panel != null && panel.activeInHierarchy)
            {
                activePanelCount++;
            }
        }
        
        // If there are panels, disable input
        if (activePanelCount > 0)
        {
            DisableGameplayInput();
        }
        else
        {
            EnableGameplayInput(true);
        }
    }
    
    // Called when a panel's visibility changes
    public void OnPanelVisibilityChanged(GameObject panel, bool isVisible)
    {
        if (isVisible)
        {
            activePanelCount++;
            if (activePanelCount == 1) // When first panel activates
            {
                DisableGameplayInput();
            }
        }
        else
        {
            if (activePanelCount > 0)
                activePanelCount--;
            
            if (activePanelCount == 0) // When all panels are closed
            {
                EnableGameplayInput(true); // Force enable
            }
        }
        
        Debug.Log($"Panel visibility changed: {(isVisible ? "Shown" : "Hidden")}. Active panel count: {activePanelCount}");
    }
    
    // Add panel to the list
    public void AddPanel(GameObject panel)
    {
        if (!uiPanels.Contains(panel))
        {
            uiPanels.Add(panel);
            
            if (panel.GetComponent<UIObjectTracker>() == null)
            {
                panel.AddComponent<UIObjectTracker>();
            }
            
            // If panel is active, disable input
            if (panel.activeInHierarchy)
            {
                OnPanelVisibilityChanged(panel, true);
            }
        }
    }
    
    // Remove panel from the list
    public void RemovePanel(GameObject panel)
    {
        if (uiPanels.Contains(panel))
        {
            // If active, decrease counter
            if (panel.activeInHierarchy)
            {
                OnPanelVisibilityChanged(panel, false);
            }
            else
            {
                // If not active, still decrease counter (for safety)
                if (activePanelCount > 0)
                    activePanelCount--;
            }
            
            uiPanels.Remove(panel);
            
            Debug.Log($"Panel removed. Remaining active panel count: {activePanelCount}");
        }
    }
    
    // Show/hide UI blocker
    private void SetUIBlockerVisibility(bool isVisible)
    {
        if (uiBlockerPanel != null)
        {
            uiBlockerPanel.SetActive(isVisible);
        }
    }
    
    // ONLY disable GAMEPLAY INPUTS, do NOT affect UI inputs
    public void DisableGameplayInput()
    {
        // Disable player inputs
        Player player = PlayerManager.instance.player;
        if (player != null)
        {
            IPlayerInput playerInput = player.playerInput;
            if (playerInput != null)
            {
                // Only disable gameplay inputs
                playerInput.DisableGameplayInput();
            }
        }
        
        // Show UI Blocker
        SetUIBlockerVisibility(true);
    }
    
    // ONLY enable GAMEPLAY INPUTS
    public void EnableGameplayInput(bool forceEnable = false)
    {
        // If force enable is requested or there are no active panels
        if (forceEnable || activePanelCount <= 0)
        {
            // Enable player inputs
            Player player = PlayerManager.instance.player;
            if (player != null)
            {
                IPlayerInput playerInput = player.playerInput;
                if (playerInput != null)
                {
                    // Enable all inputs
                    playerInput.EnableAllInput();
                }
            }
            
            // Hide UI Blocker
            SetUIBlockerVisibility(false);
        }
        else
        {
            Debug.Log($"Still {activePanelCount} active panels, input remains disabled");
        }
    }
    
    // Activate panel
    public void ShowPanel(GameObject panel)
    {
        if (panel != null)
        {
            // Add to list if not present
            if (!uiPanels.Contains(panel))
            {
                AddPanel(panel);
            }
            
            // Activate panel
            panel.SetActive(true);
        }
    }
    
    // Deactivate panel
    public void HidePanel(GameObject panel)
    {
        if (panel != null)
        {
            panel.SetActive(false);
        }
    }
} 