using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

public class UI_ChestInventory : BaseUIPanel
{
    public static UI_ChestInventory Instance;

    [Header("UI References")]
    [SerializeField] private Transform itemSlotsContainer;
    [SerializeField] private GameObject itemSlotPrefab;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button takeAllButton; // Take All buton referansı
    
    private Chest currentChest;
    private List<UI_ItemSlot> itemSlots = new List<UI_ItemSlot>();
    
    [Header("Debug")]
    [SerializeField] private bool debugMode = true;

    private void Awake()
    {
        // İlk olarak, kendimizi uygun şekilde ayarla
        if (Instance == null)
        {
            Instance = this;
            
            
            Debug.Log("UI_ChestInventory Singleton oluşturuldu, DontDestroyOnLoad aktif");
            
            // Butonları ayarla
            if (closeButton != null)
                closeButton.onClick.AddListener(CloseChest);
                
            // Take All butonu için listener
            if (takeAllButton != null)
                takeAllButton.onClick.AddListener(TakeAllItems);
            
            // Başlangıçta UI'ı gizle
            gameObject.SetActive(false);
        }
        else if (Instance != this)
        {
            Debug.Log("Fazladan UI_ChestInventory tespit edildi, yok ediliyor: " + gameObject.name);
            Destroy(gameObject);
            return;
        }
    }
    
    // Restart sonrası instance'ı yeniden tanımlamak için OnEnable ekle
    protected override void OnEnable()
    {
        // Eğer Instance farklı bir objeye atanmışsa ve bu obje etkinleştirilirse
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("UI_ChestInventory: Başka bir instance zaten var - bu obje yok edilecek");
            Destroy(gameObject);
            return;
        }
        
