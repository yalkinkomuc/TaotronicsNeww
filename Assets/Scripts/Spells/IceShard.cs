using System;
using UnityEngine;

public class IceShard : MonoBehaviour
{
    [Header("Damage Settings")]
    [SerializeField] private int damage = 15; // Base damage value
    
    [Header("Ground Check")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckDistance = 10f;
    
    private BoxCollider2D iceCollider;
    private bool canDealDamage = false;
    private bool isGroundBelow = false;

    // Method to set the ground layer from outside
    public void SetGroundLayer(LayerMask layer)
    {
        groundLayer = layer;
        // Check ground again with the new layer
        CheckGroundBelow();
        
        // Destroy if no ground below
        if (!isGroundBelow)
        {
            Destroy(gameObject);
        }
    }

    private void Awake()
    {
        iceCollider = GetComponent<BoxCollider2D>();
        if (iceCollider == null)
        {
            iceCollider = gameObject.AddComponent<BoxCollider2D>();
            iceCollider.isTrigger = true;
        }
        // Başlangıçta kesinlikle kapalı olsun
        iceCollider.enabled = false;
        canDealDamage = false;
    }

    private void Start()
    {
        // Start'ta da kapalı olduğundan emin olalım
        if (iceCollider != null)
        {
            iceCollider.enabled = false;
            canDealDamage = false;
        }
        
        // Check if ground is below
        CheckGroundBelow();
        
        // If no ground below, destroy this ice shard
        if (!isGroundBelow)
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        Player player = PlayerManager.instance.player;
        float multipliedDamage = player.stats.GetTotalElementalDamageMultiplier();
        
        Debug.Log(multipliedDamage);
    }

    private void CheckGroundBelow()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, groundLayer);
        isGroundBelow = (hit.collider != null);
        
        if (!isGroundBelow)
        {
            Debug.Log("No ground below ice shard, destroying it");
        }
    }

    // Bu metodu animasyon event ile çağıracağız
    public void EnableCollider()
    {
        if (iceCollider == null)
        {
            iceCollider = GetComponent<BoxCollider2D>();
        }
        
        canDealDamage = true;
        iceCollider.enabled = true;
    }

    // Animasyon event'i tarafından çağrılacak
    public void DestroyIceShard()
    {
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!canDealDamage) return;

        // Get player references in two ways
        Player playerFromManager = PlayerManager.instance?.player;
        Player playerFromScene = GameObject.FindObjectOfType<Player>();
        
        Debug.Log($"[IceShard DEBUG] PlayerManager.player: {playerFromManager}, FindObjectOfType<Player>: {playerFromScene}");
        
        // Compare Mind values from both
        int mindFromManager = playerFromManager?.stats?.Mind ?? -1;
        int mindFromScene = playerFromScene?.stats?.Mind ?? -1;
        Debug.Log($"[IceShard DEBUG] Mind (Manager): {mindFromManager}, Mind (Scene): {mindFromScene}");
        
        // Use the player with highest Mind value
        Player playerToUse = (mindFromManager >= mindFromScene) ? playerFromManager : playerFromScene;
        
        if (collision.CompareTag("Enemy"))
        {
            Enemy enemy = collision.GetComponent<Enemy>();
            if (enemy != null)
            {
                float elementalMultiplier = 1f;
                int mindValue = 0;
                if (playerToUse != null && playerToUse.stats != null)
                {
                    elementalMultiplier = playerToUse.stats.GetTotalElementalDamageMultiplier();
                    mindValue = playerToUse.stats.Mind;
                    Debug.Log($"[IceShard DEBUG] Using player with Mind: {mindValue}");
                }

                float finalDamage = damage * elementalMultiplier;
                Debug.Log($"[IceShard] Mind: {mindValue}, Multiplier: {elementalMultiplier}, Base Damage: {damage}, Final Damage: {finalDamage}");

                enemy.stats.TakeDamage(finalDamage, CharacterStats.DamageType.Ice);

                if (FloatingTextManager.Instance != null)
                {
                    Vector3 textPosition = enemy.transform.position + Vector3.up * 1.5f;
                    FloatingTextManager.Instance.ShowMagicDamageText(finalDamage, textPosition);
                }

                enemy.ApplyIceEffect();
                DestroyIceShard();
            }
        }
    }
} 