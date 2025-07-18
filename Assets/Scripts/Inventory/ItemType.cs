using UnityEngine;

// Main item types for categorization
public enum ItemType
{
    Weapon = 0,
    Armor = 1,
    SecondaryWeapon = 2,
    Accessory = 3,
    Rune = 4,
    UpgradeMaterial = 5,
    Collectible = 6
}

// Equipment slot types
public enum EquipmentSlot
{
    MainWeapon = 0,
    Armor = 1,
    SecondaryWeapon = 2,
    Accessory = 3
}

// Rune slot types
public enum RuneSlot
{
    Slot1 = 0,
    Slot2 = 1,
    Slot3 = 2,
    Slot4 = 3,
    Slot5 = 4,
    Slot6 = 5
}

// Legacy StatType for compatibility (used by equipment system)
public enum StatType
{
    Health = 0,
    Vitality = 1,        // Actually means health bonus from vitality
    Might = 2,           // Actually means attack damage bonus from might// Legacy - maps to AttackDamage
    Armor = 6,
    CriticalChance = 7,
    CriticalDamage = 8
}

 