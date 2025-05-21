using UnityEngine;
using System.Collections.Generic;

public class AirPush : MonoBehaviour
{
    [Header("Air Push Settings")]
    [SerializeField] private float baseDamage = 20f;
    [SerializeField] private float pushForce = 15f;
    [SerializeField] private float lifeTime = 1.5f;
    //[SerializeField] private float radius = 3f;
    [SerializeField] private LayerMask enemyLayer;
    //[SerializeField] private ParticleSystem airEffect;
    private PolygonCollider2D polyCollider;

    [Header("Mind Scaling")]
    [SerializeField] private float mindDamageMultiplier = 0.1f; // 10% damage increase per mind point

    private float direction;
    private CharacterStats playerStats;
    private float finalDamage;
    private HashSet<Collider2D> hitEnemies = new HashSet<Collider2D>(); // To prevent multiple hits on the same enemy

    private void Awake()
    {
        // Get or add polygon collider
        polyCollider = GetComponent<PolygonCollider2D>();
        if (polyCollider == null)
        {
            polyCollider = gameObject.AddComponent<PolygonCollider2D>();
        }
        
        // Make sure it's a trigger
        polyCollider.isTrigger = true;
    }

    void Start()
    {
        // Destroy after lifetime
        Destroy(gameObject, lifeTime);
    }

    public void Initialize(float dir, CharacterStats stats)
    {
        direction = dir;
        playerStats = stats;
        
        // Face the correct direction
        if (direction < 0)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
        
        // Calculate damage with mind attribute scaling if available
        if (stats != null)
        {
            // Get mind attribute value if it's a PlayerStats
            int mindValue = (stats is PlayerStats) ? ((PlayerStats)stats).Mind : 0;
            
            // Calculate damage with mind scaling
            float mindMultiplier = 1f + (mindValue * mindDamageMultiplier);
            finalDamage = baseDamage * mindMultiplier;
            
            // Apply elemental damage multiplier from stats
            finalDamage *= stats.GetTotalElementalDamageMultiplier();
        }
        else
        {
            finalDamage = baseDamage;
        }
        
        // Set layer to interact with enemyLayer
        //gameObject.layer = LayerMask.NameToLayer("PlayerAttack");
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if the collision is with an enemy and we haven't hit it already
        if (((1 << collision.gameObject.layer) & enemyLayer) != 0 && !hitEnemies.Contains(collision))
        {
            // Add to hit enemies to prevent multiple hits
            hitEnemies.Add(collision);
            
            // Get enemy component
            Enemy enemyComponent = collision.GetComponent<Enemy>();
            if (enemyComponent != null)
            {
                // Apply damage with air damage type
                if (enemyComponent.stats != null)
                {
                    enemyComponent.stats.TakeDamage(finalDamage, CharacterStats.DamageType.Air);
                }
                
                // Calculate push direction
                Vector2 pushDirection = (collision.transform.position - transform.position).normalized;
                
                // Apply push force
                Rigidbody2D enemyRb = collision.GetComponent<Rigidbody2D>();
                if (enemyRb != null)
                {
                    enemyRb.AddForce(pushDirection * pushForce, ForceMode2D.Impulse);
                }
            }
        }
    }
    
    // Editor visualization
    private void OnDrawGizmosSelected()
    {
        // Draw visual representation in editor
        Gizmos.color = new Color(0.5f, 0.8f, 1f, 0.4f); // Light blue
        
        // If we have a polygon collider, draw its shape
        if (polyCollider != null)
        {
            Vector2[] points = polyCollider.points;
            for (int i = 0; i < points.Length; i++)
            {
                Vector2 point = transform.TransformPoint(points[i]);
                Vector2 nextPoint = transform.TransformPoint(points[(i + 1) % points.Length]);
                Gizmos.DrawLine(point, nextPoint);
            }
        }
    }
}
