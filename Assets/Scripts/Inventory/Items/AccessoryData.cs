using UnityEngine;

[CreateAssetMenu(fileName = "New Accessory", menuName = "Data/Equipment/Accessory")]
public class AccessoryData : EquipmentData
{
    [Header("Accessory Properties")]
    public AccessoryType accessoryType;
    
    [Header("Special Abilities")]
    public bool grantsSpecialAbility;
    public string abilityName;
    [TextArea(3, 5)]
    public string abilityDescription;
    
    [Header("Passive Effects")]
    public bool hasPassiveEffect;
    [TextArea(2, 4)]
    public string passiveEffectDescription;
    
    private void Awake()
    {
        itemType = ItemType.Accessory;
        equipmentSlot = EquipmentSlot.Accessory;
        isStackable = false;
    }
    
    public override string GetTooltip()
    {
        string tooltip = base.GetTooltip();
        
        if (grantsSpecialAbility && !string.IsNullOrEmpty(abilityName))
        {
            tooltip += $"\n\n<color=purple><b>Special Ability:</b> {abilityName}</color>";
            if (!string.IsNullOrEmpty(abilityDescription))
            {
                tooltip += $"\n<color=purple>{abilityDescription}</color>";
            }
        }
        
        if (hasPassiveEffect && !string.IsNullOrEmpty(passiveEffectDescription))
        {
            tooltip += $"\n\n<color=green><b>Passive Effect:</b></color>\n<color=green>{passiveEffectDescription}</color>";
        }
        
        return tooltip;
    }
}

public enum AccessoryType
{
    Necklace = 0,
    Ring = 1,
    Amulet = 2,
    Pendant = 3,
    Charm = 4,
    Talisman = 5
} 