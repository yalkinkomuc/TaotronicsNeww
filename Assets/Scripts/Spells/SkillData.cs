using UnityEngine;

[CreateAssetMenu(fileName = "NewSkill", menuName = "Data/Skill")]
public class SkillData : ScriptableObject
{
    [Header("Skill Info")]
    public string skillID; // Unique skill identifier
    public string skillName;
    public string description;
    public Sprite icon;
    
    [Header("Unlock Requirements")]
    public int shardCost = 50; // Skill açmak için gereken shard miktarı
    
    [Header("Skill Effects")]
    public GameObject skillPrefab; // Skill yeteneğinin prefabı (VoidSkill vb.)
    public float cooldown = 10f; // Yetenek bekleme süresi
    public float manaCost = 30f; // Mana maliyeti
    
    // Yetenek kullanılabilir mi kontrolü
    public bool CanUseSkill()
    {
        // SkillManager'dan bu yeteneğin açık olup olmadığını kontrol et
        return SkillManager.Instance != null && SkillManager.Instance.IsSkillUnlocked(skillID);
    }
} 