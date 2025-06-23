using UnityEngine;

[CreateAssetMenu(fileName = "New Equipment Weapon", menuName = "Data/Equipment/Weapon")]
public class EquipmentWeaponData : EquipmentData
{
    [Header("Weapon Properties")]
    public EquipmentWeaponType weaponType;
    public float attackSpeed = 1.0f;
    public float criticalChance = 5.0f;
    public float criticalDamage = 150.0f;
    
    [Header("Damage")]
    public int minDamage;
    public int maxDamage;
    
    [Header("Special Effects")]
    public bool hasSpecialEffect;
    [TextArea(3, 5)]
    public string specialEffectDescription;
    
    private void Awake()
    {
        itemType = ItemType.Weapon;
        equipmentSlot = EquipmentSlot.MainWeapon;
        isStackable = false;
    }
    
    public override string GetTooltip()
    {
        string tooltip = base.GetTooltip();
        
        tooltip += $"\n<b>Damage:</b> {minDamage}-{maxDamage}";
        tooltip += $"\nAttack Speed: {attackSpeed:F1}";
        tooltip += $"\nCritical Chance: {criticalChance:F1}%";
        tooltip += $"\nCritical Damage: {criticalDamage:F0}%";
        
        if (hasSpecialEffect && !string.IsNullOrEmpty(specialEffectDescription))
        {
            tooltip += $"\n\n<color=orange><b>Special Effect:</b></color>\n{specialEffectDescription}";
        }
        
        return tooltip;
    }
    
    public int GetAverageDamage()
    {
        return (minDamage + maxDamage) / 2;
    }
}

public enum EquipmentWeaponType
{
    Sword = 0,
    Bow = 1,
    Staff = 2,
    Dagger = 3,
    Axe = 4,
    Mace = 5,
    Spear = 6
} 