        // Instance boşsa (örneğin sahne değişikliğinden sonra null olmuşsa)
        if (Instance == null)
        {
            Instance = this;
            Debug.Log("UI_ChestInventory: Instance OnEnable'da yeniden atandı");
        }
    }

    private void Update()
    {
        // ESC tuşuna basıldığında sandığı kapat
        if (Input.GetKeyDown(KeyCode.Escape) && gameObject.activeInHierarchy)
        {
            CloseChest();
        }
        
        // R tuşuna basıldığında tüm itemleri al
        if (Input.GetKeyDown(KeyCode.R) && gameObject.activeInHierarchy)
        {
            TakeAllItems();
        }
        
        // TEST AMAÇLI: UI görünürlüğünü kontrol et
        if (debugMode && Input.GetKeyDown(KeyCode.P))
        {
            PrintDebugInfo();
        }
    }
    
    private void PrintDebugInfo()
    {
        if (!debugMode) return;
        
        Debug.Log("--- UI_ChestInventory Debug Info ---");
        Debug.Log("Container: " + (itemSlotsContainer != null ? itemSlotsContainer.name : "NULL"));
        Debug.Log("Prefab: " + (itemSlotPrefab != null ? itemSlotPrefab.name : "NULL"));
        Debug.Log("Slot count: " + itemSlots.Count);
        
        if (currentChest != null)
        {
            Dictionary<string, int> stackedItems = currentChest.GetStackedItems();
            Debug.Log("Chest unique item count: " + stackedItems.Count);
        }
        else
        {
            Debug.Log("Chest: NULL");
        }
    }

    // Open chest
    public void OpenChest(Chest chest)
    {
        if (chest == null)
        {
            Debug.LogError("OpenChest: chest parameter is null!");
            return;
        }
        
        // Store chest reference
        currentChest = chest;
        
        // Add to UI Input Blocker to disable gameplay inputs
        if (UIInputBlocker.instance != null)
        {
            UIInputBlocker.instance.AddPanel(gameObject);
        }
        
        // Show UI
        gameObject.SetActive(true);
        
        // Create slots after UI is active
        RefreshItemSlots();
    }

    // Update/create slots
    private void RefreshItemSlots()
    {
        // Clear existing slots
        ClearItemSlots();
        
        // Check chest contents
        if (currentChest == null)
        {
            Debug.LogWarning("Chest is null!");
            return;
        }
        
        // Get stacked items information
        Dictionary<string, int> stackedItems = currentChest.GetStackedItems();
        if (stackedItems.Count == 0)
        {
            return; // No items left in chest
        }
        
        // UI container check
        if (itemSlotsContainer == null)
        {
            Debug.LogError("itemSlotsContainer is NULL! Cannot create slots.");
            return;
        }
        
        // Prefab check
        if (itemSlotPrefab == null)
        {
            Debug.LogError("itemSlotPrefab is NULL! Cannot create slots.");
            return;
        }
        
        // Create one slot for each stack
        foreach (var pair in stackedItems)
        {
            string itemName = pair.Key;
            int count = pair.Value;
            
            // Get item reference
            GameObject itemObj = currentChest.GetItemReference(itemName);
            if (itemObj == null) continue;
            
            ItemObject itemComponent = itemObj.GetComponent<ItemObject>();
            if (itemComponent == null || itemComponent.GetItemData() == null) continue;
            
            ItemData itemData = itemComponent.GetItemData();
            
            // Create slot
            GameObject slotObj = Instantiate(itemSlotPrefab, itemSlotsContainer);
            UI_ItemSlot slot = slotObj.GetComponent<UI_ItemSlot>();
            
            if (slot != null)
            {
                // Create InventoryItem and set stack count
                InventoryItem invItem = new InventoryItem(itemData);
                // Manually set stack count (starts with 1, so start from 1)
                for (int i = 1; i < count; i++)
                {
                    invItem.AddStack();
                }
                
                slot.UpdateSlot(invItem);
                itemSlots.Add(slot);
                
                // Add click event to slot
                Button slotButton = slotObj.GetComponent<Button>();
                if (slotButton != null)
                {
                    // Define this variable as final
                    GameObject finalItemObj = itemObj;
                    slotButton.onClick.AddListener(() => TakeItem(finalItemObj));
                }
                else
                {
                    Debug.LogError("Button component not found on slot!");
                }
            }
            else
            {
                Debug.LogError("UI_ItemSlot component not found on slot object!");
            }
        }
    }
    
    // Take individual item stack
    private void TakeItem(GameObject item)
    {
        if (currentChest != null && item != null)
        {
            // Remove item from chest
            currentChest.RemoveItem(item);
            
            // Update slots
            RefreshItemSlots();
            
            // Close UI if no items left in chest
            Dictionary<string, int> stackedItems = currentChest.GetStackedItems();
            if (stackedItems.Count == 0)
            {
                CloseChest();
            }
        }
    }
    
    // Take all items
    private void TakeAllItems()
    {
        if (currentChest != null)
        {
            // Take all items from chest
            currentChest.TakeAllItems();
            
            // Update slots
            RefreshItemSlots();
            
            // Close UI if chest is empty
            if (currentChest.GetStackedItems().Count == 0)
            {
                CloseChest();
            }
        }
    }
    
    // Clear all slots
    private void ClearItemSlots()
    {
        foreach (UI_ItemSlot slot in itemSlots)
        {
            if (slot != null)
            {
                Destroy(slot.gameObject);
            }
        }
        
        itemSlots.Clear();
    }

    // Close chest
    public void CloseChest()
    {
        // Store chest reference
        Chest tempChest = currentChest;
        currentChest = null;
        
        // IMPORTANT: First remove from Input Blocker - Enable gameplay inputs
        if (UIInputBlocker.instance != null)
        {
            UIInputBlocker.instance.RemovePanel(gameObject);
        }
        
        // Force enable input
        if (UIInputBlocker.instance != null)
        {
            UIInputBlocker.instance.EnableGameplayInput(true);
        }
        
        // Clear slots
        ClearItemSlots();
        
        // Hide UI - Do this after
        gameObject.SetActive(false);
        
        // If chest still exists, perform close operation
        if (tempChest != null)
        {
            tempChest.CloseChest();
        }
    }
} 