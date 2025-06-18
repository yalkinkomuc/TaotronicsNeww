using UnityEngine;
using System.Collections;

public class QuestGiver : DialogueNPC
{
    [SerializeField] private QuestData questToGive;
    [SerializeField] private DialogueData questDialogue;
    [SerializeField] private DialogueData questInProgressDialogue; // Quest devam ederken gösterilecek diyalog
    [SerializeField] private DialogueData questCompletedDialogue; // Quest tamamlandığında gösterilecek diyalog
    private bool questGiven = false;
    private bool completionActionExecuted = false;

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
        if (questToGive != null && QuestManager.instance != null)
        {
            // Önce quest tamamlanmış mı kontrol et
            if (QuestManager.instance.IsQuestCompleted(questToGive.questID))
            {
                // Quest tamamlandı, completion action'ı henüz çalışmadıysa çalıştır
                if (!completionActionExecuted)
                {
                    ExecuteCompletionAction();
                }
                
                // Eğer completion behavior DestroyQuestGiver veya HideQuestGiver ise,
                // interaction prompt zaten gizlenmiş olmalı, ama ekstra kontrol için:
                if (questToGive.completionBehavior?.action == QuestCompletionAction.DestroyQuestGiver ||
                    questToGive.completionBehavior?.action == QuestCompletionAction.HideQuestGiver)
                {
                    return; // Hiçbir etkileşim yapma
                }
                
                // Quest tamamlandı, completed diyaloğu göster
                ShowQuestCompletedDialogue();
                return;
            }
            
            QuestInstance activeQuest = FindActiveQuest();
            
            if (activeQuest != null)
            {
                // Quest devam ediyor
                ShowQuestInProgressDialogue(activeQuest);
                return;
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
        DialogueData dialogueToShow = questCompletedDialogue;
        
        // Eğer completion behavior'da ChangeDialogue seçilmişse o diyaloğu kullan
        if (questToGive?.completionBehavior?.action == QuestCompletionAction.ChangeDialogue &&
            questToGive.completionBehavior.postCompletionDialogue != null)
        {
            dialogueToShow = questToGive.completionBehavior.postCompletionDialogue;
        }
        
        if (dialogueToShow != null)
        {
            // Quest completed dialogue bittiğinde prompt'u kalıcı olarak kapat
            DialogueManager.Instance.OnDialogueEnd += HidePromptPermanently;
            DialogueManager.Instance.StartDialogue(dialogueToShow);
        }
        else
        {
            DialogueData tempDialogue = CreateTempDialogue("Görevi başarıyla tamamladın! Teşekkürler.");
            // Temp dialogue bittiğinde de prompt'u kapat
            DialogueManager.Instance.OnDialogueEnd += HidePromptPermanently;
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
        if (questToGive != null && !questGiven && QuestManager.instance != null)
        {
            QuestManager.instance.StartQuest(questToGive);
            questGiven = true;
            
            // Quest verilmiş durumunu kaydet
            SaveQuestGivenState();
            
            // Görevin verildiğini TalkedToNPC event'i ile bildir
            QuestManager.instance.RaiseEvent("TalkedToNPC", npcID);
            
            Debug.Log($"Quest verildi: {questToGive.title}");
        }
        else if (QuestManager.instance == null)
        {
            Debug.LogWarning("QuestManager instance is null! Cannot give quest.");
        }
    }

    // Oyun başladığında quest'in verilmiş durumunu yükle
    private void OnEnable()
    {
        LoadQuestGivenState();
        
        // QuestManager hazır olana kadar bekle
        if (QuestManager.instance != null)
        {
            LoadCompletionActionState();
        }
        else
        {
            // QuestManager henüz hazır değilse, biraz bekleyip tekrar dene
            StartCoroutine(DelayedLoadCompletionState());
        }
    }
    
    private void OnDisable()
    {
        // Event cleanup'ları
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.OnDialogueEnd -= HidePromptPermanently;
        }
    }
    
    private IEnumerator DelayedLoadCompletionState()
    {
        // QuestManager instance'ın hazır olmasını bekle
        while (QuestManager.instance == null)
        {
            yield return new WaitForSeconds(0.1f);
        }
        
        LoadCompletionActionState();
    }
    
    // Quest verilmiş durumunu kaydet
    private void SaveQuestGivenState()
    {
        if (questToGive != null && !string.IsNullOrEmpty(questToGive.questID))
        {
            string key = $"QuestGiven_{questToGive.questID}";
            PlayerPrefs.SetInt(key, questGiven ? 1 : 0);
            PlayerPrefs.Save();
        }
    }
    
    // Quest verilmiş durumunu yükle
    private void LoadQuestGivenState()
    {
        if (questToGive != null && !string.IsNullOrEmpty(questToGive.questID))
        {
            string key = $"QuestGiven_{questToGive.questID}";
            questGiven = PlayerPrefs.GetInt(key, 0) == 1;
        }
        else
        {
            questGiven = false;
        }
    }
    
    // Completion action durumunu yükle
    private void LoadCompletionActionState()
    {
        if (questToGive != null && !string.IsNullOrEmpty(questToGive.questID))
        {
            string key = $"QuestCompletionAction_{questToGive.questID}";
            completionActionExecuted = PlayerPrefs.GetInt(key, 0) == 1;
            
            // QuestManager null check ekle - scene değişiminde null olabiliyor
            if (QuestManager.instance != null && 
                QuestManager.instance.IsQuestCompleted(questToGive.questID) && 
                completionActionExecuted)
            {
                ApplyCompletionStateOnLoad();
            }
        }
        else
        {
            completionActionExecuted = false;
        }
    }
    
    // Completion action durumunu kaydet
    private void SaveCompletionActionState()
    {
        if (questToGive != null && !string.IsNullOrEmpty(questToGive.questID))
        {
            string key = $"QuestCompletionAction_{questToGive.questID}";
            PlayerPrefs.SetInt(key, completionActionExecuted ? 1 : 0);
            PlayerPrefs.Save();
        }
    }
    
    // Quest completion action'ını çalıştır
    private void ExecuteCompletionAction()
    {
        if (questToGive?.completionBehavior == null) return;
        
        completionActionExecuted = true;
        SaveCompletionActionState();
        
        var behavior = questToGive.completionBehavior;
        
        if (behavior.delayBeforeAction > 0)
        {
            StartCoroutine(ExecuteCompletionActionDelayed(behavior));
        }
        else
        {
            PerformCompletionAction(behavior);
        }
    }
    
    // Gecikmeli completion action
    private IEnumerator ExecuteCompletionActionDelayed(QuestCompletionBehavior behavior)
    {
        yield return new WaitForSeconds(behavior.delayBeforeAction);
        PerformCompletionAction(behavior);
    }
    
    // Completion action'ı gerçekleştir
    private void PerformCompletionAction(QuestCompletionBehavior behavior)
    {
        switch (behavior.action)
        {
            case QuestCompletionAction.None:
                // Hiçbir şey yapma
                break;
                
            case QuestCompletionAction.HideQuestGiver:
                gameObject.SetActive(false);
                Debug.Log($"QuestGiver {gameObject.name} hidden after quest completion");
                break;
                
            case QuestCompletionAction.DestroyQuestGiver:
                // Hemen destroy etme, sadece durumu kaydet
                // Sahne yeniden yüklendiğinde destroy edilecek
                Debug.Log($"QuestGiver {gameObject.name} marked for destruction on scene reload");
                break;
                
            case QuestCompletionAction.ChangeDialogue:
                // Diyalog değişimi ShowQuestCompletedDialogue'da handle ediliyor
                Debug.Log($"QuestGiver {gameObject.name} dialogue changed after quest completion");
                break;
                
            case QuestCompletionAction.TeleportQuestGiver:
                transform.position = behavior.teleportPosition;
                Debug.Log($"QuestGiver {gameObject.name} teleported to {behavior.teleportPosition}");
                break;
                
            case QuestCompletionAction.RunCustomAction:
                RunCustomAction(behavior.customParameter);
                break;
        }
    }
    
    // Oyun yüklendiğinde completion state'i uygula
    private void ApplyCompletionStateOnLoad()
    {
        if (questToGive?.completionBehavior == null) return;
        
        var behavior = questToGive.completionBehavior;
        
        switch (behavior.action)
        {
            case QuestCompletionAction.HideQuestGiver:
                gameObject.SetActive(false);
                Debug.Log($"QuestGiver {gameObject.name} hidden on scene load (quest completed)");
                break;
                
            case QuestCompletionAction.DestroyQuestGiver:
                Debug.Log($"QuestGiver {gameObject.name} destroyed on scene load (quest completed)");
                Destroy(gameObject);
                break;
                
            case QuestCompletionAction.TeleportQuestGiver:
                transform.position = behavior.teleportPosition;
                Debug.Log($"QuestGiver {gameObject.name} teleported to {behavior.teleportPosition} on scene load");
                break;
        }
    }
    
    // Custom action çalıştır
    private void RunCustomAction(string customParameter)
    {
        Debug.Log($"Running custom action: {customParameter}");
        
        // Custom action'ı burada implement edebilirsin
        // Örneğin: method name'e göre reflection ile method çağırma
        // Veya Unity Events kullanma
        
        // Örnek: Component'te belirli bir methodu çağır
        if (!string.IsNullOrEmpty(customParameter))
        {
            // customParameter format: "ComponentName.MethodName"
            string[] parts = customParameter.Split('.');
            if (parts.Length == 2)
            {
                var component = GetComponent(parts[0]);
                if (component != null)
                {
                    var method = component.GetType().GetMethod(parts[1]);
                    method?.Invoke(component, null);
                }
            }
        }
    }

    // Quest completed dialogue bittiğinde prompt'u kalıcı olarak kapat
    private void HidePromptPermanently()
    {
        // Event'i dinlemeyi bırak
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.OnDialogueEnd -= HidePromptPermanently;
        }
        
        // Prompt'u kalıcı olarak kapat
        HideInteractionPrompt();
        
        // Bu state'i kaydet ki scene reload'da da hatırlansın
        SaveQuestCompletedPromptState();
        
        Debug.Log($"Quest completed dialogue bitti, interaction prompt kalıcı olarak kapatıldı: {questToGive?.questID}");
    }
    
    // Quest completed prompt state'ini kaydet
    private void SaveQuestCompletedPromptState()
    {
        if (questToGive != null && !string.IsNullOrEmpty(questToGive.questID))
        {
            string key = $"QuestCompleted_PromptHidden_{questToGive.questID}";
            PlayerPrefs.SetInt(key, 1);
            PlayerPrefs.Save();
        }
    }

    // Override ShowInteractionPrompt to hide prompt when quest is completed
    public override void ShowInteractionPrompt()
    {
        // Eğer quest tamamlandı ve completed dialogue okunmuşsa prompt gösterme
        if (questToGive != null && !string.IsNullOrEmpty(questToGive.questID))
        {
            // Quest completed prompt hidden state'ini kontrol et
            string key = $"QuestCompleted_PromptHidden_{questToGive.questID}";
            bool promptHidden = PlayerPrefs.GetInt(key, 0) == 1;
            
            if (promptHidden)
            {
               // Debug.Log($"Quest completed dialogue okunmuş, interaction prompt gösterilmiyor: {questToGive.questID}");
                return;
            }
        }
        
        // Normal durumda parent method'u çağır
        base.ShowInteractionPrompt();
    }
} 