using UnityEngine;

public class SwordAttackHandler : BaseWeaponAttackHandler
{
    public override WeaponType GetWeaponType()
    {
        return WeaponType.Sword;
    }
    
    protected override void HandleWeaponSpecificAttack(Player player, Enemy enemy, IWeaponAttackState weaponAttackState, float baseDamage, bool isCritical)
    {
        int comboCounter = weaponAttackState.GetComboCounter();
        float damage = baseDamage;
        
        // Sword combo multipliers - sword has fast, precise combos
        damage *= weaponAttackState.GetDamageMultiplier(comboCounter);
        
        // Hasarı uygula
        DealDamageToEnemy(player, enemy, damage, comboCounter, isCritical);
        
        // Static olmayan düşmanlara sword knockback uygula
        if (enemy.rb.bodyType != RigidbodyType2D.Static)
        {
            // Sword has standard knockback
            enemy.ApplyComboKnockback(player.transform.position, comboCounter);
        }
    }
} 