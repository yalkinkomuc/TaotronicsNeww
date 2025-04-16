using System;

public static class GameEvents
{
    // Düşman öldürme olayları
    public static event Action<Enemy> OnEnemyDefeated;
    public static event Action OnAllEnemiesDefeated;
    
    // Quest ile ilgili olaylar
    
    
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
    
    public static void CheckpointReached(string checkpointId)
    {
        OnCheckpointReached?.Invoke(checkpointId);
    }
    
    public static void ItemCollected(Item item)
    {
        OnItemCollected?.Invoke(item);
    }
} 