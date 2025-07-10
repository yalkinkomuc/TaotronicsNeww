using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class AdvancedInventoryUI : BaseUIPanel
{
    public static AdvancedInventoryUI Instance { get; private set; }
    
    [Header("Main UI References")]
    [SerializeField] private GameObject inventoryPanel;  // Ana parent panel - hep açık kalacak
    [SerializeField] private GameObject collectiblesPanel;
    
    [Header("Inventory Sub-Panels")]
    [SerializeField] private GameObject equipmentItemsPanel;  // Equipment kısmı
    [SerializeField] private GameObject equipmentRunesPanel;  // Runes kısmı
    
    [Header("Equipment Slots (Left Side)")]
    [SerializeField] private UI_EquipmentSlot weaponSlot;
    [SerializeField] private UI_EquipmentSlot armorSlot;
    [SerializeField] private UI_EquipmentSlot secondaryWeaponSlot;
    [SerializeField] private UI_EquipmentSlot accessorySlot;
    
    [Header("Rune Slots (Right Side)")]
    [SerializeField] private UI_RuneSlot[] runeSlots = new UI_RuneSlot[6];
    
    [Header("Equipment Selection Panel")]
    [SerializeField] private UI_EquipmentSelectionPanel equipmentSelectionPanel;
    
    [Header("Upgrade Materials Display (Top) - Static UI Elements")]
    [SerializeField] private UI_MaterialDisplay leatherDisplay;
    [SerializeField] private UI_MaterialDisplay ironDisplay;
    [SerializeField] private UI_MaterialDisplay rockDisplay;
    [SerializeField] private UI_MaterialDisplay diamondDisplay;
    [SerializeField] private UI_MaterialDisplay crystalDisplay;
    [SerializeField] private UI_MaterialDisplay gemDisplay;
    
    [Header("Character Stats (Bottom)")]
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI attackDamageText;
    [SerializeField] private TextMeshProUGUI abilityPowerText;
    [SerializeField] private TextMeshProUGUI defenseText;
    [SerializeField] private TextMeshProUGUI criticalChanceText;
    [SerializeField] private TextMeshProUGUI goldText;
    
    [Header("Tab System")]
    [SerializeField] private TabManager tabManager;
    
    [Header("Page Buttons")]
    [SerializeField] private Button inventoryPageButton;
    [SerializeField] private Button collectiblesPageButton;
    
    [Header("Other UI")]
    [SerializeField] private Button closeButton;
    [SerializeField] private TextMeshProUGUI gearCapacityText;
    
    // Private fields
    private UI_MaterialDisplay[] materialDisplays;
    private bool isCollectiblesPage = false;
    
    // Public accessor for other systems
    public bool IsCollectiblesPage => isCollectiblesPage;
    public bool IsInventoryOpen => gameObject.activeInHierarchy;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Instance atandıktan sonra başlangıçta kapat
            gameObject.SetActive(false);
            
            // Also ensure all panels are closed
            EnsureAllPanelsClosed();
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }
    
    private void EnsureAllPanelsClosed()
    {
        // Force close all sub-panels
        if (inventoryPanel != null) inventoryPanel.SetActive(false);
        if (collectiblesPanel != null) collectiblesPanel.SetActive(false);
        if (equipmentItemsPanel != null) equipmentItemsPanel.SetActive(false);
        if (equipmentRunesPanel != null) equipmentRunesPanel.SetActive(false);
    }
    
    private void Start()
    {
        // Only initialize if this is the active instance
        if (Instance == this)
        {
            InitializeUI();
            SetupEventListeners();
            InitializeTabSystem();
        }
    }
    

    
    private void InitializeUI()
    {
        // Start with inventory page active
        //ShowInventoryPage();
        
        // Create UI elements
        InitializeMaterialDisplays();
        InitializeEquipmentSlots();
        InitializeRuneSlots();
        
        // Initial display update
        UpdateMaterialDisplays();
        UpdateStatsDisplay();
        UpdateGearCapacity();
    }
    
    private void SetupEventListeners()
    {
        // Equipment and rune change events
        EquipmentManager.OnEquipmentChanged += OnEquipmentChanged;
        EquipmentManager.OnRuneChanged += OnRuneChanged;
        EquipmentManager.OnStatsUpdated += UpdateStatsDisplay;
        
        // Button events (fallback for manual clicking)
        inventoryPageButton?.onClick.AddListener(() => tabManager?.SelectTab(0));
        collectiblesPageButton?.onClick.AddListener(() => tabManager?.SelectTab(1));
        closeButton?.onClick.AddListener(CloseInventory);
        
        // Tab system event
        TabManager.OnTabChanged += OnTabChanged;
    }
    
    private void InitializeTabSystem()
    {
        if (tabManager == null)
        {
            return;
        }
        
        // Don't initialize tabs here - they will be initialized when inventory opens
        // This prevents tabs from being activated during startup
        SetupTabs();
    }
    
    private void SetupTabs()
    {
        // Create inventory tab
        TabData inventoryTab = new TabData
        {
            tabName = "Inventory",
            tabPanel = inventoryPanel,
            tabButton = inventoryPageButton,
            onTabSelected = () => ShowInventoryPage()
        };
        
        // Create collectibles tab
        TabData collectiblesTab = new TabData
        {
            tabName = "Collectibles", 
            tabPanel = collectiblesPanel,
            tabButton = collectiblesPageButton,
            onTabSelected = () => ShowCollectiblesPage()
        };
        
        // Note: We would add these tabs programmatically, but since TabManager 
        // uses SerializeField tabs list, we'll configure them in the inspector instead
        // This setup is more for future expansion where tabs might be added dynamically
    }
    
    private void OnTabChanged(int tabIndex, string tabName)
    {
        // Update our internal state based on tab change
        isCollectiblesPage = (tabIndex == 1);
        
        // Update UI elements that depend on current tab
        UpdateGearCapacity();
    }
    
    private void OnDestroy()
    {
        // Unsubscribe from events
        EquipmentManager.OnEquipmentChanged -= OnEquipmentChanged;
        EquipmentManager.OnRuneChanged -= OnRuneChanged;
        EquipmentManager.OnStatsUpdated -= UpdateStatsDisplay;
        TabManager.OnTabChanged -= OnTabChanged;
        
        if (Instance == this)
        {
            Instance = null;
        }
    }
    
    #region Initialization
    
    private void InitializeMaterialDisplays()
    {
        // Initialize static material displays with their types
        materialDisplays = new UI_MaterialDisplay[]
        {
            leatherDisplay,
            ironDisplay,
            rockDisplay,
            diamondDisplay,
            crystalDisplay,
            gemDisplay
        };
        
        // Set material types for each display
        MaterialType[] materialTypes = new MaterialType[]
        {
            MaterialType.Leather,
            MaterialType.Iron,
            MaterialType.Rock,
            MaterialType.Diamond,
            MaterialType.Crystal,
            MaterialType.Gem
        };
        
        for (int i = 0; i < materialDisplays.Length; i++)
        {
            if (materialDisplays[i] != null && i < materialTypes.Length)
            {
                materialDisplays[i].SetMaterialType(materialTypes[i]);
            }
            else if (materialDisplays[i] == null)
            {
                Debug.LogWarning($"Material display {i} ({materialTypes[i]}) is not assigned in AdvancedInventoryUI!");
            }
        }
    }
    
    private void InitializeEquipmentSlots()
    {
        weaponSlot?.Initialize(EquipmentSlot.MainWeapon);
        armorSlot?.Initialize(EquipmentSlot.Armor);
        secondaryWeaponSlot?.Initialize(EquipmentSlot.SecondaryWeapon);
        accessorySlot?.Initialize(EquipmentSlot.Accessory);
    }
    
    private void InitializeRuneSlots()
    {
        for (int i = 0; i < runeSlots.Length; i++)
        {
            if (runeSlots[i] != null)
            {
                runeSlots[i].Initialize(i);
            }
        }
    }
    
    #endregion
    
    #region Public Methods
    
    public void OpenInventory()
    {
        // DOĞRU MANTIK: Başka blocking paneller açıksa inventory açılamaz!
        if (IsAnyBlockingPanelOpen())
        {
            Debug.Log("AdvancedInventoryUI: BLOCKED! Cannot open inventory while other panels are active");
            return;
        }
        
        gameObject.SetActive(true);
        
        // Initialize tab system when opening inventory
        if (tabManager != null)
        {
            // Ensure tabs are initialized first
            tabManager.InitializeTabs();
            
            // Then select the first tab (inventory tab)
            tabManager.SelectTab(0);
        }
        else
        {
            ShowInventoryPage(); // Fallback
        }
        
        UpdateMaterialDisplays();
        UpdateStatsDisplay();
        UpdateGearCapacity();
        
        if (UIInputBlocker.instance != null)
        {
            UIInputBlocker.instance.AddPanel(gameObject);
        }
    }
    
    private bool IsAnyBlockingPanelOpen()
    {
        // Checkpoint Selection Screen
        CheckpointSelectionScreen checkpointScreen = FindFirstObjectByType<CheckpointSelectionScreen>();
        if (checkpointScreen != null && checkpointScreen.gameObject.activeInHierarchy)
        {
            Debug.Log("AdvancedInventoryUI: CheckpointSelectionScreen is open - blocking inventory");
            return true;
        }
        
        // Skill Tree Panel
        SkillTreePanel skillTreePanel = FindFirstObjectByType<SkillTreePanel>();
        if (skillTreePanel != null && skillTreePanel.gameObject.activeInHierarchy)
        {
            Debug.Log("AdvancedInventoryUI: SkillTreePanel is open - blocking inventory");
            return true;
        }
        
        // Attributes Upgrade Panel
        AttributesUpgradePanel upgradePanel = FindFirstObjectByType<AttributesUpgradePanel>();
        if (upgradePanel != null && upgradePanel.gameObject.activeInHierarchy)
        {
            Debug.Log("AdvancedInventoryUI: AttributesUpgradePanel is open - blocking inventory");
            return true;
        }
        
        // Equipment Selection Panel
        UI_EquipmentSelectionPanel equipmentSelection = FindFirstObjectByType<UI_EquipmentSelectionPanel>();
        if (equipmentSelection != null && equipmentSelection.gameObject.activeInHierarchy)
        {
            Debug.Log("AdvancedInventoryUI: UI_EquipmentSelectionPanel is open - blocking inventory");
            return true;
        }
        
        // Chest UI
        UI_ChestInventory chestUI = FindFirstObjectByType<UI_ChestInventory>();
        if (chestUI != null && chestUI.gameObject.activeInHierarchy)
        {
            Debug.Log("AdvancedInventoryUI: UI_ChestInventory is open - blocking inventory");
            return true;
        }
        
        // Dialogue System
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive)
        {
            Debug.Log("AdvancedInventoryUI: DialogueManager is active - blocking inventory");
            return true;
        }
        
        return false; // Hiçbir blocking panel yok
    }
    
    public void CloseInventory()
    {
        gameObject.SetActive(false);
        
        if (UIInputBlocker.instance != null)
        {
            UIInputBlocker.instance.RemovePanel(gameObject);
        }
    }
    
    public void ShowInventoryPage()
    {
        isCollectiblesPage = false;
        
        // Ana inventory panel zaten açık - sadece alt panelleri kontrol et
        if (equipmentItemsPanel != null)
            equipmentItemsPanel.SetActive(true);
            
        if (equipmentRunesPanel != null)
            equipmentRunesPanel.SetActive(true);
            
        if (collectiblesPanel != null)
            collectiblesPanel.SetActive(false);
        
        UpdateGearCapacity();
    }
    
    public void ShowCollectiblesPage()
    {
        isCollectiblesPage = true;
        
        // Ana inventory panel açık kalır - sadece alt panelleri kapat
        if (equipmentItemsPanel != null)
            equipmentItemsPanel.SetActive(false);
            
        if (equipmentRunesPanel != null)
            equipmentRunesPanel.SetActive(false);
            
        if (collectiblesPanel != null)
            collectiblesPanel.SetActive(true);
        
        // Page açıldığında mevcut collectible'ları göster
        RefreshCollectiblesDisplay();
    }
    
    #endregion
    
    #region Display Updates
    
    public void RefreshCollectiblesDisplay()
    {
        if (Inventory.instance == null) return;
        
        // Get UI_CollectiblesPanel component from collectiblesPanel
        if (collectiblesPanel != null)
        {
            UI_CollectiblesPanel collectiblesUI = collectiblesPanel.GetComponent<UI_CollectiblesPanel>();
            if (collectiblesUI != null)
            {
                collectiblesUI.RefreshCollectiblesDisplay();
            }
            else
            {
                Debug.LogWarning("UI_CollectiblesPanel component not found on collectiblesPanel! Please add the component.");
            }
        }
        else
        {
            Debug.LogWarning("Collectibles panel reference is not set in AdvancedInventoryUI!");
        }
    }
    
    public void UpdateMaterialDisplays()
    {
        if (Inventory.instance == null) return;
        
        // Ensure material displays are initialized
        if (materialDisplays == null)
        {
            InitializeMaterialDisplays();
        }
        
        // Update each material display with current count
        if (materialDisplays != null)
        {
            foreach (var display in materialDisplays)
            {
                if (display != null)
                {
                    display.UpdateCount();
                }
            }
        }
    }
    
    private void UpdateGearCapacity()
    {
        if (Inventory.instance == null || gearCapacityText == null) return;
        
        // Count total inventory items
        int totalItems = Inventory.instance.inventoryItems.Count;
        int maxCapacity = 999; // Or whatever your max inventory size is
        
        gearCapacityText.text = $"{totalItems} / {maxCapacity}";
    }
    
    private void UpdateStatsDisplay()
    {
        if (EquipmentManager.Instance == null) return;
        
        var stats = EquipmentManager.Instance.GetAllStats();
        
        if (healthText != null)
            healthText.text = stats.ContainsKey(StatType.Health) ? stats[StatType.Health].ToString() : "0";
            
        if (attackDamageText != null)
            attackDamageText.text = stats.ContainsKey(StatType.Might) ? stats[StatType.Might].ToString() : "0";
            
        if (abilityPowerText != null)
            abilityPowerText.text = "0"; // Placeholder
            
        if (defenseText != null)
            defenseText.text = stats.ContainsKey(StatType.Armor) ? stats[StatType.Armor].ToString() : "0";
            
        if (criticalChanceText != null)
            criticalChanceText.text = stats.ContainsKey(StatType.CriticalChance) ? $"{stats[StatType.CriticalChance]}%" : "0%";
            
        // Update gold display from PlayerStats
        if (goldText != null)
        {
            PlayerStats playerStats = PlayerManager.instance?.player?.GetComponent<PlayerStats>();
            if (playerStats != null)
            {
                goldText.text = playerStats.gold.ToString() + " G";
            }
            else
            {
                goldText.text = "0 G";
            }
        }
    }
    
    #endregion
    
    #region Equipment Events
    
    private void OnEquipmentChanged(EquipmentSlot slot, EquipmentData equipment)
    {
        UpdateStatsDisplay();
    }
    
    private void OnRuneChanged(int slotIndex, RuneData rune)
    {
        UpdateStatsDisplay();
    }
    
    #endregion
} 