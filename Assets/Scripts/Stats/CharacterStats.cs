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
    
    [Header("Attribute System")]
    // These properties will be overridden by derived classes
    protected virtual int vitality { get => 0; set { } }
    protected virtual int might { get => 0; set { } }
    protected virtual int agility { get => 0; set { } }
    protected virtual int defense { get => 0; set { } }
    protected virtual int luck { get => 0; set { } }
    
    [Header("Attribute Limits")]
    protected virtual int maxAttributeLevel { get => 99; set { } }
    
    // Derived stats from attributes
    public float criticalChance { get; protected set; }
    public float criticalDamage { get; protected set; } = 1.5f;  // Base critical damage multiplier
    public float attackPower { get; protected set; }
    public float speedStat { get; protected set; }
    public float defenseStat { get; protected set; }
    
    // Base stats for level 1
    [Header("Base Stats")]
    protected virtual float baseHealthValue { get => 100f; set { } }
    protected virtual float baseDamageValue { get => 10f; set { } }
    protected virtual float baseManaValue { get => 50f; set { } }
    protected virtual float baseDefenseValue { get => 5f; set { } }
    protected virtual float baseSpeedValue { get => 300f; set { } }
    
    // Growth constants
    protected const float HEALTH_GROWTH = 0.08f;      // 8% growth per point
    protected const float DAMAGE_GROWTH = 0.06f;      // 6% growth per point
    protected const float MANA_GROWTH = 0.07f;        // 7% growth per point
    protected const float DEFENSE_GROWTH = 0.05f;     // 5% growth per point
    protected const float SPEED_GROWTH = 0.02f;       // 2% growth per point
    protected const float CRIT_CHANCE_PER_LUCK = 0.01f; // 1% per point (linear)
    
    protected float baseMaxHealth;
    protected float baseMaxDamage;
    
    public bool isInvincible { get; private set; }

    // Properties to access attribute values
    public int Vitality => vitality;
    public int Might => might;
    public int Agility => agility;
    public int Defense => defense;
    public int Luck => luck;

    protected virtual void Awake()
    {
        // Store base values before applying level multipliers
        baseMaxHealth = maxHealth.GetValue();
        baseMaxDamage = baseDamage.GetValue();
    }

    protected virtual void Start()
    {
        // Apply attribute bonuses first
        ApplyAttributeBonuses();
        
        // Apply level multipliers
        ApplyLevelMultipliers();
        
        // Initialize health and mana to max values
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
    
    // Apply all attribute bonuses to stats using exponential growth
    public virtual void ApplyAttributeBonuses()
    {
        // Reset any previous attribute bonuses
        maxHealth.RemoveAllModifiersOfType(StatModifierType.Attribute);
        baseDamage.RemoveAllModifiersOfType(StatModifierType.Attribute);
        maxMana.RemoveAllModifiersOfType(StatModifierType.Attribute);
        
        // Calculate vitality bonus (exponential growth)
        float healthMultiplier = Mathf.Pow(1 + HEALTH_GROWTH, vitality) - 1;
        float healthBonus = baseHealthValue * healthMultiplier;
        maxHealth.AddModifier(healthBonus, StatModifierType.Attribute);
        
        // Calculate might bonus (exponential growth)
        float damageMultiplier = Mathf.Pow(1 + DAMAGE_GROWTH, might) - 1;
        float damageBonus = baseDamageValue * damageMultiplier;
        baseDamage.AddModifier(damageBonus, StatModifierType.Attribute);
        
        // Calculate agility bonus for mana (exponential growth)
        float manaMultiplier = Mathf.Pow(1 + MANA_GROWTH, agility) - 1;
        float manaBonus = baseManaValue * manaMultiplier;
        maxMana.AddModifier(manaBonus, StatModifierType.Attribute);
        
        // Calculate defense stat (exponential growth)
        float defenseMultiplier = Mathf.Pow(1 + DEFENSE_GROWTH, defense) - 1;
        defenseStat = baseDefenseValue * (1 + defenseMultiplier);
        
        // Critical chance remains linear (1% per point)
        criticalChance = luck * CRIT_CHANCE_PER_LUCK;
        
        // Calculate speed bonus (smaller exponential growth)
        float speedMultiplier = Mathf.Pow(1 + SPEED_GROWTH, agility) - 1;
        speedStat = baseSpeedValue * (1 + speedMultiplier);
        
        // Calculate derived stats for UI display
        attackPower = baseDamage.GetValue();
    }
    
    // Calculates just the health bonus for a specific vitality level (for preview)
    public float CalculateHealthBonusForVitality(int vitalityLevel)
    {
        float healthMultiplier = Mathf.Pow(1 + HEALTH_GROWTH, vitalityLevel) - 1;
        return baseHealthValue * healthMultiplier;
    }
    
    // Calculates just the damage bonus for a specific might level (for preview)
    public float CalculateDamageBonusForMight(int mightLevel)
    {
        float damageMultiplier = Mathf.Pow(1 + DAMAGE_GROWTH, mightLevel) - 1;
        return baseDamageValue * damageMultiplier;
    }
    
    // Calculates just the mana bonus for a specific agility level (for preview)
    public float CalculateManaBonusForAgility(int agilityLevel)
    {
        float manaMultiplier = Mathf.Pow(1 + MANA_GROWTH, agilityLevel) - 1;
        return baseManaValue * manaMultiplier;
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
    
    // Methods to modify attribute values
    public virtual bool IncreaseVitality(int amount = 1)
    {
        if (vitality >= maxAttributeLevel) return false;
        
        vitality += amount;
        if (vitality > maxAttributeLevel) vitality = maxAttributeLevel;
        
        ApplyAttributeBonuses();
        return true;
    }
    
    public virtual bool IncreaseMight(int amount = 1)
    {
        if (might >= maxAttributeLevel) return false;
        
        might += amount;
        if (might > maxAttributeLevel) might = maxAttributeLevel;
        
        ApplyAttributeBonuses();
        return true;
    }
    
    public virtual bool IncreaseAgility(int amount = 1)
    {
        if (agility >= maxAttributeLevel) return false;
        
        agility += amount;
        if (agility > maxAttributeLevel) agility = maxAttributeLevel;
        
        ApplyAttributeBonuses();
        return true;
    }
    
    public virtual bool IncreaseDefense(int amount = 1)
    {
        if (defense >= maxAttributeLevel) return false;
        
        defense += amount;
        if (defense > maxAttributeLevel) defense = maxAttributeLevel;
        
        ApplyAttributeBonuses();
        return true;
    }
    
    public virtual bool IncreaseLuck(int amount = 1)
    {
        if (luck >= maxAttributeLevel) return false;
        
        luck += amount;
        if (luck > maxAttributeLevel) luck = maxAttributeLevel;
        
        ApplyAttributeBonuses();
        return true;
    }

    public virtual void TakeDamage(float _damage)
    {
        if (isInvincible)
            return;
            
        // Apply defense reduction to damage
        float damageReduction = Mathf.Min(defenseStat, _damage * 0.7f); // Max 70% damage reduction
        float finalDamage = _damage - damageReduction;
        
        // Ensure minimum damage of 1
        finalDamage = Mathf.Max(1f, finalDamage);
        
        // Hasarı tam sayıya yuvarla
        float roundedDamage = Mathf.Round(finalDamage);
        currentHealth -= roundedDamage;

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

    // Check if attack is a critical hit
    public virtual bool IsCriticalHit()
    {
        // Random.value 0-1 arası rastgele değer verir
        return UnityEngine.Random.value < criticalChance;
    }

    // Oyuncunun can değerini doğrudan ayarla
    public virtual void SetHealth(float value)
    {
        // Değerleri tam sayıya yuvarla
        float roundedValue = Mathf.Round(value);
        float roundedMaxHealth = Mathf.Round(maxHealth.GetValue());
        
        currentHealth = Mathf.Clamp(roundedValue, 0, roundedMaxHealth);
        
        if (currentHealth <= 0)
            Die();
    }

    // Oyuncunun canını belirtilen miktarda artır (veya azalt)
    public virtual void AddHealth(float amount)
    {
        // Miktarı tam sayıya yuvarla
        float roundedAmount = Mathf.Round(amount);
        SetHealth(currentHealth + roundedAmount);
    }
}
