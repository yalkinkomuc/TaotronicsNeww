using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class UI_ItemSelectionSlot : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image itemIcon;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI itemStatsText;
    [SerializeField] private TextMeshProUGUI enhancementText;
    [SerializeField] private TextMeshProUGUI emptyText;
    [SerializeField] private Button selectButton;
    
    [Header("Visual Settings")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color hoverColor = new Color(1f, 1f, 0.8f);
    [SerializeField] private Color selectedColor = Color.yellow;
    
    private EquipmentData equipmentData;
    private Action onSelected;
    private bool isEmpty;
    
    private void Start()
    {
        if (selectButton != null)
        {
            selectButton.onClick.AddListener(OnSlotClicked);
        }
    }
    
    public void Initialize(EquipmentData equipment, Action onSelectCallback)
    {
        equipmentData = equipment;
        onSelected = onSelectCallback;
        isEmpty = false;
        
        UpdateDisplay();
    }
    
    public void InitializeEmpty(string message)
    {
        equipmentData = null;
        onSelected = null;
        isEmpty = true;
        
        UpdateEmptyDisplay(message);
    }
    
    private void UpdateDisplay()
    {
        if (equipmentData == null) return;
        
        // Hide empty text
        if (emptyText != null)
            emptyText.gameObject.SetActive(false);
        
        // Show item icon
        if (itemIcon != null)
        {
            itemIcon.sprite = equipmentData.icon;
            itemIcon.gameObject.SetActive(true);
        }
        
        // Show item name
        if (itemNameText != null)
        {
            itemNameText.text = equipmentData.itemName;
            itemNameText.gameObject.SetActive(true);
        }
        
        // Show item stats summary
        if (itemStatsText != null)
        {
            itemStatsText.text = GetStatsText();
            itemStatsText.gameObject.SetActive(true);
        }
        
        // Show enhancement level
        if (enhancementText != null)
        {
            if (equipmentData.enhancementLevel > 0)
            {
                enhancementText.text = $"+{equipmentData.enhancementLevel}";
                enhancementText.gameObject.SetActive(true);
            }
            else
            {
                enhancementText.gameObject.SetActive(false);
            }
        }
        
        // Set background color based on rarity
        if (backgroundImage != null)
        {
            backgroundImage.color = GetRarityColor(equipmentData.rarity);
        }
        
        // Enable button
        if (selectButton != null)
            selectButton.interactable = true;
    }
    
    private void UpdateEmptyDisplay(string message)
    {
        // Hide all item-related UI
        if (itemIcon != null)
            itemIcon.gameObject.SetActive(false);
        
        if (itemNameText != null)
            itemNameText.gameObject.SetActive(false);
        
        if (itemStatsText != null)
            itemStatsText.gameObject.SetActive(false);
        
        if (enhancementText != null)
            enhancementText.gameObject.SetActive(false);
        
        // Show empty message
        if (emptyText != null)
        {
            emptyText.text = message;
            emptyText.gameObject.SetActive(true);
        }
        
        // Set gray background
        if (backgroundImage != null)
        {
            backgroundImage.color = Color.gray;
        }
        
        // Disable button
        if (selectButton != null)
            selectButton.interactable = false;
    }
    
    private string GetStatsText()
    {
        if (equipmentData?.statModifiers == null || equipmentData.statModifiers.Count == 0)
            return "";
        
        string statsText = "";
        int count = 0;
        
        foreach (var modifier in equipmentData.statModifiers)
        {
            if (count >= 2) break; // Show max 2 stats
            
            if (modifier.baseValue > 0)
            {
                if (!string.IsNullOrEmpty(statsText))
                    statsText += "\n";
                    
                statsText += $"+{modifier.baseValue} {GetStatDisplayName(modifier.statType)}";
                count++;
            }
        }
        
        // Add weapon-specific stats
        if (equipmentData is WeaponData weapon)
        {
            if (count < 2)
            {
                if (!string.IsNullOrEmpty(statsText))
                    statsText += "\n";
                statsText += $"{weapon.GetAverageDamage()} DMG";
            }
        }
        
        // Add armor-specific stats
        if (equipmentData is ArmorData armor)
        {
            if (count < 2)
            {
                if (!string.IsNullOrEmpty(statsText))
                    statsText += "\n";
                statsText += $"{armor.armorValue} ARM";
            }
        }
        
        return statsText;
    }
    
    private string GetStatDisplayName(StatType statType)
    {
        return statType switch
        {
            StatType.Health => "HP",
            StatType.Vitality => "VIT",
            StatType.Might => "STR",
            StatType.WarriorDamage => "WAR",
            StatType.HunterDamage => "HUN",
            StatType.AssassinDamage => "ASS",
            StatType.Armor => "ARM",
            StatType.CriticalChance => "CRIT",
            StatType.CriticalDamage => "CDMG",
            _ => statType.ToString()
        };
    }
    
    private Color GetRarityColor(ItemRarity rarity)
    {
        return rarity switch
        {
            ItemRarity.Common => new Color(0.8f, 0.8f, 0.8f),     // Light gray
            ItemRarity.Uncommon => new Color(0.6f, 0.9f, 0.6f),  // Light green
            ItemRarity.Rare => new Color(0.6f, 0.8f, 1f),        // Light blue
            ItemRarity.Epic => new Color(0.8f, 0.6f, 1f),        // Light purple
            ItemRarity.Legendary => new Color(1f, 0.9f, 0.5f),   // Light orange
            ItemRarity.Mythic => new Color(1f, 0.6f, 0.6f),      // Light red
            _ => Color.white
        };
    }
    
    private void OnSlotClicked()
    {
        if (!isEmpty && equipmentData != null)
        {
            onSelected?.Invoke();
        }
    }
    
    // Hover effects (optional)
    public void OnPointerEnter()
    {
        if (!isEmpty && backgroundImage != null)
        {
            Color currentColor = backgroundImage.color;
            backgroundImage.color = Color.Lerp(currentColor, hoverColor, 0.3f);
        }
    }
    
    public void OnPointerExit()
    {
        if (!isEmpty && backgroundImage != null)
        {
            backgroundImage.color = GetRarityColor(equipmentData.rarity);
        }
    }
} 