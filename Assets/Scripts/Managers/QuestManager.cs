using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class QuestManager : MonoBehaviour, IManager
{
    public static QuestManager Instance { get; private set; }
    
    [SerializeField] private List<Quest> activeQuests = new List<Quest>();
    [SerializeField] private List<Quest> completedQuests = new List<Quest>();
    
    // Mevcut sahnedeki düşmanları takip eden değişkenler
    private List<Enemy> sceneEnemies = new List<Enemy>();
    private int totalEnemyCount = 0;
    private int defeatedEnemyCount = 0;
    
    // Ödül nesneleri için referanslar
    [Header("Ödül Ayarları")]
    [SerializeField] private ItemData[] rewardItems; // Editor'da atanabilecek item'lar
    [SerializeField] private int goldRewardAmount = 100;
    [SerializeField] private int experienceRewardAmount = 50;
    
    // Quest veri tabanı (Editörde tanımlanacak)
    [Header("Quest Veritabanı")]
    [SerializeField] private List<QuestData> availableQuests = new List<QuestData>();
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void OnEnable()
    {
        // Event'lere abone ol
        GameEvents.OnEnemyDefeated += HandleEnemyDefeated;
        GameEvents.OnQuestRewardGranted += HandleQuestRewardGranted;
    }
    
    private void OnDisable()
    {
        // Event aboneliklerini kaldır
        GameEvents.OnEnemyDefeated -= HandleEnemyDefeated;
        GameEvents.OnQuestRewardGranted -= HandleQuestRewardGranted;
    }
    
    public void Initialize()
    {
        Debug.Log("QuestManager initialized");
        // Oyun başladığında mevcut düşmanları bul (quest oluşturmadan)
        UpdateEnemyCount();
    }
    
    private void Start()
    {
        // Artık başlangıçta otomatik olarak quest oluşturulmayacak
    }
    
    // Düşmanları say (quest başlatmadan)
    public void UpdateEnemyCount()
    {
        // Sahnedeki tüm düşmanları bul
        sceneEnemies.Clear();
        defeatedEnemyCount = 0;
        
        Enemy[] enemies = FindObjectsOfType<Enemy>();
        foreach (Enemy enemy in enemies)
        {
            sceneEnemies.Add(enemy);
        }
        
        totalEnemyCount = sceneEnemies.Count;
        Debug.Log($"Found {totalEnemyCount} enemies in the current scene");
    }
    
    // NPC tarafından başlatılan görevi oluşturmak için
    public void StartQuestFromNPC(string questID)
    {
        // Görevi ID'ye göre veritabanından bul
        QuestData questData = availableQuests.Find(q => q.id == questID);
        
        if (questData == null)
        {
            Debug.LogError($"Quest not found with ID: {questID}");
            return;
        }
        
        // Görev zaten aktif veya tamamlanmış mı kontrol et
        if (activeQuests.Any(q => q.id == questID) || completedQuests.Any(q => q.id == questID))
        {
            Debug.Log($"Quest {questID} is already active or completed");
            return;
        }
        
        // Yeni görevi oluştur
        Quest newQuest = new Quest(
            questData.id,
            questData.title,
            questData.description
        );
        
        // Görev tipine göre hedefleri oluştur
        switch (questData.questType)
        {
            case QuestType.KillEnemies:
                QuestObjective killObjective = new QuestObjective(
                    $"Düşmanları öldür (0/{totalEnemyCount})",
                    totalEnemyCount
                );
                newQuest.objectives = new QuestObjective[] { killObjective };
                break;
                
            case QuestType.CollectItems:
                QuestObjective collectObjective = new QuestObjective(
                    $"{questData.itemName} topla (0/{questData.itemAmount})",
                    questData.itemAmount
                );
                newQuest.objectives = new QuestObjective[] { collectObjective };
                break;
                
            case QuestType.TalkToNPC:
                QuestObjective talkObjective = new QuestObjective(
                    $"{questData.targetNPCName} ile konuş",
                    1
                );
                newQuest.objectives = new QuestObjective[] { talkObjective };
                break;
        }
        
        // Ödülleri ekle
        AddQuestRewards(newQuest);
        
        // Görevi aktif et ve listeye ekle
        AddQuest(newQuest);
    }
    
    private void AddQuestRewards(Quest quest)
    {
        // Item ödülü ekle (eğer rewardItems dizisinde item varsa)
        if (rewardItems != null && rewardItems.Length > 0)
        {
            // Rastgele bir item seç
            ItemData rewardItem = rewardItems[Random.Range(0, rewardItems.Length)];
            if (rewardItem != null)
            {
                quest.AddItemReward(rewardItem);
            }
        }
        
        // Altın ödülü ekle
        if (goldRewardAmount > 0)
        {
            quest.AddGoldReward(goldRewardAmount);
        }
        
        // Tecrübe puanı ödülü ekle
        if (experienceRewardAmount > 0)
        {
            quest.AddExperienceReward(experienceRewardAmount);
        }
    }
    
    public void AddQuest(Quest quest)
    {
        if (!activeQuests.Contains(quest) && !completedQuests.Contains(quest))
        {
            activeQuests.Add(quest);
            quest.StartQuest();
            Debug.Log($"New quest added: {quest.title}");
        }
    }
    
    private void HandleEnemyDefeated(Enemy enemy)
    {
        // Düşman listesinden çıkar
        if (sceneEnemies.Contains(enemy))
        {
            sceneEnemies.Remove(enemy);
            defeatedEnemyCount++;
            
            Debug.Log($"Enemy defeated! {defeatedEnemyCount}/{totalEnemyCount}");
            
            // Kill quest'lerin hepsini güncelle
            UpdateKillQuests();
            
            // Tüm düşmanlar öldürüldü mü kontrol et
            if (sceneEnemies.Count == 0 && totalEnemyCount > 0)
            {
                GameEvents.AllEnemiesDefeated();
                Debug.Log("All enemies in the scene have been defeated!");
            }
        }
    }
    
    private void HandleQuestRewardGranted(QuestReward reward)
    {
        // Ödül verildiğinde burası tetiklenir
        Debug.Log($"Quest reward granted: {reward.GetRewardDescription()}");
    }
    
    private void UpdateKillQuests()
    {
        // Aktif görevleri tarayarak düşman öldürme içeren tüm görevleri güncelle
        foreach (Quest quest in activeQuests)
        {
            // Her görevin hedeflerini kontrol et
            if (quest.objectives.Length > 0)
            {
                foreach (QuestObjective objective in quest.objectives)
                {
                    // Eğer açıklama "Düşmanları öldür" içeriyorsa bu bir kill quest
                    if (objective.description.Contains("Düşmanları öldür"))
                    {
                        objective.currentAmount = defeatedEnemyCount;
                        objective.description = $"Düşmanları öldür ({defeatedEnemyCount}/{totalEnemyCount})";
                        
                        if (defeatedEnemyCount >= totalEnemyCount)
                        {
                            objective.isCompleted = true;
                        }
                    }
                }
                
                // Görevi güncelle
                quest.UpdateQuest();
            }
        }
    }
    
    public void CompleteQuest(string questId)
    {
        Quest quest = activeQuests.Find(q => q.id == questId);
        
        if (quest != null)
        {
            quest.CompleteQuest();
            activeQuests.Remove(quest);
            completedQuests.Add(quest);
            Debug.Log($"Quest completed: {quest.title}");
        }
    }
    
    public List<Quest> GetActiveQuests()
    {
        return activeQuests;
    }
    
    public List<Quest> GetCompletedQuests()
    {
        return completedQuests;
    }
    
    public Quest GetQuestById(string questId)
    {
        Quest quest = activeQuests.Find(q => q.id == questId);
        
        if (quest == null)
        {
            quest = completedQuests.Find(q => q.id == questId);
        }
        
        return quest;
    }
    
    // Görevin durumunu kontrol et (aktif, tamamlanmış veya mevcut değil)
    public QuestStatus GetQuestStatus(string questId)
    {
        if (activeQuests.Any(q => q.id == questId))
            return QuestStatus.Active;
        else if (completedQuests.Any(q => q.id == questId))
            return QuestStatus.Completed;
        else
            return QuestStatus.NotStarted;
    }
} 