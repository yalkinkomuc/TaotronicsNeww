using UnityEngine;

public class FireballSpell : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [Header("Fireball Settings")]
    [SerializeField] private float speed = 15f;
    [SerializeField] private float lifetime = 5f;
    [SerializeField] private float damage = 20f;
   
    
    [Header("Effects")]
    [SerializeField] private GameObject groundImpactPrefab;
    [SerializeField] private GameObject enemyImpactPrefab;
    
    private Rigidbody2D rb;
    private bool hasHit = false;
    private PlayerStats playerStats;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    
    void Start()
    {
        // Destroy after lifetime
        Destroy(gameObject, lifetime);
    }
    
    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void Initialize(float direction, PlayerStats stats)
    {
        // Set movement direction
        rb.linearVelocity = new Vector2(direction * speed, 0);
        
        // Store player stats for damage calculation
        playerStats = stats;
        
        // Set the correct facing direction
        if (direction < 0)
        {
            transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
        }
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (hasHit) return;
        
        // Check for collision with ground
        if (other.CompareTag("Ground"))
        {
            hasHit = true;
            SpawnImpactEffect(groundImpactPrefab, other);
            Destroy(gameObject);
        }
        // Check for collision with enemy
        else if (other.CompareTag("Enemy"))
        {
            EnemyStats enemyStats = other.GetComponent<EnemyStats>();
            if (enemyStats != null)
            {
                hasHit = true;
                
                // Calculate damage using WeaponDamageManager
                float finalDamage = CalculateDamage();
                
                // Apply damage with fire type
                enemyStats.TakeDamage(finalDamage, CharacterStats.DamageType.Fire);
                
                // Apply burn effect
                EntityFX enemyFX = other.GetComponent<EntityFX>();
                if (enemyFX != null)
                {
                    enemyFX.StartCoroutine("BurnFX");
                }
                
                // Spawn hit effect
                SpawnImpactEffect(enemyImpactPrefab, other);
                
                Destroy(gameObject);
            }
        }
    }

    private float CalculateDamage()
    {
        if (playerStats == null) return damage;
        
        // Use WeaponDamageManager to get spell damage from spellbook weapon
        float finalDamage = WeaponDamageManager.GetSpellDamage(playerStats);
        
        // Check for critical hit
        if (playerStats.IsCriticalHit())
        {
            finalDamage *= playerStats.criticalDamage;
            
        }
        
        return finalDamage;
    }
    
    private void SpawnImpactEffect(GameObject effectPrefab, Collider2D collision)
    {
        if (effectPrefab == null) return;
        
        // Create impact effect at collision point
        Vector2 contactPoint = collision.ClosestPoint(transform.position);
        GameObject impactEffect = Instantiate(effectPrefab, contactPoint, Quaternion.identity);
        
        // Auto-destroy impact effect after 2 seconds
        Destroy(impactEffect, 2f);
    }
}
