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
            
            // Show critical hit feedback
            if (FloatingTextManager.Instance != null)
            {
                Vector3 playerPos = playerStats.transform.position + Vector3.up * 1.5f;
                FloatingTextManager.Instance.ShowCustomText("CRITICAL!", playerPos, Color.yellow);
            }
        }
        
        return baseDamage;
    }
    
    /// <summary>
    /// Gets the weapon damage range as a formatted string for UI display
    /// </summary>
    /// <param name="weaponType">The type of weapon</param>
    /// <returns>Formatted damage range string (e.g., "15-25")</returns>
    public static string GetWeaponDamageRangeString(WeaponType weaponType)
    {
        WeaponData weaponData = GetWeaponData(weaponType);
        if (weaponData == null)
        {
            return "N/A";
        }
        
        return weaponData.GetDamageRangeString();
    }
    
    /// <summary>
    /// Gets the minimum damage for a weapon type
    /// </summary>
    public static int GetWeaponMinDamage(WeaponType weaponType)
    {
        WeaponData weaponData = GetWeaponData(weaponType);
        return weaponData != null ? weaponData.GetTotalMinDamage() : 0;
    }
    
    /// <summary>
    /// Gets the maximum damage for a weapon type
    /// </summary>
    public static int GetWeaponMaxDamage(WeaponType weaponType)
    {
        WeaponData weaponData = GetWeaponData(weaponType);
        return weaponData != null ? weaponData.GetTotalMaxDamage() : 0;
    }
    
    /// <summary>
    /// Gets spell damage (spellbook weapon type) with elemental multiplier applied
    /// </summary>
    /// <param name="playerStats">Player stats for mind attribute scaling</param>
    /// <returns>Calculated spell damage</returns>
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