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
    
    private Chest currentChest;
    private List<UI_ItemSlot> itemSlots = new List<UI_ItemSlot>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Butonları ayarla
            if (closeButton != null)
                closeButton.onClick.AddListener(CloseChest);
            
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
    }

    // Sandığı aç
    public void OpenChest(Chest chest)
    {
        // UI'ı göster
        gameObject.SetActive(true);
        
        // Sandık referansını kaydet
        currentChest = chest;
        
        // Slotları oluştur
        RefreshItemSlots();
        
        // Input Blocker'a kaydet
        if (UIInputBlocker.instance != null)
        {
            UIInputBlocker.instance.AddPanel(gameObject);
        }
    }

    // Slotları güncelle/oluştur
    private void RefreshItemSlots()
    {
        // Mevcut slotları temizle
        ClearItemSlots();
        
        // Yeni slotlar oluştur
        if (currentChest != null)
        {
            foreach (GameObject item in currentChest.itemsInChest)
            {
                CreateItemSlot(item);
            }
        }
    }
    
    // Slot oluştur
    private void CreateItemSlot(GameObject itemObject)
    {
        ItemObject item = itemObject.GetComponent<ItemObject>();
        if (item != null && item.GetItemData() != null)
        {
            // Slot prefabını oluştur
            GameObject slotObj = Instantiate(itemSlotPrefab, itemSlotsContainer);
            UI_ItemSlot slot = slotObj.GetComponent<UI_ItemSlot>();
            
            if (slot != null)
            {
                // Slot'u güncelle
                slot.UpdateSlot(new InventoryItem(item.GetItemData()));
                itemSlots.Add(slot);
                
                // Slot'a tıklama eventi ekle
                Button slotButton = slotObj.GetComponent<Button>();
                if (slotButton != null)
                {
                    // Tıklandığında item'ı al
                    slotButton.onClick.AddListener(() => {
                        TakeItem(itemObject);
                    });
                }
            }
        }
    }
    
    // Itemı al
    private void TakeItem(GameObject item)
    {
        if (currentChest != null)
        {
            // Sandıktan itemı al
            currentChest.RemoveItem(item);
            
            // Slotları güncelle
            RefreshItemSlots();
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
        // UI'ı gizle
        gameObject.SetActive(false);
        
        // Slotları temizle
        ClearItemSlots();
        
        // Input Blocker'dan kaldır
        if (UIInputBlocker.instance != null)
        {
            UIInputBlocker.instance.RemovePanel(gameObject);
        }
        
        // Sandık kapatılma animasyonunu tetikle
        if (currentChest != null)
        {
            currentChest.Interact();
        }
        
        currentChest = null;
    }
} 