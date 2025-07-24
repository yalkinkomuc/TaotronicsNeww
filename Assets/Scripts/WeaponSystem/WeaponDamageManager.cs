using UnityEngine;

/// <summary>
/// Manages weapon damage calculations by interfacing with WeaponData ScriptableObjects
/// instead of using player stat modifiers. This ensures weapon upgrades only affect
/// the specific weapon's damage range, not the player's general stats.
/// </summary>
public static class WeaponDamageManager
{
    /// <summary>
    /// Gets the weapon damage for a specific weapon type by finding its WeaponData
    /// and calculating damage based on its current upgrade level.
    /// </summary>
    /// <param name="weaponType">The type of weapon to get damage for</param>
    /// <param name="playerStats">Player stats for critical hit calculation</param>
    /// <returns>Calculated weapon damage value</returns>
    public static float GetWeaponDamage(WeaponType weaponType, PlayerStats playerStats = null)
    {
        // Find the weapon data for this weapon type
        WeaponData weaponData = GetWeaponData(weaponType);
        if (weaponData == null)
        {
            Debug.LogWarning($"No WeaponData found for weapon type: {weaponType}");
            return 10f; // Fallback damage value
        }
        
        // Get random damage within the weapon's upgraded range
        float baseDamage = weaponData.GetRandomDamage();
        
        // Apply critical hit if player stats available
        if (playerStats != null && playerStats.IsCriticalHit())
        {
            baseDamage *= GetCriticalMultiplier(weaponData, playerStats);
        }
        
        return baseDamage;
    }
    
    /// <summary>
    /// Gets the weapon damage range as a formatted string for UI display
    /// Uses the SAME calculation as actual combat damage
    /// </summary>
    /// <param name="weaponType">The type of weapon</param>
    /// <param name="playerStats">Player stats for attribute bonuses</param>
    /// <returns>Formatted damage range string (e.g., "15-25")</returns>
    public static string GetWeaponDamageRangeString(WeaponType weaponType, PlayerStats playerStats = null)
    {
        WeaponData weaponData = GetWeaponData(weaponType);
        if (weaponData == null)
        {
            return "N/A";
        }
        
        if (playerStats == null)
        {
            return weaponData.GetDamageRangeString();
        }
        
        // Get weapon's base damage range
        int weaponMinDamage = weaponData.GetTotalMinDamage();
        int weaponMaxDamage = weaponData.GetTotalMaxDamage();
        
        // Calculate damage multiplier using the SAME formula as all weapons
        float damageMultiplier = 1.0f;
        float totalDamage = playerStats.baseDamage.GetValue() * 1.0f; // First combo
        damageMultiplier = totalDamage / playerStats.baseDamage.GetBaseValue();
        
        // Calculate final damage range using the same multiplier as combat
        int finalMinDamage = Mathf.RoundToInt(weaponMinDamage * damageMultiplier);
        int finalMaxDamage = Mathf.RoundToInt(weaponMaxDamage * damageMultiplier);
        
        // For 3rd combo, apply the combo multiplier
        float totalDamageThirdCombo = playerStats.baseDamage.GetValue() * playerStats.thirdComboDamageMultiplier.GetValue();
        float thirdComboMultiplier = totalDamageThirdCombo / playerStats.baseDamage.GetBaseValue();
        
        int maxComboDamage = Mathf.RoundToInt(weaponMaxDamage * thirdComboMultiplier);
        
        // Format the range string: "First combo min - Third combo max"
        if (finalMinDamage == maxComboDamage)
            return finalMinDamage.ToString();
        else
            return $"{finalMinDamage}-{maxComboDamage}";
    }
    
    /// <summary>
    /// Gets the weapon damage range string for next level upgrade preview
    /// </summary>
    public static string GetWeaponDamageRangeStringNextLevel(WeaponType weaponType, PlayerStats playerStats = null)
    {
        WeaponData weaponData = GetWeaponData(weaponType);
        if (weaponData == null)
        {
            return "N/A";
        }
        
        // Check if weapon can be upgraded
        if (weaponData.level >= weaponData.maxLevel) 
            return GetWeaponDamageRangeString(weaponType, playerStats);
        
        // Temporarily increase level for preview
        int originalLevel = weaponData.level;
        weaponData.level++;
        
        // Get damage range with increased level
        string nextLevelRange = GetWeaponDamageRangeString(weaponType, playerStats);
        
        // Restore original level
        weaponData.level = originalLevel;
        
        return nextLevelRange;
    }
    
   
    public static float GetSpellDamage(PlayerStats playerStats = null)
    {
        // Get base spell damage from spellbook weapon
        float baseDamage = GetWeaponDamage(WeaponType.Spellbook, playerStats);
        
        // Apply elemental damage multiplier from Mind attribute
        if (playerStats != null)
        {
            float elementalMultiplier = playerStats.GetTotalElementalDamageMultiplier();
            baseDamage *= elementalMultiplier;
        }
        
        return baseDamage;
    }
    
    /// <summary>
    /// Gets the WeaponData for a specific weapon type from BlacksmithManager
    /// </summary>
    /// <param name="weaponType">The weapon type to find</param>
    /// <returns>WeaponData or null if not found</returns>
    private static WeaponData GetWeaponData(WeaponType weaponType)
    {
        if (BlacksmithManager.Instance == null)
        {
            Debug.LogWarning("BlacksmithManager.Instance is null!");
            return null;
        }
        
        var weapons = BlacksmithManager.Instance.GetWeaponsByType(weaponType);
        if (weapons.Count > 0)
        {
            return weapons[0]; // Return the first weapon of this type
        }
        
        return null;
    }
    
    /// <summary>
    /// Calculates critical hit multiplier based on weapon and player stats
    /// </summary>
    /// <param name="weaponData">The weapon being used</param>
    /// <param name="playerStats">Player stats</param>
    /// <returns>Critical hit damage multiplier</returns>
    private static float GetCriticalMultiplier(WeaponData weaponData, PlayerStats playerStats)
    {
        // Use weapon's critical damage if available, otherwise default multiplier
        if (weaponData != null && weaponData.criticalDamage > 0)
        {
            return weaponData.criticalDamage / 100f; // Convert percentage to multiplier
        }
        
        return 1.5f; // Default 50% bonus
    }
    
    /// <summary>
    /// Gets weapon data for currently equipped weapon of specified type
    /// This method can be expanded later to support multiple weapons of same type
    /// </summary>
    /// <param name="weaponType">Type of weapon to get</param>
    /// <param name="weaponName">Specific weapon name (optional)</param>
    /// <returns>WeaponData or null</returns>
    public static WeaponData GetEquippedWeaponData(WeaponType weaponType, string weaponName = null)
    {
        if (BlacksmithManager.Instance == null) return null;
        
        if (!string.IsNullOrEmpty(weaponName))
        {
            return BlacksmithManager.Instance.GetWeapon(weaponName);
        }
        
        var weapons = BlacksmithManager.Instance.GetWeaponsByType(weaponType);
        return weapons.Count > 0 ? weapons[0] : null;
    }
    

} 