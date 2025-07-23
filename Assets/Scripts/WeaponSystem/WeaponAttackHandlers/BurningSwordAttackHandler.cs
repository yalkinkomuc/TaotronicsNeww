using UnityEngine;

public class BurningSwordAttackHandler : BaseWeaponAttackHandler
{
    public override WeaponType GetWeaponType()
    {
        return WeaponType.BurningSword;
    }
    
    protected override void HandleWeaponSpecificAttack(Player player, Enemy enemy, IWeaponAttackState weaponAttackState, float baseDamage, bool isCritical)
    {
        
        int comboCounter = weaponAttackState.GetComboCounter();
        float damage = baseDamage;
        
        // BurningSword combo multipliers - similar to sword but with fire damage
        damage *= weaponAttackState.GetDamageMultiplier(comboCounter);
        
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
       
        
        // 10% chance to apply burn effect
        if (rand <= 0.1f)
        {
            StartBurnEffect(player, enemy);
        }
     
    }
    
    private void StartBurnEffect(Player player, Enemy enemy)
    {
       
        
        // Start visual burn effect
        if (enemy.entityFX != null)
        {
            enemy.entityFX.StartCoroutine("BurnFX");
        }
        else
        {
            Debug.LogWarning("[BurningSword] Enemy has no entityFX component!");
        }
        
        // Apply burn effect to enemy (without movement speed reduction for burning sword)
        enemy.ApplyBurnEffect(false);
        
        // Start damage over time coroutine
        player.StartCoroutine(BurnDamageOverTime(player, enemy));
        
    }
    
    private System.Collections.IEnumerator BurnDamageOverTime(Player player, Enemy enemy)
    {
        float damagePerSecond = 1f;
        float damageRampUpTime = 1.5f;
        float maxDamageMultiplier = 1.3f;
        float damageInterval = 0.5f; // How often to deal damage (in seconds)
        float textDisplayInterval = 0.1f; // How often to show damage text
        
        // Get burn duration from enemy's EntityFX
        float burnDuration = 1f; // Default value
        if (enemy.entityFX != null)
        {
            burnDuration = enemy.entityFX.burnDuration;
        }
        
        float startTime = Time.time;
        float accumulatedDamage = 0f;
        float lastTextDisplayTime = 0f;
        
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
            
            // Show accumulated damage as counter (every 0.1 seconds)
            if (Time.time - lastTextDisplayTime >= textDisplayInterval)
            {
                if (FloatingTextManager.Instance != null)
                {
                    Vector3 textPosition = enemy.transform.position + Vector3.up * 1.5f;
                    FloatingTextManager.Instance.ShowMagicDamageText(accumulatedDamage, textPosition);
                }
                lastTextDisplayTime = Time.time;
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
            enemy.RemoveBurnEffect(false); // No movement speed restoration for burning sword
        }
    }
} 