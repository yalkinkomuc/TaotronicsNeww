using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class UI_RuneInventoryPanel : MonoBehaviour
{
    [Header("Grid ve Prefab")]
    [SerializeField] private Transform runeGridParent;
    [SerializeField] private GameObject runeSlotPrefab; // UI_RuneInventorySlot prefabı
    [SerializeField] private Button closeButton;
    [SerializeField] private TextMeshProUGUI panelTitleText;

    private List<UI_RuneInventorySlot> runeSlots = new List<UI_RuneInventorySlot>();

    private void Awake()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(ClosePanel);
    }

    private void OnEnable()
    {
        RefreshRuneGrid();
    }

    public void RefreshRuneGrid()
    {
        // Önce eski slotları temizle
        foreach (var slot in runeSlots)
        {
            if (slot != null)
                Destroy(slot.gameObject);
        }
        runeSlots.Clear();

        // Envanterdeki tüm rune'ları bul
        if (Inventory.instance == null) return;
        foreach (var item in Inventory.instance.inventoryItems)
        {
            if (item.data is RuneData runeData)
            {
                GameObject slotObj = Instantiate(runeSlotPrefab, runeGridParent);
                UI_RuneInventorySlot slot = slotObj.GetComponent<UI_RuneInventorySlot>();
                if (slot != null)
                {
                    slot.Setup(runeData, () => OnRuneClicked(runeData));
                    runeSlots.Add(slot);
                }
            }
        }
    }

    private void OnRuneClicked(RuneData rune)
    {
        // Equip işlemi
        if (EquipmentManager.Instance != null)
        {
            for (int i = 0; i < 6; i++)
            {
                if (EquipmentManager.Instance.IsRuneSlotEmpty(i))
                {
                    EquipmentManager.Instance.EquipRune(rune, i);
                    break;
                }
            }
        }
        // Envanterden çıkar
        if (Inventory.instance != null)
        {
            Inventory.instance.RemoveItem(rune);
        }
        // Grid'i güncelle
        RefreshRuneGrid();
        // Paneli kapatmak istersen:
        ClosePanel();
    }

    public void ClosePanel()
    {
        gameObject.SetActive(false);
    }
} 