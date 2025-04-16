using UnityEngine;

[CreateAssetMenu(fileName = "KillObjective", menuName = "Quest/Objective/Kill")]
public class KillObjective : QuestObjective
{
    public string enemyType;
    public int requiredAmount;
    private int currentAmount;

    public override void Initialize()
    {
        currentAmount = 0;
        isCompleted = false;
    }

    public override void HandleEvent(string eventName, object data)
    {
        if (eventName == "EnemyKilled" && data is string killedType && killedType == enemyType)
        {
            currentAmount++;
            if (currentAmount >= requiredAmount)
                isCompleted = true;
        }
    }
}