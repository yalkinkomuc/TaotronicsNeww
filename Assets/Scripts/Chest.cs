using System.Collections.Generic;
using UnityEngine;

public class Chest : MonoBehaviour, IInteractable
{
    [Header("Chest Settings")]
    public List<GameObject> itemsInChest = new List<GameObject>();
    [SerializeField] private Animator animator;
    [SerializeField] private InteractionPrompt prompt;
    
    private bool isOpen = false;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
            
        if (prompt == null)
            prompt = GetComponentInChildren<InteractionPrompt>();
    }

    public void Interact()
    {
        if (!isOpen)
        {
            OpenChest();
        }
        else
        {
            CloseChest();
        }
    }

    private void OpenChest()
    {
        isOpen = true;
        animator?.SetTrigger("Open");
        
        // Sandık UI'ını aç
        UI_ChestInventory.Instance.OpenChest(this);
    }

    private void CloseChest()
    {
        isOpen = false;
        animator?.SetTrigger("Close");
        
        // Sandık UI'ını kapat
        UI_ChestInventory.Instance.CloseChest();
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

    // Item alma fonksiyonu
    public void RemoveItem(GameObject item)
    {
        if (itemsInChest.Contains(item))
        {
            ItemObject itemObj = item.GetComponent<ItemObject>();
            if (itemObj != null && Inventory.instance != null)
            {
                // Envantere ekle
                Inventory.instance.AddItem(itemObj.GetItemData());
                
                // Sandıktan çıkar
                itemsInChest.Remove(item);
                Destroy(item);
            }
        }
    }
}
