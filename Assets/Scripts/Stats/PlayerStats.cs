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
            
            // Add skill point on level up
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
            
            // Show skill points if available
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
    
    // Stat upgrade methods
    public void IncreaseMaxHealth()
    {
        if (availableSkillPoints <= 0) return;
        
        // Increase by 10% of current value
        float increaseAmount = maxHealth.GetValue() * 0.10f;
        maxHealth.AddModifier(increaseAmount, StatModifierType.LevelBonus);
        
        // Update current health
        currentHealth = maxHealth.GetValue();
        
        // Use a skill point
        availableSkillPoints--;
        
        // Update UI
        UpdateLevelUI();
        
        // Update health bar
        if (player != null && player.healthBar != null)
        {
            player.healthBar.UpdateHealthBar(currentHealth, maxHealth.GetValue());
        }
    }
    
    public void IncreaseMaxMana()
    {
        if (availableSkillPoints <= 0) return;
        
        // Increase by 15% of current value
        float increaseAmount = maxMana.GetValue() * 0.15f;
        maxMana.AddModifier(increaseAmount, StatModifierType.LevelBonus);
        
        // Update current mana
        currentMana = maxMana.GetValue();
        
        // Use a skill point
        availableSkillPoints--;
        
        // Update UI
        UpdateLevelUI();
    }
    
    public void IncreaseDamage()
    {
        if (availableSkillPoints <= 0) return;
        
        // Increase by 8% of current value
        float increaseAmount = baseDamage.GetValue() * 0.08f;
        baseDamage.AddModifier(increaseAmount, StatModifierType.LevelBonus);
        
        // Use a skill point
        availableSkillPoints--;
        
        // Update UI
        UpdateLevelUI();
    }
    
    // Method to reduce skill points externally
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