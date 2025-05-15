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
    
    [Header("Attribute Limits")]
    [SerializeField] private int maxAttributeLevel = 99;  // Maximum level for any attribute
    
    // Derived stats from attributes
    public float criticalChance { get; private set; } = 0f;
    public float criticalDamage { get; private set; } = 1.5f;  // Base critical damage multiplier
    public float attackPower { get; private set; } = 0f;
    public float speedStat { get; private set; } = 0f;
    public float defenseStat { get; private set; } = 0f;
    
    // Base stats for level 1
    [Header("Base Stats")]
    [SerializeField] private float baseHealthValue = 100f;
    [SerializeField] private float baseDamageValue = 10f;
    [SerializeField] private float baseManaValue = 50f;
    [SerializeField] private float baseDefenseValue = 5f;
    [SerializeField] private float baseSpeedValue = 300f;
    
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
            AddExperience(1000);
        }
        
        // Debug key to add gold (can be removed in final build)
        if (Input.GetKeyDown(KeyCode.G))
        {
            AddGold(50);
        }
    }
    
    // Apply all attribute bonuses to stats using exponential growth
    private void ApplyAttributeBonuses()
    {
        // Reset any previous attribute bonuses
        maxHealth.RemoveAllModifiersOfType(StatModifierType.Attribute);
        baseDamage.RemoveAllModifiersOfType(StatModifierType.Attribute);
        maxMana.RemoveAllModifiersOfType(StatModifierType.Attribute);
        
        // Calculate vitality bonus (exponential growth)
        float healthMultiplier = Mathf.Pow(1 + HEALTH_GROWTH, vitality) - 1;
        float healthBonus = baseHealthValue * healthMultiplier;
        maxHealth.AddModifier(healthBonus, StatModifierType.Attribute);
        
        // Calculate might bonus (exponential growth)
        float damageMultiplier = Mathf.Pow(1 + DAMAGE_GROWTH, might) - 1;
        float damageBonus = baseDamageValue * damageMultiplier;
        baseDamage.AddModifier(damageBonus, StatModifierType.Attribute);
        
        // Calculate agility bonus for mana (exponential growth)
        float manaMultiplier = Mathf.Pow(1 + MANA_GROWTH, agility) - 1;
        float manaBonus = baseManaValue * manaMultiplier;
        maxMana.AddModifier(manaBonus, StatModifierType.Attribute);
        
        // Calculate defense stat (exponential growth)
        float defenseMultiplier = Mathf.Pow(1 + DEFENSE_GROWTH, defense) - 1;
        defenseStat = baseDefenseValue * (1 + defenseMultiplier);
        
        // Critical chance remains linear (1% per point)
        criticalChance = luck * CRIT_CHANCE_PER_LUCK;
        
        // Calculate speed bonus (smaller exponential growth)
        float speedMultiplier = Mathf.Pow(1 + SPEED_GROWTH, agility) - 1;
        speedStat = baseSpeedValue * (1 + speedMultiplier);
        
        // Calculate derived stats for UI display
        attackPower = baseDamage.GetValue();
        
        Debug.Log($"Applied attribute bonuses: HP +{healthBonus:F0}, DMG +{damageBonus:F1}, Mana +{manaBonus:F0}");
        Debug.Log($"<color=cyan>Luck: {luck}</color> → <color=yellow>Crit Chance: {criticalChance*100:F1}%</color>, Defense: {defenseStat:F1}, Speed: {speedStat:F0}");
    }
    
    // Calculates just the health bonus for a specific vitality level (for preview)
    public float CalculateHealthBonusForVitality(int vitalityLevel)
    {
        float healthMultiplier = Mathf.Pow(1 + HEALTH_GROWTH, vitalityLevel) - 1;
        return baseHealthValue * healthMultiplier;
    }
    
    // Calculates just the damage bonus for a specific might level (for preview)
    public float CalculateDamageBonusForMight(int mightLevel)
    {
        float damageMultiplier = Mathf.Pow(1 + DAMAGE_GROWTH, mightLevel) - 1;
        return baseDamageValue * damageMultiplier;
    }
    
    // Calculates just the mana bonus for a specific agility level (for preview)
    public float CalculateManaBonusForAgility(int agilityLevel)
    {
        float manaMultiplier = Mathf.Pow(1 + MANA_GROWTH, agilityLevel) - 1;
        return baseManaValue * manaMultiplier;
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
        if (availableSkillPoints <= 0 || vitality >= maxAttributeLevel) return;
        
        // Debug için önceki değerleri kaydet
        float oldMaxHealth = maxHealth.GetValue();
        
        vitality++;
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
        Debug.Log($"<color=green>VITALITY ARTIRILDI!</color> Değer: {vitality-1} → {vitality}");
        Debug.Log($"<color=red>Max Sağlık: {oldMaxHealth:F0} → {newMaxHealth:F0} (+{healthIncrease:F0})</color>");
        Debug.Log($"<color=green>Can fullendi: {currentHealth:F0}/{newMaxHealth:F0}</color>");
        
        // Save changes
        SaveStatsData();
    }
    
    public void IncreaseMight()
    {
        if (availableSkillPoints <= 0 || might >= maxAttributeLevel) return;
        
        // Debug için önceki değerleri kaydet
        float oldDamage = baseDamage.GetValue();
        
        might++;
        availableSkillPoints--;
        
        // Apply might bonus
        ApplyAttributeBonuses();
        
        // Update UI
        UpdateLevelUI();
        
        // Calculate actual damage increase for debug
        float damageIncrease = baseDamage.GetValue() - oldDamage;
        
        // Debug log
        Debug.Log($"<color=green>MIGHT ARTIRILDI!</color> Değer: {might-1} → {might}");
        Debug.Log($"<color=orange>Hasar: {oldDamage:F1} → {baseDamage.GetValue():F1} (+{damageIncrease:F1})</color>");
        
        // Save changes
        SaveStatsData();
    }
    
    public void IncreaseAgility()
    {
        if (availableSkillPoints <= 0 || agility >= maxAttributeLevel) return;
        
        // Debug için önceki değerleri kaydet
        float oldMana = maxMana.GetValue();
        float oldSpeed = speedStat;
        
        agility++;
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
        Debug.Log($"<color=green>AGILITY ARTIRILDI!</color> Değer: {agility-1} → {agility}");
        Debug.Log($"<color=blue>Mana: {oldMana:F0} → {maxMana.GetValue():F0} (+{manaIncrease:F0})</color>");
        Debug.Log($"<color=yellow>Hız: {oldSpeed:F0} → {speedStat:F0} (+{speedIncrease:F0})</color>");
        
        // Save changes
        SaveStatsData();
    }
    
    public void IncreaseDefense()
    {
        if (availableSkillPoints <= 0 || defense >= maxAttributeLevel) return;
        
        // Debug için önceki değerleri kaydet
        float oldDefense = defenseStat;
        
        defense++;
        availableSkillPoints--;
        
        // Apply defense bonus
        ApplyAttributeBonuses();
        
        // Update UI
        UpdateLevelUI();
        
        // Calculate actual defense increase for debug
        float defenseIncrease = defenseStat - oldDefense;
        
        // Debug log
        Debug.Log($"<color=green>DEFENSE ARTIRILDI!</color> Değer: {defense-1} → {defense}");
        Debug.Log($"<color=cyan>Savunma: {oldDefense:F1} → {defenseStat:F1} (+{defenseIncrease:F1})</color>");
        
        // Save changes
        SaveStatsData();
    }
    
    public void IncreaseLuck()
    {
        if (availableSkillPoints <= 0 || luck >= maxAttributeLevel) return;
        
        // Debug için önceki değerleri kaydet
        float oldCritChance = criticalChance;
        
        luck++;
        availableSkillPoints--;
        
        // Apply luck bonus
        ApplyAttributeBonuses();
        
        // Update UI
        UpdateLevelUI();
        
        // Debug logları ekle
        Debug.Log($"<color=green>LUCK ARTIRILDI!</color> Değer: {luck-1} → {luck}");
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
    
    // Kritik vuruş kontrolü yapan metot
    public bool IsCriticalHit()
    {
        // Random.value 0-1 arası rastgele değer verir
        float randomValue = Random.value;
        bool isCritical = randomValue < criticalChance;
        
        // Debug log ile kritik vuruş değerlerini göster
        if (isCritical)
        {
            Debug.Log($"<color=red>KRİTİK VURUŞ!</color> Luck: {luck}, Şans: {criticalChance*100:F1}%, Random Değer: {randomValue:F3}");
        }
        else if (Random.value < 0.1f) // Her seferinde log göstermeyelim, %10 ihtimalle gösterelim
        {
            Debug.Log($"Normal vuruş. Luck: {luck}, Şans: {criticalChance*100:F1}%, Random Değer: {randomValue:F3}");
        }
        
        return isCritical;
    }
}