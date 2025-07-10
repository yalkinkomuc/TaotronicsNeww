using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class RequiredMaterial
{
    public MaterialType materialType;
    public int baseAmount = 1;              // Base amount needed at level 1
    public bool scalesWithLevel = true;     // Whether amount increases with weapon level
    public int levelScaleRate = 2;          // Every X levels, add 1 more material
    public int minimumLevel = 1;            // At which level this material is required
    
    // Get the actual required amount for a specific weapon level
    public int GetRequiredAmount(int weaponLevel)
    {
        if (weaponLevel < minimumLevel)
            return 0;
            
        if (!scalesWithLevel)
            return baseAmount;
            
        // Calculate scaling: baseAmount + (level / levelScaleRate)
        int additionalAmount = weaponLevel / levelScaleRate;
        return baseAmount + additionalAmount;
    }
}

[System.Serializable]
public class WeaponData
{
    public string weaponId;        // Unique ID for the weapon
    public string weaponName;      // Display name for the weapon
    public WeaponType weaponType;  // Type of weapon (sword, boomerang, spellbook)
    public int level;              // Current upgrade level of the weapon
    public int maxLevel = 30;       // Maximum upgrade level
    public float baseDamageBonus;  // Base damage bonus at level 1
    
    [Header("Upgrade Settings")]
    public float upgradeDamageIncrement = 2f; // How much damage increases per level
    public int baseUpgradeCost = 100;         // Base cost for first upgrade
    public float upgradeCostMultiplier = 1.1f; // Cost multiplier per level
    
    [Header("Required Materials")]
    public List<RequiredMaterial> requiredMaterials = new List<RequiredMaterial>();
    
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
        requiredMaterials = new List<RequiredMaterial>();
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
    
    // Get required materials for next upgrade (NEW METHOD)
    public Dictionary<MaterialType, int> GetRequiredMaterialsForUpgrade()
    {
        var requirements = new Dictionary<MaterialType, int>();
        
        foreach (var material in requiredMaterials)
        {
            int requiredAmount = material.GetRequiredAmount(level + 1); // Next level
            if (requiredAmount > 0)
            {
                requirements[material.materialType] = requiredAmount;
            }
        }
        
        return requirements;
    }
}

// Enum to categorize weapon types
public enum WeaponType
{
    Sword,
    Boomerang,
    Spellbook
} 