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
    }
    
    private void Start()
    {
        // Oyun başladığında mevcut düşmanları bul
        FindAllEnemiesInScene();
    }
    
    public void FindAllEnemiesInScene()
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
        
        // Eğer sahnede düşman varsa, "Tüm düşmanları yok et" görevini başlat
        if (totalEnemyCount > 0)
        {
            CreateKillAllEnemiesQuest();
        }
    }
    
    private void CreateKillAllEnemiesQuest()
    {
        // Görev zaten varsa tekrar oluşturma
        if (activeQuests.Any(q => q.id == "kill_all_enemies"))
        {
            return;
        }
        
        // "Tüm düşmanları öldür" görevini oluştur
        Quest killAllEnemiesQuest = new Quest(
            "kill_all_enemies",
            "Temizlik Zamanı",
            "Bu bölgedeki tüm düşmanları temizle."
        );
        
        // Tek bir hedef oluştur: tüm düşmanları öldürmek
        QuestObjective killObjective = new QuestObjective(
            $"Düşmanları öldür (0/{totalEnemyCount})",
            totalEnemyCount
        );
        
        killAllEnemiesQuest.objectives = new QuestObjective[] { killObjective };
        
        // Ödülleri ekle
        AddQuestRewards(killAllEnemiesQuest);
        
        // Görevi aktif et ve listeye ekle
        AddQuest(killAllEnemiesQuest);
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
            
            // "Tüm düşmanları öldür" görevini güncelle
            UpdateKillAllEnemiesQuest();
            
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
    
    private void UpdateKillAllEnemiesQuest()
    {
        // Aktif görevler arasında "kill_all_enemies" ID'li görevi bul
        Quest killQuest = activeQuests.Find(q => q.id == "kill_all_enemies");
        
        if (killQuest != null && killQuest.isActive && !killQuest.isCompleted)
        {
            // İlk hedefi güncelle (düşman öldürme sayısı)
            if (killQuest.objectives.Length > 0)
            {
                QuestObjective killObjective = killQuest.objectives[0];
                killObjective.currentAmount = defeatedEnemyCount;
                killObjective.description = $"Düşmanları öldür ({defeatedEnemyCount}/{totalEnemyCount})";
                
                if (defeatedEnemyCount >= totalEnemyCount)
                {
                    killObjective.isCompleted = true;
                }
                
                // Görevi güncelle
                killQuest.UpdateQuest();
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
} 