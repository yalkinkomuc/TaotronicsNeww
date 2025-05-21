using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AirPush : MonoBehaviour
{
    [Header("Air Push Settings")]
    [SerializeField] private float baseDamage = 20f;
    //[SerializeField] private float pushForce = 8f; // Daha düşük bir değer
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private float minPushDistance = 3f; // Minimum itme mesafesi
    [SerializeField] private float extraPushBeyondBoundary = 1.0f; // Sınırın ötesine ekstra itme mesafesi
    private PolygonCollider2D polyCollider;
    private float effectWidth = 2.0f; // Varsayılan genişlik (collider yoksa)

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
        
        // Efektin genişliğini hesapla (X sınırlarını bul)
        CalculateEffectWidth();
    }
    
    private void CalculateEffectWidth()
    {
        // Eğer PolygonCollider2D varsa onun sınırlarını kullan
        if (polyCollider != null && polyCollider.points.Length > 0)
        {
            // X koordinatlarındaki min ve max değerleri bul
            float minX = float.MaxValue;
            float maxX = float.MinValue;
            
            foreach (Vector2 point in polyCollider.points)
            {
                if (point.x < minX) minX = point.x;
                if (point.x > maxX) maxX = point.x;
            }
            
            // Genişliği hesapla
            effectWidth = maxX - minX;
        }
        else
        {
            // Eğer collider yoksa varsayılan değeri kullan
            effectWidth = 2.0f;
        }
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
        
        // Calculate damage based on player's spellbook damage and mind attribute
        if (stats != null && stats is PlayerStats playerStatsRef)
        {
            // Get base damage from spellbook
            finalDamage = baseDamage;
            
            // Add spellbook damage if available
            if (playerStatsRef.spellbookDamage != null)
            {
                finalDamage = playerStatsRef.spellbookDamage.GetValue();
            }
            
            // Apply elemental damage multiplier from stats (includes mind scaling)
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
                
                // HER ZAMAN OYUNCUDAN UZAKLAŞTIR VE PREFABİN DIŞINA İT
                float pushDirectionX = direction; // Oyuncunun baktığı yönü kullan
                
                // Düşmanın prefaba göre göreceli pozisyonunu bul
                Vector3 localEnemyPos = transform.InverseTransformPoint(collision.transform.position);
                
                // Düşmanın prefab sınırının dışına çıkması için gereken mesafeyi hesapla
                float pushDistance = CalculatePushDistance(localEnemyPos, pushDirectionX);
                
                // Yeni pozisyonu hesapla - prefabin sınırlarının dışına
                Vector3 targetPosition = collision.transform.position + new Vector3(pushDirectionX * pushDistance, 0, 0);
                
                // Nazik bir hareket için coroutine başlat
                StartCoroutine(SmoothMoveEnemy(collision.transform, targetPosition, 0.3f));
            }
        }
    }
    
    // Düşmanı prefabin dışına itmek için gereken mesafeyi hesapla
    private float CalculatePushDistance(Vector3 localEnemyPos, float pushDir)
    {
        // Prefabin kenarına olan mesafeyi hesapla
        float distanceToBoundary;
        
        if (pushDir > 0) // Sağa doğru itiyorsak
        {
            // Prefabin sağ kenarını bul (scale'i de dikkate al)
            float rightEdge = effectWidth / 2 * Mathf.Abs(transform.localScale.x);
            distanceToBoundary = rightEdge - localEnemyPos.x;
        }
        else // Sola doğru itiyorsak
        {
            // Prefabin sol kenarını bul (scale'i de dikkate al)
            float leftEdge = -effectWidth / 2 * Mathf.Abs(transform.localScale.x);
            distanceToBoundary = localEnemyPos.x - leftEdge;
        }
        
        // Mesafe negatifse, düşman zaten sınırın dışında demektir
        distanceToBoundary = Mathf.Max(0, distanceToBoundary);
        
        // Minimum itme mesafesi ve sınırın ötesine ekstra itme mesafesini ekle
        return Mathf.Max(minPushDistance, distanceToBoundary + extraPushBeyondBoundary);
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
            
            // Ayrıca sınırları göster
            float width = effectWidth;
            if (width > 0)
            {
                Vector3 leftEdge = transform.TransformPoint(new Vector3(-width/2, 0, 0));
                Vector3 rightEdge = transform.TransformPoint(new Vector3(width/2, 0, 0));
                
                // Sınırları belirt
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(leftEdge + Vector3.up, leftEdge + Vector3.down);
                Gizmos.DrawLine(rightEdge + Vector3.up, rightEdge + Vector3.down);
                
                // İtme mesafesini belirt
                Gizmos.color = Color.red;
                Gizmos.DrawLine(rightEdge, rightEdge + Vector3.right * extraPushBeyondBoundary);
                Gizmos.DrawLine(leftEdge, leftEdge + Vector3.left * extraPushBeyondBoundary);
            }
        }
    }
}
