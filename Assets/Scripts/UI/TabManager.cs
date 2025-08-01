using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;

[System.Serializable]
public class TabData
{
    [Header("Tab Configuration")]
    public string tabName;
    public GameObject tabPanel;
    public Button tabButton;
    public Action onTabSelected;
    
    [Header("Visual")]
    public Color activeColor = Color.white;
    public Color inactiveColor = Color.gray;
}

public class TabManager : MonoBehaviour
{
    [Header("Tab Settings")]
    [SerializeField] private List<TabData> tabs = new List<TabData>();
    [SerializeField] private int currentTabIndex = 0;
    
    [Header("Navigation Hints")]
    [SerializeField] private GameObject navigationHintPanel;
    [SerializeField] private TextMeshProUGUI leftHintText;
    [SerializeField] private TextMeshProUGUI rightHintText;
    
    
    [Header("Audio")]
    [SerializeField] private AudioClip tabSwitchSound;
    
    // Events
    public static event Action<int, string> OnTabChanged;
    
    // Private fields
    private AudioSource audioSource;
    private bool isInitialized = false;
    
    private void Awake()
    {
        // Get or add AudioSource for tab switch sounds
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }
    
    private void Start()
    {
        // Don't auto-initialize tabs on Start
        // Let the parent UI call InitializeTabs() when it opens
    }
    
    private void Update()
    {
        // Only handle input when this UI is active AND its parent is active
        if (!gameObject.activeInHierarchy || !isInitialized) return;
        
        // Extra check: Only handle tab switching when parent UI is also active
        Transform parent = transform.parent;
        if (parent != null && !parent.gameObject.activeInHierarchy) return;
        
        HandleTabSwitching();
    }
    
    #region Initialization
    
    public void InitializeTabs()
    {
        if (tabs.Count == 0)
        {
            Debug.LogWarning("TabManager: No tabs configured!");
            return;
        }
        
        // Setup tab buttons
        for (int i = 0; i < tabs.Count; i++)
        {
            int tabIndex = i; // Local copy for closure
            if (tabs[i].tabButton != null)
            {
                tabs[i].tabButton.onClick.AddListener(() => SelectTab(tabIndex));
            }
        }
        
        // Initialize with first tab
        SelectTab(currentTabIndex);
        
        
        isInitialized = true;
    }
    
    public void AddTab(TabData newTab)
    {
        tabs.Add(newTab);
        
        // Setup button listener if button exists
        if (newTab.tabButton != null)
        {
            int tabIndex = tabs.Count - 1;
            newTab.tabButton.onClick.AddListener(() => SelectTab(tabIndex));
        }
        
        
    }
    
    public void RemoveTab(int index)
    {
        if (index >= 0 && index < tabs.Count)
        {
            tabs.RemoveAt(index);
            
            // Adjust current tab index if necessary
            if (currentTabIndex >= tabs.Count)
            {
                currentTabIndex = Math.Max(0, tabs.Count - 1);
            }
            
            if (tabs.Count > 0)
            {
                SelectTab(currentTabIndex);
            }
            
          
        }
    }
    
    #endregion
    
    #region Tab Navigation
    
        private void HandleTabSwitching()
    {
       
        
        

        // Handle ESC input to close the entire tab UI system
        if (UserInput.WasEscapePressed)
        {
            CloseParentUI();
            return; // Exit early since we're closing the UI
        }

        // Only handle tab switching if we have multiple tabs
        if (tabs.Count <= 1) return;

        // PERFORMANCE OPTIMIZATION: Only check expensive conditions when input is actually pressed
        bool leftPressed = UserInput.WasTabLeftPressed;
        bool rightPressed = UserInput.WasTabRightPressed;
        
        // Early exit if no input - avoid expensive checks
        if (!leftPressed && !rightPressed) return;
        
        
       
        bool allowed = IsTabSwitchingAllowed();
       
        
        if (!allowed)
        {
           
            return;
        }

     
        if (leftPressed)
        {
            SwitchToPreviousTab();
        }
        else if (rightPressed)
        {
            SwitchToNextTab();
        }
    }
    
    private bool IsTabSwitchingAllowed()
    {
        
        // Check if TabManager itself has any active tabs (this is the correct logic!)
        bool hasActiveTab = false;
        foreach (var tab in tabs)
        {
            if (tab.tabPanel != null && tab.tabPanel.activeInHierarchy)
            {
                hasActiveTab = true;
                break;
            }
        }
        
        if (!hasActiveTab)
        {
            return false;
        }
        
        
        // Check for blocking panels that should prevent tab switching
        
        // Checkpoint Selection Screen
        CheckpointSelectionScreen checkpointScreen = FindFirstObjectByType<CheckpointSelectionScreen>();
        if (checkpointScreen != null && checkpointScreen.gameObject.activeInHierarchy)
        {
            return false;
        }
        
        // Skill Tree Panel (usually named SkillTreePanel)
        GameObject skillTreePanel = GameObject.Find("SkillTreePanel");
        if (skillTreePanel != null && skillTreePanel.activeInHierarchy)
        {
            return false;
        }
        
        // Attributes Upgrade Panel
        AttributesUpgradePanel upgradePanel = FindFirstObjectByType<AttributesUpgradePanel>();
        if (upgradePanel != null && upgradePanel.gameObject.activeInHierarchy)
        {
            return false;
        }
        
        // Equipment Selection Panel (within inventory)
        UI_EquipmentSelectionPanel equipmentSelection = FindFirstObjectByType<UI_EquipmentSelectionPanel>();
        if (equipmentSelection != null && equipmentSelection.gameObject.activeInHierarchy)
        {
            return false;
        }
        
        // Dialogue System
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive)
        {
            return false;
        }
        
