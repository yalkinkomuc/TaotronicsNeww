using UnityEngine;

public class BurningSwordAttackHandler : BaseWeaponAttackHandler
{
    public override WeaponType GetWeaponType()
    {
        return WeaponType.BurningSword;
    }
    
    protected override void HandleWeaponSpecificAttack(Player player, Enemy enemy, IWeaponAttackState weaponAttackState, float baseDamage, bool isCritical)
    {
        Debug.Log($"[BurningSword] Attack triggered! Base damage: {baseDamage}");
        
        int comboCounter = weaponAttackState.GetComboCounter();
        float damage = baseDamage;
        
        // BurningSword combo multipliers - similar to sword but with fire damage
        damage *= weaponAttackState.GetDamageMultiplier(comboCounter);
        
        Debug.Log($"[BurningSword] Combo: {comboCounter}, Final damage: {damage}");
        
        // Hasarı uygula
        DealDamageToEnemy(player, enemy, damage, comboCounter, isCritical);
        
        // Static olmayan düşmanlara burning sword knockback uygula
        if (enemy.rb.bodyType != RigidbodyType2D.Static)
        {
            // BurningSword has standard knockback like sword
            enemy.ApplyComboKnockback(player.transform.position, comboCounter);
        }
        
        // Try to apply burn effect (10% chance)
        TryApplyBurnEffect(player, enemy);
    }
    
    private void TryApplyBurnEffect(Player player, Enemy enemy)
    {
        float rand = Random.value;
        Debug.Log($"[BurningSword] Trying to apply burn effect... Random value: {rand}");
        
        // 10% chance to apply burn effect
        if (rand <= 0.1f)
        {
            Debug.Log("[BurningSword] Burn effect triggered! Starting burn...");
            StartBurnEffect(player, enemy);
        }
        else
        {
            Debug.Log("[BurningSword] Burn effect failed - random value too high");
        }
    }
    
    private void StartBurnEffect(Player player, Enemy enemy)
    {
        Debug.Log("[BurningSword] Starting burn effect on enemy: " + enemy.name);
        
        // Start visual burn effect
        if (enemy.entityFX != null)
        {
            Debug.Log("[BurningSword] Starting visual burn effect");
            enemy.entityFX.StartCoroutine("BurnFX");
        }
        else
        {
            Debug.LogWarning("[BurningSword] Enemy has no entityFX component!");
        }
        
        // Apply burn effect to enemy
        enemy.ApplyBurnEffect();
        Debug.Log("[BurningSword] Applied burn effect to enemy");
        
        // Start damage over time coroutine
        player.StartCoroutine(BurnDamageOverTime(player, enemy));
        Debug.Log("[BurningSword] Started damage over time coroutine");
    }
    
    private System.Collections.IEnumerator BurnDamageOverTime(Player player, Enemy enemy)
    {
        float burnDuration = 3f;
        float damagePerSecond = 3f;
        float damageRampUpTime = 1.5f;
        float maxDamageMultiplier = 1.3f;
        float damageInterval = 0.5f; // How often to deal damage (in seconds)
        
        float startTime = Time.time;
        float accumulatedDamage = 0f;
        
        while (Time.time - startTime < burnDuration && enemy != null)
        {
            float burnTime = Time.time - startTime;
            
            // Calculate damage multiplier based on burn time
            float burnTimeFactor = Mathf.Clamp01(burnTime / damageRampUpTime);
            float currentDamageMultiplier = Mathf.Lerp(1f, maxDamageMultiplier, burnTimeFactor);
            
            // Get player's spell damage for scaling
            float spellDamage = damagePerSecond;
            if (player.stats is PlayerStats playerStats)
            {
                spellDamage = WeaponDamageManager.GetSpellDamage(playerStats);
            }
            
            // Calculate damage for this interval
            float intervalDamage = spellDamage * currentDamageMultiplier * damageInterval;
            enemy.stats.TakeDamage(intervalDamage, CharacterStats.DamageType.Fire);
            
            // Accumulate damage for counter display
            accumulatedDamage += intervalDamage;
            
            // Show accumulated damage as counter
            if (FloatingTextManager.Instance != null)
            {
                Vector3 textPosition = enemy.transform.position + Vector3.up * 1.5f;
                FloatingTextManager.Instance.ShowMagicDamageText(accumulatedDamage, textPosition);
            }
            
            yield return new WaitForSeconds(damageInterval);
        }
        
        // Remove burn effect when finished
        if (enemy != null)
        {
            if (enemy.entityFX != null)
            {
                enemy.entityFX.StopCoroutine("BurnFX");
                enemy.entityFX.ResetToOriginalMaterial();
            }
            enemy.RemoveBurnEffect();
        }
    }
} 