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
            Debug.Log("ItemObject: " + itemData.itemName + " oluşturuldu");
            SetupVisuals();
        }
        
        // Benzersiz ID kontrolü
        if (string.IsNullOrEmpty(uniqueID))
        {
            uniqueID = "item_" + System.Guid.NewGuid().ToString();
            Debug.Log("Yeni item ID oluşturuldu: " + uniqueID);
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
            // Editörde benzersiz ID oluştur
            uniqueID = "item_" + System.Guid.NewGuid().ToString();
        }
        
        // Editor'da ItemData değiştirildiğinde görselleri güncelle
        SetupVisuals();
    }

    private void SetupVisuals()
    {
        if (itemData == null)
        {
            Debug.LogWarning("SetupVisuals: itemData null!");
            return;
        }
        
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
            
        if (spriteRenderer != null && itemData.icon != null)
        {
            spriteRenderer.sprite = itemData.icon;
            Debug.Log("Sprite ayarlandı: " + itemData.itemName);
        }
        else if (spriteRenderer != null)
        {
            Debug.LogWarning("ItemData.icon null: " + itemData.name);
        }
        else
        {
            Debug.LogWarning("SpriteRenderer bulunamadı");
        }
        
        gameObject.name = "Item Object - " + itemData.itemName;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            if (rb != null)
            {
                rb.linearVelocity = velocity;
            }
        }
    }

    // UniqueID döndür
    public string GetUniqueID()
    {
        return uniqueID;
    }
    
    // UniqueID ayarla
    public void SetUniqueID(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            Debug.LogError("SetUniqueID: Geçersiz ID!");
            return;
        }
        
        uniqueID = id;
    }

    // ItemData'yı ayarla
    public void SetItemData(ItemData data)
    {
        if (data == null)
        {
            Debug.LogError("SetItemData: null data verildi!");
            return;
        }
        
        itemData = data;
        Debug.Log("ItemData ayarlandı: " + data.itemName);
        SetupVisuals();
    }

    // ItemData'yı döndür
    public ItemData GetItemData()
    {
        if (itemData == null)
        {
            Debug.LogError("GetItemData: itemData null! " + gameObject.name);
        }
        return itemData;
    }

    public void SetupItem(ItemData _itemData, Vector2 _velocity)
    {
        if (_itemData == null)
        {
            Debug.LogError("SetupItem: null itemData verildi!");
            return;
        }
        
        itemData = _itemData;
        
        if (rb != null)
        {
            rb.linearVelocity = _velocity;
        }
        
        Debug.Log("Item setup edildi: " + _itemData.itemName);
        SetupVisuals();
    }

    public void PickupItem()
    {
        // Inventory'e ekle
        if (Inventory.instance != null && itemData != null)
        {
            Inventory.instance.AddItem(itemData);
            
            // Eğer bu bir Skill Shard ise, SkillManager'a ekle
            if (itemData is SkillShard)
            {
                SkillShard shard = itemData as SkillShard;
                if (SkillManager.Instance != null)
                {
                    SkillManager.Instance.AddShards(shard.GetShardValue());
                    Debug.Log("Collected skill shard: +" + shard.GetShardValue() + " shards");
                }
            }
            
            // Eğer bu bir Collectible ise, özel mesaj göster
            if (itemData is CollectibleData collectible)
            {
                ShowCollectibleFoundMessage(collectible);
            }
            
            // Item'ı toplanan olarak işaretle
            if (ItemCollectionManager.Instance != null)
            {
                ItemCollectionManager.Instance.MarkItemAsCollected(uniqueID);
            }
            
            Destroy(gameObject);
        }
        else
        {
            Debug.LogError("PickupItem: Inventory.instance veya itemData null!");
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
        
        Debug.Log(message);
        
        // TODO: FloatingTextManager ile güzel bir UI mesajı gösterilebilir
        // FloatingTextManager.CreateText(transform.position, message, Color.gold);
    }
}
