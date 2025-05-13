using UnityEngine;

[System.Serializable]
public class WeaponData
{
    public string weaponId;        // Unique ID for the weapon
    public string weaponName;      // Display name for the weapon
    public WeaponType weaponType;  // Type of weapon (sword, boomerang, spellbook)
    public int level;              // Current upgrade level of the weapon
    public int maxLevel = 5;       // Maximum upgrade level
    public float baseDamageBonus;  // Base damage bonus at level 1
    
    [Header("Upgrade Settings")]
    public float upgradeDamageIncrement = 2f; // How much damage increases per level
    public int baseUpgradeCost = 100;         // Base cost for first upgrade
    public float upgradeCostMultiplier = 1.5f; // Cost multiplier per level
    
    [Header("UI")]
    public Sprite weaponIcon;      // Icon for the weapon in UI
    
    // Constructor for new weapons
    public WeaponData(string id, string name, WeaponType type, float baseDamage, int cost)
    {
        weaponId = id;
        weaponName = name;
        weaponType = type;
        level = 0;
        baseDamageBonus = baseDamage;
        baseUpgradeCost = cost;
    }
    
    // Get the current damage bonus for this weapon
    public float GetCurrentDamageBonus()
    {
        if (level <= 0) return 0;
        return baseDamageBonus + (upgradeDamageIncrement * (level - 1));
    }
    
    // Get the cost for the next upgrade
    public int GetNextUpgradeCost()
    {
        if (level >= maxLevel) return 0;
        return Mathf.RoundToInt(baseUpgradeCost * Mathf.Pow(upgradeCostMultiplier, level));
    }
    
    // Check if weapon can be upgraded further
    public bool CanUpgrade()
    {
        return level < maxLevel;
    }
}

// Enum to categorize weapon types
public enum WeaponType
{
    Sword,
    Boomerang,
    Spellbook
} 