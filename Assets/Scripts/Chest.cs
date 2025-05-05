using System.Collections.Generic;
using UnityEngine;

public class Chest : MonoBehaviour, IInteractable
{
    [Header("Chest Settings")]
    public List<GameObject> itemsInChest = new List<GameObject>();
    [SerializeField] private Animator animator;
    [SerializeField] private InteractionPrompt prompt;
    
    // Yardımcı liste - Geçici olarak alınmış itemların takibi için
    private List<GameObject> removedItems = new List<GameObject>();
    
    private bool isOpen = false;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
            
        if (prompt == null)
            prompt = GetComponentInChildren<InteractionPrompt>();
        
        // Sandık itemlerini kontrol et
        ValidateChestItems();
    }
    
    // Sandık içeriğini kontrol et
    private void ValidateChestItems()
    {
        if (itemsInChest == null)
        {
            Debug.LogError("itemsInChest null!");
            itemsInChest = new List<GameObject>();
            return;
        }
        
        // Null olan ya da ItemObject bileşeni olmayan itemleri logla
        for (int i = 0; i < itemsInChest.Count; i++)
        {
            if (itemsInChest[i] == null)
            {
                Debug.LogWarning("Sandıkta null item var (index: " + i + ")");
                continue;
            }
            
            ItemObject item = itemsInChest[i].GetComponent<ItemObject>();
            if (item == null)
            {
                Debug.LogWarning("Sandıktaki item'da ItemObject bileşeni yok (index: " + i + ")");
                continue;
            }
            
            if (item.GetItemData() == null)
            {
                Debug.LogWarning("Sandıktaki item'ın ItemData'sı null (index: " + i + ")");
            }
            else
            {
                Debug.Log("Geçerli item: " + item.GetItemData().itemName);
            }
        }
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
        
        // İtem durumunu kontrol et
        Debug.Log("Sandık açılıyor, item sayısı: " + itemsInChest.Count);
        
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
        Debug.Log("TakeAllItems çağrıldı, item sayısı: " + itemsInChest.Count);
        
        if (itemsInChest.Count == 0) return;
        
        // Tüm itemleri bir kopya listede tut (foreach içinde listeyi değiştirdiğimiz için)
        List<GameObject> itemsCopy = new List<GameObject>(itemsInChest);
        
        foreach (GameObject itemObj in itemsCopy)
        {
            if (itemObj == null) 
            {
                Debug.LogWarning("Item objesi null, geçiliyor.");
                continue;
            }
            
            ItemObject item = itemObj.GetComponent<ItemObject>();
            if (item == null)
            {
                Debug.LogWarning("ItemObject bileşeni yok: " + itemObj.name);
                continue;
            }
            
            ItemData itemData = item.GetItemData();
            if (itemData == null)
            {
                Debug.LogWarning("ItemData null: " + itemObj.name);
                continue;
            }
            
            // Envantere ekle
            Debug.Log("Envantere ekleniyor: " + itemData.itemName);
            Inventory.instance.AddItem(itemData);
            
            // Listeyi güncelle - itemsInChest'ten çıkar, removedItems'a ekle
            itemsInChest.Remove(itemObj);
            removedItems.Add(itemObj);
            
            // GameObject'i görünmez yap
            itemObj.SetActive(false);
        }
        
        // Listeyi temizle
        int previousCount = itemsInChest.Count;
        itemsInChest.Clear();
        Debug.Log("Sandık temizlendi, önceki sayı: " + previousCount + ", şimdiki sayı: " + itemsInChest.Count);
    }

    // Tek bir item alma fonksiyonu
    public void RemoveItem(GameObject item)
    {
        if (item == null)
        {
            Debug.LogError("RemoveItem'e null item gönderildi!");
            return;
        }
        
        if (!itemsInChest.Contains(item))
        {
            Debug.LogError("Bu item sandıkta bulunamadı: " + item.name);
            return;
        }

        ItemObject itemObj = item.GetComponent<ItemObject>();
        if (itemObj == null)
        {
            Debug.LogError("ItemObject bileşeni bulunamadı: " + item.name);
            return;
        }
        
        ItemData itemData = itemObj.GetItemData();
        if (itemData == null)
        {
            Debug.LogError("ItemData null: " + item.name);
            return;
        }
        
        if (Inventory.instance == null)
        {
            Debug.LogError("Inventory.instance null!");
            return;
        }
        
        // Envantere ekle
        Debug.Log("Envantere tek item ekleniyor: " + itemData.itemName);
        Inventory.instance.AddItem(itemData);
        
        // Sandıktan çıkar ve yedek listeye ekle
        itemsInChest.Remove(item);
        removedItems.Add(item);
        
        // GameObject'i gizle
        item.SetActive(false);
        
        Debug.Log("Item alındı, kalan item sayısı: " + itemsInChest.Count);
    }
    
    // Bu metod ile sandığı sıfırlayabilirsiniz (geliştirme aşamasında kullanışlı)
    public void ResetChest()
    {
        // Önceden alınan tüm itemleri geri ekle
        foreach (GameObject item in removedItems)
        {
            if (item != null)
            {
                itemsInChest.Add(item);
                item.SetActive(true);
            }
        }
        
        removedItems.Clear();
        Debug.Log("Sandık sıfırlandı, item sayısı: " + itemsInChest.Count);
    }
}
