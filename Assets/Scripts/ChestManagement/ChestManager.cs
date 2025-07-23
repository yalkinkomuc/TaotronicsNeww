using System.Collections.Generic;
using UnityEngine;
using System;

// ChestManager'ı diğer scriptlerden önce çalıştır
[DefaultExecutionOrder(-100)]
/// <summary>
/// Sandıklardan alınan itemları takip eden yönetici sınıf.
/// Singleton tasarım desenini kullanır.
/// </summary>
public class ChestManager : MonoBehaviour
{
    // Singleton instance
    public static ChestManager Instance { get; private set; }

    // Serileştirme ve kaydetme için veri sınıfları
    [Serializable]
    private class ChestItem
    {
        public string itemID;
        public bool collected = false;
    }

    [Serializable]
    private class ChestData
    {
        public string chestID;
        public List<ChestItem> items = new List<ChestItem>();
    }

    [Serializable]
    private class ChestSaveData
    {
        public List<ChestData> chests = new List<ChestData>();
    }

    // Tüm sandık verilerini saklayan dictionary
    private Dictionary<string, List<string>> collectedItemsByChest = new Dictionary<string, List<string>>();

    private void Awake()
    {
        // Singleton mantığını uygula
        if (Instance == null)
        {
            Instance = this;
            
            
            // Verileri yükle
            LoadChestData();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnApplicationQuit()
    {
        // Oyundan çıkarken verileri kaydet
        SaveChestData();
    }

    // Kaydedilmiş sandık verilerini yükler
    private void LoadChestData()
    {
        // Önce dictionary'yi temizle
        collectedItemsByChest.Clear();
        
        if (PlayerPrefs.HasKey("ChestData"))
        {
            try
            {
                string jsonData = PlayerPrefs.GetString("ChestData");
                ChestSaveData saveData = JsonUtility.FromJson<ChestSaveData>(jsonData);
                
                if (saveData != null && saveData.chests != null)
                {
                    foreach (ChestData chestData in saveData.chests)
                    {
                        if (chestData != null && !string.IsNullOrEmpty(chestData.chestID))
                        {
                            List<string> itemIDs = new List<string>();
                            
                            foreach (ChestItem item in chestData.items)
                            {
                                if (item.collected && !string.IsNullOrEmpty(item.itemID))
                                {
                                    itemIDs.Add(item.itemID);
                                }
                            }
                            
                            collectedItemsByChest[chestData.chestID] = itemIDs;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Sandık verileri yüklenirken hata oluştu: " + e.Message);
                // Hata olursa yeni bir dictionary oluştur
                collectedItemsByChest = new Dictionary<string, List<string>>();
            }
        }
      
    }

    // Sandık verilerini kaydeder
    private void SaveChestData()
    {
        try
        {
            ChestSaveData saveData = new ChestSaveData();
            
            foreach (KeyValuePair<string, List<string>> pair in collectedItemsByChest)
            {
                string chestID = pair.Key;
                List<string> itemIDs = pair.Value;
                
                if (!string.IsNullOrEmpty(chestID) && itemIDs != null)
                {
                    ChestData chestData = new ChestData();
                    chestData.chestID = chestID;
                    
                    foreach (string itemID in itemIDs)
                    {
                        if (!string.IsNullOrEmpty(itemID))
                        {
                            ChestItem item = new ChestItem();
                            item.itemID = itemID;
                            item.collected = true;
                            chestData.items.Add(item);
                        }
                    }
                    
                    saveData.chests.Add(chestData);
                }
            }
            
            string jsonData = JsonUtility.ToJson(saveData);
            PlayerPrefs.SetString("ChestData", jsonData);
            PlayerPrefs.Save();
        }
        catch (Exception e)
        {
            Debug.LogError("Sandık verileri kaydedilirken hata oluştu: " + e.Message);
        }
    }

    // Bir itemın belirli bir sandıktan toplandığını işaretler
    public void MarkItemAsCollected(string chestID, string itemID)
    {
        if (string.IsNullOrEmpty(chestID) || string.IsNullOrEmpty(itemID))
        {
            Debug.LogError("Geçersiz sandık veya item ID!");
            return;
        }
        
        // İlgili sandığın listesini al veya oluştur
        if (!collectedItemsByChest.TryGetValue(chestID, out List<string> items))
        {
            items = new List<string>();
            collectedItemsByChest[chestID] = items;
        }
        
        // Bu item henüz eklenmemişse ekle
        if (!items.Contains(itemID))
        {
            items.Add(itemID);
            
            SaveChestData();
        }
    }

    // Belirli bir sandıktan toplanan itemları temizler
    public void ClearCollectedItemsFromChest(string chestID)
    {
        if (string.IsNullOrEmpty(chestID))
        {
            Debug.LogError("Geçersiz sandık ID!");
            return;
        }
        
        // Sandık verisi varsa temizle
        if (collectedItemsByChest.ContainsKey(chestID))
        {
            collectedItemsByChest.Remove(chestID);
            // Değişiklik yaptıktan sonra kaydet
            SaveChestData();
        }
    }

    // Belirli bir sandıktan toplanan itemların listesini döndürür
    public List<string> GetCollectedItemsFromChest(string chestID)
    {
        if (string.IsNullOrEmpty(chestID))
        {
            Debug.LogError("Geçersiz sandık ID!");
            return new List<string>();
        }
        
        // Sandık verisi varsa döndür, yoksa boş liste döndür
        if (collectedItemsByChest.TryGetValue(chestID, out List<string> items))
        {
            return new List<string>(items); // Deep copy
        }
        
        return new List<string>();
    }

    // Tüm sandık verilerini temizler
    public void ClearAllChestData()
    {
        collectedItemsByChest.Clear();
        
        // PlayerPrefs'ten de temizle
        PlayerPrefs.DeleteKey("ChestData");
        PlayerPrefs.Save();
    }
} 