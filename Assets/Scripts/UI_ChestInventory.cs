using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

public class UI_ChestInventory : MonoBehaviour
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
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Butonları ayarla
            if (closeButton != null)
                closeButton.onClick.AddListener(CloseChest);
                
            // Take All butonu için listener
            if (takeAllButton != null)
                takeAllButton.onClick.AddListener(TakeAllItems);
            
            // Başlangıçta UI'ı gizle
            gameObject.SetActive(false);
        }
        else
        {
            Destroy(gameObject);
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
            Debug.Log("Chest içerik sayısı: " + currentChest.itemsInChest.Count);
            
            for (int i = 0; i < currentChest.itemsInChest.Count; i++)
            {
                GameObject itemObj = currentChest.itemsInChest[i];
                if (itemObj != null)
                {
                    ItemObject item = itemObj.GetComponent<ItemObject>();
                    if (item != null && item.GetItemData() != null)
                    {
                        Debug.Log("Item #" + i + ": " + item.GetItemData().itemName + ", aktif: " + itemObj.activeInHierarchy);
                    }
                    else
                    {
                        Debug.Log("Item #" + i + ": ItemObject veya ItemData NULL");
                    }
                }
                else
                {
                    Debug.Log("Item #" + i + ": NULL");
                }
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
        Debug.Log("Chest açılıyor... İtem sayısı: " + chest.itemsInChest.Count);
        
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
        if (currentChest == null || currentChest.itemsInChest == null)
        {
            Debug.LogWarning("Chest veya itemsInChest null!");
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
        
        // Önce tüm itemları logla
        if (debugMode)
        {
            for (int i = 0; i < currentChest.itemsInChest.Count; i++)
            {
                GameObject obj = currentChest.itemsInChest[i];
                string status = obj == null ? "NULL" : (obj.activeInHierarchy ? "Aktif" : "Pasif");
                Debug.Log("Item #" + i + ": " + status);
            }
        }
        
        // Yeni slotlar oluştur
        int validItemCount = 0;
        foreach (GameObject itemObj in currentChest.itemsInChest)
        {
            if (itemObj != null)
            {
                bool wasInactive = !itemObj.activeInHierarchy;
                if (wasInactive)
                {
                    itemObj.SetActive(true);
                }
                
                ItemObject item = itemObj.GetComponent<ItemObject>();
                if (item != null && item.GetItemData() != null)
                {
                    ItemData itemData = item.GetItemData();
                    GameObject slotObj = Instantiate(itemSlotPrefab, itemSlotsContainer);
                    UI_ItemSlot slot = slotObj.GetComponent<UI_ItemSlot>();
                    
                    if (slot != null)
                    {
                        slot.UpdateSlot(new InventoryItem(itemData));
                        itemSlots.Add(slot);
                        
                        // Slot'a tıklama eventi ekle
                        Button slotButton = slotObj.GetComponent<Button>();
                        if (slotButton != null)
                        {
                            // Bu tipteki değişkenleri Final olarak tanımla (lambdada düzgün çalışması için)
                            GameObject finalItemObj = itemObj;
                            slotButton.onClick.AddListener(() => TakeItem(finalItemObj));
                            validItemCount++;
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
                else
                {
                    Debug.LogWarning("ItemObject veya ItemData null! Item: " + itemObj.name);
                }
                
                if (wasInactive)
                {
                    itemObj.SetActive(false);
                }
            }
        }
        
        Debug.Log("Toplam " + currentChest.itemsInChest.Count + " item var, " + validItemCount + " tanesi gösteriliyor.");
    }
    
    // Tekil itemı al
    private void TakeItem(GameObject item)
    {
        if (currentChest != null && item != null)
        {
            Debug.Log("TakeItem çağrıldı: " + item.name);
            // Sandıktan itemı al
            currentChest.RemoveItem(item);
            
            // Slotları güncelle
            RefreshItemSlots();
        }
    }
    
    // Tüm itemleri al
    private void TakeAllItems()
    {
        if (currentChest != null)
        {
            Debug.Log("Take All çağrıldı, item sayısı: " + currentChest.itemsInChest.Count);
            
            // Tüm itemları sandıktan al
            currentChest.TakeAllItems();
            
            // Slotları güncelle
            RefreshItemSlots();
            
            // Eğer sandık boşsa UI'ı kapat
            if (currentChest.itemsInChest.Count == 0)
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