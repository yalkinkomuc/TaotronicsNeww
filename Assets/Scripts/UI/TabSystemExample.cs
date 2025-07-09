using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Example script showing how to extend the tab system with new menus
/// This demonstrates the extensibility of the TabManager system
/// </summary>
public class TabSystemExample : MonoBehaviour
{
    [Header("Example Tab Configuration")]
    [SerializeField] private TabManager tabManager;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private Button settingsTabButton;
    
    [Header("Future Tabs (Examples)")]
    [SerializeField] private GameObject statsPanel;
    [SerializeField] private Button statsTabButton;
    [SerializeField] private GameObject craftingPanel;
    [SerializeField] private Button craftingTabButton;
    
    private void Start()
    {
        // Example of how to add new tabs programmatically
        // Uncomment and configure these when you want to add new menus
        
        // AddSettingsTab();
        // AddStatsTab();
        // AddCraftingTab();
    }
    
    /// <summary>
    /// Example: Adding a Settings tab
    /// </summary>
    private void AddSettingsTab()
    {
        if (tabManager == null || settingsPanel == null) return;
        
        TabData settingsTab = new TabData
        {
            tabName = "Settings",
            tabPanel = settingsPanel,
            tabButton = settingsTabButton,
            onTabSelected = () => {
                Debug.Log("Settings tab selected!");
                // Initialize settings panel
                InitializeSettingsPanel();
            }
        };
        
        tabManager.AddTab(settingsTab);
    }
    
    /// <summary>
    /// Example: Adding a Stats tab
    /// </summary>
    private void AddStatsTab()
    {
        if (tabManager == null || statsPanel == null) return;
        
        TabData statsTab = new TabData
        {
            tabName = "Stats",
            tabPanel = statsPanel,
            tabButton = statsTabButton,
            onTabSelected = () => {
                Debug.Log("Stats tab selected!");
                // Update player stats display
                UpdateStatsDisplay();
            }
        };
        
        tabManager.AddTab(statsTab);
    }
    
    /// <summary>
    /// Example: Adding a Crafting tab
    /// </summary>
    private void AddCraftingTab()
    {
        if (tabManager == null || craftingPanel == null) return;
        
        TabData craftingTab = new TabData
        {
            tabName = "Crafting",
            tabPanel = craftingPanel,
            tabButton = craftingTabButton,
            onTabSelected = () => {
                Debug.Log("Crafting tab selected!");
                // Initialize crafting interface
                InitializeCraftingPanel();
            }
        };
        
        tabManager.AddTab(craftingTab);
    }
    
    // Example initialization methods for different panels
    private void InitializeSettingsPanel()
    {
        // Example: Setup settings UI
        // - Audio sliders
        // - Graphics options
        // - Controls configuration
        
        Debug.Log("Settings panel initialized");
    }
    
    private void UpdateStatsDisplay()
    {
        // Example: Update stats from PlayerStats
        // - Show player level, experience
        // - Display attribute points
        // - Show skill progression
        
        Debug.Log("Stats display updated");
    }
    
    private void InitializeCraftingPanel()
    {
        // Example: Setup crafting interface
        // - Load available recipes
        // - Show crafting materials
        // - Initialize crafting slots
        
        Debug.Log("Crafting panel initialized");
    }
    
    /// <summary>
    /// Example: Dynamic tab addition based on game state
    /// </summary>
    public void AddConditionalTabs()
    {
        // Example conditions for adding tabs
        
        // Add crafting tab only if player has crafting skill
        /*
        if (PlayerManager.instance.player.HasSkill("Crafting"))
        {
            AddCraftingTab();
        }
        */
        
        // Add stats tab only if player is above level 5
        /*
        if (PlayerManager.instance.player.GetLevel() > 5)
        {
            AddStatsTab();
        }
        */
        
        // Add settings tab if player has access to settings
        /*
        if (PlayerManager.instance.player.CanAccessSettings())
        {
            AddSettingsTab();
        }
        */
    }
    
    /// <summary>
    /// Example: Removing tabs based on conditions
    /// </summary>
    public void RemoveConditionalTabs()
    {
        // Example: Remove crafting tab if player loses crafting skill
        // tabManager.RemoveTab(2); // Assuming crafting is at index 2
        
        // Or disable tab instead of removing
        // tabManager.SetTabEnabled(2, false);
    }
    
    /// <summary>
    /// Demonstrates how other systems can interact with the tab system
    /// </summary>
    public void OnPlayerLevelUp(int newLevel)
    {
        // Example: Unlock new tabs when player levels up
        if (newLevel == 5)
        {
            AddStatsTab();
            Debug.Log("Stats tab unlocked!");
        }
        
        if (newLevel == 10)
        {
            AddCraftingTab();
            Debug.Log("Crafting tab unlocked!");
        }
    }
    
    /// <summary>
    /// Example: Save/Load tab preferences
    /// </summary>
    public void SaveTabPreferences()
    {
        // Save which tabs are enabled
        // Save last selected tab
        // Save tab order preferences
        
        int lastTab = tabManager.CurrentTabIndex;
        PlayerPrefs.SetInt("LastSelectedTab", lastTab);
        PlayerPrefs.Save();
    }
    
    public void LoadTabPreferences()
    {
        // Load and restore tab preferences
        int lastTab = PlayerPrefs.GetInt("LastSelectedTab", 0);
        
        if (lastTab < tabManager.TabCount)
        {
            tabManager.SelectTab(lastTab);
        }
    }
} 