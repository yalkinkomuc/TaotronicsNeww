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

    [Header("Inventory UI")] 
    
    [SerializeField] private Transform inventorySlotParent;
    private UI_ItemSlot[] itemSlot;

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

    private void OnEnable()
    {
        // Unity'nin modern event sistemini kullan
        UnityEngine.SceneManagement.SceneManager.activeSceneChanged += OnActiveSceneChanged;
    }

    private void OnDisable()
    {
        // Oyun kapatılırken envanteri kaydet
        SaveInventory();
        UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= OnActiveSceneChanged;
    }

    private void Start()
    {
        RefreshUI();
    }

    // Yeni sahne değişim event handler'ı
    private void OnActiveSceneChanged(Scene oldScene, Scene newScene)
    {
        RefreshUI();
    }

    private void RefreshUI()
    {
        if (inventorySlotParent == null)
        {
            // UI referansını bul
            GameObject invUI = GameObject.FindGameObjectWithTag("InventoryUI");
            if (invUI != null)
            {
                inventorySlotParent = invUI.transform;
            }
        }

        if (inventorySlotParent != null)
        {
            itemSlot = inventorySlotParent.GetComponentsInChildren<UI_ItemSlot>();
            UpdateSlotUI();
        }
    }

    private void UpdateSlotUI()
    {
        if (itemSlot == null) return;

        // Önce tüm slotları temizle
        foreach (var slot in itemSlot)
        {
            if (slot != null)
                slot.UpdateSlot(null);
        }

        // Mevcut itemları UI'a yerleştir
        for (int i = 0; i < inventoryItems.Count && i < itemSlot.Length; i++)
        {
            if (itemSlot[i] != null)
                itemSlot[i].UpdateSlot(inventoryItems[i]);
        }
    }

    public void AddItem(ItemData _item)
    {
        if (inventoryDictionary.TryGetValue(_item, out InventoryItem value))
        {
            value.AddStack();
        }
        else
        {
            InventoryItem newItem = new InventoryItem(_item);
            inventoryItems.Add(newItem);
            inventoryDictionary.Add(_item, newItem);
        }
        
        UpdateSlotUI();
        SaveInventory(); // Envanteri güncellediğimizde kaydet
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
        
        UpdateSlotUI();
        SaveInventory(); // Envanteri güncellediğimizde kaydet
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
                    // ItemData'yı isimle bulmalıyız
                    ItemData itemData = Resources.FindObjectsOfTypeAll<ItemData>()
                        .FirstOrDefault(item => item.itemName == savedItem.itemName);
                    
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
            
            // UI'ı güncelle
            UpdateSlotUI();
        }
    }
    
    // Player'ın ölümünden sonra açıkça çağrılabilecek yükleme metodu
    public void ReloadInventoryAfterDeath()
    {
        LoadInventory();
        UpdateSlotUI();
    }
}