using UnityEngine;

public class QuestGiver : DialogueNPC
{
    [SerializeField] private QuestData questToGive;
    [SerializeField] private DialogueData questDialogue;
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
        
        // Diyaloğu başlat
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
            
            //Debug.Log($"Quest verildi: {questToGive.title}");
        }
    }

    // Oyun başladığında quest'in verilmiş durumunu sıfırla
    private void OnEnable()
    {
        questGiven = false;
    }
} 