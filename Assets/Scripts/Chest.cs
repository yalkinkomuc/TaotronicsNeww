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

    public void CloseChest()
    {
        isOpen = false;
        // Kapanma animasyonu kaldırıldı (Close trigger yok)
        
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
            if (itemObj == null) continue;
            
            ItemObject item = itemObj.GetComponent<ItemObject>();
            if (item != null && item.GetItemData() != null)
            {
                // Envantere ekle
                Inventory.instance.AddItem(item.GetItemData());
                
                // Listeyi güncelle
                itemsInChest.Remove(itemObj);
                
                // GameObject'i pasif yap - DESTROY KULLANMA
                itemObj.SetActive(false);
            }
        }
        
        // Listeyi temizle
        itemsInChest.Clear();
    }

    // Tek bir item alma fonksiyonu
    public void RemoveItem(GameObject item)
    {
        if (item == null || !itemsInChest.Contains(item)) return;

        ItemObject itemObj = item.GetComponent<ItemObject>();
        if (itemObj != null && Inventory.instance != null)
        {
            // Envantere ekle
            Inventory.instance.AddItem(itemObj.GetItemData());
            
            // Sandıktan çıkar
            itemsInChest.Remove(item);
            
            // GameObject'i pasif yap - DESTROY KULLANMA
            item.SetActive(false);
        }
    }
}
