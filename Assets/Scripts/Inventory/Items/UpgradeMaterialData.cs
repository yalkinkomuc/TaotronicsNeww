using UnityEngine;

[CreateAssetMenu(fileName = "New Upgrade Material", menuName = "Data/Items/Upgrade Material")]
public class UpgradeMaterialData : ItemData
{
    [Header("Material Properties")]
    public MaterialType materialType;
    public MaterialRarity materialRarity;
 
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
        
        tooltip += "\n<b>Can enhance:</b>\n";
     
        
        if (!string.IsNullOrEmpty(description))
        {
            tooltip += $"\n\n<i>{description}</i>";
        }
        
        return tooltip;
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