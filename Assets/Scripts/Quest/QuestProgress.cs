using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class QuestProgress
{
    public string questId;
    public Dictionary<string, int> killedEnemyCounts = new Dictionary<string, int>();
    
    public QuestProgress(string questId)
    {
        this.questId = questId;
    }
    
    public void AddKilledEnemy(string enemyTag)
    {
        if (!killedEnemyCounts.ContainsKey(enemyTag))
        {
            killedEnemyCounts[enemyTag] = 0;
        }
        killedEnemyCounts[enemyTag]++;
    }
    
    public int GetKilledCount(string enemyTag)
    {
        return killedEnemyCounts.ContainsKey(enemyTag) ? killedEnemyCounts[enemyTag] : 0;
    }
} 