using UnityEngine;

public class IceShard : MonoBehaviour
{
    [Header("Damage Settings")]
    [SerializeField] private int damage = 15; // Increased default damage
    [SerializeField] private bool useRandomDamage = false;
    [SerializeField] private int minDamage = 12;
    [SerializeField] private int maxDamage = 20;
    
    [Header("Ground Check")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckDistance = 10f;
    
    private BoxCollider2D iceCollider;
    private bool canDealDamage = false;
    private bool isGroundBelow = false;
    private int actualDamage;

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
        
        // Calculate actual damage
        actualDamage = useRandomDamage ? Random.Range(minDamage, maxDamage + 1) : damage;
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

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!canDealDamage) return;

        if (other.GetComponent<Enemy>() != null)
        {
            if (other.TryGetComponent<Enemy>(out Enemy enemy))
            {
                enemy.Damage();
                enemy.GetComponent<CharacterStats>()?.TakeDamage(actualDamage);
                
                // Display magic damage text
                if (FloatingTextManager.Instance != null)
                {
                    Vector3 textPosition = enemy.transform.position + Vector3.up * 1.5f;
                    FloatingTextManager.Instance.ShowMagicDamageText(actualDamage, textPosition);
                }
            }
        }
    }
} 