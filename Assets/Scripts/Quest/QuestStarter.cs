using UnityEngine;
using UnityEngine.Serialization;

// Not: Diyalog sistemine entegre olmak için QuestGiver sınıfını kullanmanız önerilir.
public class QuestStarter : MonoBehaviour
{
    [FormerlySerializedAs("questIndexToStart")] public QuestData questToStart;
    
    [Tooltip("Oyun başladığında quest'i otomatik başlatır")]
    public bool startOnAwake = true;

    void Start()
    {
        if (startOnAwake && questToStart != null)
        {
            QuestManager.instance.StartQuest(questToStart);
        }
    }
    
    // QuestStarter'ı harici olarak tetikleyebilmek için manuel başlatma metodu
    public void StartQuest()
    {
        if (questToStart != null)
        {
            QuestManager.instance.StartQuest(questToStart);
        }
    }
}