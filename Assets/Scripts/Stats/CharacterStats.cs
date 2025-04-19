using System;
using UnityEngine;
using UnityEngine.Serialization;

public class CharacterStats : MonoBehaviour
{
    [FormerlySerializedAs("damage")] public Stat baseDamage;
    public Stat secondComboDamageMultiplier;
    public Stat thirdComboDamageMultiplier;
    public Stat maxHealth;
    public Stat maxMana;
    
    public float currentHealth;
    public float currentMana;
    
    [Header("Level System")]
    [SerializeField] protected int level = 1;
    [SerializeField] protected float levelHealthMultiplier = 0.1f; // Each level adds 10% health
    [SerializeField] protected float levelDamageMultiplier = 0.05f; // Each level adds 5% damage
    
    protected float baseMaxHealth;
    protected float baseMaxDamage;
    
    public bool isInvincible { get; private set; }

    protected virtual void Start()
    {
        // Store base values before applying level multipliers
        baseMaxHealth = maxHealth.GetValue();
        baseMaxDamage = baseDamage.GetValue();
        
        // Apply level multipliers
        ApplyLevelMultipliers();
        
        currentHealth = maxHealth.GetValue();
        currentMana = maxMana.GetValue();
    }
    
    public virtual void ApplyLevelMultipliers()
    {
        // Reset stats to remove old level modifiers
        maxHealth.RemoveAllModifiersOfType(StatModifierType.LevelBonus);
        baseDamage.RemoveAllModifiersOfType(StatModifierType.LevelBonus);
        
        if (level > 1)
        {
            // Apply level multipliers (level-1 because level 1 is base stats)
            float healthBonus = baseMaxHealth * levelHealthMultiplier * (level - 1);
            float damageBonus = baseMaxDamage * levelDamageMultiplier * (level - 1);
            
            maxHealth.AddModifier(healthBonus, StatModifierType.LevelBonus);
            baseDamage.AddModifier(damageBonus, StatModifierType.LevelBonus);
        }
    }
    
    public int GetLevel()
    {
        return level;
    }
    
    public virtual void SetLevel(int newLevel)
    {
        if (newLevel < 1) newLevel = 1;
        
        level = newLevel;
        ApplyLevelMultipliers();
        
        // Refresh current health and mana to the new maximum
        float healthPercentage = currentHealth / maxHealth.GetValue();
        float manaPercentage = currentMana / maxMana.GetValue();
        
        currentHealth = maxHealth.GetValue() * healthPercentage;
        currentMana = maxMana.GetValue() * manaPercentage;
    }

    public virtual void TakeDamage(float _damage)
    {
        if (isInvincible)
            return;
            
        currentHealth -= _damage;

        if (currentHealth <= 0)
            Die();
    }

    public virtual bool UseMana(float _mana)
    {
        if (currentMana >= _mana)
        {
            currentMana -= _mana;
            return true;
        }
        return false;
    }
    
    public void MakeInvincible(bool _isInvincible) => isInvincible = _isInvincible;

    public virtual void Die()
    {
        // Override in derived classes
    }
}
