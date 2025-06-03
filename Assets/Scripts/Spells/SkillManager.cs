using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public enum SkillType
{
    None,
    IceShard,
    EarthPush,
    FireSpell,
    VoidSkill,
    ElectricDash,
    AirPush,
    FireballSpell
}

[System.Serializable]
public class SkillInfo
{
    public string skillID;
    public SkillType skillType;
    public string skillName;
    public float baseCooldown = 5f;
    public float cooldownTimer = 0f;
    public float manaCost = 10f;
    public bool isUnlocked = false;
    public GameObject skillPrefab;
    
    public bool IsReady(float currentMana)
    {
        return cooldownTimer <= 0f && currentMana >= manaCost && isUnlocked;
    }
}

public class SkillManager : MonoBehaviour
{
    public static SkillManager Instance { get; private set; }
    
    // Beceri listesi
    [SerializeField] private List<SkillInfo> skills = new List<SkillInfo>();
    
    // Beceri verilerine hızlı erişim için
    private Dictionary<SkillType, SkillInfo> skillDict = new Dictionary<SkillType, SkillInfo>();
    private Dictionary<string, SkillInfo> skillIdDict = new Dictionary<string, SkillInfo>();
    
    // Cooldown azaltma
    [Range(0f, 80f)]
    [SerializeField] private float globalCooldownReduction = 0f;
    
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
            
            // Dictionary'leri doldur
            InitializeSkillDictionaries();
            
            // Kaydedilmiş verileri yükle
            LoadSkillData();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void InitializeSkillDictionaries()
    {
        skillDict.Clear();
        skillIdDict.Clear();
        
        // Varsayılan becerileri ekle
        if (skills.Count == 0)
        {
            AddDefaultSkills();
        }
        
        // Dict'lere doldur
        foreach (var skill in skills)
        {
            if (skill.skillType != SkillType.None && !skillDict.ContainsKey(skill.skillType))
            {
                skillDict.Add(skill.skillType, skill);
            }
            
            if (!string.IsNullOrEmpty(skill.skillID) && !skillIdDict.ContainsKey(skill.skillID))
            {
                skillIdDict.Add(skill.skillID, skill);
            }
        }
    }
    
    private void AddDefaultSkills()
    {
        // Earth Push becerisi (Level 1 Earth skill)
        skills.Add(new SkillInfo
        {
            skillID = "earth_push",
            skillType = SkillType.EarthPush,
            skillName = "Earth Push",
            baseCooldown = 3f,
            manaCost = 25f,
            isUnlocked = false // Skill Tree'den açılacak
        });
        
        // Ice Shard becerisi (Level 1 Ice skill)
        skills.Add(new SkillInfo
        {
            skillID = "ice_shard",
            skillType = SkillType.IceShard,
            skillName = "Ice Shard",
            baseCooldown = 5f,
            manaCost = 20f,
            isUnlocked = false // Skill Tree'den açılacak
        });
        
        // Fire Spell becerisi (Level 1 Fire skill)
        skills.Add(new SkillInfo
        {
            skillID = "FireSpell",
            skillType = SkillType.FireSpell,
            skillName = "Fire Spell",
            baseCooldown = 7f,
            manaCost = 5f,
            isUnlocked = false // Skill Tree'den açılacak
        });
        
        // Void Skill becerisi (Level 1 Void skill)
        skills.Add(new SkillInfo
        {
            skillID = "void_skill",
            skillType = SkillType.VoidSkill,
            skillName = "Void Disappear",
            baseCooldown = 15f,
            manaCost = 40f,
            isUnlocked = false // Skill Tree'den açılacak
        });
        
        // Electric Dash becerisi (Level 1 Electric skill)
        skills.Add(new SkillInfo
        {
            skillID = "electric_dash",
            skillType = SkillType.ElectricDash,
            skillName = "Electric Dash",
            baseCooldown = 8f,
            manaCost = 30f,
            isUnlocked = false // Skill Tree'den açılacak
        });
        
        // Air Push becerisi (Level 1 Air skill)
        skills.Add(new SkillInfo
        {
            skillID = "air_push",
            skillType = SkillType.AirPush,
            skillName = "Air Push",
            baseCooldown = 2f,
            manaCost = 15f,
            isUnlocked = false // Skill Tree'den açılacak
        });
        
        // Fireball Spell becerisi (Level 2 Fire skill)
        skills.Add(new SkillInfo
        {
            skillID = "fireball_spell",
            skillType = SkillType.FireballSpell,
            skillName = "Fireball Spell",
            baseCooldown = 4f,
            manaCost = 18f,
            isUnlocked = false // Skill Tree'den açılacak
        });
    }
    
