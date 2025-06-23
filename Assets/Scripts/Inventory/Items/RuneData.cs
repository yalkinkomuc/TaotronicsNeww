using UnityEngine;

[CreateAssetMenu(fileName = "New Rune", menuName = "Data/Items/Rune")]
public class RuneData : ItemData
{
    [Header("Rune Properties")]
    public RuneType runeType;
    public int runeLevel = 1;
    public EquipmentStatModifier statModifier;
    
    [Header("Combination")]
    public bool canCombineWithOthers = true;
    public RuneData[] combinationResult; // What runes this can create when combined
    
    [Header("Enhancement")]
    public int enhancementLevel = 0;
    public int maxEnhancementLevel = 10;
    
    private void Awake()
    {
        itemType = ItemType.Rune;
        isStackable = true;
        maxStackSize = 999;
    }
    
    public override string GetTooltip()
    {
        string tooltip = $"<b>{itemName}</b>\n";
        tooltip += $"<color=grey>Level {runeLevel} {runeType} Rune</color>\n";
        tooltip += $"Rarity: {rarity}\n\n";
        
        // Show stat modification
        if (statModifier.baseValue > 0)
        {
            tooltip += $"<color=cyan>+{GetTotalStatValue()} {statModifier.statType}</color>\n";
        }
        if (statModifier.percentageBonus > 0)
        {
            tooltip += $"<color=cyan>+{GetTotalPercentageBonus():F1}% {statModifier.statType}</color>\n";
        }
        
        if (enhancementLevel > 0)
        {
            tooltip += $"\n<color=green>Enhancement: +{enhancementLevel}</color>";
        }
        
        if (canCombineWithOthers)
        {
            tooltip += "\n\n<color=yellow>Can be combined with other runes</color>";
        }
        
        if (!string.IsNullOrEmpty(description))
        {
            tooltip += $"\n\n<i>{description}</i>";
        }
        
        return tooltip;
    }
    
    public int GetTotalStatValue()
    {
        int baseValue = statModifier.baseValue;
        // Enhancement bonus: +10% per level
        int enhancementBonus = Mathf.RoundToInt(baseValue * (enhancementLevel * 0.1f));
        return baseValue + enhancementBonus;
    }
    
    public float GetTotalPercentageBonus()
    {
        float baseBonus = statModifier.percentageBonus;
        // Enhancement bonus: +0.5% per level
        float enhancementBonus = enhancementLevel * 0.5f;
        return baseBonus + enhancementBonus;
    }
}

public enum RuneType
{
    Vitality = 0,      // Health/Vitality
    Strength = 1,      // Physical damage
    Dexterity = 2,     // Attack speed/critical
    Intelligence = 3,  // Magical damage
    Defense = 4,       // Armor/resistance
    Luck = 5,          // Critical chance/rare drops
    Utility = 6        // Special effects
} 