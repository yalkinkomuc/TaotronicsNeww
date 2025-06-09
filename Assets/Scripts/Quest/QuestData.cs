using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Quest", menuName = "Quest/New Quest")]
public class QuestData : ScriptableObject
{
    [Header("Basic Info")]
    public string questID;
    public string title;
    [TextArea(3, 5)]
    public string description;
    public QuestObjective[] objectives;
    
    [Space(10)]
    [Header("Quest Rewards")]
    [Tooltip("Quest tamamlandığında verilecek eşyalar")]
    public List<QuestReward> questRewards = new List<QuestReward>();
    
    [System.Serializable]
    public class QuestReward
    {
        [Tooltip("Verilecek eşya")]
        public ItemData item;
        [Tooltip("Kaç adet verilecek")]
        public int quantity = 1;
    }
    
    [Space(10)]
    [Header("Completion Behavior")]
    [Tooltip("Quest tamamlandığında QuestGiver ne yapacak?")]
    public QuestCompletionBehavior completionBehavior = new QuestCompletionBehavior();
}