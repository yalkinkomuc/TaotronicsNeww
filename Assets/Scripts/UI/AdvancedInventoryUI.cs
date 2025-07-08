using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class AdvancedInventoryUI : BaseUIPanel
{
    public static AdvancedInventoryUI Instance { get; private set; }
    
    [Header("Main UI References")]
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private GameObject collectiblesPanel;
    
    [Header("Equipment Slots (Left Side)")]
    [SerializeField] private UI_EquipmentSlot weaponSlot;
    [SerializeField] private UI_EquipmentSlot armorSlot;
    [SerializeField] private UI_EquipmentSlot secondaryWeaponSlot;
    [SerializeField] private UI_EquipmentSlot accessorySlot;
    
    [Header("Rune Slots (Right Side)")]
    [SerializeField] private UI_RuneSlot[] runeSlots = new UI_RuneSlot[6];
    
    [Header("Equipment Selection Panel")]
    [SerializeField] private UI_EquipmentSelectionPanel equipmentSelectionPanel;
    
    [Header("Upgrade Materials Display (Top)")]
    [SerializeField] private Transform materialsParent;
    [SerializeField] private GameObject materialDisplayPrefab;
    
    [Header("Character Stats (Bottom)")]
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI attackDamageText;
    [SerializeField] private TextMeshProUGUI abilityPowerText;
    [SerializeField] private TextMeshProUGUI defenseText;
    [SerializeField] private TextMeshProUGUI criticalChanceText;
    
    
    [Header("Page Buttons")]
    [SerializeField] private Button inventoryPageButton;
    [SerializeField] private Button collectiblesPageButton;
    
    [Header("Other UI")]
    [SerializeField] private Button closeButton;
    [SerializeField] private TextMeshProUGUI gearCapacityText;
    
    // Private fields
    private List<UI_MaterialDisplay> materialDisplays = new List<UI_MaterialDisplay>();
    private bool isCollectiblesPage = false;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Instance atandıktan sonra başlangıçta kapat
            gameObject.SetActive(false);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }
    
    private void Start()
    {
        // Only initialize if this is the active instance
        if (Instance == this)
        {
            InitializeUI();
            SetupEventListeners();
        }
    }
    
    private void InitializeUI()
    {
        // Start with inventory page active
        ShowInventoryPage();
        
        // Create UI elements
        CreateMaterialDisplays();
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
        
        // Button events
        inventoryPageButton?.onClick.AddListener(ShowInventoryPage);
        collectiblesPageButton?.onClick.AddListener(ShowCollectiblesPage);
        closeButton?.onClick.AddListener(CloseInventory);
    }
    
    private void OnDestroy()
    {
        // Unsubscribe from events
        EquipmentManager.OnEquipmentChanged -= OnEquipmentChanged;
        EquipmentManager.OnRuneChanged -= OnRuneChanged;
        EquipmentManager.OnStatsUpdated -= UpdateStatsDisplay;
        
        if (Instance == this)
        {
            Instance = null;
        }
    }
    
    #region Initialization
    
    private void CreateMaterialDisplays()
    {
        materialDisplays.Clear();
        
        // Fixed 6 materials that will always be displayed
        MaterialType[] fixedMaterials = new MaterialType[]
        {
            MaterialType.Leather,
            MaterialType.Iron,
            MaterialType.Rock,
            MaterialType.Diamond,
            MaterialType.Crystal,
            MaterialType.Gem
        };
        
        foreach (MaterialType materialType in fixedMaterials)
        {
            GameObject displayObj = Instantiate(materialDisplayPrefab, materialsParent);
            UI_MaterialDisplay display = displayObj.GetComponent<UI_MaterialDisplay>();
            
            if (display != null)
            {
                display.SetMaterialType(materialType);
                materialDisplays.Add(display);
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
        gameObject.SetActive(true);
        UpdateMaterialDisplays();
        UpdateStatsDisplay();
        UpdateGearCapacity();
        
        if (UIInputBlocker.instance != null)
        {
            UIInputBlocker.instance.AddPanel(gameObject);
        }
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
        inventoryPanel.SetActive(true);
        collectiblesPanel.SetActive(false);
        UpdateGearCapacity();
    }
    
    public void ShowCollectiblesPage()
    {
        isCollectiblesPage = true;
        inventoryPanel.SetActive(false);
        collectiblesPanel.SetActive(true);
        RefreshCollectiblesDisplay();
    }
    
    #endregion
    
    #region Display Updates
    
    private void RefreshCollectiblesDisplay()
    {
        if (Inventory.instance == null || !isCollectiblesPage) return;
        
        // Get collectible items
        var collectibles = Inventory.instance.inventoryItems
            .Where(item => item.data.itemType == ItemType.Collectible)
            .ToList();
        
        // TODO: Implement collectibles display with UI_CollectiblesPanel
        Debug.Log($"Displaying {collectibles.Count} collectibles");
    }
    
    private void UpdateMaterialDisplays()
    {
        if (Inventory.instance == null) return;
        
        // Update each material display with current count
        foreach (var display in materialDisplays)
        {
            display.UpdateCount();
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
        // Find PlayerStats directly
        PlayerStats playerStats = FindFirstObjectByType<PlayerStats>();
        if (playerStats == null) return;

        if (healthText != null)
            healthText.text = Mathf.RoundToInt(playerStats.maxHealth.GetValue()).ToString();

        if (attackDamageText != null)
            attackDamageText.text = Mathf.RoundToInt(playerStats.baseDamage.GetValue()).ToString();

        if (abilityPowerText != null)
        {
            float elementalMultiplier = playerStats.GetTotalElementalDamageMultiplier();
            float percentage = (elementalMultiplier - 1f) * 100f;
            abilityPowerText.text = $"{Mathf.RoundToInt(percentage)}%";
        }

        if (defenseText != null)
        {
            // Defense shows damage reduction percentage
            float defenseReduction = Mathf.Clamp(playerStats.defenseStat, 0f, 80f);
            defenseText.text = $"{Mathf.RoundToInt(defenseReduction)}%";
        }

        if (criticalChanceText != null)
        {
            float critPercentage = playerStats.criticalChance * 100f;
            criticalChanceText.text = $"{Mathf.RoundToInt(critPercentage)}%";
        }
    }
    
    #endregion
    
    #region Event Handlers
    
    private void OnEquipmentChanged(EquipmentSlot slot, EquipmentData equipment)
    {
        // Equipment display is handled by individual equipment slots
        UpdateStatsDisplay();
        UpdateGearCapacity();
    }
    
    private void OnRuneChanged(int slotIndex, RuneData rune)
    {
        // Rune display is handled by individual rune slots
        UpdateStatsDisplay();
        UpdateGearCapacity();
    }
    
    #endregion
} 