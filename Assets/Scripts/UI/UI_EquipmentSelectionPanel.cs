using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class UI_EquipmentSelectionPanel : MonoBehaviour
{
    public static UI_EquipmentSelectionPanel Instance { get; private set; }
    
    [Header("Panel References")]
    [SerializeField] private GameObject selectionPanel;
    [SerializeField] private Transform itemGridParent;
    [SerializeField] private GameObject itemSelectionSlotPrefab;
    [SerializeField] private Button closeButton;
    [SerializeField] private TextMeshProUGUI panelTitleText;
    
    [Header("Positioning")]
    [SerializeField] private Vector2 panelOffset = new Vector2(50, 0);
    
    private EquipmentSlot currentSlotType;
    private List<UI_ItemSelectionSlot> selectionSlots = new List<UI_ItemSelectionSlot>();
    private System.Action<EquipmentData> onItemSelected;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
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
    
    private void Start()
    {
        // Setup close button
        if (closeButton != null)
            closeButton.onClick.AddListener(ClosePanel);
    }
    
    public void ShowSelectionPanel(EquipmentSlot slotType, Vector3 worldPosition, System.Action<EquipmentData> onSelected)
    {
        currentSlotType = slotType;
        onItemSelected = onSelected;
        
        // Position panel near the clicked slot
        PositionPanel(worldPosition);
        
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
    
    private void PositionPanel(Vector3 targetWorldPosition)
    {
        if (selectionPanel == null) return;
        
        // Convert world position to screen position
        Camera cam = Camera.main;
        if (cam == null) cam = FindFirstObjectByType<Camera>();
        
        Vector2 screenPosition = cam.WorldToScreenPoint(targetWorldPosition);
        
        // Apply offset
        screenPosition += panelOffset;
        
        // Get canvas for proper positioning
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            RectTransform canvasRect = canvas.GetComponent<RectTransform>();
            Vector2 localPosition;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect, screenPosition, canvas.worldCamera, out localPosition);
            
            selectionPanel.GetComponent<RectTransform>().localPosition = localPosition;
        }
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
        
        // Get items that match this equipment slot
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