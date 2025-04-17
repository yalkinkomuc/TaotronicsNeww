using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "TargetedKillObjective", menuName = "Quest/Objective/Targeted Kill")]
public class TargetedKillObjective : QuestObjective
{
    [Header("Target Settings")]
    [Tooltip("Öldürülecek düşman türleri (Enemy.enemyType değerleri)")]
    public List<string> targetEnemyTypes = new List<string>();
    
    [Space(10)]
    [Tooltip("Öldürülecek düşmanlar arasında her birinden kaç tane gerektiği")]
    public int requiredAmount = 1;
    
    // İlerlemeyi takip etmek için
    private Dictionary<string, int> killedCounts = new Dictionary<string, int>();

    public override void Initialize()
    {
        isCompleted = false;
        isInitialized = true;
        
        // Öldürme sayaçlarını sıfırla
        killedCounts.Clear();
        foreach (var enemyType in targetEnemyTypes)
        {
            killedCounts[enemyType] = 0;
        }
        
        // Description'ı güncelle
        UpdateDescription();
    }

    public override void HandleEvent(string eventName, object data)
    {
        // "EnemyKilled" eventini yakala
        if (eventName == "EnemyKilled" && data is string killedType && !isCompleted)
        {
            // Hedef düşman türü mü kontrol et
            if (targetEnemyTypes.Contains(killedType))
            {
                // Sayacı artır
                killedCounts[killedType]++;
                Debug.Log($"Hedef düşman öldürüldü: {killedType}, Sayı: {killedCounts[killedType]}/{requiredAmount}");
                
                // Tamamlama durumunu kontrol et
                CheckCompletion();
            }
        }
    }
    
    // Tüm düşman türleri için gerekli sayıda öldürme tamamlandı mı kontrol et
    private void CheckCompletion()
    {
        bool allCompleted = true;
        
        foreach (var enemyType in targetEnemyTypes)
        {
            if (killedCounts.ContainsKey(enemyType) && killedCounts[enemyType] < requiredAmount)
            {
                allCompleted = false;
                break;
            }
        }
        
        if (allCompleted && targetEnemyTypes.Count > 0)
        {
            isCompleted = true;
            Debug.Log("Tüm hedef düşmanlar öldürüldü!");
        }
    }
    
    // Açıklama metnini güncelle
    private void UpdateDescription()
    {
        if (targetEnemyTypes.Count == 0)
        {
            description = "Hiç hedef düşman belirtilmemiş!";
            return;
        }
        
        if (targetEnemyTypes.Count == 1)
        {
            description = $"{requiredAmount} adet {targetEnemyTypes[0]} öldür";
            return;
        }
        
        string targetDesc = string.Join(", ", targetEnemyTypes);
        description = $"Her birinden {requiredAmount} adet olmak üzere şu düşmanları öldür: {targetDesc}";
    }

    // Unity Editor için
    private void OnValidate()
    {
        UpdateDescription();
    }
}