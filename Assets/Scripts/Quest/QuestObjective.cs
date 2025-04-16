using UnityEngine;

public abstract class QuestObjective : ScriptableObject
{
    public string description;
    public bool isCompleted { get; protected set; }

    public abstract void Initialize();
    public abstract void HandleEvent(string eventName, object data);
}