using UnityEngine;

[CreateAssetMenu(fileName = "New Secondary Weapon", menuName = "Data/Equipment/Secondary Weapon")]
public class SecondaryWeaponData : EquipmentData
{
    [Header("Secondary Weapon Properties")]
    public SecondaryWeaponType secondaryWeaponType;
    public int damage;
    public float cooldown = 3.0f;
    public int uses = 1; // How many times can be used before cooldown
    
    [Header("Effect")]
    public bool hasEffect;
    [TextArea(3, 5)]
    public string effectDescription;
    
    private void Awake()
    {
        itemType = ItemType.SecondaryWeapon;
        equipmentSlot = EquipmentSlot.SecondaryWeapon;
        isStackable = false;
    }
    
    public override string GetTooltip()
    {
        string tooltip = base.GetTooltip();
        
        if (damage > 0)
        {
            tooltip += $"\n<b>Damage:</b> {damage}";
        }
        
        tooltip += $"\nCooldown: {cooldown:F1}s";
        
        if (uses > 1)
        {
            tooltip += $"\nUses: {uses}";
        }
        
        if (hasEffect && !string.IsNullOrEmpty(effectDescription))
        {
            tooltip += $"\n\n<color=yellow><b>Effect:</b></color>\n{effectDescription}";
        }
        
        return tooltip;
    }
}

public enum SecondaryWeaponType
{
    Shield = 0,
    Spellbook = 1,
    Boomerang = 2
    
} 