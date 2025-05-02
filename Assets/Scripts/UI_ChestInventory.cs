using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

public class UI_ChestInventory : MonoBehaviour
{
    public static UI_ChestInventory Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private Transform itemSlotParent;
    [SerializeField] private GameObject itemSlotPrefab;
    [SerializeField] private Button takeAllButton;
    [SerializeField] private Button closeButton;
    
    private List<ItemData> currentItems = new List<ItemData>();
    private List<UI_ItemSlot> itemSlots = new List<UI_ItemSlot>();
    private Chest currentChest;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Buton eventlerini ayarla
            if (takeAllButton != null)
                takeAllButton.onClick.AddListener(TakeAllItems);
                
            if (closeButton != null)
                closeButton.onClick.AddListener(CloseInventory);
                
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
        // R tuşuna basıldığında tüm itemleri al
        if (Input.GetKeyDown(KeyCode.R))
        {
            TakeAllItems();
        }
        
        // ESC tuşuna basıldığında envanteri kapat
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CloseInventory();
        }
    }

    public void OpenChestInventory(Chest chest, List<ItemData> items)
    {
        currentChest = chest;
        currentItems = items;
        CreateItemSlots();
        UpdateUI();
        gameObject.SetActive(true);
    }

    private void CreateItemSlots()
    {
        // Mevcut slotları temizle
        foreach (var slot in itemSlots)
        {
            if (slot != null)
                Destroy(slot.gameObject);
        }
        itemSlots.Clear();

        // Yeni slotlar oluştur
        for (int i = 0; i < currentItems.Count; i++)
        {
            GameObject slotObj = Instantiate(itemSlotPrefab, itemSlotParent);
            UI_ItemSlot slot = slotObj.GetComponent<UI_ItemSlot>();
            
            if (slot != null)
            {
                slot.UpdateSlot(new InventoryItem(currentItems[i]));
                itemSlots.Add(slot);
                
                // Slot'a tıklama eventi ekle
                Button slotButton = slotObj.GetComponent<Button>();
                if (slotButton != null)
                {
                    int index = i; // Capture the index
                    slotButton.onClick.AddListener(() => TakeItem(currentItems[index]));
                }
            }
        }
    }

    public void UpdateUI()
    {
        // Slotları güncelle
        for (int i = 0; i < itemSlots.Count; i++)
        {
            if (i < currentItems.Count)
            {
                itemSlots[i].UpdateSlot(new InventoryItem(currentItems[i]));
            }
            else
            {
                itemSlots[i].UpdateSlot(null);
            }
        }
    }

    private void TakeAllItems()
    {
        if (currentChest != null)
        {
            currentChest.TakeAllItems();
        }
    }

    private void TakeItem(ItemData item)
    {
        if (currentChest != null)
        {
            currentChest.TakeItem(item);
        }
    }

    public void CloseInventory()
    {
        gameObject.SetActive(false);
        currentChest = null;
        currentItems.Clear();
    }
} 