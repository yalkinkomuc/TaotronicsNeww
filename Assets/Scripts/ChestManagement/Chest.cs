using System.Collections.Generic;
using UnityEngine;
using System.Collections;

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
        }
        
        // PlayerPrefs anahtarını ayarla
        prefsKey = "chest_open_" + chestUniqueID;
        
        // Sandık itemlerini kontrol et ve stack'le
        ProcessChestItems();
    }

    // Start'a taşıdık - ChestManager yüklendikten sonra çalışması için
    private void Start()
    {
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
            }
        }
        

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
            Debug.LogWarning("ChestManager bulunamadı, sonraki frame'de tekrar denenecek.");
            // ChestManager oluşana kadar bekle ve tekrar dene
            StartCoroutine(WaitForChestManager());
            return;
        }
        
        List<string> collectedItems = ChestManager.Instance.GetCollectedItemsFromChest(chestUniqueID);
        
        if (collectedItems == null || collectedItems.Count == 0)
        {
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
        

    }
    
    // ChestManager'ı bekleme coroutine'i
    private System.Collections.IEnumerator WaitForChestManager()
    {
        int attemptCount = 0;
        int maxAttempts = 10; // Maximum bekleme sayısı
        
        while (ChestManager.Instance == null && attemptCount < maxAttempts)
        {
            yield return new WaitForSeconds(0.1f);
            attemptCount++;
        }
        
        if (ChestManager.Instance != null)
        {
            // ChestManager bulunduğunda işleme devam et
            RemoveCollectedItems();
            LoadChestState();
        }
        else
        {
            Debug.LogError("ChestManager " + maxAttempts + " denemeden sonra bulunamadı!");
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
        
        // UI_ChestInventory.Instance kontrolü
        if (UI_ChestInventory.Instance == null)
        {
            Debug.LogError("UI_ChestInventory.Instance null! Prefabı yüklemeye çalışılıyor...");
            
            // Prefabı Resources'dan yüklemeyi deneyelim
            GameObject chestUIPrefab = Resources.Load<GameObject>("UI/UI_ChestInventory");
            if (chestUIPrefab != null)
            {
                GameObject chestUIObj = Instantiate(chestUIPrefab);
                DontDestroyOnLoad(chestUIObj);
                
                // Bir süre bekleyip tekrar deneyelim
                Invoke("TryOpenChestUIAgain", 0.1f);
                return;
            }
            else
            {
                Debug.LogError("UI_ChestInventory prefabı bulunamadı! Sandık UI'ı açılamıyor.");
                return;
            }
        }
        
        // Sadece UI'ı aç
        UI_ChestInventory.Instance.OpenChest(this);
    }
    
    // UI açma işlemini tekrar deneyen metod
    private void TryOpenChestUIAgain()
    {
        if (UI_ChestInventory.Instance != null)
        {
            UI_ChestInventory.Instance.OpenChest(this);
        }
        else
        {
            Debug.LogError("UI_ChestInventory.Instance hala null! Sandık açılamıyor.");
        }
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
        
        string itemName = itemObj.GetItemData().itemName;
        if (!stackedItems.ContainsKey(itemName))
        {
            Debug.LogError("Bu item stackedItems listesinde bulunamadı: " + itemName);
            return;
        }
        
        int count = stackedItems[itemName];
        ItemData itemData = itemObj.GetItemData();
        
        // Inventory instance kontrol
        if (Inventory.instance == null)
        {
            Debug.LogError("Inventory.instance null! Inventory GameObject'inin DontDestroyOnLoad olduğundan emin olun!");
            return;
        }
        
        // Envantere ekle
        for (int i = 0; i < count; i++)
        {
            Inventory.instance.AddItem(itemData);
            
            if (itemData is SkillShard)
            {
                SkillShard shard = itemData as SkillShard;
                if (SkillManager.Instance != null)
                {
                    SkillManager.Instance.AddShards(shard.GetShardValue());
                }
                else
                {
                    Debug.LogError("SkillManager.Instance bulunamadı! SkillShard eklenemiyor.");
                }
            }
        }
        
        // İtemleri bul ve listeden çıkar
        List<GameObject> itemsToRemove = new List<GameObject>();
        foreach (GameObject obj in itemsInChest)
        {
            if (obj == null) continue;
            
            ItemObject objItem = obj.GetComponent<ItemObject>();
            if (objItem == null || objItem.GetItemData() == null) continue;
            
            if (objItem.GetItemData().itemName == itemName)
            {
                itemsToRemove.Add(obj);
                string itemID = objItem.GetUniqueID();
                if (!string.IsNullOrEmpty(itemID))
                {
                    if (ChestManager.Instance != null)
                    {
                        ChestManager.Instance.MarkItemAsCollected(chestUniqueID, itemID);
                    }
                    else
                    {
                        Debug.LogError("ChestManager.Instance bulunamadı! Item toplanmış olarak işaretlenemiyor.");
                    }
                }
            }
        }
        
        // Görünmez yap ve listeden çıkar
        foreach (GameObject obj in itemsToRemove)
        {
            itemsInChest.Remove(obj);
            removedItems.Add(obj);
            obj.SetActive(false);
        }
        
        // Stack bilgisini sil
        stackedItems.Remove(itemName);
        itemReferences.Remove(itemName);
        
    }
    
    // Tüm itemleri envantere aktarma
    public void TakeAllItems()
    {
        if (itemsInChest.Count == 0) return;
        
        // Inventory instance kontrol
        if (Inventory.instance == null)
        {
            Debug.LogError("TakeAllItems: Inventory.instance null! Inventory GameObject'inin DontDestroyOnLoad olduğundan emin olun!");
            return;
        }
        
        Dictionary<string, int> itemsToAdd = new Dictionary<string, int>(stackedItems);
        
        foreach (var pair in itemsToAdd)
        {
            string itemName = pair.Key;
            int count = pair.Value;
            
            if (!itemReferences.ContainsKey(itemName))
            {
                Debug.LogWarning($"{itemName} için referans bulunamadı, geçiliyor...");
                continue;
            }
            
            GameObject itemObj = itemReferences[itemName];
            if (itemObj == null)
            {
                Debug.LogWarning($"{itemName} için GameObject null, geçiliyor...");
                continue;
            }
            
            ItemObject item = itemObj.GetComponent<ItemObject>();
            if (item == null)
            {
                Debug.LogWarning($"{itemName} için ItemObject bileşeni bulunamadı, geçiliyor...");
                continue;
            }
            
            ItemData itemData = item.GetItemData();
            if (itemData == null)
            {
                Debug.LogWarning($"{itemName} için ItemData null, geçiliyor...");
                continue;
            }
            
            // Envantere ekle
            for (int i = 0; i < count; i++)
            {
                Inventory.instance.AddItem(itemData);
                
                if (itemData is SkillShard)
                {
                    SkillShard shard = itemData as SkillShard;
                    if (SkillManager.Instance != null)
                    {
                        SkillManager.Instance.AddShards(shard.GetShardValue());
                    }
                    else
                    {
                        Debug.LogError("SkillManager.Instance bulunamadı! SkillShard eklenemiyor.");
                    }
                }
            }
            
            // İtemleri bul ve işaretle
            List<GameObject> itemsToRemove = new List<GameObject>();
            foreach (GameObject obj in itemsInChest)
            {
                if (obj == null) continue;
                
                ItemObject objItem = obj.GetComponent<ItemObject>();
                if (objItem == null || objItem.GetItemData() == null) continue;
                
                if (objItem.GetItemData().itemName == itemName)
                {
                    itemsToRemove.Add(obj);
                    string itemID = objItem.GetUniqueID();
                    if (!string.IsNullOrEmpty(itemID))
                    {
                        if (ChestManager.Instance != null)
                        {
                            ChestManager.Instance.MarkItemAsCollected(chestUniqueID, itemID);
                        }
                        else
                        {
                            Debug.LogError("ChestManager.Instance bulunamadı! Item toplanmış olarak işaretlenemiyor.");
                        }
                    }
                }
            }
            // Görünmez yap ve listeden çıkar
            foreach (GameObject obj in itemsToRemove)
            {
                itemsInChest.Remove(obj);
                removedItems.Add(obj);
                obj.SetActive(false);
            }
        }
        
        // Tüm listeleri temizle
        stackedItems.Clear();
        itemReferences.Clear();
        
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
        
    }
    
    // Sandık ID'sini döndür
    public string GetChestID()
    {
        return chestUniqueID;
    }
}
