using System;
using UnityEngine;

public class ItemObject : MonoBehaviour
{

    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Vector2 velocity;
    [SerializeField] private ItemData itemData;
    [SerializeField] private InteractionPrompt prompt;
    [SerializeField] private string uniqueID; // Her eşya için benzersiz ID
    
    
    [Header("Ground Check")]
    [SerializeField] public Transform groundCheck;
    [SerializeField] protected float groundCheckDistance;
    [SerializeField] protected LayerMask whatIsGround;
    
    
    private BoxCollider2D boxCollider;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // ItemData'yı doğrula
        if (itemData == null)
        {
            Debug.LogError("ItemData atanmamış: " + gameObject.name);
        }
        else
        {
            //Debug.Log("ItemObject: " + itemData.itemName + " oluşturuldu");
            SetupVisuals();
        }
        
        // Benzersiz ID kontrolü
        if (string.IsNullOrEmpty(uniqueID))
        {
            uniqueID = "item_" + Guid.NewGuid();
            //Debug.Log("Yeni item ID oluşturuldu: " + uniqueID);
        }
    }

    private void Start()
    {
        // Eğer bu eşya daha önce toplandıysa, yok et
        if (ItemCollectionManager.Instance != null && 
            ItemCollectionManager.Instance.WasItemCollected(uniqueID))
        {
            Destroy(gameObject);
        }
        
        // Item verilerini görsel olarak ayarla
        SetupVisuals();
    }

    private void OnValidate()
    {
        if (string.IsNullOrEmpty(uniqueID))
        {
            // Generate unique ID in editor
            uniqueID = "item_" + System.Guid.NewGuid().ToString();
        }
        
        // Update visuals when ItemData changes in editor
        SetupVisuals();
    }

    private void SetupVisuals()
    {
        if (itemData == null) return;
        
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
            
        if (spriteRenderer != null && itemData.icon != null)
        {
            spriteRenderer.sprite = itemData.icon;
            gameObject.name = "Item Object - " + itemData.itemName;
        }
    }

    // Get unique ID
    public string GetUniqueID()
    {
        return uniqueID;
    }
    
    // Set unique ID
    public void SetUniqueID(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            
            return;
        }
        
        uniqueID = id;
    }

    // Set ItemData
    public void SetItemData(ItemData data)
    {
        if (data == null)
        {
            return;
        }
        
        itemData = data;
        SetupVisuals();
    }

    // Get ItemData
    public ItemData GetItemData()
    {
        if (itemData == null)
        {
            Debug.LogError("GetItemData: itemData is null! " + gameObject.name);
        }
        return itemData;
    }

    public void SetupItem(ItemData _itemData, Vector2 _velocity)
    {
        if (_itemData == null)
        {
            Debug.LogError("SetupItem: null itemData provided!");
            return;
        }
        
        itemData = _itemData;
        
        if (rb != null)
        {
            rb.linearVelocity = _velocity;
        }
        
        SetupVisuals();
    }

    public void PickupItem()
    {
        // Envantere ekle
        if (Inventory.instance != null && itemData != null)
        {
            Inventory.instance.AddItem(itemData);
            
            // If this is a Skill Shard, add to SkillManager
            if (itemData is SkillShard shard)
            {
                if (SkillManager.Instance != null)
                {
                    SkillManager.Instance.AddShards(shard.GetShardValue());
                }
            }
            
            // If this is a Collectible, show special message
            if (itemData is CollectibleData collectible)
            {
                ShowCollectibleFoundMessage(collectible);
            }
            
            // Mark item as collected
            if (ItemCollectionManager.Instance != null)
            {
                ItemCollectionManager.Instance.MarkItemAsCollected(uniqueID);
            }
            
            Destroy(gameObject);
        }
        else
        {
            Debug.LogError("PickupItem: Inventory.instance or itemData is null!");
        }
    }

    public void ShowInteractionPrompt()
    {
        if (prompt != null)
            prompt.ShowPrompt();
    }

    public void HideInteractionPrompt()
    {
        if (prompt != null)
            prompt.HidePrompt();
    }

    protected virtual void OnDrawGizmos()
    {
        if (groundCheck != null)
        {
            Gizmos.DrawLine(groundCheck.position,
                new Vector3(groundCheck.position.x, groundCheck.position.y - groundCheckDistance));
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.GetComponent<Player>() != null)
        {
            PickupItem();
        }
    }
    
    private void ShowCollectibleFoundMessage(CollectibleData collectible)
    {
        string message = $"✨ Collectible Found! ✨\n{collectible.itemName}";
        
        if (!string.IsNullOrEmpty(collectible.discoveryLocation))
        {
            message += $"\nDiscovered in: {collectible.discoveryLocation}";
        }
        
        if (collectible.isRareCollectible)
        {
            message = $"🌟 RARE {message} 🌟";
        }
        
        // TODO: Show beautiful UI message with FloatingTextManager
        // FloatingTextManager.CreateText(transform.position, message, Color.gold);
    }
}
