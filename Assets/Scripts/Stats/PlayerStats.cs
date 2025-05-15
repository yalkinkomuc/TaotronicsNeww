using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStats : CharacterStats
{
    public Player player;
    
    [Header("Attribute System")]
    [SerializeField] private int vitality = 0;  // Increases max health
    [SerializeField] private int might = 0;     // Increases attack damage
    [SerializeField] private int agility = 0;   // Increases speed (already used for mana)
    [SerializeField] private int defense = 0;   // Reduces incoming damage
    [SerializeField] private int luck = 0;      // Increases critical chance
    
    // Derived stats from attributes
    public float criticalChance { get; private set; } = 0f;
    public float criticalDamage { get; private set; } = 1.5f;  // Base critical damage multiplier
    public float attackPower { get; private set; } = 0f;
    public float speedStat { get; private set; } = 0f;
    public float defenseStat { get; private set; } = 0f;
    
    // Per-point attribute bonuses
    private const float HEALTH_PER_VITALITY = 30f;
    private const float DAMAGE_PER_MIGHT = 2.5f;
    private const float MANA_PER_AGILITY = 10f;
    private const float DEFENSE_PER_POINT = 3f;
    private const float CRIT_CHANCE_PER_LUCK = 0.01f;  // 1% per point
    
    [Header("Experience System")]
    [SerializeField] private int experience = 0;
    [SerializeField] private int experienceToNextLevel = 100;
    [SerializeField] private float experienceLevelMultiplier = 1.5f; // Each level requires 50% more XP
    
    [Header("Skill Points")]
    [SerializeField] private int availableSkillPoints = 0;
    public int AvailableSkillPoints => availableSkillPoints;
    
    // For accessing attributes from UI
    public int Vitality => vitality;
    public int Might => might;
    public int Agility => agility;
    public int Defense => defense;
    public int Luck => luck;
    
    [Header("Currency")]
    [SerializeField] public int gold = 0;
    
    [Header("UI References")]
    [SerializeField] private Image experienceBar;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI goldText;

    private float playerBaseMaxHealth;
    private float playerBaseMaxDamage;

    protected override void Awake()
    {
        // Önce baz sınıfın Awake metodunu çağır
        base.Awake();
    }

    protected override void Start()
    {
        // Baz değerleri sakla
        playerBaseMaxHealth = maxHealth.GetValue();
        playerBaseMaxDamage = baseDamage.GetValue();
        
        // Önce kaydedilmiş verileri yükle
        LoadPlayerData();
        
        // Apply attribute bonuses before CharacterStats initialization
        ApplyAttributeBonuses();
        
        // Sonra CharacterStats'dan gelen işlemleri devam ettir
        base.Start();
        
        player = GetComponent<Player>();
        
        // UI'ı güncelle
        UpdateLevelUI();
        
        // Ensure weapons are upgraded with saved values
        if (BlacksmithManager.Instance != null)
        {
            BlacksmithManager.Instance.ApplyWeaponUpgrades(this);
        }
    }
    
    private void Update()
    {
        // Debug key to gain experience (can be removed in final build)
        if (Input.GetKeyDown(KeyCode.X))
        {
            AddExperience(25);
        }
        
        // Debug key to add gold (can be removed in final build)
        if (Input.GetKeyDown(KeyCode.G))
        {
            AddGold(50);
        }
    }
    
    // Apply all attribute bonuses to stats
    private void ApplyAttributeBonuses()
    {
        // Reset any previous attribute bonuses
        maxHealth.RemoveAllModifiersOfType(StatModifierType.Attribute);
        baseDamage.RemoveAllModifiersOfType(StatModifierType.Attribute);
        maxMana.RemoveAllModifiersOfType(StatModifierType.Attribute);
        
        // Apply vitality to health
        float healthBonus = vitality * HEALTH_PER_VITALITY;
        maxHealth.AddModifier(healthBonus, StatModifierType.Attribute);
        
        // Apply might to damage
        float damageBonus = might * DAMAGE_PER_MIGHT;
        baseDamage.AddModifier(damageBonus, StatModifierType.Attribute);
        
        // Apply agility to mana (replacing the old mana upgrade)
        float manaBonus = agility * MANA_PER_AGILITY;
        maxMana.AddModifier(manaBonus, StatModifierType.Attribute);
        
        // Apply defense stat
        defenseStat = defense * DEFENSE_PER_POINT;
        
        // Apply luck to critical hit chance
        criticalChance = luck * CRIT_CHANCE_PER_LUCK;
        
        // Calculate derived stats for UI
        attackPower = baseDamage.GetValue();
        speedStat = 300 + (agility * 6); // Base speed + agility bonus
        
        Debug.Log($"Applied attribute bonuses: HP +{healthBonus}, DMG +{damageBonus}, Mana +{manaBonus}");
        Debug.Log($"Defense: {defenseStat}, Crit Chance: {criticalChance*100}%, Speed: {speedStat}");
    }

    public void AddExperience(int amount)
    {
        if (amount <= 0) return;
        
        experience += amount;
        
        // Check for level up
        CheckLevelUp();
        
        // Update UI
        UpdateLevelUI();
        
        // Save experience data after gaining XP
        SaveExperienceData();
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
            
            // Save experience and level data after leveling up
            SaveExperienceData();
            
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
        
        // Update gold text if assigned
        if (goldText != null)
        {
            goldText.text = gold.ToString() + " G";
        }
    }
    
    // Gold handling methods
    public void AddGold(int amount)
    {
        if (amount <= 0) return;
        
        gold += amount;
        UpdateLevelUI();
        SaveGoldData();
        
        Debug.Log($"Added {amount} gold. Total gold: {gold}");
    }
    
    public bool SpendGold(int amount)
    {
        if (amount <= 0) return true;
        if (gold < amount) return false;
        
        gold -= amount;
        UpdateLevelUI();
        SaveGoldData();
        
        Debug.Log($"Spent {amount} gold. Remaining gold: {gold}");
        return true;
    }
    
    private void SaveGoldData()
    {
        PlayerPrefs.SetInt("PlayerGold", gold);
        PlayerPrefs.Save();
    }
    
    // Save player experience and level data to PlayerPrefs
    private void SaveExperienceData()
    {
        // Save level and experience values to PlayerPrefs
        PlayerPrefs.SetInt("PlayerLevel", level);
        PlayerPrefs.SetInt("PlayerExperience", experience);
        PlayerPrefs.SetInt("PlayerExperienceToNextLevel", experienceToNextLevel);
        PlayerPrefs.SetInt("PlayerSkillPoints", availableSkillPoints);
        PlayerPrefs.Save();
        
        Debug.Log($"Player progress saved: Level={level}, XP={experience}/{experienceToNextLevel}, SP={availableSkillPoints}");
    }
    
    // Kaydedilmiş oyuncu verilerini PlayerPrefs'den yükle
    private void LoadPlayerData()
    {
        // Eğer kaydedilmiş veriler varsa yükle
        if (PlayerPrefs.HasKey("PlayerLevel"))
        {
            // Seviye, deneyim ve skill point verilerini yükle
            level = PlayerPrefs.GetInt("PlayerLevel", 1);
            experience = PlayerPrefs.GetInt("PlayerExperience", 0);
            experienceToNextLevel = PlayerPrefs.GetInt("PlayerExperienceToNextLevel", 100);
            availableSkillPoints = PlayerPrefs.GetInt("PlayerSkillPoints", 0);
            gold = PlayerPrefs.GetInt("PlayerGold", 0);
            
            // Attribute values
            vitality = PlayerPrefs.GetInt("PlayerVitality", 0);
            might = PlayerPrefs.GetInt("PlayerMight", 0);
            agility = PlayerPrefs.GetInt("PlayerAgility", 0);
            defense = PlayerPrefs.GetInt("PlayerDefense", 0);
            luck = PlayerPrefs.GetInt("PlayerLuck", 0);
            
            Debug.Log($"Oyuncu verileri yüklendi: Seviye={level}, Vit={vitality}, Might={might}, Agi={agility}, Def={defense}, Luck={luck}, Gold={gold}, XP={experience}/{experienceToNextLevel}, SP={availableSkillPoints}");
        }
        
        // Can ve manayı doldur
        currentHealth = maxHealth.GetValue();
        currentMana = maxMana.GetValue();
        
        // Player ve healthBar referanslarının güncellendiğinden emin olalım
        RefreshReferences();
        
        // Health bar'ı güncelle
        UpdateHealthBarUI();
    }
    
    // Gerekli referansları güncelle
    private void RefreshReferences()
    {
        if (player == null)
        {
            player = GetComponent<Player>();
        }
        
        if (player != null && player.healthBar == null)
        {
            player.healthBar = GetComponent<HealthBar>();
        }
    }
    
    // Health Bar UI güncellemesi
    private void UpdateHealthBarUI()
    {
        if (player != null && player.healthBar != null)
        {
            player.healthBar.UpdateHealthBar(currentHealth, maxHealth.GetValue());
            Debug.Log($"Health Bar UI güncellendi: {currentHealth}/{maxHealth.GetValue()}");
        }
    }
    
    // New attribute upgrade methods
    public void IncreaseVitality()
    {
        if (availableSkillPoints <= 0) return;
        
        vitality++;
        availableSkillPoints--;
        
        // Apply vitality bonus
        ApplyAttributeBonuses();
        
        // Update health
        float healthPercentage = currentHealth / maxHealth.GetValue();
        currentHealth = maxHealth.GetValue() * healthPercentage;
        
        // Update UI
        UpdateLevelUI();
        UpdateHealthBarUI();
        
        // Save changes
        SaveStatsData();
    }
    
    public void IncreaseMight()
    {
        if (availableSkillPoints <= 0) return;
        
        might++;
        availableSkillPoints--;
        
        // Apply might bonus
        ApplyAttributeBonuses();
        
        // Update UI
        UpdateLevelUI();
        
        // Save changes
        SaveStatsData();
    }
    
    public void IncreaseAgility()
    {
        if (availableSkillPoints <= 0) return;
        
        agility++;
        availableSkillPoints--;
        
        // Apply agility bonus
        ApplyAttributeBonuses();
        
        // Update mana
        float manaPercentage = currentMana / maxMana.GetValue();
        currentMana = maxMana.GetValue() * manaPercentage;
        
        // Update UI
        UpdateLevelUI();
        
        // Save changes
        SaveStatsData();
    }
    
    public void IncreaseDefense()
    {
        if (availableSkillPoints <= 0) return;
        
        defense++;
        availableSkillPoints--;
        
        // Apply defense bonus
        ApplyAttributeBonuses();
        
        // Update UI
        UpdateLevelUI();
        
        // Save changes
        SaveStatsData();
    }
    
    public void IncreaseLuck()
    {
        if (availableSkillPoints <= 0) return;
        
        luck++;
        availableSkillPoints--;
        
        // Apply luck bonus
        ApplyAttributeBonuses();
        
        // Update UI
        UpdateLevelUI();
        
        // Save changes
        SaveStatsData();
    }
    
    // Legacy methods for compatibility - redirect to the new attribute methods
    public void IncreaseMaxHealth()
    {
        IncreaseVitality();
    }
    
    public void IncreaseDamage()
    {
        IncreaseMight();
    }
    
    public void IncreaseMaxMana()
    {
        IncreaseAgility();
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

    public override void TakeDamage(float damage)
    {
        // Apply defense reduction to damage
        float damageReduction = Mathf.Min(defenseStat, damage * 0.7f); // Max 70% damage reduction
        float finalDamage = damage - damageReduction;
        
        // Ensure minimum damage of 1
        finalDamage = Mathf.Max(1f, finalDamage);
        
        // Apply the reduced damage
        base.TakeDamage(finalDamage);
        
        // Health bar'ı güncelle
        UpdateHealthBarUI();
    }

    public override void Die()
    {
        base.Die();
        
        player.Die();
    }

    // Save stat values to PlayerPrefs
    private void SaveStatsData()
    {
        // Save all attribute values
        PlayerPrefs.SetInt("PlayerVitality", vitality);
        PlayerPrefs.SetInt("PlayerMight", might);
        PlayerPrefs.SetInt("PlayerAgility", agility);
        PlayerPrefs.SetInt("PlayerDefense", defense);
        PlayerPrefs.SetInt("PlayerLuck", luck);
        
        // Save skill points
        PlayerPrefs.SetInt("PlayerSkillPoints", availableSkillPoints);
        PlayerPrefs.Save();
        
        Debug.Log($"Player attributes saved: Vitality={vitality}, Might={might}, Agility={agility}, Defense={defense}, Luck={luck}, SP={availableSkillPoints}");
    }

    // Reset all attributes and get skill points back
    public void ResetAllAttributes()
    {
        // Calculate total spent points
        int totalSpentPoints = vitality + might + agility + defense + luck;
        
        // Restore skill points
        availableSkillPoints += totalSpentPoints;
        
        // Reset attribute values
        vitality = 0;
        might = 0;
        agility = 0;
        defense = 0;
        luck = 0;
        
        // Reapply all attribute bonuses (zeros out the bonuses)
        ApplyAttributeBonuses();
        
        // Reset health and mana percentage
        currentHealth = maxHealth.GetValue();
        currentMana = maxMana.GetValue();
        
        // Update UI elements
        UpdateHealthBarUI();
        UpdateLevelUI();
        
        // Save changes
        SaveStatsData();
        
        Debug.Log($"All attributes reset. Returned {totalSpentPoints} skill points. Total available: {availableSkillPoints}");
    }
}