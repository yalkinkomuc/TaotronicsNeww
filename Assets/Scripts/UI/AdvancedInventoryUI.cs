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
    
    [Header("Inventory Grid (Center)")]
    [SerializeField] private Transform inventoryGridParent;
    [SerializeField] private GameObject itemSlotPrefab;
    [SerializeField] private int inventorySlotCount = 40;
    
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
    [SerializeField] private TextMeshProUGUI unusedText1;
    [SerializeField] private TextMeshProUGUI unusedText2;
    
    [Header("Page Buttons")]
    [SerializeField] private Button inventoryPageButton;
    [SerializeField] private Button collectiblesPageButton;
    
    [Header("Other UI")]
    [SerializeField] private Button closeButton;
    [SerializeField] private TextMeshProUGUI gearCapacityText;
    
    // Private fields
    private List<UI_ItemSlot> inventorySlots = new List<UI_ItemSlot>();
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
        CreateInventorySlots();
        CreateMaterialDisplays();
        InitializeEquipmentSlots();
        InitializeRuneSlots();
        
        // Initial display update
        RefreshInventoryDisplay();
        UpdateMaterialDisplays();
        UpdateStatsDisplay();
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
    
    private void CreateInventorySlots()
    {
        inventorySlots.Clear();
        
        // Clear existing slots
        foreach (Transform child in inventoryGridParent)
        {
            Destroy(child.gameObject);
        }
        
        // Create new slots
        for (int i = 0; i < inventorySlotCount; i++)
        {
            GameObject slotObj = Instantiate(itemSlotPrefab, inventoryGridParent);
            UI_ItemSlot slot = slotObj.GetComponent<UI_ItemSlot>();
            
            if (slot != null)
            {
                inventorySlots.Add(slot);
                
                // Add click listener with slot index
                int slotIndex = i;
                Button slotButton = slotObj.GetComponent<Button>();
                if (slotButton != null)
                {
                    slotButton.onClick.AddListener(() => OnInventorySlotClicked(slotIndex));
                }
            }
        }
    }
    
    private void CreateMaterialDisplays()
    {
        materialDisplays.Clear();
        
        // Get all upgrade material types and create displays
        var materialTypes = System.Enum.GetValues(typeof(MaterialType));
        
        foreach (MaterialType materialType in materialTypes)
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
        RefreshInventoryDisplay();
        UpdateMaterialDisplays();
        UpdateStatsDisplay();
        
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
        RefreshInventoryDisplay();
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
    
    private void RefreshInventoryDisplay()
    {
        if (Inventory.instance == null || isCollectiblesPage) return;
        
        // Clear all slots first
        foreach (var slot in inventorySlots)
        {
            slot.UpdateSlot(null);
        }
        
        // Get filtered items
        List<InventoryItem> filteredItems = GetFilteredItems();
        
        // Update capacity display (only show runes and materials now)
        if (gearCapacityText != null)
        {
            gearCapacityText.text = $"{filteredItems.Count} / {inventorySlotCount}";
        }
        
        // Fill slots with filtered items
        for (int i = 0; i < filteredItems.Count && i < inventorySlots.Count; i++)
        {
            inventorySlots[i].UpdateSlot(filteredItems[i]);
        }
    }
    
    private void RefreshCollectiblesDisplay()
    {
        if (Inventory.instance == null || !isCollectiblesPage) return;
        
        // Get collectible items
        var collectibles = Inventory.instance.inventoryItems
            .Where(item => item.data.itemType == ItemType.Collectible)
            .ToList();
        
        // TODO: Implement collectibles display
        Debug.Log($"Displaying {collectibles.Count} collectibles");
    }
    
    private List<InventoryItem> GetFilteredItems()
    {
        if (Inventory.instance == null) return new List<InventoryItem>();
        
        // Show all items except collectibles (they have their own page)
        // and equipment items (they are selected via equipment slots now)
        return Inventory.instance.inventoryItems
            .Where(item => item.data.itemType != ItemType.Collectible &&
                          item.data.itemType != ItemType.Weapon &&
                          item.data.itemType != ItemType.Armor &&
                          item.data.itemType != ItemType.SecondaryWeapon &&
                          item.data.itemType != ItemType.Accessory)
            .ToList();
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
    
    private void UpdateStatsDisplay()
    {
        // Find PlayerStats component
        PlayerStats playerStats = FindFirstObjectByType<PlayerStats>();
        if (playerStats == null) return;
        
        // 5 stat gösterimi - AttributesUpgradePanel'deki gibi hesaplama
        
        // 1. Health (from Vitality)
        if (healthText != null)
            healthText.text = Mathf.RoundToInt(playerStats.maxHealth.GetValue()).ToString();
        
        // 2. Attack Damage (from Might)
        if (attackDamageText != null)
            attackDamageText.text = Mathf.RoundToInt(playerStats.baseDamage.GetValue()).ToString();
        
        // 3. Ability Power (from Mind) - elemental damage multiplier
        if (abilityPowerText != null)
        {
            float elementalMultiplier = playerStats.GetTotalElementalDamageMultiplier();
            int abilityPowerPercent = Mathf.RoundToInt((elementalMultiplier - 1f) * 100f);
            abilityPowerText.text = abilityPowerPercent.ToString() + "%";
        }
        
        // 4. Defense (from Defense stat) - damage reduction percentage
        if (defenseText != null)
        {
            int defenseReduction = Mathf.Min(Mathf.RoundToInt(playerStats.defenseStat), 80); // max 80% reduction
            defenseText.text = defenseReduction.ToString() + "%";
        }
        
        // 5. Critical Chance (from Luck)
        if (criticalChanceText != null)
            criticalChanceText.text = (playerStats.criticalChance * 100f).ToString("F1") + "%";
        
        // Hide unused fields
        if (unusedText1 != null)
            unusedText1.text = "";
        if (unusedText2 != null)
            unusedText2.text = "";
        
        Debug.Log("Character stats updated - 5 main stats displayed");
    }
    
    #endregion
    
    #region Event Handlers
    
    private void OnInventorySlotClicked(int slotIndex)
    {
        if (slotIndex >= inventorySlots.Count) return;
        
        var slot = inventorySlots[slotIndex];
        if (slot.item?.data == null) return;
        
        // Try to equip runes or use items
        var itemData = slot.item.data;
        
        if (itemData is RuneData rune)
        {
            // Try to equip rune to first available slot
            if (EquipmentManager.Instance != null)
            {
                for (int i = 0; i < 6; i++)
                {
                    if (EquipmentManager.Instance.IsRuneSlotEmpty(i))
                    {
                        bool equipped = EquipmentManager.Instance.EquipRune(rune, i);
                        if (equipped)
                        {
                            // Remove from inventory
                            Inventory.instance.RemoveItem(itemData);
                            break;
                        }
                    }
                }
            }
        }
        
        RefreshInventoryDisplay();
    }
    
    private void OnEquipmentChanged(EquipmentSlot slot, EquipmentData equipment)
    {
        // Equipment display is handled by individual equipment slots
        UpdateStatsDisplay();
    }
    
    private void OnRuneChanged(int slotIndex, RuneData rune)
    {
        // Rune display is handled by individual rune slots
        UpdateStatsDisplay();
    }
    
    #endregion
} 