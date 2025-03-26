using System;
using UnityEngine;
using UnityEngine.Serialization;

public class CharacterStats : MonoBehaviour
{
    [FormerlySerializedAs("damage")] public Stat baseDamage;
    public Stat secondComboDamageMultiplier;
    public Stat thirdComboDamageMultiplier;
    public Stat maxHealth;
    
    public float currentHealth;
    
    public bool isInvincible { get; private set; }

    protected virtual void Start()
    {
        currentHealth = maxHealth.GetValue();
        
        //damage.AddModifier(2);
    }

    public virtual void TakeDamage(float _damage)
    {
        if (isInvincible)
            return;
            
        currentHealth -= _damage;

        if (currentHealth <= 0)
            Die();
    }
    
    public void MakeInvincible (bool _isInvincible) => isInvincible = _isInvincible;

    public virtual void Die()
    {
        // Debug.Log(gameObject.name+ " Died "); - Kaldırıldı
    }
}
