using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class SkillButtonManager : MonoBehaviour
{
    [Header("Skill System")]
    [SerializeField] private SkillType[] availableSkillTypes; // Inspector'dan atanacak skill type'lar
    private SkillType[] assignedSkillTypes = new SkillType[4]; // Buttonlara atanan skill type'lar
    
    [Header("UI Components")]
    [SerializeField] private Button[] skillButtons; // Skill button'ları
    [SerializeField] private Image[] buttonIcons; // Button icon'ları
    [SerializeField] private Image[] cooldownBars; // Cooldown bar'ları

    private void Awake()
    {
        // Rastgele 4 skill ata
        AssignRandomSkills();
    }
    
    private void AssignRandomSkills()
    {
        if (availableSkillTypes == null || availableSkillTypes.Length == 0)
        {
            Debug.LogWarning("No available skill types assigned to SkillButtonManager!");
            return;
        }
        
        // Mevcut skill type'lardan rastgele 4 tane seç
        List<SkillType> shuffledSkills = availableSkillTypes.ToList();
        for (int i = shuffledSkills.Count - 1; i > 0; i--)
        {
            int randomIndex = UnityEngine.Random.Range(0, i + 1);
            SkillType temp = shuffledSkills[i];
            shuffledSkills[i] = shuffledSkills[randomIndex];
            shuffledSkills[randomIndex] = temp;
        }
        
        // İlk 4 skill'i buttonlara ata
        for (int i = 0; i < Mathf.Min(4, shuffledSkills.Count); i++)
        {
            assignedSkillTypes[i] = shuffledSkills[i];
            UpdateButtonUI(i);
        }
    }
    
    private void UpdateButtonUI(int buttonIndex)
    {
        if (buttonIndex >= assignedSkillTypes.Length || assignedSkillTypes[buttonIndex] == SkillType.None)
            return;
            
        SkillType skillType = assignedSkillTypes[buttonIndex];
        
        // Icon'u SkillType'dan al ve güncelle
        if (buttonIndex < buttonIcons.Length && buttonIcons[buttonIndex] != null)
        {
            Sprite icon = skillType.GetIcon();
            if (icon != null)
            {
                buttonIcons[buttonIndex].sprite = icon;
                buttonIcons[buttonIndex].enabled = true;
            }
        }
        
        // Cooldown bar'ı sıfırla
        if (buttonIndex < cooldownBars.Length && cooldownBars[buttonIndex] != null)
        {
            cooldownBars[buttonIndex].fillAmount = 0f;
        }
    }
    
    private void Update()
    {
        // Cooldown bar'larını güncelle
        UpdateCooldownBars();
    }
    
    private void UpdateCooldownBars()
    {
        for (int i = 0; i < assignedSkillTypes.Length; i++)
        {
            if (assignedSkillTypes[i] == SkillType.None || i >= cooldownBars.Length || cooldownBars[i] == null)
                continue;
                
            // SkillManager'dan cooldown bilgisini al
            if (SkillManager.Instance != null)
            {
                float cooldownTimer = SkillManager.Instance.GetSkillCooldown(assignedSkillTypes[i]);
                float baseCooldown = GetBaseCooldown(assignedSkillTypes[i]);
                
                if (baseCooldown > 0)
                {
                    float cooldownProgress = 1f - (cooldownTimer / baseCooldown);
                    cooldownBars[i].fillAmount = Mathf.Clamp01(cooldownProgress);
                }
            }
        }
    }
    
    private float GetBaseCooldown(SkillType skillType)
    {
        // SkillManager'dan skill bilgisini al
        if (SkillManager.Instance != null)
        {
            var allSkills = SkillManager.Instance.GetAllSkills();
            foreach (var skill in allSkills)
            {
                if (skill.skillType == skillType)
                {
                    return skill.baseCooldown;
                }
            }
        }
        return 5f; // Default cooldown
    }
    
    // Button'a tıklandığında skill kullan
    public void OnSkillButtonClicked(int buttonIndex)
    {
        if (buttonIndex >= assignedSkillTypes.Length || assignedSkillTypes[buttonIndex] == SkillType.None)
            return;
            
        SkillType skillType = assignedSkillTypes[buttonIndex];
        
        // Skill'i kullan
        if (SkillManager.Instance != null)
        {
            SkillManager.Instance.UseSkill(skillType);
            Debug.Log($"Used skill: {skillType}");
        }
    }
    
    // Manuel olarak skill'leri yeniden ata
    public void ReassignSkills()
    {
        AssignRandomSkills();
    }
    
    // Belirli bir skill'i belirli bir button'a ata
    public void AssignSkillToButton(int buttonIndex, SkillType skillType)
    {
        if (buttonIndex >= assignedSkillTypes.Length)
            return;
            
        assignedSkillTypes[buttonIndex] = skillType;
        UpdateButtonUI(buttonIndex);
    }
    
    // Atanan skill'leri al
    public SkillType[] GetAssignedSkills()
    {
        return assignedSkillTypes;
    }
} 