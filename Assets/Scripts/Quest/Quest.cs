using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public class Quest
{
    public string id;
    public string title;
    public string description;
    
    public bool isActive;
    public bool isCompleted;
    
    public QuestObjective[] objectives;
    
    // Ödül sistemi
    public List<QuestReward> rewards = new List<QuestReward>();
    
    public Quest(string id, string title, string description)
    {
        this.id = id;
        this.title = title;
        this.description = description;
        
        isActive = false;
        isCompleted = false;
    }
    
    public void StartQuest()
    {
        isActive = true;
        GameEvents.QuestStarted(this);
    }
    
    public void UpdateQuest()
    {
        GameEvents.QuestUpdated(this);
        
        // Tüm hedefler tamamlandı mı kontrol et
        bool allObjectivesComplete = true;
        foreach (QuestObjective objective in objectives)
        {
            if (!objective.isCompleted)
            {
                allObjectivesComplete = false;
                break;
            }
        }
        
        if (allObjectivesComplete && !isCompleted)
        {
            CompleteQuest();
        }
    }
    
    public void CompleteQuest()
    {
        isActive = false;
        isCompleted = true;
        
        // Görev tamamlandığında ödülleri ver
        GiveRewards();
        
        GameEvents.QuestCompleted(this);
    }
    
    private void GiveRewards()
    {
        if (rewards.Count > 0)
        {
            foreach (QuestReward reward in rewards)
            {
                GameEvents.QuestRewardGranted(reward);
                
                // Ödül tipine göre işlem yap
                switch (reward.rewardType)
                {
                    case RewardType.Item:
                        // Item vermek için Inventory sistemine ekle
                        if (Inventory.instance != null && reward.itemDataReward != null)
                        {
                            Inventory.instance.AddItem(reward.itemDataReward);
                        }
                        break;
                        
                    case RewardType.Gold:
                        // Para vermek için
                        if (PlayerManager.instance != null && PlayerManager.instance.player != null)
                        {
                            PlayerManager.instance.player.AddGold(reward.goldAmount);
                        }
                        break;
                        
                    case RewardType.Experience:
                        // Deneyim puanı vermek için
                        if (PlayerManager.instance != null && PlayerManager.instance.player != null)
                        {
                            PlayerManager.instance.player.AddExperience(reward.experienceAmount);
                        }
                        break;
                }
            }
        }
    }
    
    // Göreve ödül eklemek için metodlar
    public void AddItemReward(ItemData item)
    {
        QuestReward reward = new QuestReward();
        reward.rewardType = RewardType.Item;
        reward.itemDataReward = item;
        rewards.Add(reward);
    }
    
    public void AddGoldReward(int amount)
    {
        QuestReward reward = new QuestReward();
        reward.rewardType = RewardType.Gold;
        reward.goldAmount = amount;
        rewards.Add(reward);
    }
    
    public void AddExperienceReward(int amount)
    {
        QuestReward reward = new QuestReward();
        reward.rewardType = RewardType.Experience;
        reward.experienceAmount = amount;
        rewards.Add(reward);
    }
}

[Serializable]
public class QuestObjective
{
    public string description;
    public int currentAmount;
    public int requiredAmount;
    public bool isCompleted;
    
    public QuestObjective(string description, int requiredAmount)
    {
        this.description = description;
        this.requiredAmount = requiredAmount;
        this.currentAmount = 0;
        this.isCompleted = false;
    }
    
    public void UpdateProgress(int amount)
    {
        currentAmount += amount;
        if (currentAmount >= requiredAmount && !isCompleted)
        {
            isCompleted = true;
        }
    }
}

[Serializable]
public enum RewardType
{
    Item,
    Gold,
    Experience
}

[Serializable]
public class QuestReward
{
    public RewardType rewardType;
    public ItemData itemDataReward;
    public int goldAmount;
    public int experienceAmount;
    
    public string GetRewardDescription()
    {
        switch (rewardType)
        {
            case RewardType.Item:
                return itemDataReward != null ? itemDataReward.itemName : "Bilinmeyen Eşya";
            case RewardType.Gold:
                return $"{goldAmount} Altın";
            case RewardType.Experience:
                return $"{experienceAmount} Tecrübe Puanı";
            default:
                return "Bilinmeyen Ödül";
        }
    }
} 