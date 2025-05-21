using UnityEngine;

public class ElectricDash : MonoBehaviour
{
    [Header("Electric Dash Properties")]
    [SerializeField] private float damage = 25f;
    [SerializeField] private float lifeTime = 0.5f;
    [SerializeField] private float dashDistance = 5f;
    [SerializeField] private LayerMask enemyLayerMask;
    
    private BoxCollider2D boxCollider;
    
    [Header("VFX")]
    //[SerializeField] private ParticleSystem electricEffect;
    //[SerializeField] private float effectRadius = 2.5f;
    
    private Player player;
    private float mindMultiplier = 1f;
    private float spellbookMultiplier = 1f;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        boxCollider = GetComponent<BoxCollider2D>();
        
        player = FindObjectOfType<Player>();
        if (player != null)
        {
            
            
            
            // Scale damage with mind attribute and spellbook
            if (player.stats is PlayerStats playerStats)
            {
                // Apply mind attribute scaling
                mindMultiplier = playerStats.GetTotalElementalDamageMultiplier();
                
                // Apply spellbook damage if available
                if (playerStats.spellbookDamage != null)
                {
                    spellbookMultiplier = playerStats.spellbookDamage.GetValue() / playerStats.baseDamage.GetValue();
                }
            }
            
            // Calculate final damage
            damage *= mindMultiplier * spellbookMultiplier;
            
            // Apply damage to enemies in radius
            ApplyDamageToEnemiesInRadius();
        }
        
        // Activate particle effect
      
        
        // Destroy after lifetime
        Destroy(gameObject, lifeTime);
    }

    private void ApplyDamageToEnemiesInRadius()
    {
        // Find all enemies in radius
        Collider2D[] hitEnemies = Physics2D.OverlapBoxAll(transform.position,boxCollider.size , enemyLayerMask);
        
        foreach (Collider2D enemyCollider in hitEnemies)
        {
            Enemy enemy = enemyCollider.GetComponent<Enemy>();
            if (enemy != null && !player.HasHitEntity(enemy))
            {
                // Mark as hit to prevent multiple hits
                player.MarkEntityAsHit(enemy);
                
                // Apply electric damage
                if (enemy.stats != null)
                {
                    enemy.stats.TakeDamage(damage, CharacterStats.DamageType.Electric);
                    
                    // Apply knockback effect
                    Vector2 knockbackDirection = (enemy.transform.position - transform.position).normalized;
                    enemy.StartCoroutine(enemy.HitKnockback(knockbackDirection * 5f));
                    
                    // Apply electric visual effect
                    EntityFX enemyFX = enemy.GetComponent<EntityFX>();
                    if (enemyFX != null)
                    {
                        enemyFX.StartCoroutine("ElectricFX");
                    }
                }
            }
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        // // Draw the effect radius in editor
        // Gizmos.color = new Color(1f, 1f, 0f, 0.3f); // Yellow for electric
        // Gizmos.DrawWireSphere(transform.position, effectRadius);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
