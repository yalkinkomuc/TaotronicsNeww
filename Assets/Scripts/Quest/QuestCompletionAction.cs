using UnityEngine;

[System.Serializable]
public enum QuestCompletionAction
{
    None,                    // Hiçbir şey yapma, QuestGiver kalır
    HideQuestGiver,         // QuestGiver'ı gizle (deactivate)
    DestroyQuestGiver,      // QuestGiver'ı tamamen yok et
    ChangeDialogue,         // Sadece diyaloğu değiştir
    TeleportQuestGiver,     // QuestGiver'ı başka bir yere taşı
    RunCustomAction         // Custom script çalıştır
}

[System.Serializable]
public class QuestCompletionBehavior
{
    [Header("Completion Action")]
    public QuestCompletionAction action = QuestCompletionAction.None;
    
    [Header("Action Parameters")]
    [Tooltip("Custom action için script adı veya özel parametre")]
    public string customParameter = "";
    
    [Space(5)]
    [Tooltip("ChangeDialogue seçilirse bu diyalog kullanılır")]
    public DialogueData postCompletionDialogue;
    
    [Space(5)]
    [Tooltip("TeleportQuestGiver seçilirse bu pozisyona taşınır")]
    public Vector3 teleportPosition = Vector3.zero;
    
    [Space(5)]
    [Tooltip("Aksiyon gerçekleşmeden önce beklenecek süre (saniye)")]
    public float delayBeforeAction = 0f;
} 