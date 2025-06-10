using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class SpecificEnemySelector : MonoBehaviour
{
    [System.Serializable]
    public class ObjectiveEnemyPair
    {
        [Header("Objective Info")] [Tooltip("Hangi objective için düşman ekliyorsun")]
        public SpecificEnemyKillObjective objective;

        [Space(5)]
        [Header("Target Enemies for this Objective")]
        [Tooltip("Bu objective için öldürülmesi gereken düşmanlar")]
        public List<Enemy> targetEnemies = new List<Enemy>();

        [Space(5)] [Header("Generated IDs for this Objective")] [SerializeField, ReadOnly]
        public List<string> generatedIDs = new List<string>();

        
    
}
    
    [Header("Multiple Objectives Configuration")]
    [Tooltip("Her objective için ayrı düşman listesi ekleyebilirsin")]
    public List<ObjectiveEnemyPair> objectiveEnemyPairs = new List<ObjectiveEnemyPair>();
    
    [Space(10)]
    [Header("Debug Info")]
    [SerializeField, ReadOnly] private int totalObjectives = 0;
    [SerializeField, ReadOnly] private int totalEnemies = 0;

    [ContextMenu("Setup Target IDs")]
    public void SetupTargetIDs()
    {
        if (objectiveEnemyPairs == null || objectiveEnemyPairs.Count == 0)
        {
            Debug.LogError("No objective-enemy pairs found! Add some objectives first.");
            return;
        }

        int totalSetup = 0;
        int totalEnemiesProcessed = 0;

        foreach (var pair in objectiveEnemyPairs)
        {
            if (pair.objective == null)
            {
                Debug.LogWarning("Objective is null in one of the pairs, skipping...");
                continue;
            }

            var enemyIDs = new List<string>();
            pair.generatedIDs.Clear();

            foreach (var enemy in pair.targetEnemies)
            {
                if (enemy == null) continue;

                // Enemy'nin unique ID'sini al veya oluştur
                string enemyID = GetOrCreateEnemyID(enemy);
                if (!string.IsNullOrEmpty(enemyID))
                {
                    enemyIDs.Add(enemyID);
                    pair.generatedIDs.Add($"{enemy.name}: {enemyID}");
                    totalEnemiesProcessed++;
                }
            }

            // Target objective'e ID'leri ata
            pair.objective.SetTargetEnemyIDs(enemyIDs);

#if UNITY_EDITOR
            // Editor'da değişiklikleri kaydet
            EditorUtility.SetDirty(pair.objective);
#endif

            Debug.Log($"✅ {pair.objective.name}: {enemyIDs.Count} enemy IDs assigned");
            totalSetup++;
        }

        UpdateDebugInfo();
        Debug.Log($"🎯 Setup complete! {totalSetup} objectives configured with {totalEnemiesProcessed} total enemies.");
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
        if (objectiveEnemyPairs == null || objectiveEnemyPairs.Count == 0)
        {
            Debug.LogWarning("No objectives to clear.");
            return;
        }

        int clearedCount = 0;
        foreach (var pair in objectiveEnemyPairs)
        {
            if (pair.objective != null)
            {
                pair.objective.SetTargetEnemyIDs(new List<string>());
                
#if UNITY_EDITOR
                EditorUtility.SetDirty(pair.objective);
#endif
                clearedCount++;
            }
            
            pair.generatedIDs.Clear();
        }
        
        UpdateDebugInfo();
        Debug.Log($"🧹 Target IDs cleared for {clearedCount} objectives.");
    }

    [ContextMenu("Refresh Generated IDs Display")]
    public void RefreshGeneratedIDsDisplay()
    {
        if (objectiveEnemyPairs == null) return;

        foreach (var pair in objectiveEnemyPairs)
        {
            pair.generatedIDs.Clear();
            
            foreach (var enemy in pair.targetEnemies)
            {
                if (enemy == null) continue;
                
                string enemyID = enemy.GetUniqueEnemyID();
                if (!string.IsNullOrEmpty(enemyID))
                {
                    pair.generatedIDs.Add($"{enemy.name}: {enemyID}");
                }
                else
                {
                    pair.generatedIDs.Add($"{enemy.name}: [No ID yet]");
                }
            }
        }
        
        UpdateDebugInfo();
    }

    private void UpdateDebugInfo()
    {
        totalObjectives = objectiveEnemyPairs?.Count ?? 0;
        totalEnemies = 0;
        
        if (objectiveEnemyPairs != null)
        {
            foreach (var pair in objectiveEnemyPairs)
            {
                totalEnemies += pair.targetEnemies?.Count ?? 0;
            }
        }
    }

    private void OnValidate()
    {
        // Inspector'da değişiklik olduğunda refresh et
        RefreshGeneratedIDsDisplay();
    }

    [ContextMenu("Add New Objective")]
    public void AddNewObjective()
    {
        if (objectiveEnemyPairs == null)
            objectiveEnemyPairs = new List<ObjectiveEnemyPair>();
            
        objectiveEnemyPairs.Add(new ObjectiveEnemyPair());
        UpdateDebugInfo();
        Debug.Log("➕ New objective pair added!");
    }
    
    [ContextMenu("Remove Empty Objectives")]
    public void RemoveEmptyObjectives()
    {
        if (objectiveEnemyPairs == null) return;
        
        int removedCount = objectiveEnemyPairs.RemoveAll(pair => pair.objective == null);
        UpdateDebugInfo();
        
        if (removedCount > 0)
            Debug.Log($"🗑️ Removed {removedCount} empty objective pairs.");
        else
            Debug.Log("No empty objectives found.");
    }

    // Sadece Editor'da çalışır
    private void Awake()
    {
        // Runtime'da bu component'i disable et
        enabled = false;
    }
} 