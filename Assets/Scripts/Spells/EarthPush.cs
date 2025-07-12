using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EarthPush : MonoBehaviour
{
    [Header("Damage Settings")]
    [SerializeField] private float damage = 20f;
    [SerializeField] private Vector2 knockbackForce = new Vector2(5f, 2f);
     
    
    [Header("References")]
    [SerializeField] private BoxCollider2D damageCollider;
    [SerializeField] private LayerMask enemyLayer;
    
    [Header("Visual Settings")]
    [SerializeField] private Color damageTextColor = new Color(0.2f, 0.8f, 0.2f); // Green color for earth damage
    
    [Header("Knockback Settings")]
    [SerializeField] private string[] ignoreKnockbackTags = { "Boss", "HeavyEnemy" }; // Enemy tags that ignore knockback
    
    private bool isDamageActive;
    private Animator animator;
    
    // Track which enemies have already been hit by this spell
    private HashSet<int> hitEnemies = new HashSet<int>();
    
    private void Awake()
    {
        animator = GetComponent<Animator>();
        
        // Get box collider if not assigned
        if (damageCollider == null)
            damageCollider = GetComponent<BoxCollider2D>();
            
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
            
        // Damage enemies using the collider's size
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
        if (!isDamageActive) return;
        
        // Get player reference for Mind attribute scaling
        Player player = PlayerManager.instance?.player;
        float finalDamage = damage;
        bool isCritical = false;
        
        // Scale damage with player's Mind attribute and spellbook upgrades if available
        if (player != null && player.stats is PlayerStats playerStats)
        {
            float elementalMultiplier = player.stats.GetTotalElementalDamageMultiplier();
            finalDamage = (damage + playerStats.spellbookDamage.GetValue()) * elementalMultiplier;
            
            // Critical hit check
            if (player.stats.IsCriticalHit())
            {
                finalDamage *= 1.5f;
                isCritical = true;
            }
        }
        
        // Get all enemies in the box area
        Collider2D[] hitColliders = Physics2D.OverlapBoxAll(
            transform.position,
            damageCollider.size * transform.localScale,
            transform.eulerAngles.z,
            enemyLayer);
        
        foreach (Collider2D enemyCollider in hitColliders)
        {
            // Get the enemy component
            Enemy enemy = enemyCollider.GetComponent<Enemy>();
            if (enemy != null)
            {
                // Get unique ID for this enemy
                int enemyID = enemy.gameObject.GetInstanceID();
                
                // Skip if this enemy has already been hit
                if (hitEnemies.Contains(enemyID))
                    continue;
                    
                // Add this enemy to the hit list
                hitEnemies.Add(enemyID);
                
                // Calculate knockback direction (away from spell origin)
                Vector2 knockbackDirection = (enemyCollider.transform.position - transform.position).normalized;
                Vector2 finalKnockback = new Vector2(
                    knockbackDirection.x * knockbackForce.x,
                    knockbackForce.y
                );
                
                // Apply earth damage with knockback
                enemy.stats.TakeDamage(finalDamage, CharacterStats.DamageType.Earth);
                
                // Display Earth damage text in green or yellow if critical
                ShowEarthDamageText(finalDamage, enemy.transform.position, isCritical);
                
                // Check if this enemy should ignore knockback
                if (!ShouldIgnoreKnockback(enemy.gameObject))
                {
                    enemy.StartCoroutine(enemy.HitKnockback(finalKnockback));
                }
                
                // Apply visual hit effect
                enemy.entityFX.StartCoroutine("HitFX");
            }
            
            // Also check if there's an EnemyStats component directly
            EnemyStats enemyStats = enemyCollider.GetComponent<EnemyStats>();
            if (enemyStats != null && enemy == null)
            {
                // Get unique ID for this object
                int enemyID = enemyStats.gameObject.GetInstanceID();
                
                // Skip if this object has already been hit
                if (hitEnemies.Contains(enemyID))
                    continue;
                    
                // Add this object to the hit list
                hitEnemies.Add(enemyID);
                
                enemyStats.TakeDamage(finalDamage, CharacterStats.DamageType.Earth);
                
                // Display Earth damage text in green or yellow if critical
                ShowEarthDamageText(finalDamage, enemyCollider.transform.position, isCritical);
            }
        }
    }
    
    private void ShowEarthDamageText(float damageAmount, Vector3 position, bool isCritical = false)
    {
        if (FloatingTextManager.Instance != null)
        {
            Color color = isCritical ? Color.yellow : damageTextColor;
            FloatingTextManager.Instance.ShowCustomText(
                damageAmount.ToString("0"),
                position + Vector3.up * 1.2f,
                color);
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isDamageActive) return;
        
        // Get player reference for spell damage calculation
        Player player = PlayerManager.instance?.player;
        float finalDamage = damage;
        bool isCritical = false;
        
        // Use WeaponDamageManager for spell damage calculation
        if (player != null && player.stats is PlayerStats playerStats)
        {
            // Get spell damage from spellbook weapon with elemental multiplier
            finalDamage = WeaponDamageManager.GetSpellDamage(playerStats);
            
            // Critical hit check
            if (playerStats.IsCriticalHit())
            {
                finalDamage *= 1.5f;
                isCritical = true;
            }
        }
        
        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy != null)
        {
            // Get unique ID for this enemy
            int enemyID = enemy.gameObject.GetInstanceID();
                
            // Skip if this enemy has already been hit
            if (hitEnemies.Contains(enemyID))
                return;
                
            // Add this enemy to the hit list
            hitEnemies.Add(enemyID);
            
            // Calculate knockback direction (away from spell origin)
            Vector2 knockbackDirection = (other.transform.position - transform.position).normalized;
            Vector2 finalKnockback = new Vector2(
                knockbackDirection.x * knockbackForce.x,
                knockbackForce.y
            );
            
            // Apply earth damage with knockback
            enemy.stats.TakeDamage(finalDamage, CharacterStats.DamageType.Earth);
            
            // Display Earth damage text in green or yellow if critical
            ShowEarthDamageText(finalDamage, enemy.transform.position, isCritical);
            
            // Check if this enemy should ignore knockback
            if (!ShouldIgnoreKnockback(enemy.gameObject))
            {
                enemy.StartCoroutine(enemy.HitKnockback(finalKnockback));
            }
            
            // Apply visual hit effect
            enemy.entityFX.StartCoroutine("HitFX");
        }
    }
    
    // Helper method to check if an enemy should ignore knockback
    private bool ShouldIgnoreKnockback(GameObject enemy)
    {
        if (enemy == null) return false;
        
        // Check if the enemy's tag is in the ignore list
        string enemyTag = enemy.tag;
        foreach (string ignoreTag in ignoreKnockbackTags)
        {
            if (enemyTag == ignoreTag)
                return true;
        }
        
        return false;
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.5f, 0.3f, 0.1f, 0.3f); // Brown color for earth
        
        // Draw box gizmo if collider is available
        if (damageCollider != null)
        {
            // Calculate box position and size
            Vector3 boxPosition = transform.position;
            Vector3 boxSize = damageCollider.size * transform.localScale;
            
            // Draw the box using current transform rotation
            Matrix4x4 originalMatrix = Gizmos.matrix;
            Gizmos.matrix = Matrix4x4.TRS(boxPosition, transform.rotation, Vector3.one);
            Gizmos.DrawWireCube(Vector3.zero, boxSize);
            Gizmos.matrix = originalMatrix;
        }
        
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
