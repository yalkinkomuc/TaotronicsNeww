using UnityEngine;
using System.Collections;

public class EarthPush : MonoBehaviour
{
    [Header("Damage Settings")]
    [SerializeField] private float damage = 20f;
    [SerializeField] private Vector2 knockbackForce = new Vector2(5f, 2f);
    [SerializeField] private float damageRadius = 2f;
    
    [Header("References")]
    [SerializeField] private Collider2D damageCollider;
    [SerializeField] private LayerMask enemyLayer;
    
    private bool isDamageActive;
    private Animator animator;
    
    private void Awake()
    {
        animator = GetComponent<Animator>();
        
        // Ensure collider is initially disabled
        if (damageCollider != null)
            damageCollider.enabled = false;
        
        isDamageActive = false;
    }
    
    // Called from animation event
    public void ActivateDamage()
    {
        isDamageActive = true;
        
        if (damageCollider != null)
            damageCollider.enabled = true;
            
        // Alternative approach using Physics2D.OverlapCircle
        DealDamageInArea();
    }
    
    // Called from animation event
    public void DeactivateDamage()
    {
        isDamageActive = false;
        
        if (damageCollider != null)
            damageCollider.enabled = false;
    }
    
    private void DealDamageInArea()
    {
        // Find all enemies in the damage radius
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, damageRadius, enemyLayer);
        
        foreach (Collider2D enemyCollider in hitEnemies)
        {
            // Get the enemy component and apply earth damage
            Enemy enemy = enemyCollider.GetComponent<Enemy>();
            if (enemy != null)
            {
                // Calculate knockback direction (away from spell origin)
                Vector2 knockbackDirection = (enemyCollider.transform.position - transform.position).normalized;
                Vector2 finalKnockback = new Vector2(
                    knockbackDirection.x * knockbackForce.x,
                    knockbackForce.y
                );
                
                // Apply earth damage with knockback
                // First apply the damage directly to the enemy stats
                enemy.stats.TakeDamage(damage, CharacterStats.DamageType.Earth);
                
                // Then apply the knockback using the HitKnockback coroutine
                enemy.StartCoroutine(enemy.HitKnockback(finalKnockback));
                
                // Apply visual hit effect
                enemy.entityFX.StartCoroutine("HitFX");
            }
            
            // Also check if there's an EnemyStats component directly
            EnemyStats enemyStats = enemyCollider.GetComponent<EnemyStats>();
            if (enemyStats != null && enemy == null)
            {
                enemyStats.TakeDamage(damage, CharacterStats.DamageType.Earth);
            }
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isDamageActive) return;
        
        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy != null)
        {
            // Calculate knockback direction (away from spell origin)
            Vector2 knockbackDirection = (other.transform.position - transform.position).normalized;
            Vector2 finalKnockback = new Vector2(
                knockbackDirection.x * knockbackForce.x,
                knockbackForce.y
            );
            
            // Apply earth damage with knockback
            // First apply the damage directly to the enemy stats
            enemy.stats.TakeDamage(damage, CharacterStats.DamageType.Earth);
            
            // Then apply the knockback using the HitKnockback coroutine
            enemy.StartCoroutine(enemy.HitKnockback(finalKnockback));
            
            // Apply visual hit effect
            enemy.entityFX.StartCoroutine("HitFX");
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.5f, 0.3f, 0.1f, 0.3f); // Brown color for earth
        Gizmos.DrawWireSphere(transform.position, damageRadius);
    }
    
    // Called by animation event to destroy the spell
    public void DestroySpell()
    {
        // Disable collider and any effects before destroying
        if (damageCollider != null)
            damageCollider.enabled = false;
            
        // You can add particle effects or other visual feedback here
        
        // Finally destroy the game object
        Destroy(gameObject);
    }
}
