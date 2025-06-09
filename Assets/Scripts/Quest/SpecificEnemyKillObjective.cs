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
            Debug.Log("🎯 All specific target enemies killed!");
        }
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
}

// ReadOnly attribute for inspector
public class ReadOnlyAttribute : PropertyAttribute { } 