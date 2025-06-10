using UnityEngine;
using System.Collections;

public class ElectricDash : MonoBehaviour
{
    [Header("Electric Dash Properties")]
    [SerializeField] private float damage = 25f;
    [SerializeField] private float lifeTime = 0.5f;
    [SerializeField] private float checkFrequency = 0.05f; // Her 0.05 saniyede bir kontrol
    [SerializeField] private LayerMask enemyLayerMask;
    [SerializeField] private bool useTriggerDetection = true; // Trigger tabanlı algılama kullan
    
    private BoxCollider2D boxCollider;
    
    [Header("VFX")]
    //[SerializeField] private ParticleSystem electricEffect;
    //[SerializeField] private float effectRadius = 2.5f;
    
    private Player player;
    private float mindMultiplier = 1f;
    private float spellbookMultiplier = 1f;
    private bool isActive = false;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        
        // Trigger detection için collider'ı ayarla
        if (useTriggerDetection && boxCollider != null)
        {
            boxCollider.isTrigger = true;
        }
        
        player = PlayerManager.instance.player;
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
            
            // Start continuous damage checking
            isActive = true;
            
            // Trigger kullanmıyorsak sürekli kontrol yap
            if (!useTriggerDetection)
            {
                StartCoroutine(ContinuousEnemyCheck());
            }
        }
        
        // Activate particle effect
        
        // Destroy after lifetime
        Destroy(gameObject, lifeTime);
    }

    // Trigger tabanlı düşman algılama
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isActive || !useTriggerDetection) return;
        
        // Layer mask kontrolü
        if (((1 << other.gameObject.layer) & enemyLayerMask) == 0) return;
        
        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy != null && !player.HasHitEntity(enemy))
        {
            ApplyDamageToEnemy(enemy);
        }
    }

    // Trigger alanında kalan düşmanlar için sürekli kontrol
    private void OnTriggerStay2D(Collider2D other)
    {
        if (!isActive || !useTriggerDetection) return;
        
        // Layer mask kontrolü
        if (((1 << other.gameObject.layer) & enemyLayerMask) == 0) return;
        
        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy != null && !player.HasHitEntity(enemy))
        {
            ApplyDamageToEnemy(enemy);
        }
    }

    private IEnumerator ContinuousEnemyCheck()
    {
        while (isActive && gameObject != null)
        {
            ApplyDamageToEnemiesInRadius();
            yield return new WaitForSeconds(checkFrequency);
        }
    }

    private void ApplyDamageToEnemiesInRadius()
    {
        if (!isActive) return;
        
        // Find all enemies in radius
        Collider2D[] hitEnemies = Physics2D.OverlapBoxAll(transform.position, boxCollider.size, 0, enemyLayerMask);
        
        foreach (Collider2D enemyCollider in hitEnemies)
        {
            Enemy enemy = enemyCollider.GetComponent<Enemy>();
            if (enemy != null && !player.HasHitEntity(enemy))
            {
                ApplyDamageToEnemy(enemy);
            }
        }
    }
    
    private void ApplyDamageToEnemy(Enemy enemy)
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
    
    private void OnDestroy()
    {
        isActive = false;
    }
    
    private void OnDrawGizmosSelected()
    {
        if (boxCollider != null)
        {
            // Draw the effect radius in editor
            Gizmos.color = new Color(1f, 1f, 0f, 0.3f); // Yellow for electric
            Gizmos.DrawWireCube(transform.position, boxCollider.size);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