        return true;
    }
    
    private void CloseParentUI()
    {
        foreach (var tab in tabs)
        {
            if (tab.tabPanel != null)
            {
                tab.tabPanel.SetActive(false);
                
                // Remove all tab panels from UIInputBlocker when closing
                if (UIInputBlocker.instance != null)
                {
                    UIInputBlocker.instance.RemovePanel(tab.tabPanel);
                }
            }
        }
    }
    
    public void SwitchToNextTab()
    {
        if (tabs.Count <= 1) return;
        
        int nextIndex = (currentTabIndex + 1) % tabs.Count;
        SelectTab(nextIndex);
        PlayTabSwitchSound();
    }
    
    public void SwitchToPreviousTab()
    {
        if (tabs.Count <= 1) return;
        
        int prevIndex = (currentTabIndex - 1 + tabs.Count) % tabs.Count;
        SelectTab(prevIndex);
        PlayTabSwitchSound();
    }
    
    public void SelectTab(int tabIndex)
    {
        if (tabIndex < 0 || tabIndex >= tabs.Count) return;
        
        // Deactivate current tab
        if (currentTabIndex < tabs.Count)
        {
            SetTabActive(currentTabIndex, false);
        }
        
        // Activate new tab
        currentTabIndex = tabIndex;
        SetTabActive(currentTabIndex, true);
        
        // Execute tab's callback
        tabs[currentTabIndex].onTabSelected?.Invoke();
        
       
        
        // Fire event
        OnTabChanged?.Invoke(currentTabIndex, tabs[currentTabIndex].tabName);
    }
    
    private void SetTabActive(int tabIndex, bool active)
    {
        if (tabIndex < 0 || tabIndex >= tabs.Count) return;
        
        TabData tab = tabs[tabIndex];
        
        // Set panel visibility
        if (tab.tabPanel != null)
        {
            tab.tabPanel.SetActive(active);
            
            // Handle UIInputBlocker management for tab panels
            if (UIInputBlocker.instance != null)
            {
                if (active)
                {
                    // Add active tab panel to UIInputBlocker (if it's not the main inventory)
                    if (!tab.tabPanel.name.Contains("AdvancedInventoryUI"))
                    {
                        UIInputBlocker.instance.AddPanel(tab.tabPanel);
                    }
                }
                else
                {
                    // Remove inactive tab panel from UIInputBlocker
                    UIInputBlocker.instance.RemovePanel(tab.tabPanel);
                }
            }
        }
        
        // Set button visual state
        if (tab.tabButton != null)
        {
            Image buttonImage = tab.tabButton.GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.color = active ? tab.activeColor : tab.inactiveColor;
            }
            
            // Update button interactable state (active tab is not clickable)
            tab.tabButton.interactable = !active;
        }
    }
    
    #endregion
    
    #region Cleanup
    
    private void OnDestroy()
    {
        // Clean up UIInputBlocker when TabManager is destroyed
        if (UIInputBlocker.instance != null)
        {
            foreach (var tab in tabs)
            {
                if (tab.tabPanel != null)
                {
                    UIInputBlocker.instance.RemovePanel(tab.tabPanel);
                }
            }
        }
    }
    
    #endregion
    
    #region Navigation Hints
    
   
    
    
    
    #endregion
    
    #region Utility
    
    
    private void PlayTabSwitchSound()
    {
        if (audioSource != null && tabSwitchSound != null)
        {
            audioSource.PlayOneShot(tabSwitchSound);
        }
    }
    
    #endregion
    
    #region Public API
    
    public int CurrentTabIndex => currentTabIndex;
    public string CurrentTabName => currentTabIndex < tabs.Count ? tabs[currentTabIndex].tabName : "";
    public int TabCount => tabs.Count;
    
    public TabData GetCurrentTab()
    {
        return currentTabIndex < tabs.Count ? tabs[currentTabIndex] : null;
    }
    
    public TabData GetTab(int index)
    {
        return index >= 0 && index < tabs.Count ? tabs[index] : null;
    }
    
    public void SetTabEnabled(int index, bool enabled)
    {
        if (index >= 0 && index < tabs.Count && tabs[index].tabButton != null)
        {
            tabs[index].tabButton.gameObject.SetActive(enabled);
        }
    }
    
    #endregion
} 