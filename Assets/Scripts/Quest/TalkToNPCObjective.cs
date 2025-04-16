using UnityEngine;

[CreateAssetMenu(fileName = "TalkToNPCObjective", menuName = "Quest/Objective/TalkToNPC")]
public class TalkToNPCObjective : QuestObjective
{
    public string npcID;

    public override void Initialize()
    {
        isCompleted = false;
    }

    public override void HandleEvent(string eventName, object data)
    {
        if (eventName == "TalkedToNPC" && data is string talkedID && talkedID == npcID)
        {
            isCompleted = true;
        }
    }
}