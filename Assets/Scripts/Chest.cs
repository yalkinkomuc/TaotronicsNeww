using System.Collections.Generic;
using UnityEngine;

public class Chest : MonoBehaviour, IInteractable
{
    [Header("Chest Settings")]
    public List<GameObject> itemsInChest = new List<GameObject>();
    [SerializeField] private Animator animator;
    [SerializeField] private InteractionPrompt prompt;
    [SerializeField] private string chestUniqueID; // Her sandık için benzersiz ID
    
    // Yardımcı liste - Geçici olarak alınmış itemların takibi için
    private List<GameObject> removedItems = new List<GameObject>();
    
    private bool isOpen = false;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
            
        if (prompt == null)
            prompt = GetComponentInChildren<InteractionPrompt>();
        
        // Benzersiz ID oluştur (eğer yoksa)
        if (string.IsNullOrEmpty(chestUniqueID))
        {
            chestUniqueID = "chest_" + System.Guid.NewGuid().ToString();
            Debug.Log("Yeni Chest ID oluşturuldu: " + chestUniqueID);
        }
        
        // Sandık itemlerini kontrol et
        ValidateChestItems();
        
        // Daha önce alınan itemları kaldır (oyun yeniden başlatıldığında)
        RemoveCollectedItems();
    }
    
    // Daha önce toplanan itemleri kaldır
    private void RemoveCollectedItems()
    {
        if (ChestManager.Instance == null)
        {
            Debug.LogWarning("ChestManager bulunamadı, toplanmış itemlar kontrol edilemedi!");
            return;
        }
        
        List<string> collectedItems = ChestManager.Instance.GetCollectedItemsFromChest(chestUniqueID);
        
        if (collectedItems == null || collectedItems.Count == 0)
        {
            Debug.Log("Bu sandıktan daha önce toplanmış item yok: " + chestUniqueID);
            return;
        }
        
        // Toplanmış itemları kaldır
        for (int i = itemsInChest.Count - 1; i >= 0; i--)
        {
            GameObject item = itemsInChest[i];
            if (item == null) continue;
            
            ItemObject itemObj = item.GetComponent<ItemObject>();
            if (itemObj == null || string.IsNullOrEmpty(itemObj.GetUniqueID())) continue;
            
            // Eğer bu item daha önce toplanmışsa, listeden çıkar
            if (collectedItems.Contains(itemObj.GetUniqueID()))
            {
                Debug.Log("Daha önce toplanmış item kaldırıldı: " + itemObj.GetUniqueID());
                itemsInChest.RemoveAt(i);
                removedItems.Add(item);
                item.SetActive(false);
            }
        }
        
        Debug.Log("Toplam " + collectedItems.Count + " item daha önce toplanmış, kalan item sayısı: " + itemsInChest.Count);
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
            
            // Eğer ID yoksa otomatik ekle
            if (string.IsNullOrEmpty(item.GetUniqueID()))
            {
                string uniqueID = "item_" + System.Guid.NewGuid().ToString();
                item.SetUniqueID(uniqueID);
                Debug.Log("Item için yeni ID oluşturuldu: " + uniqueID);
            }
        }
    }
    
    private void OnValidate()
    {
        // Editor'da sandık ID'si yoksa oluştur
        if (string.IsNullOrEmpty(chestUniqueID))
        {
            chestUniqueID = "chest_" + System.Guid.NewGuid().ToString();
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
            
            // Item'ı toplanan olarak işaretle
            string itemID = item.GetUniqueID();
            if (!string.IsNullOrEmpty(itemID))
            {
                ChestManager.Instance?.MarkItemAsCollected(chestUniqueID, itemID);
            }
            
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
        
        // Item'ı toplanan olarak işaretle
        string itemID = itemObj.GetUniqueID();
        if (!string.IsNullOrEmpty(itemID))
        {
            ChestManager.Instance?.MarkItemAsCollected(chestUniqueID, itemID);
        }
        
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
        
        // Kaydedilmiş itemları temizle
        ChestManager.Instance?.ClearCollectedItemsFromChest(chestUniqueID);
        
        Debug.Log("Sandık sıfırlandı, item sayısı: " + itemsInChest.Count);
    }
    
    // Sandık ID'sini döndür
    public string GetChestID()
    {
        return chestUniqueID;
    }
}
