using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class QuestManager : MonoBehaviour
{
    public static QuestManager instance;
    
    [Header("Quest Configuration")]
    public List<QuestData> availableQuests;
    
    private List<QuestInstance> activeQuests = new List<QuestInstance>();
    private HashSet<string> completedQuests = new HashSet<string>();

    private void Awake()
    {
        InitializeSingleton();
        LoadQuestProgress();
    }

    #region Quest Management
    
    public void StartQuest(QuestData questData)
    {
        if (!CanStartQuest(questData)) return;
        
        var quest = new QuestInstance { Data = questData };
        activeQuests.Add(quest);
        
        // Quest başladığında ilk objective'i initialize et
        InitializeFirstObjective(quest);
        
        Debug.Log($"Started quest: {questData.title}");
        SaveQuestProgress();
    }

    public void RaiseEvent(string eventName, object data)
    {
        var questsToComplete = ProcessQuestEvents(eventName, data);
        CompleteQuests(questsToComplete);
        
        SaveQuestProgress();
    }

    public bool IsQuestCompleted(string questID)
    {
        return !string.IsNullOrEmpty(questID) && completedQuests.Contains(questID);
    }

    public bool IsQuestActive(string questID)
    {
        if (string.IsNullOrEmpty(questID)) return false;
        return activeQuests.Any(q => q.Data != null && q.Data.questID == questID);
    }

    public List<QuestInstance> GetActiveQuests()
    {
        return new List<QuestInstance>(activeQuests);
    }

    public void ResetAllQuests()
    {
        activeQuests.Clear();
        completedQuests.Clear();
        ResetObjectiveStates();
        QuestSaveSystem.ClearAllQuestData(availableQuests);
        
        Debug.Log("All quests and objectives reset.");
    }

    #endregion

    #region Debug

    [ContextMenu("Debug Quest Status")]
    public void DebugQuestStatus()
    {
        Debug.Log($"Active Quests: {activeQuests.Count}, Completed Quests: {completedQuests.Count}");
        
        DebugActiveQuests();
        DebugCompletedQuests();
    }

    #endregion

    #region Private Methods

    private void InitializeSingleton()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private bool CanStartQuest(QuestData questData)
    {
        if (questData == null)
        {
            Debug.LogWarning("Quest data is null");
            return false;
        }

        if (IsQuestCompleted(questData.questID))
        {
            Debug.Log($"Quest already completed: {questData.title}");
            return false;
        }

        if (activeQuests.Any(q => q.Data == questData))
        {
            Debug.Log($"Quest already active: {questData.title}");
            return false;
        }

        return true;
    }

    private List<QuestInstance> ProcessQuestEvents(string eventName, object data)
    {
        var questsToComplete = new List<QuestInstance>();

        foreach (var quest in activeQuests)
        {
            quest.HandleEvent(eventName, data);
            
            if (quest.IsCompleted && !string.IsNullOrEmpty(quest.Data.questID))
            {
                questsToComplete.Add(quest);
            }
        }

        return questsToComplete;
    }

    private void CompleteQuests(List<QuestInstance> questsToComplete)
    {
        foreach (var quest in questsToComplete)
        {
            completedQuests.Add(quest.Data.questID);
            activeQuests.Remove(quest);
            
            // Quest completion rewards ver
            GiveQuestRewards(quest.Data);
            
            Debug.Log($"🎉 Quest completed: {quest.Data.title}");
        }
    }
    
    private void GiveQuestRewards(QuestData questData)
    {
        if (questData?.questRewards == null || questData.questRewards.Count == 0) return;
        
        foreach (var reward in questData.questRewards)
        {
            if (reward.item == null) continue;
            
            // Her quantity için ayrı ayrı ekle
            for (int i = 0; i < reward.quantity; i++)
            {
                Inventory.instance?.AddItem(reward.item);
            }
            
            Debug.Log($"🎁 Quest Reward: {reward.quantity}x {reward.item.itemName}");
        }
        
        Debug.Log($"🎉 Total {questData.questRewards.Count} quest reward types given for: {questData.title}");
    }

    private void ResetObjectiveStates()
    {
        foreach (var quest in availableQuests)
        {
            if (quest?.objectives == null) continue;
            
            foreach (var objective in quest.objectives)
            {
                if (objective != null)
                {
                    objective.isCompleted = false;
                    objective.isInitialized = false;
                }
            }
        }
    }

    private void LoadQuestProgress()
    {
        var saveData = QuestSaveSystem.LoadQuestProgress(availableQuests);
        activeQuests = saveData.activeQuests;
        completedQuests = saveData.completedQuests;
    }

    private void SaveQuestProgress()
    {
        QuestSaveSystem.SaveQuestProgress(activeQuests, completedQuests);
    }

    private void DebugActiveQuests()
    {
        Debug.Log("=== ACTIVE QUESTS ===");
        foreach (var quest in activeQuests)
        {
            if (quest.Data == null) continue;
            
            Debug.Log($"Quest: {quest.Data.title} (ID: {quest.Data.questID})");
            Debug.Log($"  - Objective Index: {quest.currentObjectiveIndex}");
            Debug.Log($"  - Is Completed: {quest.IsCompleted}");
            
            if (quest.CurrentObjective != null)
            {
                Debug.Log($"  - Current: {quest.CurrentObjective.description}");
                Debug.Log($"  - Status: {(quest.CurrentObjective.isCompleted ? "Completed" : "In Progress")}");
            }
        }
    }

    private void DebugCompletedQuests()
    {
        Debug.Log("=== COMPLETED QUESTS ===");
        foreach (var completedQuestID in completedQuests)
        {
            var questData = availableQuests.FirstOrDefault(q => q?.questID == completedQuestID);
            string questTitle = questData?.title ?? "Unknown Quest";
            Debug.Log($"Completed: {questTitle} (ID: {completedQuestID})");
        }
    }

    // Quest başladığında ilk objective'i initialize et
    private void InitializeFirstObjective(QuestInstance quest)
    {
        if (quest?.Data?.objectives == null || quest.Data.objectives.Length == 0)
        {
            Debug.LogWarning($"Quest '{quest?.Data?.title}' has no objectives!");
            return;
        }

        var firstObjective = quest.CurrentObjective;
        if (firstObjective != null && !firstObjective.isInitialized)
        {
            Debug.Log($"Initializing first objective: {firstObjective.GetType().Name}");
            firstObjective.Initialize();
        }
    }

    #endregion
}
