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
            
            // Stat değerlerini yükle
            float savedMaxHealth = PlayerPrefs.GetFloat("PlayerMaxHealth", maxHealth.GetValue());
            float savedMaxMana = PlayerPrefs.GetFloat("PlayerMaxMana", maxMana.GetValue());
            float savedBaseDamage = PlayerPrefs.GetFloat("PlayerBaseDamage", baseDamage.GetValue());
            
            // Farkları hesapla ve modifierları ekle
            float healthDiff = savedMaxHealth - maxHealth.GetValue();
            if (healthDiff > 0)
            {
                maxHealth.AddModifier(healthDiff, StatModifierType.Equipment);
            }
            
            float manaDiff = savedMaxMana - maxMana.GetValue();
            if (manaDiff > 0)
            {
                maxMana.AddModifier(manaDiff, StatModifierType.Equipment);
            }
            
            // Not: baseDamage artık Blacksmith tarafından yönetiliyor
            // Silah upgrade'leri ayrıca uygulanacak
            
            Debug.Log($"Oyuncu verileri yüklendi: Seviye={level}, MaxHP={savedMaxHealth}, MaxMana={savedMaxMana}, Gold={gold}, XP={experience}/{experienceToNextLevel}, SP={availableSkillPoints}");
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
        
        // Save changes after upgrade
        SaveStatsData();
        
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
        
        // Save changes after upgrade
        SaveStatsData();
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
        
        // Save changes after upgrade
        SaveStatsData();
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
        // Base sınıfta hasar işlemi uygulanır
        base.TakeDamage(_damage);
        
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
        // Save stat values
        PlayerPrefs.SetFloat("PlayerMaxHealth", maxHealth.GetValue());
        PlayerPrefs.SetFloat("PlayerMaxMana", maxMana.GetValue());
        PlayerPrefs.SetFloat("PlayerBaseDamage", baseDamage.GetValue());
        PlayerPrefs.SetInt("PlayerSkillPoints", availableSkillPoints);
        PlayerPrefs.Save();
        
        Debug.Log($"Player stats saved: Health={maxHealth.GetValue()}, Mana={maxMana.GetValue()}, Damage={baseDamage.GetValue()}, SP={availableSkillPoints}");
    }
}