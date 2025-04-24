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

    private void Awake()
    {
        boxCollider = GetComponent<BoxCollider2D>();
    }

    private void Start()
    {
        // Eğer bu eşya daha önce toplandıysa, yok et
        if (ItemCollectionManager.Instance != null && 
            ItemCollectionManager.Instance.WasItemCollected(uniqueID))
        {
            Destroy(gameObject);
        }
    }

    private void OnValidate()
    {
       // SetupVisuals();

        if (string.IsNullOrEmpty(uniqueID))
        {
            // Editörde benzersiz ID oluştur
            uniqueID = System.Guid.NewGuid().ToString();
        }
    }

    private void SetupVisuals()
    {
        if (itemData == null)
         return;
        
        GetComponent<SpriteRenderer>().sprite = itemData.icon;
        gameObject.name = "Item Object - " + itemData.itemName;
        
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            rb.linearVelocity = velocity;
        }

      
    }

    // public void Interact()
    // {
    //     if (Inventory.instance != null)
    //     {
    //         PickupItem();
    //     }
    // }

    public void SetupItem(ItemData _itemData , Vector2 _velocity)
    {
        itemData = _itemData;
        rb.linearVelocity = _velocity;
        
        SetupVisuals();
    }

    public void PickupItem()
    {
        // Inventory'e ekle
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
        
        // Item'ı toplanan olarak işaretle
        ItemCollectionManager.Instance.MarkItemAsCollected(uniqueID);
        Destroy(gameObject);
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
        Gizmos.DrawLine(groundCheck.position,
            new Vector3(groundCheck.position.x, groundCheck.position.y - groundCheckDistance));
        
        
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.GetComponent<Player>() != null)
        {
            PickupItem();
        }
    }
}
