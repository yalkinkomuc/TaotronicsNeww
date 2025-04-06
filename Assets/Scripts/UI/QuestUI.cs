using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QuestUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject questPanel;
    [SerializeField] private TextMeshProUGUI questTitleText;
    [SerializeField] private TextMeshProUGUI questDescriptionText;
    [SerializeField] private Transform objectivesContainer;
    [SerializeField] private GameObject objectivePrefab;
    
    [Header("Rewards UI")]
    [SerializeField] private Transform rewardsContainer;
    [SerializeField] private GameObject rewardItemPrefab;
    [SerializeField] private TextMeshProUGUI rewardsHeaderText;
    
    [Header("Notification")]
    [SerializeField] private GameObject questCompletionNotification;
    [SerializeField] private TextMeshProUGUI notificationTitleText;
    [SerializeField] private TextMeshProUGUI notificationRewardsText;
    [SerializeField] private float notificationDuration = 3f;
    
    private List<GameObject> objectiveItems = new List<GameObject>();
    private List<GameObject> rewardItems = new List<GameObject>();
    
    private void OnEnable()
    {
        // Event'lere abone ol
        GameEvents.OnQuestStarted += HandleQuestStarted;
        GameEvents.OnQuestUpdated += HandleQuestUpdated;
        GameEvents.OnQuestCompleted += HandleQuestCompleted;
        GameEvents.OnAllEnemiesDefeated += HandleAllEnemiesDefeated;
        GameEvents.OnQuestRewardGranted += HandleQuestRewardGranted;
    }
    
    private void OnDisable()
    {
        // Event aboneliklerini kaldır
        GameEvents.OnQuestStarted -= HandleQuestStarted;
        GameEvents.OnQuestUpdated -= HandleQuestUpdated;
        GameEvents.OnQuestCompleted -= HandleQuestCompleted;
        GameEvents.OnAllEnemiesDefeated -= HandleAllEnemiesDefeated;
        GameEvents.OnQuestRewardGranted -= HandleQuestRewardGranted;
    }
    
    private void Start()
    {
        // Başlangıçta görev panelini gizle
        if (questPanel != null)
            questPanel.SetActive(false);
            
        if (questCompletionNotification != null)
            questCompletionNotification.SetActive(false);
            
        // Aktif görevleri yükle (eğer varsa)
        if (QuestManager.Instance != null)
        {
            List<Quest> activeQuests = QuestManager.Instance.GetActiveQuests();
            foreach (Quest quest in activeQuests)
            {
                UpdateQuestUI(quest);
            }
        }
    }
    
    private void HandleQuestStarted(Quest quest)
    {
        UpdateQuestUI(quest);
    }
    
    private void HandleQuestUpdated(Quest quest)
    {
        UpdateQuestUI(quest);
    }
    
    private void HandleQuestCompleted(Quest quest)
    {
        // Görev tamamlama bildirimi göster
        ShowQuestCompletionNotification(quest);
        
        // Görev panelini güncelle veya kaldır
        if (QuestManager.Instance != null)
        {
            List<Quest> activeQuests = QuestManager.Instance.GetActiveQuests();
            if (activeQuests.Count > 0)
            {
                UpdateQuestUI(activeQuests[0]);
            }
            else
            {
                ClearQuestUI();
            }
        }
    }
    
    private void HandleQuestRewardGranted(QuestReward reward)
    {
        Debug.Log($"UI: Quest reward granted: {reward.GetRewardDescription()}");
    }
    
    private void HandleAllEnemiesDefeated()
    {
        Debug.Log("QuestUI: All enemies defeated notification");
        // Özel bir bildirim gösterebilir
    }
    
    private void UpdateQuestUI(Quest quest)
    {
        if (quest == null || questPanel == null) return;
        
        questPanel.SetActive(true);
        
        if (questTitleText != null)
            questTitleText.text = quest.title;
            
        if (questDescriptionText != null)
            questDescriptionText.text = quest.description;
        
        // Hedefleri göster
        ClearObjectives();
        
        if (quest.objectives != null && objectivesContainer != null && objectivePrefab != null)
        {
            foreach (QuestObjective objective in quest.objectives)
            {
                GameObject objectiveItem = Instantiate(objectivePrefab, objectivesContainer);
                TextMeshProUGUI objectiveText = objectiveItem.GetComponentInChildren<TextMeshProUGUI>();
                Image checkImage = objectiveItem.GetComponentInChildren<Image>();
                
                if (objectiveText != null)
                {
                    string progressText = objective.currentAmount >= objective.requiredAmount ?
                        $"{objective.description} (Tamamlandı)" :
                        objective.description;
                    
                    objectiveText.text = progressText;
                }
                
                if (checkImage != null)
                {
                    checkImage.enabled = objective.isCompleted;
                }
                
                objectiveItems.Add(objectiveItem);
            }
        }
        
        // Ödülleri göster
        UpdateRewardsUI(quest);
    }
    
    private void UpdateRewardsUI(Quest quest)
    {
        ClearRewards();
        
        if (rewardsContainer == null || rewardItemPrefab == null) return;
        
        if (quest.rewards != null && quest.rewards.Count > 0)
        {
            // Ödüller başlığını göster
            if (rewardsHeaderText != null)
            {
                rewardsHeaderText.gameObject.SetActive(true);
                rewardsHeaderText.text = "Ödüller:";
            }
            
            // Ödülleri listele
            foreach (QuestReward reward in quest.rewards)
            {
                GameObject rewardItem = Instantiate(rewardItemPrefab, rewardsContainer);
                TextMeshProUGUI rewardText = rewardItem.GetComponentInChildren<TextMeshProUGUI>();
                Image rewardIcon = rewardItem.GetComponentInChildren<Image>();
                
                if (rewardText != null)
                {
                    rewardText.text = reward.GetRewardDescription();
                }
                
                // Eğer bu bir item ödülü ise ve item'ın ikonu varsa göster
                if (reward.rewardType == RewardType.Item && reward.itemDataReward != null && rewardIcon != null)
                {
                    rewardIcon.sprite = reward.itemDataReward.icon;
                    rewardIcon.enabled = true;
                }
                else if (rewardIcon != null)
                {
                    // İtem değilse, ödül türüne göre farklı bir ikon seçilebilir
                    switch (reward.rewardType)
                    {
                        case RewardType.Gold:
                            // Altın ikonu
                            rewardIcon.sprite = Resources.Load<Sprite>("Icons/GoldIcon");
                            break;
                        case RewardType.Experience:
                            // XP ikonu
                            rewardIcon.sprite = Resources.Load<Sprite>("Icons/XPIcon");
                            break;
                    }
                    
                    rewardIcon.enabled = rewardIcon.sprite != null;
                }
                
                rewardItems.Add(rewardItem);
            }
        }
        else
        {
            // Ödül yoksa başlığı gizle
            if (rewardsHeaderText != null)
            {
                rewardsHeaderText.gameObject.SetActive(false);
            }
        }
    }
    
    private void ClearObjectives()
    {
        foreach (GameObject objectiveItem in objectiveItems)
        {
            Destroy(objectiveItem);
        }
        
        objectiveItems.Clear();
    }
    
    private void ClearRewards()
    {
        foreach (GameObject rewardItem in rewardItems)
        {
            Destroy(rewardItem);
        }
        
        rewardItems.Clear();
    }
    
    private void ClearQuestUI()
    {
        if (questPanel != null)
            questPanel.SetActive(false);
            
        ClearObjectives();
        ClearRewards();
    }
    
    private void ShowQuestCompletionNotification(Quest quest)
    {
        if (questCompletionNotification == null || notificationTitleText == null) return;
        
        notificationTitleText.text = $"Görev Tamamlandı: {quest.title}";
        
        // Ödülleri bildirimde göster
        if (notificationRewardsText != null && quest.rewards.Count > 0)
        {
            string rewardsText = "Kazanılan Ödüller:\n";
            foreach (QuestReward reward in quest.rewards)
            {
                rewardsText += "- " + reward.GetRewardDescription() + "\n";
            }
            
            notificationRewardsText.text = rewardsText;
            notificationRewardsText.gameObject.SetActive(true);
        }
        else if (notificationRewardsText != null)
        {
            notificationRewardsText.gameObject.SetActive(false);
        }
        
        questCompletionNotification.SetActive(true);
        
        // Birkaç saniye sonra bildirimi gizle
        StartCoroutine(HideNotificationAfterDelay());
    }
    
    private IEnumerator HideNotificationAfterDelay()
    {
        yield return new WaitForSeconds(notificationDuration);
        
        if (questCompletionNotification != null)
            questCompletionNotification.SetActive(false);
    }
} 