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


}