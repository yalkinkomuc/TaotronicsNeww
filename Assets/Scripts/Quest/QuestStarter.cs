using UnityEngine;
using UnityEngine.Serialization;

public class QuestStarter : MonoBehaviour
{
    [FormerlySerializedAs("questIndexToStart")] public QuestData questToStart;

    void Start()
    {
        QuestManager.instance.StartQuest(questToStart);
    }
}