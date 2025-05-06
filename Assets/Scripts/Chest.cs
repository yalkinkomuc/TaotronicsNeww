using System.Collections.Generic;
using UnityEngine;

public class Chest : MonoBehaviour, IInteractable
{
    [Header("Chest Settings")]
    public List<GameObject> itemsInChest = new List<GameObject>();
    [SerializeField] private Animator animator; // Animator artık kullanılacak
    [SerializeField] private InteractionPrompt prompt;
    [SerializeField] private string chestUniqueID; // Her sandık için benzersiz ID
    
    // Dictionary to track stacked items by item name
    private Dictionary<string, int> stackedItems = new Dictionary<string, int>();
    private Dictionary<string, GameObject> itemReferences = new Dictionary<string, GameObject>();
    
    [Header("Sprites")]
    [SerializeField] private Sprite closedChestSprite; // Kapalı sandık sprite'ı
    [SerializeField] private Sprite openChestSprite;   // Açık sandık sprite'ı
    
    private SpriteRenderer spriteRenderer; // Sprite'ı değiştirmek için
    
    // Yardımcı liste - Geçici olarak alınmış itemların takibi için
    private List<GameObject> removedItems = new List<GameObject>();
    
    private bool isOpen = false;
    private string prefsKey; // PlayerPrefs için anahtar
    private bool isAnimationPlaying = false;

    private void Awake()
    {
        // SpriteRenderer komponenti al
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        if (prompt == null)
            prompt = GetComponentInChildren<InteractionPrompt>();
        
        // Benzersiz ID oluştur (eğer yoksa)
        if (string.IsNullOrEmpty(chestUniqueID))
        {
            chestUniqueID = "chest_" + System.Guid.NewGuid().ToString();
            Debug.Log("Yeni Chest ID oluşturuldu: " + chestUniqueID);
        }
        
        // PlayerPrefs anahtarını ayarla
        prefsKey = "chest_open_" + chestUniqueID;
        
        // Sandık itemlerini kontrol et ve stack'le
        ProcessChestItems();
        
        // Daha önce alınan itemları kaldır (oyun yeniden başlatıldığında)
        RemoveCollectedItems();
        
        // Sandık durumunu yükle
        LoadChestState();
    }

    // Sandık içindeki itemleri işle ve stack yap
    private void ProcessChestItems()
    {
        if (itemsInChest == null)
        {
            Debug.LogError("itemsInChest null!");
            itemsInChest = new List<GameObject>();
            return;
        }

        stackedItems.Clear();
        itemReferences.Clear();
        
        // Önce tüm itemleri işle ve stack say
        for (int i = 0; i < itemsInChest.Count; i++)
        {
            GameObject itemObj = itemsInChest[i];
            if (itemObj == null) continue;
            
            ItemObject item = itemObj.GetComponent<ItemObject>();
            if (item == null || item.GetItemData() == null) continue;
            
            string itemName = item.GetItemData().itemName;
            
            // Stack count'u artır
            if (stackedItems.ContainsKey(itemName))
            {
                stackedItems[itemName]++;
            }
            else
            {
                stackedItems[itemName] = 1;
                itemReferences[itemName] = itemObj;
            }
            
            // Eğer ID yoksa otomatik ekle
            if (string.IsNullOrEmpty(item.GetUniqueID()))
            {
                string uniqueID = "item_" + System.Guid.NewGuid().ToString();
                item.SetUniqueID(uniqueID);
                Debug.Log("Item için yeni ID oluşturuldu: " + uniqueID);
            }
        }
        
        Debug.Log($"Chest items processed. Unique items: {stackedItems.Count}, Total items: {itemsInChest.Count}");
    }

