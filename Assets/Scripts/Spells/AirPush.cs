using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AirPush : MonoBehaviour
{
    [Header("Air Push Settings")]
    [SerializeField] private float baseDamage = 20f;
    [SerializeField] private float pushForce = 8f; // Daha düşük bir değer
    [SerializeField] private LayerMask enemyLayer;
    private PolygonCollider2D polyCollider;

    [Header("Mind Scaling")]
    [SerializeField] private float mindDamageMultiplier = 0.1f; // 10% damage increase per mind point

    private float direction; // Oyuncunun baktığı yön (1 veya -1)
    private CharacterStats playerStats;
    private float finalDamage;
    private HashSet<Collider2D> hitEnemies = new HashSet<Collider2D>();
    
    // Oyuncunun transform referansı
    private Transform playerTransform;

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

    public void Initialize(float dir, CharacterStats stats)
    {
        direction = dir; // Oyuncunun baktığı yön
        playerStats = stats;
        
        // Oyuncuyu bul (Player komponenti olan)
        Player player = FindObjectOfType<Player>();
        if (player != null)
        {
            playerTransform = player.transform;
        }
        
        // Face the correct direction
        if (direction < 0)
        {
            transform.localScale = new Vector3(-2.5f, 1.5f, 1);
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
                
                // HER ZAMAN OYUNCUDAN UZAKLAŞTIR
                // Oyuncunun yönünü kullanarak düşmanı it
                float pushDirectionX = direction; // Oyuncunun baktığı yönü kullan
                
                // Yeni pozisyonu hesapla - oyuncunun pozisyonundan uzaklaşacak şekilde
                Vector3 targetPosition = collision.transform.position + new Vector3(pushDirectionX * 1.5f, 0, 0);
                
                // Nazik bir hareket için coroutine başlat
                StartCoroutine(SmoothMoveEnemy(collision.transform, targetPosition, 0.3f));
            }
        }
    }
    
    // Düşmanı daha yumuşak hareket ettir
    private IEnumerator SmoothMoveEnemy(Transform enemyTransform, Vector3 targetPosition, float duration)
    {
        if (enemyTransform == null) yield break;
        
        Vector3 startPosition = enemyTransform.position;
        float elapsed = 0;
        
        while (elapsed < duration && enemyTransform != null)
        {
            enemyTransform.position = Vector3.Lerp(startPosition, targetPosition, Mathf.SmoothStep(0, 1, elapsed / duration));
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        if (enemyTransform != null)
        {
            enemyTransform.position = targetPosition;
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
