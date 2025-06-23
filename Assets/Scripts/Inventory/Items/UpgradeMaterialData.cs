using UnityEngine;

[CreateAssetMenu(fileName = "New Upgrade Material", menuName = "Data/Items/Upgrade Material")]
public class UpgradeMaterialData : ItemData
{
    [Header("Material Properties")]
    public MaterialType materialType;
    public MaterialRarity materialRarity;
    
    [Header("Usage")]
    public bool canEnhanceWeapons = true;
    public bool canEnhanceArmor = true;
    public bool canEnhanceAccessories = true;
    public bool canEnhanceRunes = false;
    
    [Header("Enhancement Power")]
    public int enhancementPower = 1; // How much enhancement it provides
    public float successRate = 100f; // Success rate percentage
    
    [Header("Special Properties")]
    public bool hasSpecialEffect;
    [TextArea(2, 4)]
    public string specialEffectDescription;
    
    private void Awake()
    {
        itemType = ItemType.UpgradeMaterial;
        isStackable = true;
        maxStackSize = 9999; // Materials can be stacked in large quantities
    }
    
    public override string GetTooltip()
    {
        string tooltip = $"<b>{itemName}</b>\n";
        tooltip += $"<color=grey>{materialRarity} {materialType}</color>\n";
        tooltip += $"Enhancement Power: +{enhancementPower}\n";
        
        if (successRate < 100f)
        {
            tooltip += $"Success Rate: {successRate:F1}%\n";
        }
        
        tooltip += "\n<b>Can enhance:</b>\n";
        if (canEnhanceWeapons) tooltip += "• Weapons\n";
        if (canEnhanceArmor) tooltip += "• Armor\n";
        if (canEnhanceAccessories) tooltip += "• Accessories\n";
        if (canEnhanceRunes) tooltip += "• Runes\n";
        
        if (hasSpecialEffect && !string.IsNullOrEmpty(specialEffectDescription))
        {
            tooltip += $"\n<color=orange><b>Special Effect:</b></color>\n{specialEffectDescription}";
        }
        
        if (!string.IsNullOrEmpty(description))
        {
            tooltip += $"\n\n<i>{description}</i>";
        }
        
        return tooltip;
    }
    
    public bool CanEnhanceItem(ItemType itemTypeToEnhance)
    {
        return itemTypeToEnhance switch
        {
            ItemType.Weapon => canEnhanceWeapons,
            ItemType.Armor => canEnhanceArmor,
            ItemType.Accessory => canEnhanceAccessories,
            ItemType.Rune => canEnhanceRunes,
            _ => false
        };
    }
}

public enum MaterialType
{
    Leather = 0,
    Iron = 1,
    Rock = 2,
    Diamond = 3,
    Crystal = 4,
    Gem = 5,
    Steel = 6,
    Mithril = 7,
    Adamantine = 8,
    Essence = 9,
    Rune_Fragment = 10,
    Dragon_Scale = 11,
    Phoenix_Feather = 12,
    Void_Shard = 13
}

public enum MaterialRarity
{
    Common = 0,
    Uncommon = 1,
    Rare = 2,
    Epic = 3,
    Legendary = 4,
    Mythic = 5
} 