    // Sandık durumunu PlayerPrefs'ten yükle
    private void LoadChestState()
    {
        // PlayerPrefs'ten sandığın açık olup olmadığını kontrol et
        if (PlayerPrefs.HasKey(prefsKey) && PlayerPrefs.GetInt(prefsKey) == 1)
        {
            isOpen = true;
            // Sprite'ı açık sandık olarak değiştir
            if (spriteRenderer != null && openChestSprite != null)
            {
                spriteRenderer.sprite = openChestSprite;
                
                // Animatörü devre dışı bırak çünkü sadece sprite'ı göstereceğiz
                if (animator != null)
                {
                    animator.enabled = false;
                }
            }
            
            Debug.Log("Sandık daha önce açılmış, açık duruma getirildi: " + chestUniqueID);
        }
        else
        {
            // Sandık kapalı, sprite'ı ayarla
            if (spriteRenderer != null && closedChestSprite != null)
            {
                spriteRenderer.sprite = closedChestSprite;
                
                // Animatörü etkinleştir çünkü açılma animasyonu oynayabilir
                if (animator != null)
                {
                    animator.enabled = true;
                }
            }
        }
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
                
                // Stack bilgisini güncelle
                string itemName = itemObj.GetItemData().itemName;
                if (stackedItems.ContainsKey(itemName) && stackedItems[itemName] > 0)
                {
                    stackedItems[itemName]--;
                    if (stackedItems[itemName] <= 0)
                    {
                        stackedItems.Remove(itemName);
                        itemReferences.Remove(itemName);
                    }
                }
                
                itemsInChest.RemoveAt(i);
                removedItems.Add(item);
                item.SetActive(false);
            }
        }
        
        Debug.Log("Toplam " + collectedItems.Count + " item daha önce toplanmış, kalan item sayısı: " + itemsInChest.Count);
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
        // Eğer animasyon oynatılıyorsa hiçbir şey yapma
        if (isAnimationPlaying)
            return;
        
        // Artık isOpen kontrolü yapmıyoruz, her zaman UI'ı açıyoruz
        // Görsel olarak zaten açık kalacak
        OpenChestUI();
    }

    // Açılma metodu - Animasyonu oynatır ve UI'ı açar
    private void OpenChest()
    {
        // Eğer zaten açıksa, sadece UI'ı aç
        if (isOpen)
        {
            OpenChestUI();
            return;
        }
        
        isOpen = true;
        isAnimationPlaying = true;
        
        // Animasyonu oynat - AnimEvent'i bekle
        if (animator != null)
        {
            animator.enabled = true;
            animator.SetTrigger("Open");
            
            // Eğer Animation Event yoksa, 0.5 saniye sonra sprite'ı değiştir
            // (Animasyon Event'i varsa bu zaten çağrılmayacak)
            Invoke("OnChestOpenAnimationComplete", 0.5f);
        }
        else
        {
            // Eğer animator yoksa, direkt sprite'ı değiştir
            OnChestOpenAnimationComplete();
        }
        
        // PlayerPrefs'e durumu kaydet (1 = açık)
        PlayerPrefs.SetInt(prefsKey, 1);
        PlayerPrefs.Save();
        
        // İtem durumunu kontrol et
        Debug.Log("Sandık açılıyor, item sayısı: " + itemsInChest.Count);
    }
    
    // Animation Event ile çağrılacak metod
    // (Bu metodu AnimationEvent ayarlarından çağırmalısın!)
    public void OnChestOpenAnimationComplete()
    {
        isAnimationPlaying = false;
        
        // Sprite'ı açık sandık olarak değiştir
        if (spriteRenderer != null && openChestSprite != null)
        {
            spriteRenderer.sprite = openChestSprite;
        }
        
        // Animatörü devre dışı bırak ki sprite değişmesin
        if (animator != null)
        {
            animator.enabled = false;
        }
        
        // Açılma animasyonu bittikten sonra UI'ı aç
        OpenChestUI();
    }

    // Sadece UI'ı açmak için yeni metod
    private void OpenChestUI()
    {
        // Eğer sandık görsel olarak kapalıysa önce aç
        if (!isOpen)
        {
            OpenChest();
            return; // OpenChest zaten UI'ı açacak
        }
        
        // Stacked item bilgisini tazele
        ProcessChestItems();
        
        // Sadece UI'ı aç
        UI_ChestInventory.Instance.OpenChest(this);
    }

    // UI'ı kapatmak için metod (sandık görsel olarak açık kalır)
    public void CloseChest()
    {
        // Sandık görselini değiştirmiyoruz, sadece UI'ı kapatıyoruz
        // isOpen değişmiyor - hep true kalıyor
        
        // Sadece UI'ı kapat
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
        
        // Stack'lenmiş item bilgisini kullanalım
        Dictionary<string, int> itemsToAdd = new Dictionary<string, int>(stackedItems);
        
        foreach (var pair in itemsToAdd)
        {
            string itemName = pair.Key;
            int count = pair.Value;
            
            if (!itemReferences.ContainsKey(itemName)) continue;
            
            GameObject itemObj = itemReferences[itemName];
            if (itemObj == null) continue;
            
            ItemObject item = itemObj.GetComponent<ItemObject>();
            if (item == null) continue;
            
            ItemData itemData = item.GetItemData();
            if (itemData == null) continue;
            
            // İtem referansını envantere ekle (ilgili sayıda)
            for (int i = 0; i < count; i++)
            {
                Debug.Log("Envantere ekleniyor: " + itemData.itemName);
                Inventory.instance.AddItem(itemData);
                
                // Eğer bu bir Skill Shard ise, SkillManager'a ekle
                if (itemData is SkillShard)
                {
                    SkillShard shard = itemData as SkillShard;
                    if (SkillManager.Instance != null)
                    {
                        SkillManager.Instance.AddShards(shard.GetShardValue());
                        Debug.Log("Sandıktan Skill Shard toplandı: +" + shard.GetShardValue() + " shards");
                    }
                }
            }
            
            // ItemObject referanslarını bul ve işaretle
            List<GameObject> itemsToMark = new List<GameObject>();
            foreach (GameObject obj in itemsInChest)
            {
                if (obj == null) continue;
                
                ItemObject objItem = obj.GetComponent<ItemObject>();
                if (objItem == null || objItem.GetItemData() == null) continue;
                
                if (objItem.GetItemData().itemName == itemName)
                {
                    itemsToMark.Add(obj);
                }
            }
            
            // Her bir item referansını işaretle ve kaldır
            foreach (GameObject obj in itemsToMark)
            {
                // Item'ı toplanan olarak işaretle
                ItemObject objItem = obj.GetComponent<ItemObject>();
                string itemID = objItem.GetUniqueID();
                if (!string.IsNullOrEmpty(itemID))
                {
                    ChestManager.Instance?.MarkItemAsCollected(chestUniqueID, itemID);
                }
                
                // Listeyi güncelle
                itemsInChest.Remove(obj);
                removedItems.Add(obj);
                
                // GameObject'i görünmez yap
                obj.SetActive(false);
            }
        }
        
        // Dictionary'leri temizle
        stackedItems.Clear();
        itemReferences.Clear();
        
        // Listeyi temizle
        int previousCount = itemsInChest.Count;
        itemsInChest.Clear();
        Debug.Log("Sandık temizlendi, önceki sayı: " + previousCount + ", şimdiki sayı: " + itemsInChest.Count);
    }

    // Tek bir item stack'ını alma fonksiyonu
    public void RemoveItem(GameObject item)
    {
        if (item == null)
        {
            Debug.LogError("RemoveItem'e null item gönderildi!");
            return;
        }

        ItemObject itemObj = item.GetComponent<ItemObject>();
        if (itemObj == null || itemObj.GetItemData() == null)
        {
            Debug.LogError("ItemObject bileşeni bulunamadı veya ItemData null");
            return;
        }
        
        // Stack bilgisine ulaş
        string itemName = itemObj.GetItemData().itemName;
        if (!stackedItems.ContainsKey(itemName))
        {
            Debug.LogError("Bu item stackedItems listesinde bulunamadı: " + itemName);
            return;
        }
        
        // Stack sayısını al
        int count = stackedItems[itemName];
        ItemData itemData = itemObj.GetItemData();
        
        // Envantere ekle (ilgili sayıda)
        for (int i = 0; i < count; i++)
        {
            Debug.Log("Envantere ekleniyor: " + itemData.itemName);
            Inventory.instance.AddItem(itemData);
            
            // Eğer bu bir Skill Shard ise, SkillManager'a ekle
            if (itemData is SkillShard)
            {
                SkillShard shard = itemData as SkillShard;
                if (SkillManager.Instance != null)
                {
                    SkillManager.Instance.AddShards(shard.GetShardValue());
                    Debug.Log("Sandıktan Skill Shard toplandı: +" + shard.GetShardValue() + " shards");
                }
            }
        }
        
        // ItemObject referanslarını bul ve işaretle
        List<GameObject> itemsToMark = new List<GameObject>();
        foreach (GameObject obj in itemsInChest)
        {
            if (obj == null) continue;
            
            ItemObject objItem = obj.GetComponent<ItemObject>();
            if (objItem == null || objItem.GetItemData() == null) continue;
            
            if (objItem.GetItemData().itemName == itemName)
            {
                itemsToMark.Add(obj);
                // Item'ı toplanan olarak işaretle
                string itemID = objItem.GetUniqueID();
                if (!string.IsNullOrEmpty(itemID))
                {
                    ChestManager.Instance?.MarkItemAsCollected(chestUniqueID, itemID);
                }
            }
        }
        
        // Her bir item referansını kaldır
        foreach (GameObject obj in itemsToMark)
        {
            // Listeyi güncelle
            itemsInChest.Remove(obj);
            removedItems.Add(obj);
            
            // GameObject'i görünmez yap
            obj.SetActive(false);
        }
        
        // Stack bilgisini sil
        stackedItems.Remove(itemName);
        itemReferences.Remove(itemName);
        
        Debug.Log($"Item stack alındı ({count}x {itemName}), kalan unique item sayısı: {stackedItems.Count}");
    }
    
    // UI için stack bilgisini döndür
    public Dictionary<string, int> GetStackedItems()
    {
        return new Dictionary<string, int>(stackedItems);
    }
    
    // UI için item referansını döndür
    public GameObject GetItemReference(string itemName)
    {
        if (itemReferences.TryGetValue(itemName, out GameObject item))
        {
            return item;
        }
        return null;
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
        
        // PlayerPrefs'ten sandık durumunu sil
        PlayerPrefs.DeleteKey(prefsKey);
        
        // Sandığı kapat
        isOpen = false;
        isAnimationPlaying = false;
        
        // Sprite'ı kapalı sandık olarak değiştir
        if (spriteRenderer != null && closedChestSprite != null)
        {
            spriteRenderer.sprite = closedChestSprite;
        }
        
        // Animatörü tekrar etkinleştir
        if (animator != null)
        {
            animator.enabled = true;
        }
        
        // Kaydedilmiş itemları temizle
        ChestManager.Instance?.ClearCollectedItemsFromChest(chestUniqueID);
        
        // Stack bilgisini yenile
        ProcessChestItems();
        
        Debug.Log("Sandık sıfırlandı, item sayısı: " + itemsInChest.Count);
    }
    
    // Sandık ID'sini döndür
    public string GetChestID()
    {
        return chestUniqueID;
    }
}
