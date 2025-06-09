using UnityEngine;
using System.Reflection;
using System.Collections.Generic;

public static class QuestObjectiveSaveSystem
{
    public static void SaveObjectiveStates(QuestInstance quest)
    {
        if (quest.Data == null || quest.Data.objectives == null) return;
        
        for (int i = 0; i < quest.Data.objectives.Length; i++)
        {
            var objective = quest.Data.objectives[i];
            if (objective != null)
            {
                string objectiveKey = GetObjectiveKey(quest.Data.questID, i);
                SaveBaseObjectiveData(objective, objectiveKey);
                
            }
        }
    }
    
    public static void LoadObjectiveStates(QuestInstance quest)
    {
        if (quest.Data == null || quest.Data.objectives == null) return;
        
        for (int i = 0; i < quest.Data.objectives.Length; i++)
        {
            var objective = quest.Data.objectives[i];
            if (objective != null)
            {
                string objectiveKey = GetObjectiveKey(quest.Data.questID, i);
                LoadBaseObjectiveData(objective, objectiveKey);
                LoadSpecificObjectiveData(objective, objectiveKey);
            }
        }
    }
    
    public static void ClearObjectiveData(QuestData quest)
    {
        for (int i = 0; i < quest.objectives.Length; i++)
        {
            string objectiveKey = GetObjectiveKey(quest.questID, i);
            ClearBaseObjectiveData(objectiveKey);
           
        }
    }
    
    private static string GetObjectiveKey(string questID, int objectiveIndex)
    {
        return $"Quest_{questID}_Objective_{objectiveIndex}";
    }
    
    private static void SaveBaseObjectiveData(QuestObjective objective, string key)
    {
        PlayerPrefs.SetInt($"{key}_Completed", objective.isCompleted ? 1 : 0);
        PlayerPrefs.SetInt($"{key}_Initialized", objective.isInitialized ? 1 : 0);
    }
    
    private static void LoadBaseObjectiveData(QuestObjective objective, string key)
    {
        objective.isCompleted = PlayerPrefs.GetInt($"{key}_Completed", 0) == 1;
        objective.isInitialized = PlayerPrefs.GetInt($"{key}_Initialized", 0) == 1;
    }
    
    private static void ClearBaseObjectiveData(string key)
    {
        PlayerPrefs.DeleteKey($"{key}_Completed");
        PlayerPrefs.DeleteKey($"{key}_Initialized");
    }
    
   
    
    private static void LoadSpecificObjectiveData(QuestObjective objective, string key)
    {
        switch (objective)
        {
            case SpecificEnemyKillObjective specificKillObj:
                LoadSpecificEnemyKillObjectiveData(specificKillObj, key);
                break;
        }
    }
    
   
    
    
    
    
    
   
    
   
    
    private static void ClearTargetedKillObjectiveData(string key)
    {
        int enemyTypeCount = PlayerPrefs.GetInt($"{key}_EnemyTypeCount", 0);
        for (int j = 0; j < enemyTypeCount; j++)
        {
            string enemyType = PlayerPrefs.GetString($"{key}_EnemyType_{j}", "");
            if (!string.IsNullOrEmpty(enemyType))
            {
                PlayerPrefs.DeleteKey($"{key}_KillCount_{enemyType}");
            }
            PlayerPrefs.DeleteKey($"{key}_EnemyType_{j}");
        }
        PlayerPrefs.DeleteKey($"{key}_EnemyTypeCount");
    }
    
    private static void SaveSpecificEnemyKillObjectiveData(SpecificEnemyKillObjective specificKillObj, string key)
    {
        var killedEnemyIDs = specificKillObj.GetKilledEnemyIDs();
        var targetEnemyIDs = specificKillObj.GetTargetEnemyIDs();
        
        // Save killed enemy IDs
        PlayerPrefs.SetInt($"{key}_KilledCount", killedEnemyIDs.Count);
        int index = 0;
        foreach (var killedID in killedEnemyIDs)
        {
            PlayerPrefs.SetString($"{key}_KilledID_{index}", killedID);
            index++;
        }
        
        // Save target enemy IDs
        PlayerPrefs.SetInt($"{key}_TargetCount", targetEnemyIDs.Count);
        for (int i = 0; i < targetEnemyIDs.Count; i++)
        {
            PlayerPrefs.SetString($"{key}_TargetID_{i}", targetEnemyIDs[i]);
        }
    }
    
    private static void LoadSpecificEnemyKillObjectiveData(SpecificEnemyKillObjective specificKillObj, string key)
    {
        // Load killed enemy IDs
        var killedEnemyIDs = new HashSet<string>();
        int killedCount = PlayerPrefs.GetInt($"{key}_KilledCount", 0);
        for (int i = 0; i < killedCount; i++)
        {
            string killedID = PlayerPrefs.GetString($"{key}_KilledID_{i}", "");
            if (!string.IsNullOrEmpty(killedID))
            {
                killedEnemyIDs.Add(killedID);
            }
        }
        specificKillObj.SetKilledEnemyIDs(killedEnemyIDs);
        
        // Load target enemy IDs
        var targetEnemyIDs = new List<string>();
        int targetCount = PlayerPrefs.GetInt($"{key}_TargetCount", 0);
        for (int i = 0; i < targetCount; i++)
        {
            string targetID = PlayerPrefs.GetString($"{key}_TargetID_{i}", "");
            if (!string.IsNullOrEmpty(targetID))
            {
                targetEnemyIDs.Add(targetID);
            }
        }
        specificKillObj.SetTargetEnemyIDs(targetEnemyIDs);
    }
    
    private static void ClearSpecificEnemyKillObjectiveData(string key)
    {
        // Clear killed enemy IDs
        int killedCount = PlayerPrefs.GetInt($"{key}_KilledCount", 0);
        for (int i = 0; i < killedCount; i++)
        {
            PlayerPrefs.DeleteKey($"{key}_KilledID_{i}");
        }
        PlayerPrefs.DeleteKey($"{key}_KilledCount");
        
        // Clear target enemy IDs
        int targetCount = PlayerPrefs.GetInt($"{key}_TargetCount", 0);
        for (int i = 0; i < targetCount; i++)
        {
            PlayerPrefs.DeleteKey($"{key}_TargetID_{i}");
        }
        PlayerPrefs.DeleteKey($"{key}_TargetCount");
    }
} 