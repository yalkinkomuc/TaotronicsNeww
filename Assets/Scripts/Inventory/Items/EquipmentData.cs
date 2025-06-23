using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class EquipmentStatModifier
{
    public StatType statType;
    public int baseValue;
    public float percentageBonus;
    
    public EquipmentStatModifier(StatType type, int value, float percentage = 0f)
    {
        statType = type;
        baseValue = value;
        percentageBonus = percentage;
    }
}

public abstract class EquipmentData : ItemData
{
    [Header("Equipment Info")]
    public int requiredLevel = 1;
    public EquipmentSlot equipmentSlot;
    
    [Header("Stats")]
    public List<EquipmentStatModifier> statModifiers = new List<EquipmentStatModifier>();
    
    [Header("Enhancement")]
    public int enhancementLevel = 0;
    public int maxEnhancementLevel = 15;
    
    private void Awake()
    {
        // Equipment items are not stackable by default
        isStackable = false;
        maxStackSize = 1;
    }
    
    public override string GetTooltip()
    {
        string tooltip = $"<b>{itemName}</b>\n";
        tooltip += $"<color=grey>{rarity}</color>\n";
        tooltip += $"Required Level: {requiredLevel}\n\n";
        
        if (statModifiers.Count > 0)
        {
            tooltip += "<b>Stats:</b>\n";
            foreach (var stat in statModifiers)
            {
                if (stat.baseValue > 0)
                    tooltip += $"+{stat.baseValue} {stat.statType}\n";
                if (stat.percentageBonus > 0)
                    tooltip += $"+{stat.percentageBonus:F1}% {stat.statType}\n";
            }
        }
        
        if (enhancementLevel > 0)
        {
            tooltip += $"\n<color=green>Enhancement: +{enhancementLevel}</color>";
        }
        
        if (!string.IsNullOrEmpty(description))
        {
            tooltip += $"\n\n<i>{description}</i>";
        }
        
        return tooltip;
    }
    
    // Get total stat value including enhancement bonus
    public int GetTotalStatValue(StatType statType)
    {
        int totalValue = 0;
        foreach (var modifier in statModifiers)
        {
            if (modifier.statType == statType)
            {
                totalValue += modifier.baseValue;
                // Enhancement bonus: +5% per level
                totalValue += Mathf.RoundToInt(modifier.baseValue * (enhancementLevel * 0.05f));
            }
        }
        return totalValue;
    }
} 