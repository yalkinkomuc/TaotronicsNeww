using System;

public static class GameEvents
{
    // Düşman öldürme olayları
    public static event Action<Enemy> OnEnemyDefeated;
    public static event Action OnAllEnemiesDefeated;
    
    // Quest ile ilgili olaylar
    public static event Action<Quest> OnQuestStarted;
    public static event Action<Quest> OnQuestUpdated;
    public static event Action<Quest> OnQuestCompleted;
    public static event Action<QuestReward> OnQuestRewardGranted;
    
    // Oyun ilerlemesi olayları
    public static event Action<string> OnCheckpointReached;
    
    // Envanter ve item olayları
    public static event Action<Item> OnItemCollected;
    
    // Olay tetikleyiciler
    public static void EnemyDefeated(Enemy enemy)
    {
        OnEnemyDefeated?.Invoke(enemy);
    }
    
    public static void AllEnemiesDefeated()
    {
        OnAllEnemiesDefeated?.Invoke();
    }
    
    public static void QuestStarted(Quest quest)
    {
        OnQuestStarted?.Invoke(quest);
    }
    
    public static void QuestUpdated(Quest quest)
    {
        OnQuestUpdated?.Invoke(quest);
    }
    
    public static void QuestCompleted(Quest quest)
    {
        OnQuestCompleted?.Invoke(quest);
    }
    
    public static void QuestRewardGranted(QuestReward reward)
    {
        OnQuestRewardGranted?.Invoke(reward);
    }
    
    public static void CheckpointReached(string checkpointId)
    {
        OnCheckpointReached?.Invoke(checkpointId);
    }
    
    public static void ItemCollected(Item item)
    {
        OnItemCollected?.Invoke(item);
    }
} 