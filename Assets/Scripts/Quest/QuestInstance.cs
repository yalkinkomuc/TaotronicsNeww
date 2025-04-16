using UnityEngine;

public class QuestInstance
{
    public QuestData data;
    public bool IsCompleted => System.Array.TrueForAll(objectives, o => o.isCompleted);

    private QuestObjective[] objectives;

    public QuestInstance(QuestData questData)
    {
        data = questData;
        objectives = new QuestObjective[questData.objectives.Length];

        for (int i = 0; i < objectives.Length; i++)
        {
            objectives[i] = Object.Instantiate(questData.objectives[i]); // Runtime instance
            objectives[i].Initialize();
        }
    }

    public void HandleEvent(string eventName, object data)
    {
        foreach (var obj in objectives)
        {
            if (!obj.isCompleted)
            {
             obj.HandleEvent(eventName, data);
             Debug.Log($"handled event: {eventName}");
            }

        }
    }
}