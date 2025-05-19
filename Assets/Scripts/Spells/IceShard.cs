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
        
       
    }

    private void CheckGroundBelow()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, groundLayer);
        isGroundBelow = (hit.collider != null);
        
      
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

        // Get player reference from PlayerManager
        Player player = PlayerManager.instance?.player;
        
        if (collision.CompareTag("Enemy"))
        {
            Enemy enemy = collision.GetComponent<Enemy>();
            if (enemy != null)
            {
                float spellbookBonus = 0f;
                float elementalMultiplier = 1f;
                int mindValue = 0;
                if (player != null && player.stats is PlayerStats playerStats)
                {
                    elementalMultiplier = player.stats.GetTotalElementalDamageMultiplier();
                    mindValue = player.stats.Mind;
                    spellbookBonus = playerStats.spellbookDamage.GetValue();
                }

                // KRİTİK KONTROLÜ
                bool isCritical = false;
                float critMultiplier = 1f;
                if (player != null && player.stats != null && player.stats.IsCriticalHit())
                {
                    critMultiplier = 1.5f;
                    isCritical = true;
                }

                float finalDamage = (damage + spellbookBonus) * elementalMultiplier * critMultiplier;

                enemy.stats.TakeDamage(finalDamage, CharacterStats.DamageType.Ice);

                if (FloatingTextManager.Instance != null)
                {
                    Vector3 textPosition = enemy.transform.position + Vector3.up * 1.5f;
                    if (isCritical)
                        FloatingTextManager.Instance.ShowCustomText(finalDamage.ToString("0"), textPosition, Color.yellow);
                    else
                        FloatingTextManager.Instance.ShowMagicDamageText(finalDamage, textPosition);
                }

                enemy.ApplyIceEffect();
                DestroyIceShard();
            }
        }
    }
} 