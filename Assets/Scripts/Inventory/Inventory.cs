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

    // Serileştirme için
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
            
            // Envanter verilerini yükle
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
        Debug.Log("Inventory system initialized with specialized UI");
    }

    public void AddItem(ItemData _item)
    {
        if (_item == null) return;
        
        // Check if item is stackable
        if (_item.isStackable && inventoryDictionary.TryGetValue(_item, out InventoryItem value))
        {
            // Add to existing stack if within limits
            if (value.stackSize < _item.maxStackSize)
            {
                value.AddStack();
            }
            else
            {
                // Create new stack if current is at max
                InventoryItem newItem = new InventoryItem(_item);
                inventoryItems.Add(newItem);
                inventoryDictionary.Add(_item, newItem);
            }
        }
        else
        {
            // Create new item entry
            InventoryItem newItem = new InventoryItem(_item);
            inventoryItems.Add(newItem);
            
            if (_item.isStackable)
            {
                inventoryDictionary.Add(_item, newItem);
            }
        }
        
        // Notify UI systems about inventory changes
        NotifyInventoryChanged();
        SaveInventory(); // Envanteri güncellediğimizde kaydet
        
        Debug.Log($"Added {_item.itemName} to inventory. Type: {_item.itemType}");
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
        }
        
        // Notify UI systems about inventory changes
        NotifyInventoryChanged();
        SaveInventory(); // Envanteri güncellediğimizde kaydet
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

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.G) && inventoryItems.Count > 0)
        {
            ItemData newItem = inventoryItems[inventoryItems.Count - 1].data;
            RemoveItem(newItem);
        }
    }
    
    // Envanteri kaydetme fonksiyonu
    public void SaveInventory()
    {
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
    }
    
    // Envanteri yükleme fonksiyonu
    public void LoadInventory()
    {
        if (PlayerPrefs.HasKey("PlayerInventory"))
        {
            string jsonData = PlayerPrefs.GetString("PlayerInventory");
            InventorySaveData saveData = JsonUtility.FromJson<InventorySaveData>(jsonData);
            
            // Mevcut envanteri temizle
            inventoryItems.Clear();
            inventoryDictionary.Clear();
            
            // Kaydedilen öğeleri yükle
            if (saveData != null && saveData.items != null)
            {
                foreach (var savedItem in saveData.items)
                {
                    // Item'ı Resources klasöründen yüklemeyi dene
                    ItemData itemData = LoadItemFromResources(savedItem.itemName);
                    
                    // Eğer Resources'dan yüklenemezse, sahne içindeki tüm ItemData'ları tara
                    if (itemData == null)
                    {
                        // Not: Bu yöntem hala kullanılabilir ama tercihen Resources kullanılmalı
                        itemData = FindItemDataInScene(savedItem.itemName);
                    }
                    
                    if (itemData != null)
                    {
                        InventoryItem newItem = new InventoryItem(itemData);
                        // Stack sayısını ayarla (zaten 1 eklenmiş olacak, o yüzden -1 yapıp istediğimiz kadar ekliyoruz)
                        for (int i = 0; i < savedItem.stackSize - 1; i++)
                        {
                            newItem.AddStack();
                        }
                        
                        inventoryItems.Add(newItem);
                        inventoryDictionary.Add(itemData, newItem);
                    }
                }
            }
            
            // Notify UI systems about loaded inventory
            NotifyInventoryChanged();
        }
    }

    // Resources klasöründen ItemData'yı yükle
    private ItemData LoadItemFromResources(string itemName)
    {
        // Resources/Items klasöründe doğrudan ara
        Debug.Log($"Resources'tan yükleniyor: '{itemName}'");
        
        // Önce doğrudan Resources/Items/itemName yolunu dene
        ItemData directItem = Resources.Load<ItemData>($"Items/{itemName}");
        if (directItem != null)
        {
            Debug.Log($"Resources'tan doğrudan yüklendi: Items/{itemName}");
            return directItem;
        }
        
        // Tüm ItemData'ları tara
        ItemData[] allItems = Resources.LoadAll<ItemData>("Items");
        Debug.Log($"Resources/Items klasöründe {allItems.Length} ItemData bulundu.");
        
        foreach (var item in allItems)
        {
            Debug.Log($"Bulunan item: {item.itemName}");
            if (item.itemName == itemName)
            {
                Debug.Log($"Resources'tan item yüklendi: {itemName}");
                return item;
            }
        }
        
        Debug.LogWarning($"Resources/Items klasöründe {itemName} adlı item bulunamadı!");
        return null;
    }

    // Sahnedeki ItemData'ları bul (yedek yöntem)
    private ItemData FindItemDataInScene(string itemName)
    {
        // Sadece yedek olarak kullanılmalı, bunun yerine Resources kullanmak tercih edilmeli
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
    
    // Player'ın ölümünden sonra açıkça çağrılabilecek yükleme metodu
    public void ReloadInventoryAfterDeath()
    {
        LoadInventory();
        NotifyInventoryChanged();
    }

    // Oyun kapatıldığında çağrılır
    private void OnApplicationQuit()
    {
        // Oyun kapatılırken envanteri kesin olarak kaydet
        SaveInventory();
        Debug.Log("Oyun kapatılıyor, envanter kaydedildi!");
    }
}