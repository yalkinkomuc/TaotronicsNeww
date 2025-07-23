using UnityEngine;

public class IceHammerAttackHandler : BaseWeaponAttackHandler
{
    public override WeaponType GetWeaponType()
    {
        return WeaponType.IceHammer;
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
}
