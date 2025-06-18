using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(fileName = "SpecificEnemyKillObjective", menuName = "Quest/Objective/Specific Enemy Kill")]
public class SpecificEnemyKillObjective : QuestObjective
{
    [Header("Target Enemy IDs")]
    [Tooltip("Öldürülmesi gereken spesifik düşmanların ID'leri")]
    public List<string> targetEnemyIDs = new List<string>();
    
    [Space(10)]
    [Header("Enemy Spawning")]
    [Tooltip("Objective başladığında bu düşmanları spawn et")]
    public bool spawnEnemiesOnStart = false;
    
    [Space(10)]
    [Header("Item Rewards")]
    [Tooltip("Objective tamamlandığında verilecek eşyalar")]
    public List<ItemReward> itemRewards = new List<ItemReward>();
    
    [System.Serializable]
    public class ItemReward
    {
        [Tooltip("Verilecek eşya")]
        public ItemData item;
        [Tooltip("Kaç adet verilecek")]
        public int quantity = 1;
    }
    
    [Space(10)]
    [Header("Info")]
    [TextArea(3, 5)]
    [Tooltip("Bu objective'i nasıl kullanacağınız hakkında bilgi")]
    public string usageInfo = "1. Scene'de bir SpecificEnemySelector component'i oluşturun\n2. Hedef düşmanları ona ekleyin\n3. 'Setup Target IDs' butonuna basın";
    
    // Runtime'da öldürülen düşmanların ID'lerini takip et
    private HashSet<string> killedEnemyIDs = new HashSet<string>();

    public override void Initialize()
    {
        isCompleted = false;
        isInitialized = true;
        
        // Öldürülen düşmanları sıfırla
        killedEnemyIDs.Clear();
        
        Debug.Log($"=== SpecificEnemyKillObjective Initialize başladı ===");
        Debug.Log($"spawnEnemiesOnStart: {spawnEnemiesOnStart}");
        Debug.Log($"targetEnemyIDs count: {targetEnemyIDs?.Count ?? 0}");
        
        if (targetEnemyIDs != null && targetEnemyIDs.Count > 0)
        {
            for (int i = 0; i < targetEnemyIDs.Count; i++)
            {
                Debug.Log($"Target Enemy ID[{i}]: '{targetEnemyIDs[i]}'");
            }
        }
        
        // Eğer spawn aktifse, hedef düşmanları spawn et
        if (spawnEnemiesOnStart)
        {
            SpawnTargetEnemies();
        }
        else
        {
            Debug.Log("spawnEnemiesOnStart false olduğu için spawn edilmiyor.");
        }
        
        UpdateDescription();
        
        Debug.Log($"Specific Enemy Kill Objective initialized with {targetEnemyIDs.Count} targets");
    }

    public override void HandleEvent(string eventName, object data)
    {
        if (eventName == "SpecificEnemyKilled" && data is string killedEnemyID && !isCompleted)
        {
            // Hedef düşman mı kontrol et
            if (targetEnemyIDs.Contains(killedEnemyID))
            {
                killedEnemyIDs.Add(killedEnemyID);
                Debug.Log($"Target enemy killed: {killedEnemyID}. Progress: {killedEnemyIDs.Count}/{targetEnemyIDs.Count}");
                
                // Tamamlanma kontrolü
                CheckCompletion();
            }
        }
    }
    
    // Tüm hedef düşmanlar öldürüldü mü kontrol et
    private void CheckCompletion()
    {
        if (killedEnemyIDs.Count >= targetEnemyIDs.Count)
        {
            isCompleted = true;
            
            // Objective tamamlandığında eşyaları ver
            GiveItemRewards();
            
            Debug.Log("🎯 All specific target enemies killed!");
        }
    }
    
    private void GiveItemRewards()
    {
        if (itemRewards == null || itemRewards.Count == 0) return;
        
        foreach (var reward in itemRewards)
        {
            if (reward.item == null) continue;
            
            // Her quantity için ayrı ayrı ekle
            for (int i = 0; i < reward.quantity; i++)
            {
                Inventory.instance?.AddItem(reward.item);
            }
            
            Debug.Log($"🎁 Rewarded: {reward.quantity}x {reward.item.itemName}");
        }
        
        Debug.Log($"🎉 Total {itemRewards.Count} item types given as enemy kill rewards!");
    }
    
