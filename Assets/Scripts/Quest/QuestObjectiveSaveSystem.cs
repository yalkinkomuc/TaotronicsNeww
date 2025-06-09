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
                SaveSpecificObjectiveData(objective, objectiveKey);
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
            ClearSpecificObjectiveData(quest.objectives[i], objectiveKey);
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
    
    private static void SaveSpecificObjectiveData(QuestObjective objective, string key)
    {
        switch (objective)
        {
            case KillObjective killObj:
                SaveKillObjectiveData(killObj, key);
                break;
            case TargetedKillObjective targetedKillObj:
                SaveTargetedKillObjectiveData(targetedKillObj, key);
                break;
        }
    }
    
    private static void LoadSpecificObjectiveData(QuestObjective objective, string key)
    {
        switch (objective)
        {
            case KillObjective killObj:
                LoadKillObjectiveData(killObj, key);
                break;
            case TargetedKillObjective targetedKillObj:
                LoadTargetedKillObjectiveData(targetedKillObj, key);
                break;
        }
    }
    
    private static void ClearSpecificObjectiveData(QuestObjective objective, string key)
    {
        switch (objective)
        {
            case KillObjective _:
                PlayerPrefs.DeleteKey($"{key}_CurrentAmount");
                break;
            case TargetedKillObjective _:
                ClearTargetedKillObjectiveData(key);
                break;
        }
    }
    
    private static void SaveKillObjectiveData(KillObjective killObj, string key)
    {
        var currentAmountField = typeof(KillObjective).GetField("currentAmount", 
            BindingFlags.NonPublic | BindingFlags.Instance);
        
        if (currentAmountField != null)
        {
            int currentAmount = (int)currentAmountField.GetValue(killObj);
            PlayerPrefs.SetInt($"{key}_CurrentAmount", currentAmount);
        }
    }
    
    private static void LoadKillObjectiveData(KillObjective killObj, string key)
    {
        int currentAmount = PlayerPrefs.GetInt($"{key}_CurrentAmount", 0);
        
        var currentAmountField = typeof(KillObjective).GetField("currentAmount", 
            BindingFlags.NonPublic | BindingFlags.Instance);
        
        if (currentAmountField != null)
        {
            currentAmountField.SetValue(killObj, currentAmount);
        }
    }
    
    private static void SaveTargetedKillObjectiveData(TargetedKillObjective targetedKillObj, string key)
    {
        var killedCountsField = typeof(TargetedKillObjective).GetField("killedCounts", 
            BindingFlags.NonPublic | BindingFlags.Instance);
        
        if (killedCountsField != null)
        {
            var killedCounts = killedCountsField.GetValue(targetedKillObj) as Dictionary<string, int>;
            if (killedCounts != null)
            {
                PlayerPrefs.SetInt($"{key}_EnemyTypeCount", killedCounts.Count);
                
                int index = 0;
                foreach (var kvp in killedCounts)
                {
                    PlayerPrefs.SetString($"{key}_EnemyType_{index}", kvp.Key);
                    PlayerPrefs.SetInt($"{key}_KillCount_{kvp.Key}", kvp.Value);
                    index++;
                }
            }
        }
    }
    
    private static void LoadTargetedKillObjectiveData(TargetedKillObjective targetedKillObj, string key)
    {
        var killedCountsField = typeof(TargetedKillObjective).GetField("killedCounts", 
            BindingFlags.NonPublic | BindingFlags.Instance);
        
        if (killedCountsField != null)
        {
            var killedCounts = new Dictionary<string, int>();
            int enemyTypeCount = PlayerPrefs.GetInt($"{key}_EnemyTypeCount", 0);
            
            for (int i = 0; i < enemyTypeCount; i++)
            {
                string enemyType = PlayerPrefs.GetString($"{key}_EnemyType_{i}", "");
                if (!string.IsNullOrEmpty(enemyType))
                {
                    int killCount = PlayerPrefs.GetInt($"{key}_KillCount_{enemyType}", 0);
                    killedCounts[enemyType] = killCount;
                }
            }
            
            killedCountsField.SetValue(targetedKillObj, killedCounts);
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
} 