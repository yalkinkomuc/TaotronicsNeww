using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public static class QuestSaveSystem
{
    private const string ACTIVE_QUESTS_KEY = "ActiveQuestIDs";
    private const string OBJECTIVE_INDICES_KEY = "QuestObjectiveIndices";
    private const string COMPLETED_QUESTS_KEY = "CompletedQuests";
    
    public static void SaveQuestProgress(List<QuestInstance> activeQuests, HashSet<string> completedQuests)
    {
        SaveActiveQuests(activeQuests);
        SaveCompletedQuests(completedQuests);
        PlayerPrefs.Save();
        
    }
    
    public static QuestSaveData LoadQuestProgress(List<QuestData> availableQuests)
    {
        var saveData = new QuestSaveData();
        
        // Load completed quests
        if (PlayerPrefs.HasKey(COMPLETED_QUESTS_KEY))
        {
            string completedJson = PlayerPrefs.GetString(COMPLETED_QUESTS_KEY);
            var completedList = JsonUtility.FromJson<SerializableStringList>(completedJson);
            saveData.completedQuests = new HashSet<string>(completedList.items);
        }
        
        // Load active quests
        if (PlayerPrefs.HasKey(ACTIVE_QUESTS_KEY))
        {
            saveData.activeQuests = LoadActiveQuests(availableQuests);
        }
        
        return saveData;
    }
    
    public static void ClearAllQuestData(List<QuestData> availableQuests)
    {
        PlayerPrefs.DeleteKey(ACTIVE_QUESTS_KEY);
        PlayerPrefs.DeleteKey(OBJECTIVE_INDICES_KEY);
        PlayerPrefs.DeleteKey(COMPLETED_QUESTS_KEY);
        
        ClearQuestObjectiveData(availableQuests);
        ClearQuestGiverData(availableQuests);
        
        PlayerPrefs.Save();
    }
    
    private static void SaveActiveQuests(List<QuestInstance> activeQuests)
    {
        var questIDs = new List<string>();
        var objectiveIndices = new List<int>();
        
        foreach (var quest in activeQuests)
        {
            if (quest.Data != null && !string.IsNullOrEmpty(quest.Data.questID))
            {
                questIDs.Add(quest.Data.questID);
                objectiveIndices.Add(quest.currentObjectiveIndex);
                QuestObjectiveSaveSystem.SaveObjectiveStates(quest);
            }
        }
        
        string questIDsJson = JsonUtility.ToJson(new SerializableStringList { items = questIDs.ToArray() });
        PlayerPrefs.SetString(ACTIVE_QUESTS_KEY, questIDsJson);
        
        string indicesJson = JsonUtility.ToJson(new SerializableIntList { items = objectiveIndices.ToArray() });
        PlayerPrefs.SetString(OBJECTIVE_INDICES_KEY, indicesJson);
    }
    
    private static void SaveCompletedQuests(HashSet<string> completedQuests)
    {
        string completedJson = JsonUtility.ToJson(new SerializableStringList { items = completedQuests.ToArray() });
        PlayerPrefs.SetString(COMPLETED_QUESTS_KEY, completedJson);
    }
    
    private static List<QuestInstance> LoadActiveQuests(List<QuestData> availableQuests)
    {
        var activeQuests = new List<QuestInstance>();
        
        string questIDsJson = PlayerPrefs.GetString(ACTIVE_QUESTS_KEY);
        var questIDs = JsonUtility.FromJson<SerializableStringList>(questIDsJson);
        
        string indicesJson = PlayerPrefs.GetString(OBJECTIVE_INDICES_KEY, "");
        var objectiveIndices = new SerializableIntList();
        if (!string.IsNullOrEmpty(indicesJson))
        {
            objectiveIndices = JsonUtility.FromJson<SerializableIntList>(indicesJson);
        }
        
        for (int i = 0; i < questIDs.items.Length; i++)
        {
            string questID = questIDs.items[i];
            QuestData questData = FindQuestByID(availableQuests, questID);
            
            if (questData != null)
            {
                var quest = new QuestInstance();
                quest.Data = questData;
                
                if (i < objectiveIndices.items.Length)
                {
                    quest.currentObjectiveIndex = objectiveIndices.items[i];
                }
                
                QuestObjectiveSaveSystem.LoadObjectiveStates(quest);
                activeQuests.Add(quest);
            }
        }
        
        return activeQuests;
    }
    
    private static QuestData FindQuestByID(List<QuestData> availableQuests, string questID)
    {
        return availableQuests.FirstOrDefault(q => q != null && q.questID == questID);
    }
    
    private static void ClearQuestObjectiveData(List<QuestData> availableQuests)
    {
        foreach (var quest in availableQuests)
        {
            if (quest != null && !string.IsNullOrEmpty(quest.questID) && quest.objectives != null)
            {
                QuestObjectiveSaveSystem.ClearObjectiveData(quest);
            }
        }
    }
    
    private static void ClearQuestGiverData(List<QuestData> availableQuests)
    {
        foreach (var quest in availableQuests)
        {
            if (quest != null && !string.IsNullOrEmpty(quest.questID))
            {
                string givenKey = $"QuestGiven_{quest.questID}";
                PlayerPrefs.DeleteKey(givenKey);
                
                string completionKey = $"QuestCompletionAction_{quest.questID}";
                PlayerPrefs.DeleteKey(completionKey);
            }
        }
    }
}

[System.Serializable]
public class QuestSaveData
{
    public List<QuestInstance> activeQuests = new List<QuestInstance>();
    public HashSet<string> completedQuests = new HashSet<string>();
}

[System.Serializable]
public class SerializableStringList
{
    public string[] items;
}

[System.Serializable]
public class SerializableIntList
{
    public int[] items;
} 