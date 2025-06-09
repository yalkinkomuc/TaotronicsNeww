using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class SpecificEnemySelector : MonoBehaviour
{
    [Header("Configuration")]
    [Tooltip("Target quest objective to setup")]
    public SpecificEnemyKillObjective targetObjective;
    
    [Space(10)]
    [Header("Target Enemies")]
    [Tooltip("Drag specific enemies from the scene here")]
    public List<Enemy> targetEnemies = new List<Enemy>();
    
    [Space(10)]
    [Header("Debug Info")]
    [SerializeField, ReadOnly] private List<string> generatedIDs = new List<string>();

    [ContextMenu("Setup Target IDs")]
    public void SetupTargetIDs()
    {
        if (targetObjective == null)
        {
            Debug.LogError("Target objective is not assigned!");
            return;
        }

        var enemyIDs = new List<string>();
        generatedIDs.Clear();

        foreach (var enemy in targetEnemies)
        {
            if (enemy == null) continue;

            // Enemy'nin unique ID'sini al veya oluştur
            string enemyID = GetOrCreateEnemyID(enemy);
            if (!string.IsNullOrEmpty(enemyID))
            {
                enemyIDs.Add(enemyID);
                generatedIDs.Add($"{enemy.name}: {enemyID}");
            }
        }

        // Target objective'e ID'leri ata
        targetObjective.SetTargetEnemyIDs(enemyIDs);

#if UNITY_EDITOR
        // Editor'da değişiklikleri kaydet
        EditorUtility.SetDirty(targetObjective);
#endif

        Debug.Log($"Setup complete! {enemyIDs.Count} enemy IDs assigned to objective.");
    }

    private string GetOrCreateEnemyID(Enemy enemy)
    {
        // Önce mevcut ID'yi kontrol et
        if (!string.IsNullOrEmpty(enemy.GetUniqueEnemyID()))
        {
            return enemy.GetUniqueEnemyID();
        }

        // Yoksa yeni ID oluştur
        string newID = $"{enemy.enemyType}_{enemy.gameObject.GetInstanceID()}_{System.Guid.NewGuid().ToString("N")[..8]}";
        enemy.SetUniqueEnemyID(newID);

        Debug.Log($"Generated new enemy ID: {newID} for {enemy.gameObject.name}");
        return newID;
    }

    [ContextMenu("Clear Target IDs")]
    public void ClearTargetIDs()
    {
        if (targetObjective != null)
        {
            targetObjective.SetTargetEnemyIDs(new List<string>());
            
#if UNITY_EDITOR
            EditorUtility.SetDirty(targetObjective);
#endif
        }
        
        generatedIDs.Clear();
        Debug.Log("Target IDs cleared.");
    }

    [ContextMenu("Refresh Generated IDs Display")]
    public void RefreshGeneratedIDsDisplay()
    {
        generatedIDs.Clear();
        
        foreach (var enemy in targetEnemies)
        {
            if (enemy == null) continue;
            
            string enemyID = enemy.GetUniqueEnemyID();
            if (!string.IsNullOrEmpty(enemyID))
            {
                generatedIDs.Add($"{enemy.name}: {enemyID}");
            }
            else
            {
                generatedIDs.Add($"{enemy.name}: [No ID yet]");
            }
        }
    }

    private void OnValidate()
    {
        // Inspector'da değişiklik olduğunda refresh et
        RefreshGeneratedIDsDisplay();
    }

    // Sadece Editor'da çalışır
    private void Awake()
    {
        // Runtime'da bu component'i disable et
        enabled = false;
    }
} 