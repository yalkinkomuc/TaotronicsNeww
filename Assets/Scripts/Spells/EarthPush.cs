using UnityEngine;
using System.Collections;

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
    
    private bool isDamageActive;
    private Animator animator;
    
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
        if (damageCollider == null) return;
        
        // Calculate box position and size
        Vector2 boxPosition = transform.position;
        Vector2 boxSize = damageCollider.size * transform.localScale; // Adjust for scale
        float boxAngle = transform.eulerAngles.z; // Use Z rotation
        
        // Find all enemies in the damage box
        Collider2D[] hitEnemies = Physics2D.OverlapBoxAll(
            boxPosition, 
            boxSize, 
            boxAngle, 
            enemyLayer);
        
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
                
                // Display Earth damage text in green
                ShowEarthDamageText(damage, enemy.transform.position);
                
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
                
                // Display Earth damage text in green
                ShowEarthDamageText(damage, enemyCollider.transform.position);
            }
        }
    }
    
    private void ShowEarthDamageText(float damageAmount, Vector3 position)
    {
        // Show the damage text with Earth color (green)
        if (FloatingTextManager.Instance != null)
        {
            // Option 1: Use custom text method if available
            FloatingTextManager.Instance.ShowCustomText(
                damageAmount.ToString("0"), // Round to nearest integer
                position + Vector3.up * 1.2f, // Position above enemy
                damageTextColor);
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
            
            // Display Earth damage text in green
            ShowEarthDamageText(damage, enemy.transform.position);
            
            // Then apply the knockback using the HitKnockback coroutine
            enemy.StartCoroutine(enemy.HitKnockback(finalKnockback));
            
            // Apply visual hit effect
            enemy.entityFX.StartCoroutine("HitFX");
        }
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
