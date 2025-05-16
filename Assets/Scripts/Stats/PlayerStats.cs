using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStats : CharacterStats
{
    public Player player;
    
    [Header("Attribute System")]
    [SerializeField] private int _vitality = 0;  // Increases max health
    [SerializeField] private int _might = 0;     // Increases attack damage
    [SerializeField] private int _agility = 0;   // Increases speed (already used for mana)
    [SerializeField] private int _defense = 0;   // Reduces incoming damage
    [SerializeField] private int _luck = 0;      // Increases critical chance
    
    [Header("Attribute Limits")]
    [SerializeField] private int _maxAttributeLevel = 99;  // Maximum level for any attribute
    
    // Derived stats from attributes
    public new float criticalChance => base.criticalChance;
    public new float criticalDamage => base.criticalDamage;
    public new float attackPower => base.attackPower;
    public new float speedStat => base.speedStat;
    public new float defenseStat => base.defenseStat;
    
    // Base stats for level 1
    [Header("Base Stats")]
    [SerializeField] private float _baseHealthValue = 100f;
    [SerializeField] private float _baseDamageValue = 10f;
    [SerializeField] private float _baseManaValue = 50f;
    [SerializeField] private float _baseDefenseValue = 5f;
    [SerializeField] private float _baseSpeedValue = 300f;
    
    // Growth constants
    private const float HEALTH_GROWTH = 0.08f;      // 8% growth per point
    private const float DAMAGE_GROWTH = 0.06f;      // 6% growth per point
    private const float MANA_GROWTH = 0.07f;        // 7% growth per point
    private const float DEFENSE_GROWTH = 0.05f;     // 5% growth per point
    private const float SPEED_GROWTH = 0.02f;       // 2% growth per point
    private const float CRIT_CHANCE_PER_LUCK = 0.01f; // 1% per point (linear)
    
    [Header("Experience System")]
    [SerializeField] private int experience = 0;
    [SerializeField] private int experienceToNextLevel = 100;
    [SerializeField] private float experienceLevelMultiplier = 1.5f; // Each level requires 50% more XP
    
    [Header("Skill Points")]
    [SerializeField] private int availableSkillPoints = 0;
    public int AvailableSkillPoints => availableSkillPoints;
    
    // For accessing attributes from UI
    public new  int Vitality => _vitality;
    public new int Might => _might;
    public new int Agility => _agility;
    public new int Defense => _defense;
    public new int Luck => _luck;
    
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
            AddExperience(1000);
        }
        
        // Debug key to add gold (can be removed in final build)
        if (Input.GetKeyDown(KeyCode.G))
        {
            AddGold(50);
        }
    }
    
    // Apply all attribute bonuses to stats using exponential growth
    public new void ApplyAttributeBonuses() => base.ApplyAttributeBonuses();
    
    // Calculates just the health bonus for a specific vitality level (for preview)
    public new float CalculateHealthBonusForVitality(int vitalityLevel)
    {
        return base.CalculateHealthBonusForVitality(vitalityLevel);
    }
    
    // Calculates just the damage bonus for a specific might level (for preview)
    public new float CalculateDamageBonusForMight(int mightLevel)
    {
        return base.CalculateDamageBonusForMight(mightLevel);
    }
    
    // Calculates just the mana bonus for a specific agility level (for preview)
    public new float CalculateManaBonusForAgility(int agilityLevel)
    {
        return base.CalculateManaBonusForAgility(agilityLevel);
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
            _vitality = PlayerPrefs.GetInt("PlayerVitality", 0);
            _might = PlayerPrefs.GetInt("PlayerMight", 0);
            _agility = PlayerPrefs.GetInt("PlayerAgility", 0);
            _defense = PlayerPrefs.GetInt("PlayerDefense", 0);
            _luck = PlayerPrefs.GetInt("PlayerLuck", 0);
            
            Debug.Log($"Oyuncu verileri yüklendi: Seviye={level}, Vit={_vitality}, Might={_might}, Agi={_agility}, Def={_defense}, Luck={_luck}, Gold={gold}, XP={experience}/{experienceToNextLevel}, SP={availableSkillPoints}");
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
        if (availableSkillPoints <= 0 || _vitality >= _maxAttributeLevel) return;
        
        // Debug için önceki değerleri kaydet
        float oldMaxHealth = maxHealth.GetValue();
        
        _vitality++;
        availableSkillPoints--;
        
        // Apply vitality bonus (increases max health)
        ApplyAttributeBonuses();
        
        // Yeni max health değerini al
        float newMaxHealth = maxHealth.GetValue();
        
        // Current health'i direkt MAX health'e eşitle (Full can yap)
        currentHealth = newMaxHealth;
        
        // Update UI
        UpdateLevelUI();
        UpdateHealthBarUI();
        
        // Calculate actual health increase for debug
        float healthIncrease = newMaxHealth - oldMaxHealth;
        
        // Debug log
        Debug.Log($"<color=green>VITALITY ARTIRILDI!</color> Değer: {_vitality-1} → {_vitality}");
        Debug.Log($"<color=red>Max Sağlık: {oldMaxHealth:F0} → {newMaxHealth:F0} (+{healthIncrease:F0})</color>");
        Debug.Log($"<color=green>Can fullendi: {currentHealth:F0}/{newMaxHealth:F0}</color>");
        
        // Save changes
        SaveStatsData();
    }
    
    public void IncreaseMight()
    {
        if (availableSkillPoints <= 0 || _might >= _maxAttributeLevel) return;
        
        // Debug için önceki değerleri kaydet
        float oldDamage = baseDamage.GetValue();
        
        _might++;
        availableSkillPoints--;
        
        // Apply might bonus
        ApplyAttributeBonuses();
        
        // Update UI
        UpdateLevelUI();
        
        // Calculate actual damage increase for debug
        float damageIncrease = baseDamage.GetValue() - oldDamage;
        
        // Debug log
        Debug.Log($"<color=green>MIGHT ARTIRILDI!</color> Değer: {_might-1} → {_might}");
        Debug.Log($"<color=orange>Hasar: {oldDamage:F1} → {baseDamage.GetValue():F1} (+{damageIncrease:F1})</color>");
        
        // Save changes
        SaveStatsData();
    }
    
    public void IncreaseAgility()
    {
        if (availableSkillPoints <= 0 || _agility >= _maxAttributeLevel) return;
        
        // Debug için önceki değerleri kaydet
        float oldMana = maxMana.GetValue();
        float oldSpeed = speedStat;
        
        _agility++;
        availableSkillPoints--;
        
        // Apply agility bonus
        ApplyAttributeBonuses();
        
        // Update mana
        float manaPercentage = currentMana / oldMana;
        currentMana = maxMana.GetValue() * manaPercentage;
        
        // Update UI
        UpdateLevelUI();
        
        // Calculate actual increases for debug
        float manaIncrease = maxMana.GetValue() - oldMana;
        float speedIncrease = speedStat - oldSpeed;
        
        // Debug log
        Debug.Log($"<color=green>AGILITY ARTIRILDI!</color> Değer: {_agility-1} → {_agility}");
        Debug.Log($"<color=blue>Mana: {oldMana:F0} → {maxMana.GetValue():F0} (+{manaIncrease:F0})</color>");
        Debug.Log($"<color=yellow>Hız: {oldSpeed:F0} → {speedStat:F0} (+{speedIncrease:F0})</color>");
        
        // Save changes
        SaveStatsData();
    }
    
    public void IncreaseDefense()
    {
        if (availableSkillPoints <= 0 || _defense >= _maxAttributeLevel) return;
        
        // Debug için önceki değerleri kaydet
        float oldDefense = defenseStat;
        
        _defense++;
        availableSkillPoints--;
        
        // Apply defense bonus
        ApplyAttributeBonuses();
        
        // Update UI
        UpdateLevelUI();
        
        // Calculate actual defense increase for debug
        float defenseIncrease = defenseStat - oldDefense;
        
        // Debug log
        Debug.Log($"<color=green>DEFENSE ARTIRILDI!</color> Değer: {_defense-1} → {_defense}");
        Debug.Log($"<color=cyan>Savunma: {oldDefense:F1} → {defenseStat:F1} (+{defenseIncrease:F1})</color>");
        
        // Save changes
        SaveStatsData();
    }
    
    public void IncreaseLuck()
    {
        if (availableSkillPoints <= 0 || _luck >= _maxAttributeLevel) return;
        
        // Debug için önceki değerleri kaydet
        float oldCritChance = criticalChance;
        
        _luck++;
        availableSkillPoints--;
        
        // Apply luck bonus
        ApplyAttributeBonuses();
        
        // Update UI
        UpdateLevelUI();
        
        // Debug logları ekle
        Debug.Log($"<color=green>LUCK ARTIRILDI!</color> Değer: {_luck-1} → {_luck}");
        Debug.Log($"<color=yellow>Kritik Vuruş Şansı: {oldCritChance*100:F1}% → {criticalChance*100:F1}%</color>");
        
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
        PlayerPrefs.SetInt("PlayerVitality", _vitality);
        PlayerPrefs.SetInt("PlayerMight", _might);
        PlayerPrefs.SetInt("PlayerAgility", _agility);
        PlayerPrefs.SetInt("PlayerDefense", _defense);
        PlayerPrefs.SetInt("PlayerLuck", _luck);
        
        // Save skill points
        PlayerPrefs.SetInt("PlayerSkillPoints", availableSkillPoints);
        PlayerPrefs.Save();
        
        Debug.Log($"Player attributes saved: Vitality={_vitality}, Might={_might}, Agility={_agility}, Defense={_defense}, Luck={_luck}, SP={availableSkillPoints}");
    }

    // Reset all attributes and get skill points back
    public void ResetAllAttributes()
    {
        // Calculate total spent points
        int totalSpentPoints = _vitality + _might + _agility + _defense + _luck;
        
        // Restore skill points
        availableSkillPoints += totalSpentPoints;
        
        // Reset attribute values
        _vitality = 0;
        _might = 0;
        _agility = 0;
        _defense = 0;
        _luck = 0;
        
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
    
    // Kritik vuruş kontrolü yapan metot
    public new bool IsCriticalHit()
    {
        return base.IsCriticalHit();
    }
}