using UnityEngine;
using System.Collections.Generic;

public class QuestGiverNPC : DialogueNPC
{
    [Header("Quest Giver Settings")]
    [SerializeField] private QuestData[] availableQuests;
    [SerializeField] private GameObject questIndicator; // NPC üzerinde görev işareti
    
    private int currentQuestIndex = 0;
    private List<QuestStatus> questStatuses = new List<QuestStatus>();
    
    private void Start()
    {
        UpdateQuestStatuses();
        UpdateQuestIndicator();
    }
    
    private void UpdateQuestStatuses()
    {
        questStatuses.Clear();
        
        if (QuestManager.Instance == null || availableQuests == null) return;
        
        foreach (QuestData questData in availableQuests)
        {
            QuestStatus status = QuestManager.Instance.GetQuestStatus(questData.id);
            questStatuses.Add(status);
        }
    }
    
    private void UpdateQuestIndicator()
    {
        if (questIndicator == null) return;
        
        // Eğer NPC'nin verebileceği veya tamamlanacak görevleri varsa görev işaretini göster
        bool hasAvailableOrActiveQuests = false;
        
        for (int i = 0; i < questStatuses.Count; i++)
        {
            QuestStatus status = questStatuses[i];
            
            if (status == QuestStatus.NotStarted)
            {
                // Görev gereksinimlerini kontrol et
                if (CheckQuestRequirements(availableQuests[i]))
                {
                    hasAvailableOrActiveQuests = true;
                    break;
                }
            }
            else if (status == QuestStatus.Active)
            {
                // Aktif görevleri kontrol et, tamamlanabilir mi?
                Quest activeQuest = QuestManager.Instance.GetQuestById(availableQuests[i].id);
                if (activeQuest != null && CheckQuestCompletion(activeQuest))
                {
                    hasAvailableOrActiveQuests = true;
                    break;
                }
            }
        }
        
        questIndicator.SetActive(hasAvailableOrActiveQuests);
    }
    
    private bool CheckQuestRequirements(QuestData questData)
    {
        // Görev gereksinimlerini kontrol et
        if (questData.requiredQuestIds == null || questData.requiredQuestIds.Length == 0)
            return true; // Hiç gereksinim yoksa görev alınabilir
            
        foreach (string requiredQuestId in questData.requiredQuestIds)
        {
            if (QuestManager.Instance.GetQuestStatus(requiredQuestId) != QuestStatus.Completed)
                return false; // Gerekli görevlerden biri tamamlanmamışsa görev alınamaz
        }
        
        return true; // Tüm gereksinimler tamamlanmış
    }
    
    private bool CheckQuestCompletion(Quest quest)
    {
        // Görevin tamamlanıp tamamlanmadığını kontrol et
        foreach (QuestObjective objective in quest.objectives)
        {
            if (!objective.isCompleted)
                return false;
        }
        
        return true;
    }
    
    public override void Interact()
    {
        if (DialogueManager.Instance == null)
        {
            Debug.LogError("DialogueManager bulunamadı!");
            return;
        }
        
        UpdateQuestStatuses();
        
        // Görevin durumuna göre diyalog seç ve görev işlemleri
        if (availableQuests.Length > 0 && currentQuestIndex < availableQuests.Length)
        {
            QuestData currentQuestData = availableQuests[currentQuestIndex];
            QuestStatus currentStatus = questStatuses[currentQuestIndex];
            
            switch (currentStatus)
            {
                case QuestStatus.NotStarted:
                    // Görev gereksinimlerini kontrol et
                    if (CheckQuestRequirements(currentQuestData))
                    {
                        // Görevi başlatma diyaloğunu göster
                        if (currentQuestData.questStartDialogue != null)
                        {
                            DialogueManager.Instance.StartDialogue(currentQuestData.questStartDialogue);
                            DialogueManager.Instance.OnDialogueEnd += QuestAccepted;
                        }
                        else
                        {
                            // Diyalog yoksa doğrudan görevi başlat
                            QuestAccepted();
                        }
                    }
                    else
                    {
                        // Gereksinimler karşılanmıyorsa normal diyaloğu göster
                        base.Interact();
                    }
                    break;
                    
                case QuestStatus.Active:
                    // Aktif görev diyaloğunu göster
                    Quest activeQuest = QuestManager.Instance.GetQuestById(currentQuestData.id);
                    
                    if (activeQuest != null && CheckQuestCompletion(activeQuest))
                    {
                        // Görev tamamlandıysa tamamlama diyaloğunu göster
                        if (currentQuestData.questCompletedDialogue != null)
                        {
                            DialogueManager.Instance.StartDialogue(currentQuestData.questCompletedDialogue);
                            DialogueManager.Instance.OnDialogueEnd += QuestCompleted;
                        }
                        else
                        {
                            // Diyalog yoksa doğrudan görevi tamamla
                            QuestCompleted();
                        }
                    }
                    else
                    {
                        // Görev henüz tamamlanmadıysa, aktif görev diyaloğunu göster
                        if (currentQuestData.questActiveDialogue != null)
                        {
                            DialogueManager.Instance.StartDialogue(currentQuestData.questActiveDialogue);
                        }
                        else
                        {
                            // Aktif görev diyaloğu yoksa normal diyaloğu göster
                            base.Interact();
                        }
                    }
                    break;
                    
                case QuestStatus.Completed:
                    // Görev tamamlanmışsa bir sonraki göreve geç
                    currentQuestIndex++;
                    
                    if (currentQuestIndex < availableQuests.Length)
                    {
                        // Bir sonraki görevle etkileşime geç
                        Interact();
                    }
                    else
                    {
                        // Tüm görevler tamamlanmışsa normal diyaloğu göster
                        base.Interact();
                    }
                    break;
            }
        }
        else
        {
            // Görev yoksa veya tüm görevler tamamlanmışsa normal diyaloğu göster
            base.Interact();
        }
        
        UpdateQuestIndicator();
    }
    
    private void QuestAccepted()
    {
        // Diyalog bittiğinde görevi başlat
        if (availableQuests.Length > 0 && currentQuestIndex < availableQuests.Length)
        {
            QuestData questData = availableQuests[currentQuestIndex];
            QuestManager.Instance.StartQuestFromNPC(questData.id);
            
            // DialogueManager event'ini temizle
            if (DialogueManager.Instance != null)
            {
                DialogueManager.Instance.OnDialogueEnd -= QuestAccepted;
            }
            
            UpdateQuestStatuses();
            UpdateQuestIndicator();
        }
    }
    
    private void QuestCompleted()
    {
        // Diyalog bittiğinde görevi tamamla
        if (availableQuests.Length > 0 && currentQuestIndex < availableQuests.Length)
        {
            QuestData questData = availableQuests[currentQuestIndex];
            QuestManager.Instance.CompleteQuest(questData.id);
            
            // DialogueManager event'ini temizle
            if (DialogueManager.Instance != null)
            {
                DialogueManager.Instance.OnDialogueEnd -= QuestCompleted;
            }
            
            UpdateQuestStatuses();
            UpdateQuestIndicator();
            
            // Bir sonraki göreve geç
            currentQuestIndex++;
        }
    }
} 