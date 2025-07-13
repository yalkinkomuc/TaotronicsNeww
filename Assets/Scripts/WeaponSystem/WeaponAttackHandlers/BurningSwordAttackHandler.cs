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
        
        // Burning sword specific effects could be added here
        // For example: apply burn damage over time, fire particles, etc.
    }
} 