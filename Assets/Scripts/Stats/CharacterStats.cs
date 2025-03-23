using System;
using UnityEngine;

public class CharacterStats : MonoBehaviour
{
    public Stat damage;
    public Stat maxHealth;
    
    public float currentHealth;
    
    public bool isInvincible { get; private set; }

    protected virtual void Start()
    {
        currentHealth = maxHealth.GetValue();
        
        damage.AddModifier(2);
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
