using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SkillManager : MonoBehaviour
{
    public static SkillManager Instance { get; private set; }
    
    // Mevcut shard sayısı
    private int currentShards = 0;
    
    // Açılmış beceriler
    private HashSet<string> unlockedSkills = new HashSet<string>();
    
    // PlayerPrefs anahtarları
    private const string SHARDS_KEY = "PlayerShards";
    private const string SKILLS_KEY = "UnlockedSkills";
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadSkillData();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void OnDisable()
    {
        SaveSkillData();
    }
    
    // Oyun kapatıldığında çağrılır
    private void OnApplicationQuit()
    {
        // Oyun kapatılırken skill verilerini kesin olarak kaydet
        SaveSkillData();
        Debug.Log("Oyun kapatılıyor, skill verileri kaydedildi!");
    }
    
    // Shard sayısını al
    public int GetShardCount()
    {
        return currentShards;
    }
    
    // Shard ekle (item toplandığında)
    public void AddShards(int count)
    {
        currentShards += count;
        SaveSkillData();
    }
    
    // Shard kullan (skill açıldığında)
    public bool UseShards(int count)
    {
        if (currentShards >= count)
        {
            currentShards -= count;
            SaveSkillData();
            return true;
        }
        return false;
    }
    
    // Beceri açıldı mı kontrol et
    public bool IsSkillUnlocked(string skillName)
    {
        return unlockedSkills.Contains(skillName);
    }
    
    // Beceriyi aç
    public bool UnlockSkill(string skillName, int shardCost)
    {
        if (UseShards(shardCost))
        {
            unlockedSkills.Add(skillName);
            SaveSkillData();
            return true;
        }
        return false;
    }
    
    // Beceri veri kaydı
    private void SaveSkillData()
    {
        // Shard sayısını kaydet
        PlayerPrefs.SetInt(SHARDS_KEY, currentShards);
        
        // Açılmış becerileri kaydet
        string skillsJson = JsonUtility.ToJson(new SerializableStringArray { items = unlockedSkills.ToArray() });
        PlayerPrefs.SetString(SKILLS_KEY, skillsJson);
        
        PlayerPrefs.Save();
    }
    
    // Beceri verilerini yükle
    private void LoadSkillData()
    {
        // Shard sayısını yükle
        currentShards = PlayerPrefs.GetInt(SHARDS_KEY, 0);
        
        // Açılmış becerileri yükle
        if (PlayerPrefs.HasKey(SKILLS_KEY))
        {
            string skillsJson = PlayerPrefs.GetString(SKILLS_KEY);
            SerializableStringArray loadedSkills = JsonUtility.FromJson<SerializableStringArray>(skillsJson);
            
            unlockedSkills = new HashSet<string>(loadedSkills.items);
        }
    }
    
    // Debug için tüm becerileri sıfırla
    public void ResetAllSkills()
    {
        unlockedSkills.Clear();
        SaveSkillData();
    }
    
    // String dizisini serileştirmek için yardımcı sınıf (ItemCollectionManager'daki gibi)
    [System.Serializable]
    private class SerializableStringArray
    {
        public string[] items;
    }
} 