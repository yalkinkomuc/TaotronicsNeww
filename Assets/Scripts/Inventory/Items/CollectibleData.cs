using UnityEngine;

[CreateAssetMenu(fileName = "New Collectible", menuName = "Data/Items/Collectible")]
public class CollectibleData : ItemData
{
    [Header("Collectible Properties")]
    public CollectibleType collectibleType;
    public CollectibleCategory category;
    
    [Header("Collectible Info")]
    public bool isRareCollectible;
    
    [Header("Rewards")]
    public bool grantsReward;
    public string rewardDescription;
    public int goldReward;
    public int experienceReward;
    
    [Header("Lore")]
    [TextArea(4, 8)]
    public string loreText;
    public string discoveryLocation;
    
    private void Awake()
    {
        itemType = ItemType.Collectible;
        isStackable = false; // Collectibles are unique
        maxStackSize = 1;
    }
    
    public override string GetTooltip()
    {
        string tooltip = $"<b>{itemName}</b>\n";
        tooltip += $"<color=grey>{category} Collectible</color>\n";
        

        
        if (isRareCollectible)
        {
            tooltip += "<color=gold>★ Rare Collectible ★</color>\n";
        }
        
        if (!string.IsNullOrEmpty(discoveryLocation))
        {
            tooltip += $"Found in: {discoveryLocation}\n";
        }
        
        if (grantsReward)
        {
            tooltip += "\n<b>Collection Reward:</b>\n";
            if (goldReward > 0)
                tooltip += $"• {goldReward} Gold\n";
            if (experienceReward > 0)
                tooltip += $"• {experienceReward} Experience\n";
            if (!string.IsNullOrEmpty(rewardDescription))
                tooltip += $"• {rewardDescription}\n";
        }
        
        if (!string.IsNullOrEmpty(loreText))
        {
            tooltip += $"\n<i>\"{loreText}\"</i>";
        }
        
        if (!string.IsNullOrEmpty(description))
        {
            tooltip += $"\n\n{description}";
        }
        
        return tooltip;
    }
}

public enum CollectibleType
{
    Artifact = 0,
    Tome = 1,
    Relic = 2,
    Painting = 3,
    Sculpture = 4,
    Jewelry = 5,
    Fossil = 6,
    Coin = 7,
    Map = 8,
    Document = 9,
    Weapon_Fragment = 10,
    Mysterious_Object = 11
}

public enum CollectibleCategory
{
    Ancient_Artifacts = 0,
    Lost_Knowledge = 1,
    Royal_Treasures = 2,
    Natural_Wonders = 3,
    Mystical_Items = 4,
    Historical_Documents = 5,
    Cursed_Objects = 6,
    Divine_Relics = 7
}
 