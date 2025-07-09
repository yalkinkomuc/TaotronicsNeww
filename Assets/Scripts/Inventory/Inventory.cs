using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

public class Inventory : MonoBehaviour
{
    public static Inventory instance;

    public List<InventoryItem> inventoryItems;
    public Dictionary<ItemData, InventoryItem> inventoryDictionary;

    // Auto-save settings
    [Header("Save Settings")]
    [SerializeField] private bool autoSaveEnabled = true;
    [SerializeField] private float autoSaveInterval = 30f; // Save every 30 seconds
    private float lastSaveTime;

    // Serialization classes
    [Serializable]
    private class SavedItem
    {
        public string itemName;
        public int stackSize;
    }

    [Serializable]
    private class InventorySaveData
    {
        public List<SavedItem> items = new List<SavedItem>();
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            
            inventoryItems = new List<InventoryItem>();
            inventoryDictionary = new Dictionary<ItemData, InventoryItem>();
            
            // Load inventory data
            LoadInventory();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // UI updates are now handled by AdvancedInventoryUI and other specialized systems
        lastSaveTime = Time.time;
    }

    private void Update()
    {
        // Auto-save system
        if (autoSaveEnabled && Time.time - lastSaveTime >= autoSaveInterval)
        {
            SaveInventory();
            lastSaveTime = Time.time;
        }
    }

    public void AddItem(ItemData _item)
    {
        if (_item == null) return;
        
        // Check if item is stackable and already exists
        if (_item.isStackable && inventoryDictionary.TryGetValue(_item, out InventoryItem existingItem))
        {
            // Add to existing stack if within limits
            if (existingItem.stackSize < _item.maxStackSize)
            {
                existingItem.AddStack();
            }
            else
            {
                // Create new stack if current is at max
                CreateNewItemEntry(_item);
            }
        }
        else
        {
            // Create new item entry
            CreateNewItemEntry(_item);
        }
        
        // Notify UI systems about inventory changes
        NotifyInventoryChanged();
        
        // Mark inventory as dirty for next auto-save
        MarkInventoryDirty();
    }

    private void CreateNewItemEntry(ItemData _item)
    {
        InventoryItem newItem = new InventoryItem(_item);
        inventoryItems.Add(newItem);
        
        // Only add to dictionary if stackable to prevent duplicates
        if (_item.isStackable && !inventoryDictionary.ContainsKey(_item))
        {
            inventoryDictionary.Add(_item, newItem);
        }
    }

    public void RemoveItem(ItemData _item)
    {
        if (inventoryDictionary.TryGetValue(_item, out InventoryItem value))
        {
            if (value.stackSize <= 1)
            {
                inventoryItems.Remove(value);
                inventoryDictionary.Remove(_item);
            }
            else
            {
                value.RemoveStack();
            }
            
            // Notify UI systems about inventory changes
            NotifyInventoryChanged();
            
            // Mark inventory as dirty for next auto-save
            MarkInventoryDirty();
        }
    }

    private void NotifyInventoryChanged()
    {
        // Update AdvancedInventoryUI if it exists and is active
        if (AdvancedInventoryUI.Instance != null && AdvancedInventoryUI.Instance.gameObject.activeInHierarchy)
        {
            // Update material displays
            AdvancedInventoryUI.Instance.UpdateMaterialDisplays();
            
            // Check if a collectible was added and refresh if needed
            var hasCollectibles = inventoryItems.Any(item => item.data is CollectibleData);
            if (hasCollectibles)
            {
                AdvancedInventoryUI.Instance.RefreshCollectiblesDisplay();
            }
        }
    }

    private bool inventoryDirty = false;
    
    private void MarkInventoryDirty()
    {
        inventoryDirty = true;
    }
    
    // Manual save function
    public void SaveInventory()
    {
        if (!inventoryDirty && !autoSaveEnabled) return; // Don't save if nothing changed
        
        InventorySaveData saveData = new InventorySaveData();
        
        foreach (var item in inventoryItems)
        {
            saveData.items.Add(new SavedItem
            {
                itemName = item.data.itemName,
                stackSize = item.stackSize
            });
        }
        
        string jsonData = JsonUtility.ToJson(saveData);
        PlayerPrefs.SetString("PlayerInventory", jsonData);
        PlayerPrefs.Save();
        
        inventoryDirty = false;
    }
    
    // Load inventory function
    public void LoadInventory()
    {
        if (PlayerPrefs.HasKey("PlayerInventory"))
        {
            string jsonData = PlayerPrefs.GetString("PlayerInventory");
            InventorySaveData saveData = JsonUtility.FromJson<InventorySaveData>(jsonData);
            
            // Clear current inventory
            inventoryItems.Clear();
            inventoryDictionary.Clear();
            
            // Load saved items
            if (saveData != null && saveData.items != null)
            {
                foreach (var savedItem in saveData.items)
                {
                    // Try to load item from Resources folder
                    ItemData itemData = LoadItemFromResources(savedItem.itemName);
                    
                    // Fallback to scene search if not found in Resources
                    if (itemData == null)
                    {
                        // Note: This method is still available but Resources should be preferred
                        itemData = FindItemDataInScene(savedItem.itemName);
                    }
                    
                    if (itemData != null)
                    {
                        InventoryItem newItem = new InventoryItem(itemData);
                        // Set stack size (already has 1, so add the remainder)
                        for (int i = 0; i < savedItem.stackSize - 1; i++)
                        {
                            newItem.AddStack();
                        }
                        
                        inventoryItems.Add(newItem);
                        
                        // Add to dictionary only for stackable items
                        if (itemData.isStackable && !inventoryDictionary.ContainsKey(itemData))
                        {
                            inventoryDictionary.Add(itemData, newItem);
                        }
                    }
                }
            }
            
            // Notify UI systems about loaded inventory
            NotifyInventoryChanged();
        }
    }

    // Load ItemData from Resources folder
    private ItemData LoadItemFromResources(string itemName)
    {
        // Search directly in Resources/Items folder
        ItemData directItem = Resources.Load<ItemData>($"Items/{itemName}");
        if (directItem != null)
        {
            return directItem;
        }
        
        // Search through all ItemData in Resources
        ItemData[] allItems = Resources.LoadAll<ItemData>("Items");
        
        foreach (var item in allItems)
        {
            if (item.itemName == itemName)
            {
                return item;
            }
        }
        
        Debug.LogWarning($"Item '{itemName}' not found in Resources/Items folder!");
        return null;
    }

    // Find ItemData in scene (fallback method)
    private ItemData FindItemDataInScene(string itemName)
    {
        // Should only be used as fallback, prefer Resources
        ItemData[] sceneItems = FindObjectsByType<ItemData>(FindObjectsSortMode.None);
        foreach (var item in sceneItems)
        {
            if (item.itemName == itemName)
            {
                return item;
            }
        }
        
        return null;
    }
    
    // Explicitly callable reload method after player death
    public void ReloadInventoryAfterDeath()
    {
        LoadInventory();
        NotifyInventoryChanged();
    }

    // Called when application quits
    private void OnApplicationQuit()
    {
        // Ensure inventory is saved when game closes
        SaveInventory();
    }
    
    // Called when object is destroyed
    private void OnDestroy()
    {
        // Save inventory when inventory manager is destroyed
        if (instance == this)
        {
            SaveInventory();
        }
    }
}