using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class UI_EquipmentSelectionPanel : MonoBehaviour
{
    private static UI_EquipmentSelectionPanel _instance;
    public static UI_EquipmentSelectionPanel Instance 
    { 
        get 
        {
            if (_instance == null && !isInitializing)
            {
                isInitializing = true;
                LoadInstance();
            }
            return _instance;
        }
        private set => _instance = value;
    }
    
    [Header("Panel References")]
    [SerializeField] private GameObject selectionPanel;
    [SerializeField] private Transform itemGridParent;
    [SerializeField] private GameObject itemSelectionSlotPrefab;
    [SerializeField] private Button closeButton;
    [SerializeField] private TextMeshProUGUI panelTitleText;
    
    
    [Header("Positioning")]
    [SerializeField] private Vector2 panelOffset = new Vector2(200, 0); // Increased offset to position panel to the right of inventory
    
    private EquipmentSlot currentSlotType;
    private List<UI_ItemSelectionSlot> selectionSlots = new List<UI_ItemSelectionSlot>();
    private System.Action<EquipmentData> onItemSelected;
    
    // Lazy loading support
    private static string prefabPath = "UI/EquipmentSelection";
    private static bool isInitializing = false;
    
    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            isInitializing = false;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        // Initially hide the panel
        if (selectionPanel != null)
            selectionPanel.SetActive(false);
    }
    
    private static void LoadInstance()
    {
        // Load prefab from Resources
        GameObject prefab = Resources.Load<GameObject>(prefabPath);
        if (prefab != null)
        {
            // Find the Menu UI object
           GameObject menuUI = GameObject.Find("Menu_UI");
            if (menuUI != null)
            {
                GameObject instance = Instantiate(prefab, menuUI.transform);
                instance.name = "UI_EquipmentSelectionPanel (Lazy Loaded)";
                
                // The Awake method will set the instance
                Debug.Log("UI_EquipmentSelectionPanel loaded from Resources and added to Menu UI");
            }
            else
            {
                Debug.LogError("Menu UI object not found in scene! Cannot instantiate UI panel.");
                isInitializing = false;
            }
        }
        else
        {
            Debug.LogError($"Failed to load UI_EquipmentSelectionPanel prefab from path: {prefabPath}");
            isInitializing = false;
        }
    }
    
    private void Start()
    {
        // Setup close button
        if (closeButton != null)
            closeButton.onClick.AddListener(ClosePanel);
    }
    
    public void ShowSelectionPanel(EquipmentSlot slotType, Vector3 screenPosition, System.Action<EquipmentData> onSelected)
    {
        currentSlotType = slotType;
        onItemSelected = onSelected;
        
        // Position panel near the clicked slot
        PositionPanel(screenPosition);
        
        // Update panel title
        UpdatePanelTitle(slotType);
        
        // Populate with appropriate items
        PopulateItems(slotType);
        
        // Show panel
        selectionPanel.SetActive(true);
    }
    
    public void ClosePanel()
    {
        if (selectionPanel != null)
            selectionPanel.SetActive(false);
        
        onItemSelected = null;
    }
    
    private void PositionPanel(Vector3 screenPosition)
    {
        if (selectionPanel == null) return;
        
        // TEMPORARY: Just position at center for now
        selectionPanel.GetComponent<RectTransform>().localPosition = Vector3.zero;
        Debug.Log("Panel positioned at center (0,0,0)");
        
        // TODO: Fix positioning logic later
        /*
        // Apply offset to screen position
        Vector2 offsetPosition = (Vector2)screenPosition + panelOffset;
        
        // Get canvas for proper positioning
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            RectTransform canvasRect = canvas.GetComponent<RectTransform>();
            Vector2 localPosition;
            
            // Convert screen position to canvas local position
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect, offsetPosition, canvas.worldCamera, out localPosition))
            {
                selectionPanel.GetComponent<RectTransform>().localPosition = localPosition;
                Debug.Log($"Panel positioned at local position: {localPosition}");
            }
            else
            {
                Debug.LogError("Failed to convert screen position to local position!");
                // Fallback: position at center of canvas
                selectionPanel.GetComponent<RectTransform>().localPosition = Vector3.zero;
            }
        }
        else
        {
            Debug.LogError("No Canvas found for panel positioning!");
            // Fallback: position at center
            selectionPanel.GetComponent<RectTransform>().localPosition = Vector3.zero;
        }
        */
    }
    
    private void UpdatePanelTitle(EquipmentSlot slotType)
    {
        if (panelTitleText == null) return;
        
        string title = slotType switch
        {
            EquipmentSlot.MainWeapon => "Select Weapon",
            EquipmentSlot.Armor => "Select Armor",
            EquipmentSlot.SecondaryWeapon => "Select Secondary Weapon",
            EquipmentSlot.Accessory => "Select Accessory",
            _ => "Select Equipment"
        };
        
        panelTitleText.text = title;
    }
    
    private void PopulateItems(EquipmentSlot slotType)
    {
        // Clear existing slots
        ClearSelectionSlots();
        
        if (Inventory.instance == null) return;
        
        // Handle weapon slots differently
        if (slotType == EquipmentSlot.MainWeapon || slotType == EquipmentSlot.SecondaryWeapon)
        {
            PopulateWeaponSlots(slotType);
        }
        else
        {
            // Handle other equipment types (armor, accessory)
            List<EquipmentData> matchingItems = GetMatchingEquipment(slotType);
            
            // Create selection slots for each matching item
            foreach (var equipment in matchingItems)
            {
                CreateSelectionSlot(equipment);
            }
            
            // If no items available, show empty message
            if (matchingItems.Count == 0)
            {
                CreateEmptySlot();
            }
        }
    }
    
    private void PopulateWeaponSlots(EquipmentSlot slotType)
    {
        List<WeaponData> matchingWeapons = GetMatchingWeapons(slotType);
        
        // Create selection slots for each matching weapon
        foreach (var weapon in matchingWeapons)
        {
            CreateWeaponSelectionSlot(weapon);
        }
        
        // If no weapons available, show empty message
        if (matchingWeapons.Count == 0)
        {
            CreateEmptySlot();
        }
    }
    
    private List<EquipmentData> GetMatchingEquipment(EquipmentSlot slotType)
    {
        List<EquipmentData> matchingItems = new List<EquipmentData>();
        
        foreach (var inventoryItem in Inventory.instance.inventoryItems)
        {
            if (inventoryItem.data is EquipmentData equipment && 
                equipment.equipmentSlot == slotType)
            {
                matchingItems.Add(equipment);
            }
        }
        
        // Sort by rarity and level
        return matchingItems.OrderByDescending(e => e.rarity)
                          .ThenByDescending(e => e.requiredLevel)
                          .ToList();
    }
    
    private List<WeaponData> GetMatchingWeapons(EquipmentSlot slotType)
    {
        List<WeaponData> matchingWeapons = new List<WeaponData>();
        
        foreach (var inventoryItem in Inventory.instance.inventoryItems)
        {
            if (inventoryItem.data is WeaponData weapon)
            {
                // Check if weapon type matches slot type
                bool isMainWeapon = slotType == EquipmentSlot.MainWeapon;
                bool isSecondaryWeapon = slotType == EquipmentSlot.SecondaryWeapon;
                
                // Main weapons: Sword, BurningSword, Hammer
                if (isMainWeapon && (weapon.weaponType == WeaponType.Sword || 
                                    weapon.weaponType == WeaponType.BurningSword || 
                                    weapon.weaponType == WeaponType.Hammer))
                {
                    matchingWeapons.Add(weapon);
                }
                // Secondary weapons: Boomerang, Spellbook
                else if (isSecondaryWeapon && (weapon.weaponType == WeaponType.Boomerang || 
                                              weapon.weaponType == WeaponType.Spellbook))
                {
                    matchingWeapons.Add(weapon);
                }
            }
        }
        
        // Sort by rarity and level
        return matchingWeapons.OrderByDescending(w => w.rarity)
                             .ThenByDescending(w => w.requiredLevel)
                             .ToList();
    }
    
    private void CreateSelectionSlot(EquipmentData equipment)
    {
        GameObject slotObj = Instantiate(itemSelectionSlotPrefab, itemGridParent);
        UI_ItemSelectionSlot slot = slotObj.GetComponent<UI_ItemSelectionSlot>();
        
        if (slot != null)
        {
            slot.Initialize(equipment, () => OnItemSelected(equipment));
            selectionSlots.Add(slot);
        }
    }
    
    private void CreateWeaponSelectionSlot(WeaponData weapon)
    {
        GameObject slotObj = Instantiate(itemSelectionSlotPrefab, itemGridParent);
        UI_ItemSelectionSlot slot = slotObj.GetComponent<UI_ItemSelectionSlot>();
        
        if (slot != null)
        {
            // Convert WeaponData to EquipmentData for compatibility
            EquipmentData equipmentData = weapon as EquipmentData;
            slot.Initialize(equipmentData, () => OnWeaponSelected(weapon));
            selectionSlots.Add(slot);
        }
    }
    
    private void CreateEmptySlot()
    {
        GameObject slotObj = Instantiate(itemSelectionSlotPrefab, itemGridParent);
        UI_ItemSelectionSlot slot = slotObj.GetComponent<UI_ItemSelectionSlot>();
        
        if (slot != null)
        {
            slot.InitializeEmpty("No items available");
            selectionSlots.Add(slot);
        }
    }
    
    private void ClearSelectionSlots()
    {
        foreach (var slot in selectionSlots)
        {
            if (slot != null && slot.gameObject != null)
                Destroy(slot.gameObject);
        }
        
        selectionSlots.Clear();
    }
    
    private void OnItemSelected(EquipmentData selectedEquipment)
    {
        onItemSelected?.Invoke(selectedEquipment);
        ClosePanel();
    }
    
    private void OnWeaponSelected(WeaponData selectedWeapon)
    {
        // Convert WeaponData to EquipmentData for callback compatibility
        EquipmentData equipmentData = selectedWeapon as EquipmentData;
        onItemSelected?.Invoke(equipmentData);
        ClosePanel();
    }
    
    private void Update()
    {
        // Close panel if player clicks outside or presses escape
        if (selectionPanel.activeInHierarchy)
        {
            if (Input.GetKeyDown(KeyCode.Escape) || 
                (Input.GetMouseButtonDown(0) && !IsMouseOverPanel()))
            {
                ClosePanel();
            }
        }
    }
    
    private bool IsMouseOverPanel()
    {
        if (selectionPanel == null) return false;
        
        RectTransform panelRect = selectionPanel.GetComponent<RectTransform>();
        return RectTransformUtility.RectangleContainsScreenPoint(
            panelRect, Input.mousePosition, 
            GetComponentInParent<Canvas>().worldCamera);
    }
} 