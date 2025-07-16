using UnityEngine;

public static class HammerCalculations
{
    private static float hammerKnockbackMultiplier = 1.5f;
    private static float comboWindow = 3f;
    
    public static float GetDamageMultiplier(Player player, int comboIndex)
    {
        float baseMultiplier = 1.0f;
        switch (comboIndex)
        {
            case 0:
                baseMultiplier = 1.0f; // İlk saldırı için standart hasar
                break;
            case 1:
                baseMultiplier = player.stats.secondComboDamageMultiplier.GetValue(); // İkinci saldırı
                break;
            case 2:
                baseMultiplier = player.stats.thirdComboDamageMultiplier.GetValue(); // Üçüncü saldırı
                break;
            default:
                baseMultiplier = 1.0f;
                break;
        }
        
        // Might bonus'u da dahil et
        var playerStats = player.stats as PlayerStats;
        if (playerStats != null)
        {
            float totalDamage = playerStats.baseDamage.GetValue() * baseMultiplier;
            return totalDamage / playerStats.baseDamage.GetBaseValue();
        }
        
        return baseMultiplier;
    }
    
    public static float GetKnockbackMultiplier(int comboIndex)
    {
        switch (comboIndex)
        {
            case 2: // 3. saldırı (0-based)
                return 1f; // 3. saldırı için özel knockback
            default:
                return hammerKnockbackMultiplier;
        }
    }
    
    public static float GetComboWindow()
    {
        return comboWindow;
    }
    
    public static void SetHammerKnockbackMultiplier(float multiplier)
    {
        hammerKnockbackMultiplier = multiplier;
    }
    
    public static void SetComboWindow(float window)
    {
        comboWindow = window;
    }
} 