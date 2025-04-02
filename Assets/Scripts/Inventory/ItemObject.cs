using System;
using UnityEngine;

public class ItemObject : MonoBehaviour, IInteractable
{

    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Vector2 velocity;
    [SerializeField] private ItemData itemData;
    [SerializeField] private InteractionPrompt prompt;
    [SerializeField] private string uniqueID; // Her eşya için benzersiz ID

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
        if (string.IsNullOrEmpty(uniqueID))
        {
            // Editörde benzersiz ID oluştur
            uniqueID = System.Guid.NewGuid().ToString();
        }
        
        GetComponent<SpriteRenderer>().sprite = itemData.icon;
        gameObject.name = "Item Object - " + itemData.itemName;
    }

    public void Interact()
    {
        if (Inventory.instance != null)
        {
            PickupItem();
        }
    }

    public void PickupItem()
    {
        Inventory.instance.AddItem(itemData);
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
}
