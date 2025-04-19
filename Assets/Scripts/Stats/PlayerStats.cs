using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStats : CharacterStats
{
    public Player player;
    
    [Header("Experience System")]
    [SerializeField] private int experience = 0;
    [SerializeField] private int experienceToNextLevel = 100;
    [SerializeField] private float experienceLevelMultiplier = 1.5f; // Each level requires 50% more XP
    
    [Header("Skill Points")]
    [SerializeField] private int availableSkillPoints = 0;
    public int AvailableSkillPoints => availableSkillPoints;
    
    [Header("UI References")]
    [SerializeField] private Image experienceBar;
    [SerializeField] private TextMeshProUGUI levelText;

    protected override void Start()
    {
        base.Start();
        
        player = GetComponent<Player>();
        
        // Initialize UI if available
        UpdateLevelUI();
    }
    
    private void Update()
    {
        // Debug key to gain experience (can be removed in final build)
        if (Input.GetKeyDown(KeyCode.X))
        {
            AddExperience(25);
        }
    }

    public void AddExperience(int amount)
    {
        if (amount <= 0) return;
        
        experience += amount;
        Debug.Log($"Added {amount} XP. Total XP: {experience}");
        
        // Check for level up
        CheckLevelUp();
        
        // Update UI
        UpdateLevelUI();
    }
    
    private void CheckLevelUp()
    {
        while (experience >= experienceToNextLevel)
        {
            // Level up
            level++;
            experience -= experienceToNextLevel;
            
            // Increase XP needed for next level
            experienceToNextLevel = (int)(experienceToNextLevel * experienceLevelMultiplier);
            
            Debug.Log($"Level up! New level: {level}");
            
            // Her level atlamada skill point kazandır
            availableSkillPoints++;
            
            // Heal on level up
            currentHealth = maxHealth.GetValue();
            currentMana = maxMana.GetValue();
            
            // Update UI
            UpdateLevelUI();
            
            // Could trigger level up effects here
            if (player != null && player.entityFX != null)
            {
                // Play level up effect if implemented
                // player.entityFX.PlayLevelUpEffect();
            }
        }
    }
    
    private void UpdateLevelUI()
    {
        // Update level text if assigned
        if (levelText != null)
        {
            levelText.text = "Lvl " + level.ToString();
            
            // Eğer skill point varsa göster
            if (availableSkillPoints > 0)
            {
                levelText.text += " (SP: " + availableSkillPoints + ")";
            }
        }
        
        // Update experience bar if assigned
        if (experienceBar != null)
        {
            float experienceRatio = (float)experience / experienceToNextLevel;
            experienceBar.fillAmount = experienceRatio;
        }
    }
    
    // Stat artırma metodları
    public void IncreaseMaxHealth()
    {
        if (availableSkillPoints <= 0) return;
        
        // Mevcut değerin %10'u kadar artış
        float increaseAmount = maxHealth.GetValue() * 0.10f;
        maxHealth.AddModifier(increaseAmount, StatModifierType.LevelBonus);
        
        // Can değerini de güncelle
        currentHealth = maxHealth.GetValue();
        
        // Bir skill point harca
        availableSkillPoints--;
        
        Debug.Log($"Max Health increased to {maxHealth.GetValue()}");
        
        // UI güncellemesi
        UpdateLevelUI();
        
        // Health bar güncelleme
        if (player != null && player.healthBar != null)
        {
            player.healthBar.UpdateHealthBar(currentHealth, maxHealth.GetValue());
        }
    }
    
    public void IncreaseMaxMana()
    {
        if (availableSkillPoints <= 0) return;
        
        // Mevcut değerin %15'i kadar artış
        float increaseAmount = maxMana.GetValue() * 0.15f;
        maxMana.AddModifier(increaseAmount, StatModifierType.LevelBonus);
        
        // Mana değerini de güncelle
        currentMana = maxMana.GetValue();
        
        // Bir skill point harca
        availableSkillPoints--;
        
        Debug.Log($"Max Mana increased to {maxMana.GetValue()}");
        
        // UI güncellemesi
        UpdateLevelUI();
    }
    
    public void IncreaseDamage()
    {
        if (availableSkillPoints <= 0) return;
        
        // Mevcut değerin %8'i kadar artış
        float increaseAmount = baseDamage.GetValue() * 0.08f;
        baseDamage.AddModifier(increaseAmount, StatModifierType.LevelBonus);
        
        // Bir skill point harca
        availableSkillPoints--;
        
        Debug.Log($"Damage increased to {baseDamage.GetValue()}");
        
        // UI güncellemesi
        UpdateLevelUI();
    }
    
    // Dışarıdan skill point azaltmak için
    public void ReduceSkillPoint()
    {
        if (availableSkillPoints > 0)
        {
            availableSkillPoints--;
            UpdateLevelUI();
        }
    }

    public override void TakeDamage(float _damage)
    {
        base.TakeDamage(_damage);
    }

    public override void Die()
    {
        base.Die();
        
        player.Die();
    }
}