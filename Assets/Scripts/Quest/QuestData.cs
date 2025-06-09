using UnityEngine;

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
    [Header("Completion Behavior")]
    [Tooltip("Quest tamamlandığında QuestGiver ne yapacak?")]
    public QuestCompletionBehavior completionBehavior = new QuestCompletionBehavior();
}