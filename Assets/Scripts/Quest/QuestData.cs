using UnityEngine;

[CreateAssetMenu(fileName = "Quest", menuName = "Quest/New Quest")]
public class QuestData : ScriptableObject
{
    public string questID;
    public string title;
    public string description;
    public QuestObjective[] objectives;
}