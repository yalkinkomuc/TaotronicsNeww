using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class QuestManager : MonoBehaviour
{
    
    public static QuestManager instance;
    
    public List<QuestData> availableQuests;
    private List<QuestInstance> activeQuests = new List<QuestInstance>();

    private void Awake()
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

    public void StartQuest(QuestData questData)
    {
        if (questData != null && activeQuests.All(q => q.Data != questData))
        {
            var quest = new QuestInstance();
            quest.Data = questData;
            activeQuests.Add(quest);
            Debug.Log($"Started quest: {questData.title}");
        }
    }


    public void RaiseEvent(string eventName, object data)
    {
        Debug.Log($"Event Raised: {eventName}");  // Event'in doğru şekilde tetiklendiğini görmek için debug ekleyelim

        foreach (var quest in activeQuests)
        {
            quest.HandleEvent(eventName, data);
        }
    }

    // Aktif questleri döndürür (sadece okunabilir)
    public List<QuestInstance> GetActiveQuests()
    {
        return new List<QuestInstance>(activeQuests);
    }
    
    // Tüm questleri ve objective'leri sıfırlar
    public void ResetAllQuests()
    {
        // Aktif questleri temizle
        activeQuests.Clear();
        
        // Tüm mevcut questlerin objective'lerini sıfırla
        foreach (var quest in availableQuests)
        {
            if (quest != null && quest.objectives != null)
            {
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
        
        Debug.Log("Tüm questler ve objective'ler sıfırlandı.");
    }
}