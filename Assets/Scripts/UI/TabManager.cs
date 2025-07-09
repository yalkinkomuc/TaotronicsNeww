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
    [SerializeField] private bool showNavigationHints = true;
    
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
        InitializeTabs();
    }
    
    private void Update()
    {
        // Only handle input when this UI is active
        if (!gameObject.activeInHierarchy || !isInitialized) return;
        
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
        UpdateNavigationHints();
        
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
        
        UpdateNavigationHints();
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
            
            UpdateNavigationHints();
        }
    }
    
    #endregion
    
    #region Tab Navigation
    
    private void HandleTabSwitching()
    {
        // Get player input
        IPlayerInput playerInput = GetPlayerInput();
        if (playerInput == null) return;
        
        bool leftPressed = playerInput.tabLeftInput;
        bool rightPressed = playerInput.tabRightInput;
        
        if (leftPressed)
        {
            SwitchToPreviousTab();
        }
        else if (rightPressed)
        {
            SwitchToNextTab();
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
        
        // Update navigation hints
        UpdateNavigationHints();
        
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
    
    #region Navigation Hints
    
    private void UpdateNavigationHints()
    {
        if (!showNavigationHints || navigationHintPanel == null) return;
        
        // Show hints only if there are multiple tabs
        bool shouldShowHints = tabs.Count > 1;
        navigationHintPanel.SetActive(shouldShowHints);
        
        if (!shouldShowHints) return;
        
        // Update hint texts based on platform
        IPlayerInput playerInput = GetPlayerInput();
        bool isGamepad = playerInput is GamepadInput;
        
        if (leftHintText != null)
        {
            leftHintText.text = isGamepad ? "LB" : "Q";
        }
        
        if (rightHintText != null)
        {
            rightHintText.text = isGamepad ? "RB" : "E";
        }
    }
    
    public void SetNavigationHintsVisible(bool visible)
    {
        showNavigationHints = visible;
        UpdateNavigationHints();
    }
    
    #endregion
    
    #region Utility
    
    private IPlayerInput GetPlayerInput()
    {
        // Get player input from PlayerManager or find Player
        if (PlayerManager.instance != null && PlayerManager.instance.player != null)
        {
            return PlayerManager.instance.player.playerInput;
        }
        
        // Fallback: find player in scene
        Player player = FindFirstObjectByType<Player>();
        return player?.playerInput;
    }
    
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