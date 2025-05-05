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
    
    // Tüm itemleri envantere aktarma
    public void TakeAllItems()
    {
        if (itemsInChest.Count == 0) return;
        
        // Tüm itemleri bir kopya listede tut (foreach içinde listeyi değiştirdiğimiz için)
        List<GameObject> itemsCopy = new List<GameObject>(itemsInChest);
        
        foreach (GameObject itemObj in itemsCopy)
        {
            ItemObject item = itemObj.GetComponent<ItemObject>();
            if (item != null && item.GetItemData() != null)
            {
                // Envantere ekle
                Inventory.instance.AddItem(item.GetItemData());
            }
        }
        
        // Sandıktaki tüm itemleri temizle
        foreach (GameObject item in itemsCopy)
        {
            if (Application.isEditor && !Application.isPlaying)
            {
                DestroyImmediate(item, true);
            }
            else
            {
                Destroy(item);
            }
        }
        
        itemsInChest.Clear();
    }

    // Tek bir item alma fonksiyonu
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
                
                // Editor modunda prefab/asset'leri silme hatası için
                if (Application.isEditor && !Application.isPlaying)
                {
                    DestroyImmediate(item, true);
                }
                else
                {
                    Destroy(item);
                }
            }
        }
    }
}
