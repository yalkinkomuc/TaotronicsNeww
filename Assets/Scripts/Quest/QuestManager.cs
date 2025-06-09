using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class QuestManager : MonoBehaviour
{
    
    public static QuestManager instance;
    
    public List<QuestData> availableQuests;
    private List<QuestInstance> activeQuests = new List<QuestInstance>();
    private HashSet<string> completedQuests = new HashSet<string>(); // Tamamlanan questlerin ID'leri

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            LoadQuestProgress();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void StartQuest(QuestData questData)
    {
        if (questData != null && 
            activeQuests.All(q => q.Data != questData) && 
            !IsQuestCompleted(questData.questID))
        {
            var quest = new QuestInstance();
            quest.Data = questData;
            activeQuests.Add(quest);
            Debug.Log($"Started quest: {questData.title}");
            SaveQuestProgress();
        }
        else if (IsQuestCompleted(questData.questID))
        {
            Debug.Log($"Quest zaten tamamlanmış: {questData.title}");
        }
    }

    public void RaiseEvent(string eventName, object data)
    {
        Debug.Log($"Event Raised: {eventName}");

        // Quest tamamlanma durumunu kontrol etmek için liste kopyası al
        var questsToRemove = new List<QuestInstance>();

        foreach (var quest in activeQuests)
        {
            quest.HandleEvent(eventName, data);
            
            // Quest tamamlandıysa completed listesine ekle
            if (quest.IsCompleted && !string.IsNullOrEmpty(quest.Data.questID))
            {
                completedQuests.Add(quest.Data.questID);
                questsToRemove.Add(quest);
                Debug.Log($"🎉 Quest tamamlandı ve kaydedildi: {quest.Data.title}");
            }
        }
        
        // Tamamlanan questleri aktif listeden çıkar
        foreach (var completedQuest in questsToRemove)
        {
            activeQuests.Remove(completedQuest);
        }
        
        // Event işlendikten sonra ilerlemeyi kaydet
        SaveQuestProgress();
    }

    // Aktif questleri döndürür (sadece okunabilir)
    public List<QuestInstance> GetActiveQuests()
    {
        return new List<QuestInstance>(activeQuests);
    }
    
    // Quest tamamlanmış mı kontrol et
    public bool IsQuestCompleted(string questID)
    {
        return !string.IsNullOrEmpty(questID) && completedQuests.Contains(questID);
    }
    
    // Debug: Aktif questlerin durumunu logla
    [ContextMenu("Debug Quest Status")]
    public void DebugQuestStatus()
    {
        Debug.Log($"Aktif Quest Sayısı: {activeQuests.Count}");
        Debug.Log($"Tamamlanan Quest Sayısı: {completedQuests.Count}");
        
        Debug.Log("=== AKTIF QUESTLER ===");
        foreach (var quest in activeQuests)
        {
            if (quest.Data != null)
            {
                Debug.Log($"Quest: {quest.Data.title}");
                Debug.Log($"  - ID: {quest.Data.questID}");
                Debug.Log($"  - Current Objective Index: {quest.currentObjectiveIndex}");
                Debug.Log($"  - Is Completed: {quest.IsCompleted}");
                
                if (quest.CurrentObjective != null)
                {
                    Debug.Log($"  - Current Objective: {quest.CurrentObjective.description}");
                    Debug.Log($"  - Objective Completed: {quest.CurrentObjective.isCompleted}");
                    Debug.Log($"  - Objective Initialized: {quest.CurrentObjective.isInitialized}");
                }
            }
        }
        
        Debug.Log("=== TAMAMLANAN QUESTLER ===");
        foreach (var completedQuestID in completedQuests)
        {
            var questData = FindQuestDataByID(completedQuestID);
            string questTitle = questData != null ? questData.title : "Bilinmeyen Quest";
            Debug.Log($"Completed Quest: {questTitle} (ID: {completedQuestID})");
        }
    }
    
    // Tüm questleri ve objective'leri sıfırlar
    public void ResetAllQuests()
    {
        // Aktif questleri temizle
        activeQuests.Clear();
        
        // Tamamlanan questleri temizle
        completedQuests.Clear();
        
        // Tüm mevcut questlerin objective'lerini sıfırla
        foreach (var quest in availableQuests)
        {
            if (quest != null && quest.objectives != null)
            {
                foreach (var objective in quest.objectives)
                {
                    if (objective != null)
                    {
                        objective.isCompleted = false;
                        objective.isInitialized = false;
                    }
                }
            }
        }
        
        // Kaydedilen ilerlemeyi de temizle
        ClearQuestProgress();
        
        // QuestGiver durumlarını da temizle
        ClearQuestGiverStates();
        
        Debug.Log("Tüm questler ve objective'ler sıfırlandı.");
    }
    
    // Quest ilerlemelerini kaydet
    private void SaveQuestProgress()
    {
        // Aktif questlerin listesini kaydet
        List<string> activeQuestIDs = new List<string>();
        List<int> questObjectiveIndices = new List<int>();
        
        foreach (var quest in activeQuests)
        {
            if (quest.Data != null && !string.IsNullOrEmpty(quest.Data.questID))
            {
                activeQuestIDs.Add(quest.Data.questID);
                questObjectiveIndices.Add(quest.currentObjectiveIndex);
                
                // Quest objective'lerinin completion durumlarını kaydet
                SaveQuestObjectiveStates(quest);
            }
        }
        
        // Aktif quest ID'lerini kaydet
        string questIDsJson = JsonUtility.ToJson(new SerializableStringList { items = activeQuestIDs.ToArray() });
        PlayerPrefs.SetString("ActiveQuestIDs", questIDsJson);
        
        // Quest objective indekslerini kaydet
        string indicesJson = JsonUtility.ToJson(new SerializableIntList { items = questObjectiveIndices.ToArray() });
        PlayerPrefs.SetString("QuestObjectiveIndices", indicesJson);
        
        // Tamamlanan questleri kaydet
        string completedQuestsJson = JsonUtility.ToJson(new SerializableStringList { items = completedQuests.ToArray() });
        PlayerPrefs.SetString("CompletedQuests", completedQuestsJson);
        
        PlayerPrefs.Save();
        Debug.Log($"Quest progress saved. Active quests: {activeQuests.Count}, Completed quests: {completedQuests.Count}");
    }
    
    // Quest objective durumlarını kaydet
    private void SaveQuestObjectiveStates(QuestInstance quest)
    {
        if (quest.Data == null || quest.Data.objectives == null) return;
        
        for (int i = 0; i < quest.Data.objectives.Length; i++)
        {
            var objective = quest.Data.objectives[i];
            if (objective != null)
            {
                string objectiveKey = $"Quest_{quest.Data.questID}_Objective_{i}";
                PlayerPrefs.SetInt($"{objectiveKey}_Completed", objective.isCompleted ? 1 : 0);
                PlayerPrefs.SetInt($"{objectiveKey}_Initialized", objective.isInitialized ? 1 : 0);
                
                // Özel objective tiplerinin ek verilerini kaydet
                SaveObjectiveSpecificData(objective, objectiveKey);
            }
        }
    }
    
    // Objective tipine özgü verileri kaydet
    private void SaveObjectiveSpecificData(QuestObjective objective, string objectiveKey)
    {
        // KillObjective için özel kaydetme
        if (objective is KillObjective killObj)
        {
            // Reflection kullanarak private currentAmount field'ına erişim
            var currentAmountField = typeof(KillObjective).GetField("currentAmount", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (currentAmountField != null)
            {
                int currentAmount = (int)currentAmountField.GetValue(killObj);
                PlayerPrefs.SetInt($"{objectiveKey}_CurrentAmount", currentAmount);
            }
        }
        // TargetedKillObjective için özel kaydetme
        else if (objective is TargetedKillObjective targetedKillObj)
        {
            // Reflection kullanarak private killedCounts dictionary'sini al
            var killedCountsField = typeof(TargetedKillObjective).GetField("killedCounts", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (killedCountsField != null)
            {
                var killedCounts = killedCountsField.GetValue(targetedKillObj) as System.Collections.Generic.Dictionary<string, int>;
                if (killedCounts != null)
                {
                    // Her düşman türü için kill count'u kaydet
                    foreach (var kvp in killedCounts)
                    {
                        PlayerPrefs.SetInt($"{objectiveKey}_KillCount_{kvp.Key}", kvp.Value);
                    }
                    
                    // Kaç tür düşman kaydettiğimizi de sakla
                    PlayerPrefs.SetInt($"{objectiveKey}_EnemyTypeCount", killedCounts.Count);
                    
                    // Düşman türlerini de kaydet
                    int index = 0;
                    foreach (var enemyType in killedCounts.Keys)
                    {
                        PlayerPrefs.SetString($"{objectiveKey}_EnemyType_{index}", enemyType);
                        index++;
                    }
                }
            }
        }
    }
    
    // Quest ilerlemelerini yükle
    private void LoadQuestProgress()
    {
        // Tamamlanan questleri yükle
        if (PlayerPrefs.HasKey("CompletedQuests"))
        {
            string completedQuestsJson = PlayerPrefs.GetString("CompletedQuests");
            SerializableStringList completedQuestsList = JsonUtility.FromJson<SerializableStringList>(completedQuestsJson);
            completedQuests = new HashSet<string>(completedQuestsList.items);
        }
        
        if (!PlayerPrefs.HasKey("ActiveQuestIDs")) return;
        
        // Aktif quest ID'lerini yükle
        string questIDsJson = PlayerPrefs.GetString("ActiveQuestIDs");
        SerializableStringList questIDs = JsonUtility.FromJson<SerializableStringList>(questIDsJson);
        
        // Quest objective indekslerini yükle
        string indicesJson = PlayerPrefs.GetString("QuestObjectiveIndices", "");
        SerializableIntList objectiveIndices = new SerializableIntList();
        if (!string.IsNullOrEmpty(indicesJson))
        {
            objectiveIndices = JsonUtility.FromJson<SerializableIntList>(indicesJson);
        }
        
        // Questleri yeniden oluştur
        activeQuests.Clear();
        for (int i = 0; i < questIDs.items.Length; i++)
        {
            string questID = questIDs.items[i];
            QuestData questData = FindQuestDataByID(questID);
            
            if (questData != null)
            {
                var quest = new QuestInstance();
                quest.Data = questData;
                
                // Objective indeksini yükle
                if (i < objectiveIndices.items.Length)
                {
                    quest.currentObjectiveIndex = objectiveIndices.items[i];
                }
                
                // Objective durumlarını yükle
                LoadQuestObjectiveStates(quest);
                
                activeQuests.Add(quest);
                Debug.Log($"Loaded quest: {questData.title} (Objective: {quest.currentObjectiveIndex})");
            }
        }
        
        Debug.Log($"Quest progress loaded. Active quests: {activeQuests.Count}, Completed quests: {completedQuests.Count}");
    }
    
    // Quest objective durumlarını yükle
    private void LoadQuestObjectiveStates(QuestInstance quest)
    {
        if (quest.Data == null || quest.Data.objectives == null) return;
        
        for (int i = 0; i < quest.Data.objectives.Length; i++)
        {
            var objective = quest.Data.objectives[i];
            if (objective != null)
            {
                string objectiveKey = $"Quest_{quest.Data.questID}_Objective_{i}";
                
                objective.isCompleted = PlayerPrefs.GetInt($"{objectiveKey}_Completed", 0) == 1;
                objective.isInitialized = PlayerPrefs.GetInt($"{objectiveKey}_Initialized", 0) == 1;
                
                // Özel objective tiplerinin ek verilerini yükle
                LoadObjectiveSpecificData(objective, objectiveKey);
            }
        }
    }
    
    // Objective tipine özgü verileri yükle
    private void LoadObjectiveSpecificData(QuestObjective objective, string objectiveKey)
    {
        // KillObjective için özel yükleme
        if (objective is KillObjective killObj)
        {
            int currentAmount = PlayerPrefs.GetInt($"{objectiveKey}_CurrentAmount", 0);
            
            // Reflection kullanarak private currentAmount field'ını ayarla
            var currentAmountField = typeof(KillObjective).GetField("currentAmount", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (currentAmountField != null)
            {
                currentAmountField.SetValue(killObj, currentAmount);
            }
        }
        // TargetedKillObjective için özel yükleme
        else if (objective is TargetedKillObjective targetedKillObj)
        {
            // Reflection kullanarak private killedCounts dictionary'sini al
            var killedCountsField = typeof(TargetedKillObjective).GetField("killedCounts", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (killedCountsField != null)
            {
                var killedCounts = new System.Collections.Generic.Dictionary<string, int>();
                
                // Kaydedilen düşman türü sayısını al
                int enemyTypeCount = PlayerPrefs.GetInt($"{objectiveKey}_EnemyTypeCount", 0);
                
                // Her düşman türü için kaydedilen değerleri yükle
                for (int i = 0; i < enemyTypeCount; i++)
                {
                    string enemyType = PlayerPrefs.GetString($"{objectiveKey}_EnemyType_{i}", "");
                    if (!string.IsNullOrEmpty(enemyType))
                    {
                        int killCount = PlayerPrefs.GetInt($"{objectiveKey}_KillCount_{enemyType}", 0);
                        killedCounts[enemyType] = killCount;
                    }
                }
                
                // Dictionary'yi geri ata
                killedCountsField.SetValue(targetedKillObj, killedCounts);
            }
        }
    }
    
    // Quest ID'ye göre QuestData bul
    private QuestData FindQuestDataByID(string questID)
    {
        foreach (var quest in availableQuests)
        {
            if (quest != null && quest.questID == questID)
            {
                return quest;
            }
        }
        return null;
    }
    
    // Quest ilerlemelerini temizle
    private void ClearQuestProgress()
    {
        PlayerPrefs.DeleteKey("ActiveQuestIDs");
        PlayerPrefs.DeleteKey("QuestObjectiveIndices");
        PlayerPrefs.DeleteKey("CompletedQuests");
        
        // Tüm quest objective verilerini temizle
        foreach (var quest in availableQuests)
        {
            if (quest != null && !string.IsNullOrEmpty(quest.questID) && quest.objectives != null)
            {
                for (int i = 0; i < quest.objectives.Length; i++)
                {
                    string objectiveKey = $"Quest_{quest.questID}_Objective_{i}";
                    PlayerPrefs.DeleteKey($"{objectiveKey}_Completed");
                    PlayerPrefs.DeleteKey($"{objectiveKey}_Initialized");
                    PlayerPrefs.DeleteKey($"{objectiveKey}_CurrentAmount");
                    
                    // TargetedKillObjective için ek temizlik
                    int enemyTypeCount = PlayerPrefs.GetInt($"{objectiveKey}_EnemyTypeCount", 0);
                    for (int j = 0; j < enemyTypeCount; j++)
                    {
                        string enemyType = PlayerPrefs.GetString($"{objectiveKey}_EnemyType_{j}", "");
                        if (!string.IsNullOrEmpty(enemyType))
                        {
                            PlayerPrefs.DeleteKey($"{objectiveKey}_KillCount_{enemyType}");
                        }
                        PlayerPrefs.DeleteKey($"{objectiveKey}_EnemyType_{j}");
                    }
                    PlayerPrefs.DeleteKey($"{objectiveKey}_EnemyTypeCount");
                }
            }
        }
        
        PlayerPrefs.Save();
    }
    
    // QuestGiver durumlarını temizle
    private void ClearQuestGiverStates()
    {
        foreach (var quest in availableQuests)
        {
            if (quest != null && !string.IsNullOrEmpty(quest.questID))
            {
                string key = $"QuestGiven_{quest.questID}";
                PlayerPrefs.DeleteKey(key);
            }
        }
        PlayerPrefs.Save();
    }
    
    // JSON serileştirme için yardımcı sınıflar
    [System.Serializable]
    private class SerializableStringList
    {
        public string[] items;
    }
    
    [System.Serializable]
    private class SerializableIntList
    {
        public int[] items;
    }
}