    // Açıklama metnini güncelle
    private void UpdateDescription()
    {
        if (targetEnemyIDs.Count == 0)
        {
            description = "No target enemies specified! Use SpecificEnemySelector to setup.";
            return;
        }
        
        if (targetEnemyIDs.Count == 1)
        {
            description = $"Kill the specific enemy with ID: {targetEnemyIDs[0]}";
            return;
        }
        
        description = $"Kill {targetEnemyIDs.Count} specific enemies";
    }

    // Unity Editor için
    private void OnValidate()
    {
        if (Application.isPlaying) return;
        UpdateDescription();
    }
    
    // Save sistemi için
    public HashSet<string> GetKilledEnemyIDs()
    {
        return new HashSet<string>(killedEnemyIDs);
    }
    
    public void SetKilledEnemyIDs(HashSet<string> killedIDs)
    {
        killedEnemyIDs = killedIDs ?? new HashSet<string>();
    }
    
    public List<string> GetTargetEnemyIDs()
    {
        return new List<string>(targetEnemyIDs);
    }
    
    public void SetTargetEnemyIDs(List<string> targetIDs)
    {
        targetEnemyIDs = targetIDs ?? new List<string>();
        UpdateDescription();
    }

    // Hedef düşmanları spawn et
    private void SpawnTargetEnemies()
    {
        Debug.Log("=== SpawnTargetEnemies başladı ===");
        
        if (targetEnemyIDs == null || targetEnemyIDs.Count == 0)
        {
            Debug.LogWarning("Target enemy IDs listesi boş! Spawn edilecek düşman yok.");
            return;
        }
        
        // Scene'deki tüm deaktif Enemy'leri bul
        Enemy[] allEnemies = Object.FindObjectsByType<Enemy>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        Debug.Log($"Scene'de toplam {allEnemies.Length} enemy bulundu (aktif + deaktif)");
        
        int spawnedCount = 0;
        int deactiveEnemyCount = 0;
        int shouldSpawnCount = 0;
        
        foreach (Enemy enemy in allEnemies)
        {
            bool isActive = enemy.gameObject.activeInHierarchy;
            bool shouldSpawn = enemy.ShouldSpawnOnObjective(); // Public method kullanacağız
            string enemyID = enemy.GetUniqueEnemyID();
            
            Debug.Log($"Enemy: {enemy.gameObject.name} | Active: {isActive} | ShouldSpawn: {shouldSpawn} | ID: '{enemyID}'");
            
            if (!isActive)
                deactiveEnemyCount++;
                
            if (shouldSpawn)
                shouldSpawnCount++;
            
            // Sadece shouldSpawnOnObjective = true olan ve deaktif olan düşmanları kontrol et
            if (!isActive && shouldSpawn)
            {
                // Eğer bu enemy'nin ID'si hedef listesinde varsa aktifleştir
                if (!string.IsNullOrEmpty(enemyID) && targetEnemyIDs.Contains(enemyID))
                {
                    enemy.gameObject.SetActive(true);
                    spawnedCount++;
                    Debug.Log($"✅ Objective spawn: {enemy.gameObject.name} (ID: {enemyID}) aktifleştirildi");
                }
                else
                {
                    Debug.Log($"❌ Enemy ID '{enemyID}' target listesinde bulunamadı");
                }
            }
        }
        
        Debug.Log($"=== Spawn özeti ===");
        Debug.Log($"Toplam enemy: {allEnemies.Length}");
        Debug.Log($"Deaktif enemy: {deactiveEnemyCount}");
        Debug.Log($"ShouldSpawn=true enemy: {shouldSpawnCount}");
        Debug.Log($"Spawn edilen enemy: {spawnedCount}");
    }
}

// ReadOnly attribute for inspector
public class ReadOnlyAttribute : PropertyAttribute { } 