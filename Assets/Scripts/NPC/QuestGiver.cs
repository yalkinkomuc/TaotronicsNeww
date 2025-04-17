using UnityEngine;

public class QuestGiver : DialogueNPC
{
    [SerializeField] private QuestData questToGive;
    [SerializeField] private DialogueData questDialogue;
    [SerializeField] private DialogueData questInProgressDialogue; // Quest devam ederken gösterilecek diyalog
    [SerializeField] private DialogueData questCompletedDialogue; // Quest tamamlandığında gösterilecek diyalog
    private bool questGiven = false;

    public override void Interact()
    {
        // DialogueManager kontrolü
        if (DialogueManager.Instance == null)
        {
            Debug.LogError("DialogueManager bulunamadı!");
            return;
        }
        
        Debug.Log("QuestGiver Interact");
        
        // Quest durumunu kontrol et
        if (questToGive != null)
        {
            QuestInstance activeQuest = FindActiveQuest();
            
            if (activeQuest != null)
            {
                // Quest devam ediyor, güncel durumu kontrol et
                if (activeQuest.IsCompleted)
                {
                    // Quest tamamlandı
                    ShowQuestCompletedDialogue();
                    return;
                }
                else
                {
                    // Quest devam ediyor
                    ShowQuestInProgressDialogue(activeQuest);
                    return;
                }
            }
        }
        
        // Quest henüz verilmemiş, normal diyaloğu göster
        if (questDialogue != null)
        {
            // Diyalog bittiğinde quest'i tetikleyecek event'e abone ol
            DialogueManager.Instance.OnDialogueEnd += GiveQuestAfterDialogue;
            DialogueManager.Instance.StartDialogue(questDialogue);
        }
        else
        {
            // Diyalog yoksa doğrudan quest'i ver
            GiveQuest();
        }
    }
    
    // Aktif quest'i bul
    private QuestInstance FindActiveQuest()
    {
        if (QuestManager.instance == null || questToGive == null)
            return null;
            
        foreach (var quest in QuestManager.instance.GetActiveQuests())
        {
            if (quest.Data == questToGive)
                return quest;
        }
        
        return null;
    }
    
    // Quest devam ederken gösterilecek diyalog
    private void ShowQuestInProgressDialogue(QuestInstance activeQuest)
    {
        if (questInProgressDialogue != null)
        {
            DialogueManager.Instance.StartDialogue(questInProgressDialogue);
        }
        else
        {
            // Özel diyalog yoksa, current objective'in açıklamasını göster
            if (activeQuest.CurrentObjective != null)
            {
                string objectiveDesc = activeQuest.CurrentObjective.description;
                DialogueData tempDialogue = CreateTempDialogue(
                    $"Görevin devam ediyor. Şu anki hedefin: {objectiveDesc}"
                );
                DialogueManager.Instance.StartDialogue(tempDialogue);
            }
            else
            {
                Debug.LogWarning("CurrentObjective null, bu bir hata olabilir.");
            }
        }
    }
    
    // Quest tamamlandığında gösterilecek diyalog
    private void ShowQuestCompletedDialogue()
    {
        if (questCompletedDialogue != null)
        {
            DialogueManager.Instance.StartDialogue(questCompletedDialogue);
        }
        else
        {
            DialogueData tempDialogue = CreateTempDialogue("Görevi başarıyla tamamladın! Teşekkürler.");
            DialogueManager.Instance.StartDialogue(tempDialogue);
        }
    }
    
    // Geçici diyalog oluştur
    private DialogueData CreateTempDialogue(string message)
    {
        DialogueData tempDialogue = ScriptableObject.CreateInstance<DialogueData>();
        tempDialogue.characterName = name;
        tempDialogue.dialogueLines = new string[] { message };
        return tempDialogue;
    }

    private void GiveQuestAfterDialogue()
    {
        // Olay tetiklendiğinde aboneliği kaldır
        DialogueManager.Instance.OnDialogueEnd -= GiveQuestAfterDialogue;
        
        // Quest'i ver
        GiveQuest();
    }

    private void GiveQuest()
    {
        if (questToGive != null && !questGiven)
        {
            QuestManager.instance.StartQuest(questToGive);
            questGiven = true;
            
            // Görevin verildiğini TalkedToNPC event'i ile bildir
            QuestManager.instance.RaiseEvent("TalkedToNPC", npcID);
            
            Debug.Log($"Quest verildi: {questToGive.title}");
        }
    }

    // Oyun başladığında quest'in verilmiş durumunu sıfırla
    private void OnEnable()
    {
        questGiven = false;
    }
} 