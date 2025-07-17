using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New ItemData", menuName = "Data/Item")]
public class ItemData : ScriptableObject
{
    [Header("Basic Info")]
    public string itemName;
    public string description;
    public Sprite icon;
    public ItemType itemType;
    
    [Header("Drop Settings")]
    [Range(0,100)]
    public float dropChance;
    
    [Header("Value & Rarity")]
    public int sellValue;
    public ItemRarity rarity = ItemRarity.Common;
    
    [Header("Stacking")]
    public bool isStackable = true;
    public int maxStackSize = 99;

    // Item'ın Resources klasöründeki yolu - Bu genellikle "Items/itemName" şeklinde olmalı
    public string resourcePath;

    private void OnValidate()
    {
        // Editor'da resource path'i otomatik ayarla
        if (string.IsNullOrEmpty(resourcePath) && !string.IsNullOrEmpty(itemName))
        {
            resourcePath = $"Items/{itemName}";
           // Debug.Log($"{name} için resourcePath ayarlandı: {resourcePath}");
        }
    }
    
    // Virtual method to get item tooltip info
    public virtual string GetTooltip()
    {
        return $"<b>{itemName}</b>\n{description}";
    }
}

public enum ItemRarity
{
    Common = 0,
    Uncommon = 1,
    Rare = 2,
    Epic = 3,
    Legendary = 4,
    Mythic = 5
}
