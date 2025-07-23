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

[CreateAssetMenu(fileName = "New Weapon", menuName = "Data/Equipment/Weapon")]
public class WeaponData : EquipmentData
{
    [Header("Weapon Properties")]
    public WeaponType weaponType;
    public float attackSpeed = 1.0f;
    public float criticalChance = 5.0f;
    public float criticalDamage = 150.0f;
    
    [Header("Damage")]
    public int minDamage;
    public int maxDamage;
    public float baseDamageBonus;  // Base damage bonus at level 1
    
    [Header("Upgrade System")]
    public int level = 0;              // Current upgrade level of the weapon
    public int maxLevel = 30;          // Maximum upgrade level
    public float upgradeDamageIncrement = 2f; // How much damage increases per level
    public int baseUpgradeCost = 100;         // Base cost for first upgrade
    public float upgradeCostMultiplier = 1.1f; // Cost multiplier per level
    
    [Header("Required Materials")]
    public List<RequiredMaterial> requiredMaterials = new List<RequiredMaterial>();
    
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
    
    // Get required materials for next upgrade
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
    
    public override string GetTooltip()
    {
        string tooltip = base.GetTooltip();
        
        // Current damage
        if (level > 0)
        {
            tooltip += $"\n<b>Level:</b> {level}/{maxLevel}";
            tooltip += $"\n<b>Damage Bonus:</b> +{GetCurrentDamageBonus():F1}";
        }
        else
        {
            tooltip += $"\n<color=grey>Not upgraded</color>";
        }
        
        tooltip += $"\n<b>Base Damage:</b> {minDamage}-{maxDamage}";
        tooltip += $"\nAttack Speed: {attackSpeed:F1}";
        tooltip += $"\nCritical Chance: {criticalChance:F1}%";
        tooltip += $"\nCritical Damage: {criticalDamage:F0}%";
        
        // Upgrade info
        if (CanUpgrade())
        {
            tooltip += $"\n\n<color=yellow><b>Next Level ({level + 1}):</b></color>";
            tooltip += $"\nUpgrade Cost: {GetNextUpgradeCost()} Gold";
            
            var materials = GetRequiredMaterialsForUpgrade();
            if (materials.Count > 0)
            {
                tooltip += "\nRequired Materials:";
                foreach (var mat in materials)
                {
                    tooltip += $"\n• {mat.Key}: {mat.Value}";
                }
            }
        }
        else if (level >= maxLevel)
        {
            tooltip += $"\n\n<color=gold><b>MAX LEVEL</b></color>";
        }
        
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
    
    // Get total damage including upgrades
    public int GetTotalMinDamage()
    {
        return minDamage + Mathf.RoundToInt(GetCurrentDamageBonus());
    }
    
    public int GetTotalMaxDamage()
    {
        return maxDamage + Mathf.RoundToInt(GetCurrentDamageBonus());
    }
    
    // Get random damage in min-max range
    public int GetRandomDamage()
    {
        int min = GetTotalMinDamage();
        int max = GetTotalMaxDamage();
        
        // Ensure max is at least min + 1 for random range
        if (max <= min) max = min + 1;
        
        return UnityEngine.Random.Range(min, max + 1); // +1 because Range is exclusive for int
    }
    
    // Get damage range as formatted string for UI
    public string GetDamageRangeString()
    {
        int min = GetTotalMinDamage();
        int max = GetTotalMaxDamage();
        
        if (min == max)
            return min.ToString();
        else
            return $"{min}-{max}";
    }
}

// Enum to categorize weapon types - Updated to match weapon state machine
public enum WeaponType
{
    Sword = 0,
    Boomerang = 1,
    Spellbook = 2,
    BurningSword=3,
    Hammer=4,
    Shield=5,
    IceHammer =6
} 