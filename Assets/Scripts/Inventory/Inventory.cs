using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Inventory : MonoBehaviour
{
    public static Inventory instance;

    public List<InventoryItem> inventoryItems;
    public Dictionary<ItemData, InventoryItem> inventoryDictionary;

    [Header("Inventory UI")] 
    
    [SerializeField] private Transform inventorySlotParent;
    private UI_ItemSlot[] itemSlot;


    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            
            inventoryItems = new List<InventoryItem>();
            inventoryDictionary = new Dictionary<ItemData, InventoryItem>();
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
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.G) && inventoryItems.Count > 0)
        {
            ItemData newItem = inventoryItems[inventoryItems.Count - 1].data;
            RemoveItem(newItem);
        }
    }
}