using UnityEngine;

public abstract class BaseWeaponAttackHandler : IWeaponAttackHandler
{
    public abstract WeaponType GetWeaponType();
    
    public virtual void HandleAttack(Player player, Vector2 attackPosition)
    {
        // Saldırı alanındaki nesneleri kontrol et
        Collider2D[] colliders = Physics2D.OverlapBoxAll(attackPosition, player.attackSize, 0, player.passableEnemiesLayerMask);

        // Tüm hedeflere saldırıyı uygula
        foreach (var hit in colliders)
        {
            // Düşmana saldır
            TryAttackEnemy(player, hit);
        }
    }
    
    protected virtual void TryAttackEnemy(Player player, Collider2D hit)
    {
        Enemy enemy = hit.GetComponent<Enemy>();
        if (enemy == null) return;
        
        // Bu düşmana zaten vurduk mu kontrol et
        if (player.HasHitEntity(enemy)) return;
                
        // Bu düşmanı vurulmuş olarak işaretle
        player.MarkEntityAsHit(enemy);
        
        // Hasar hesapla ve uygula
        ApplyDamageToEnemy(player, enemy);
    }
    
  
    
    protected virtual void ApplyDamageToEnemy(Player player, Enemy enemy)
    {
        // Temel hasar değerini al
        float currentDamage = CalculateDamage(player, out bool isCritical);
        
        // Saldırı türüne göre hasar ve knockback'i ayarla
        if (player.stateMachine.currentState is IWeaponAttackState weaponAttackState)
        {
            // Weapon-specific attack handling
            HandleWeaponSpecificAttack(player, enemy, weaponAttackState, currentDamage, isCritical);
        }
        else if (player.stateMachine.currentState == player.crouchAttackState)
        {
            // Çömelme saldırısı için hasar hesapla
            HandleCrouchAttack(player, enemy, currentDamage, isCritical);
        }
    }
    
    protected virtual void ApplyDamageToDummy(Player player)
    {
        // Temel hasar değerini al
        float damage = CalculateDamage(player, out bool isCritical);
        int comboCounter = 0;
        
        // Saldırı türüne göre hasarı ayarla
        if (player.stateMachine.currentState is IWeaponAttackState weaponAttackState)
        {
            comboCounter = weaponAttackState.GetComboCounter();
            damage *= weaponAttackState.GetDamageMultiplier(comboCounter);
        }
        else if (player.stateMachine.currentState == player.crouchAttackState)
        {
            // Çömelme saldırısı için %20 ekstra hasar
            damage *= 1.2f;
        }
        
    }
    
    protected abstract void HandleWeaponSpecificAttack(Player player, Enemy enemy, IWeaponAttackState weaponAttackState, float baseDamage, bool isCritical);
    
    protected virtual void HandleCrouchAttack(Player player, Enemy enemy, float baseDamage, bool isCritical)
    {
        // Çömelme saldırısı için %20 ekstra hasar
        float damage = baseDamage * 1.2f;
        
        // Hasarı uygula
        DealDamageToEnemy(player, enemy, damage, 0, isCritical);
        
        // Static olmayan düşmanlara knockback uygula
        if (enemy.rb.bodyType != RigidbodyType2D.Static)
        {
            enemy.ApplyKnockback(player.transform.position);
        }
    }
    
    protected virtual void DealDamageToEnemy(Player player, Enemy enemy, float damage, int comboCounter, bool isCritical)
    {
        // Hasarı doğrudan uygula
        enemy.stats.TakeDamage(damage, CharacterStats.DamageType.Physical);
        
        // Vuruş efekti göster
        if (enemy.entityFX != null)
        {
            enemy.entityFX.StartCoroutine("HitFX");
        }
    }
    
    protected virtual float CalculateDamage(Player player, out bool isCritical)
    {
        float damage = 0f;
        isCritical = false;
        
        if (player.stats is PlayerStats playerStats)
        {
            // Get weapon damage using WeaponDamageManager
            damage = WeaponDamageManager.GetWeaponDamage(GetWeaponType(), playerStats);
            
            // Check if it was a critical hit
            isCritical = playerStats.IsCriticalHit();
        }
        
        return damage;
    }
} 