    private void Update()
    {
        // Tüm cooldown'ları güncelle
        UpdateAllCooldowns();
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
    
    private void UpdateAllCooldowns()
    {
        foreach (var skill in skills)
        {
            if (skill.cooldownTimer > 0)
            {
                skill.cooldownTimer -= Time.deltaTime;
                if (skill.cooldownTimer < 0)
                {
                    skill.cooldownTimer = 0;
                }
            }
        }
    }
    
    // Beceri kullanma işlemi
    public void UseSkill(SkillType skillType)
    {
        if (skillDict.TryGetValue(skillType, out SkillInfo skill))
        {
            // Cooldown'a etkili azaltma uygula
            float effectiveCooldown = skill.baseCooldown * (1f - globalCooldownReduction / 100f);
            skill.cooldownTimer = effectiveCooldown;
        }
    }
    
    // Beceri hazır mı kontrolü (cooldown + mana + unlock)
    public bool IsSkillReady(SkillType skillType, float currentMana)
    {
        if (skillDict.TryGetValue(skillType, out SkillInfo skill))
        {
            return skill.IsReady(currentMana);
        }
        return false;
    }
    
    // Beceri mana maliyeti
    public float GetSkillManaCost(SkillType skillType)
    {
        if (skillDict.TryGetValue(skillType, out SkillInfo skill))
        {
            return skill.manaCost;
        }
        return 0f;
    }
    
    // Beceri cooldown süresi
    public float GetSkillCooldown(SkillType skillType)
    {
        if (skillDict.TryGetValue(skillType, out SkillInfo skill))
        {
            return skill.cooldownTimer;
        }
        return 0f;
    }
    
    // Beceri açık mı kontrolü (ID ile)
    public bool IsSkillUnlocked(string skillID)
    {
        // Önce dictionary'de kontrol et
        if (skillIdDict.TryGetValue(skillID, out SkillInfo skill))
        {
            // Eğer skill dictionary'de varsa ve isUnlocked true ise, direkt true döndür
            // HashSet'e de bakmamız gerekiyor çünkü runtime'da açılan skillerin durumu
            return skill.isUnlocked || unlockedSkills.Contains(skillID);
        }
        
        // Dictionary'de yoksa HashSet'te kontrol et
        return unlockedSkills.Contains(skillID);
    }
    
    // Beceriyi aç
    public bool UnlockSkill(string skillID, int shardCost)
    {
        if (UseShards(shardCost))
        {
            unlockedSkills.Add(skillID);
            
            // Dictionary'deki skill'i de güncelle
            if (skillIdDict.TryGetValue(skillID, out SkillInfo skill))
            {
                skill.isUnlocked = true;
            }
            
            SaveSkillData();
            return true;
        }
        return false;
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
    
    // Skill verilerini kaydet
    private void SaveSkillData()
    {
        // Shard sayısını kaydet
        PlayerPrefs.SetInt(SHARDS_KEY, currentShards);
        
        // Açılmış becerileri kaydet
        string skillsJson = JsonUtility.ToJson(new SerializableStringArray { items = unlockedSkills.ToArray() });
        PlayerPrefs.SetString(SKILLS_KEY, skillsJson);
        
        PlayerPrefs.Save();
    }
    
    // Skill verilerini yükle
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
            
            // Dictionary'deki skill'leri de güncelle
            foreach (string skillID in unlockedSkills)
            {
                if (skillIdDict.TryGetValue(skillID, out SkillInfo skill))
                {
                    skill.isUnlocked = true;
                }
            }
        }
    }
    
    // Debug için tüm becerileri sıfırla
    public void ResetAllSkills()
    {
        unlockedSkills.Clear();
        SaveSkillData();
        
        foreach (var skill in skills)
        {
            skill.cooldownTimer = 0f;
        }
    }
    
    // Debug için skill durumunu kontrol etme metodu
    public void DebugSkillStatus(string skillID)
    {
        Debug.Log($"=== Debugging Skill: {skillID} ===");
        
        if (skillIdDict.TryGetValue(skillID, out SkillInfo skill))
        {
            Debug.Log($"Found in dictionary: isUnlocked = {skill.isUnlocked}");
            Debug.Log($"Skill Type: {skill.skillType}");
            Debug.Log($"Skill Name: {skill.skillName}");
        }
        else
        {
            Debug.Log($"NOT found in skillIdDict!");
        }
        
        bool inHashSet = unlockedSkills.Contains(skillID);
        Debug.Log($"Found in unlockedSkills HashSet: {inHashSet}");
        
        bool finalResult = IsSkillUnlocked(skillID);
        Debug.Log($"Final IsSkillUnlocked result: {finalResult}");
        
        Debug.Log($"skillIdDict count: {skillIdDict.Count}");
        Debug.Log($"unlockedSkills count: {unlockedSkills.Count}");
        
        // Tüm dictionary keylerini listele
        Debug.Log("All skillIdDict keys:");
        foreach (string key in skillIdDict.Keys)
        {
            Debug.Log($"  - {key}");
        }
    }
    
    // String dizisini serileştirmek için yardımcı sınıf (ItemCollectionManager'daki gibi)
    [System.Serializable]
    private class SerializableStringArray
    {
        public string[] items;
    }
} 