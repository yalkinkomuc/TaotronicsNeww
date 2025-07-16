using UnityEngine;

public class HammerAttackHandler : BaseWeaponAttackHandler
{
    public override WeaponType GetWeaponType()
    {
        return WeaponType.Hammer;
    }
    
    protected override void HandleWeaponSpecificAttack(Player player, Enemy enemy, IWeaponAttackState weaponAttackState, float baseDamage, bool isCritical)
    {
        int comboCounter = weaponAttackState.GetComboCounter();
        float damage = baseDamage;
        
        // Hammer combo multipliers - hammer has slower but more powerful attacks
        damage *= weaponAttackState.GetDamageMultiplier(comboCounter);
        
        // Hasarı uygula
        DealDamageToEnemy(player, enemy, damage, comboCounter, isCritical);

        // Eğer 3. combo ise, gerçek hasarı kaydet
        if (comboCounter == 2)
        {
            player.lastHammerCombo3Damage = damage;
        }
        
        // Static olmayan düşmanlara hammer knockback uygula
        if (enemy.rb.bodyType != RigidbodyType2D.Static)
        {
            // Hammer has stronger knockback
            float knockbackMultiplier = weaponAttackState.GetKnockbackMultiplier(comboCounter);
            enemy.ApplyComboKnockback(player.transform.position, comboCounter, knockbackMultiplier);
        }
    }
    
    public override void HandleAttack(Player player, Vector2 attackPosition)
    {
        // Hammer might have a larger attack area, so we could modify this
        // For now, use the base implementation but with potential for future enhancement
        base.HandleAttack(player, attackPosition);
        
        // Hammer-specific effects could be added here
        // For example: screen shake, dust particles, etc.
    }
} 