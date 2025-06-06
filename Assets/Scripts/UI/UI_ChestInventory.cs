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
        Debug.Log("--- UI_ChestInventory Debug Info ---");
        Debug.Log("Container: " + (itemSlotsContainer != null ? itemSlotsContainer.name : "NULL"));
        Debug.Log("Prefab: " + (itemSlotPrefab != null ? itemSlotPrefab.name : "NULL"));
        Debug.Log("Slot sayısı: " + itemSlots.Count);
        
        if (currentChest != null)
        {
            Dictionary<string, int> stackedItems = currentChest.GetStackedItems();
            Debug.Log("Chest unique item sayısı: " + stackedItems.Count);
            
            foreach (var pair in stackedItems)
            {
                Debug.Log($"Item: {pair.Key}, Count: {pair.Value}");
            }
        }
        else
        {
            Debug.Log("Chest: NULL");
        }
    }

    // Sandığı aç
    public void OpenChest(Chest chest)
    {
        if (chest == null)
        {
            Debug.LogError("OpenChest: chest parametresi null!");
            return;
        }
        
        // Debug.Log - Sandık açılırken
        Dictionary<string, int> stackedItems = chest.GetStackedItems();
        Debug.Log($"Chest açılıyor... Unique item sayısı: {stackedItems.Count}");
        
        // Sandık referansını kaydet
        currentChest = chest;
        
        // UI Input Blocker'a ekle - Gameplay inputları devre dışı bırakmak için
        if (UIInputBlocker.instance != null)
        {
            UIInputBlocker.instance.AddPanel(gameObject);
        }
        
        // UI'ı göster
        gameObject.SetActive(true);
        
        // DAHA SONRA slotları oluştur (UI aktif olduktan sonra)
        RefreshItemSlots();
    }

    // Slotları güncelle/oluştur
    private void RefreshItemSlots()
    {
        if (debugMode)
        {
            Debug.Log("RefreshItemSlots çağrıldı");
        }
        
        // Mevcut slotları temizle
        ClearItemSlots();
        
        // Sandığın içeriğini kontrol et
        if (currentChest == null)
        {
            Debug.LogWarning("Chest null!");
            return;
        }
        
        // Stacked items bilgisini al
        Dictionary<string, int> stackedItems = currentChest.GetStackedItems();
        if (stackedItems.Count == 0)
        {
            Debug.Log("Sandıkta hiç item kalmamış.");
            return;
        }
        
        // UI CONTAINER KONTROL
        if (itemSlotsContainer == null)
        {
            Debug.LogError("itemSlotsContainer NULL! Slotlar oluşturulamıyor.");
            return;
        }
        
        // PREFAB KONTROL
        if (itemSlotPrefab == null)
        {
            Debug.LogError("itemSlotPrefab NULL! Slotlar oluşturulamıyor.");
            return;
        }
        
        // Her stack için tek bir slot oluştur
        foreach (var pair in stackedItems)
        {
            string itemName = pair.Key;
            int count = pair.Value;
            
            // Item referansını al
            GameObject itemObj = currentChest.GetItemReference(itemName);
            if (itemObj == null) continue;
            
            ItemObject itemComponent = itemObj.GetComponent<ItemObject>();
            if (itemComponent == null || itemComponent.GetItemData() == null) continue;
            
            ItemData itemData = itemComponent.GetItemData();
            
            // Slot oluştur
            GameObject slotObj = Instantiate(itemSlotPrefab, itemSlotsContainer);
            UI_ItemSlot slot = slotObj.GetComponent<UI_ItemSlot>();
            
            if (slot != null)
            {
                // InventoryItem oluştur ve stack sayısını ayarla
                InventoryItem invItem = new InventoryItem(itemData);
                // Stack sayısını manuel ayarla
                for (int i = 1; i < count; i++)
                {
                    invItem.AddStack(); // Başlangıçta zaten 1, o yüzden 1'den başla
                }
                
                slot.UpdateSlot(invItem);
                itemSlots.Add(slot);
                
                // Slot'a tıklama eventi ekle
                Button slotButton = slotObj.GetComponent<Button>();
                if (slotButton != null)
                {
                    // Bu değişkeni Final olarak tanımla
                    GameObject finalItemObj = itemObj;
                    slotButton.onClick.AddListener(() => TakeItem(finalItemObj));
                }
                else
                {
                    Debug.LogError("Slot'ta button bileşeni bulunamadı!");
                }
            }
            else
            {
                Debug.LogError("Slot objesinde UI_ItemSlot bileşeni bulunamadı!");
            }
        }
        
        Debug.Log($"Toplam {stackedItems.Count} farklı item türü gösteriliyor.");
    }
    
    // Tekil item stack'ını al
    private void TakeItem(GameObject item)
    {
        if (currentChest != null && item != null)
        {
            ItemObject itemObj = item.GetComponent<ItemObject>();
            if (itemObj != null && itemObj.GetItemData() != null)
            {
                Debug.Log($"TakeItem çağrıldı: {itemObj.GetItemData().itemName}");
            }
            else
            {
                Debug.Log("TakeItem çağrıldı: " + item.name);
            }
            
            // Sandıktan itemı al
            currentChest.RemoveItem(item);
            
            // Slotları güncelle
            RefreshItemSlots();
            
            // Eğer sandıktaki tüm itemlar bittiyse UI'ı kapat
            Dictionary<string, int> stackedItems = currentChest.GetStackedItems();
            if (stackedItems.Count == 0)
            {
                CloseChest();
            }
        }
    }
    
    // Tüm itemleri al
    private void TakeAllItems()
    {
        if (currentChest != null)
        {
            Dictionary<string, int> stackedItems = currentChest.GetStackedItems();
            Debug.Log($"Take All çağrıldı, unique item sayısı: {stackedItems.Count}");
            
            // Tüm itemları sandıktan al
            currentChest.TakeAllItems();
            
            // Slotları güncelle
            RefreshItemSlots();
            
            // Eğer sandık boşsa UI'ı kapat
            if (currentChest.GetStackedItems().Count == 0)
            {
                CloseChest();
            }
        }
    }
    
    // Tüm slotları temizle
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

    // Sandığı kapat
    public void CloseChest()
    {
        // Sandık referansını sakla
        Chest tempChest = currentChest;
        currentChest = null;
        
        // ÖNEMLİ: Önce Input Blocker'dan kaldır - Gameplay inputları etkinleştir
        if (UIInputBlocker.instance != null)
        {
            UIInputBlocker.instance.RemovePanel(gameObject);
        }
        
        // Zorla input'u etkinleştir
        if (UIInputBlocker.instance != null)
        {
            UIInputBlocker.instance.EnableGameplayInput(true);
        }
        
        // Slotları temizle
        ClearItemSlots();
        
        // UI'ı gizle - SONRA yap
        gameObject.SetActive(false);
        
        // Sandık hala varsa, kapatma işlemini yap
        if (tempChest != null)
        {
            tempChest.CloseChest();
        }
        
        // Debug.Log ile input durumunu kontrol et
        Debug.Log("Chest UI kapandı, input etkinleştirildi!");
    }
} 