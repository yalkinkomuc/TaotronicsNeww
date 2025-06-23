using UnityEngine;

[CreateAssetMenu(fileName = "New Armor", menuName = "Data/Equipment/Armor")]
public class ArmorData : EquipmentData
{
    [Header("Armor Properties")]
    public ArmorType armorType;
    public int armorValue;
    public float magicResistance;
    
    [Header("Set Bonus")]
    public bool isPartOfSet;
    public string setName;
    [TextArea(2, 4)]
    public string setBonusDescription;
    
    private void Awake()
    {
        itemType = ItemType.Armor;
        equipmentSlot = EquipmentSlot.Armor;
        isStackable = false;
    }
    
    public override string GetTooltip()
    {
        string tooltip = base.GetTooltip();
        
        tooltip += $"\n<b>Armor:</b> {armorValue}";
        
        if (magicResistance > 0)
        {
            tooltip += $"\nMagic Resistance: {magicResistance:F1}%";
        }
        
        if (isPartOfSet && !string.IsNullOrEmpty(setName))
        {
            tooltip += $"\n\n<color=cyan><b>Set:</b> {setName}</color>";
            if (!string.IsNullOrEmpty(setBonusDescription))
            {
                tooltip += $"\n<color=cyan>{setBonusDescription}</color>";
            }
        }
        
        return tooltip;
    }
}

public enum ArmorType
{
    Cloth = 0,
    Leather = 1,
    Chain = 2,
    Plate = 3,
    Robe = 4
} 