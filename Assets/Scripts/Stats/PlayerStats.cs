using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStats : CharacterStats
{
    public Player player;
    
    [Header("Attribute System")]
    [SerializeField] private int _vitality = 0;  // Increases max health
    [SerializeField] private int _might = 0;     // Increases attack damage
    [SerializeField] private int _defense = 0;   // Reduces incoming damage
    [SerializeField] private int _luck = 0;      // Increases critical chance
    [SerializeField] private int _mind = 0;      // Increases elemental damage
    
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
    //[SerializeField] private float _baseManaValue = 50f;
    [SerializeField] private float _baseSpeedValue = 300f;
    
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
    public new int Defense => _defense;
    public new int Luck => _luck;
    public new int Mind
    {
        get
        {
            return _mind;
        }
    }
    
    [Header("Currency")]
    [SerializeField] public int gold = 0;
    
    [Header("UI References")]
    [SerializeField] private Image experienceBar;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI goldText;

    private float playerBaseMaxHealth;
    private float playerBaseMaxDamage;

    [Header("Weapon Damage")]
    public Stat boomerangDamage;

    public Stat baseDamage;

    [Header("Spellbook Damage")]
    public Stat spellbookDamage;

    protected override void Awake()
    {
        // Önce baz sınıfın Awake metodunu çağır
        base.Awake();
        baseDamage = new Stat(_baseDamageValue);
        boomerangDamage = new Stat(_baseDamageValue * 0.8f);
        spellbookDamage = new Stat(_baseDamageValue * 0.7f);
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
            AddExperience(900000);
        }
        
        // Debug key to add gold (can be removed in final build)
        if (Input.GetKeyDown(KeyCode.G))
        {
            AddGold(50);
        }
    }
    
    // Apply all attribute bonuses to stats using exponential growth
    public new void ApplyAttributeBonuses()
    {
        // Reset any previous attribute bonuses
        maxHealth.RemoveAllModifiersOfType(StatModifierType.Attribute);
        baseDamage.RemoveAllModifiersOfType(StatModifierType.Attribute);
        boomerangDamage.RemoveAllModifiersOfType(StatModifierType.Attribute);
        maxMana.RemoveAllModifiersOfType(StatModifierType.Attribute);
        
        // Calculate vitality bonus (exponential growth)
        float healthMultiplier = Mathf.Pow(1 + HEALTH_GROWTH, _vitality) - 1;
        float healthBonus = _baseHealthValue * healthMultiplier;
        maxHealth.AddModifier(healthBonus, StatModifierType.Attribute);
        
        // Calculate might bonus (exponential growth)
        float damageMultiplier = Mathf.Pow(1 + DAMAGE_GROWTH, _might) - 1;
        float damageBonus = _baseDamageValue * damageMultiplier;
        baseDamage.AddModifier(damageBonus, StatModifierType.Attribute);
        boomerangDamage.AddModifier(damageBonus, StatModifierType.Attribute); // Apply full might bonus to boomerang
        
        // Calculate defense stat (exponential growth)
        float defenseMultiplier = Mathf.Pow(1 + DEFENSE_GROWTH, _defense) - 1;
        
        // Critical chance remains linear (1% per point)
        base.criticalChance = _luck * CRIT_CHANCE_PER_LUCK;
        
        float mindMultiplier = Mathf.Pow(1 + SPEED_GROWTH, _mind) - 1;
        base.speedStat = _baseSpeedValue * (1 + mindMultiplier);
        
        // Set defense stat - this was missing!
        base.defenseStat = _defense;
        
        // Calculate derived stats for UI display
        base.attackPower = baseDamage.GetValue();
        
       
    }
    
    // Calculates just the health bonus for a specific vitality level (for preview)
    public new float CalculateHealthBonusForVitality(int vitalityLevel)
    {
        return base.CalculateHealthBonusForVitality(vitalityLevel);
    }
    
    #region Damage Range System
    
    /// <summary>
    /// Get minimum damage from equipped weapon + attributes + upgrades
    /// </summary>
    public int GetMinDamage()
    {
        // Get equipped main weapon
        WeaponData weapon = EquipmentManager.Instance?.GetCurrentMainWeapon();
        
        if (weapon != null)
        {
            // Weapon min damage + upgrade bonuses
            int weaponMinDamage = weapon.GetTotalMinDamage();
            
            // Add attribute bonus from might
            float mightBonus = baseDamage.GetValue() - baseDamage.GetBaseValue();
            
            return weaponMinDamage + Mathf.RoundToInt(mightBonus);
        }
        else
        {
            // Fallback to base damage system
            return Mathf.RoundToInt(baseDamage.GetValue());
        }
    }
    
    /// <summary>
    /// Get maximum damage from equipped weapon + attributes + upgrades
    /// </summary>
    public int GetMaxDamage()
    {
        // Get equipped main weapon
        WeaponData weapon = EquipmentManager.Instance?.GetCurrentMainWeapon();
        
        if (weapon != null)
        {
            // Weapon max damage + upgrade bonuses
            int weaponMaxDamage = weapon.GetTotalMaxDamage();
            
            // Add attribute bonus from might
            float mightBonus = baseDamage.GetValue() - baseDamage.GetBaseValue();
            
            return weaponMaxDamage + Mathf.RoundToInt(mightBonus);
        }
        else
        {
            // Fallback to base damage system
            return Mathf.RoundToInt(baseDamage.GetValue());
        }
    }
    
    /// <summary>
    /// Get random damage in min-max range for combat
    /// </summary>
    public int GetRandomDamage()
    {
        int min = GetMinDamage();
        int max = GetMaxDamage();
        
        // Ensure max is at least min + 1 for random range
        if (max <= min) max = min + 1;
        
        return UnityEngine.Random.Range(min, max + 1);
    }
    
    /// <summary>
    /// Get damage range as formatted string for UI display
    /// </summary>
    public string GetDamageRangeString()
    {
        int min = GetMinDamage();
        int max = GetMaxDamage();
        
        if (min == max)
            return min.ToString();
        else
            return $"{min}-{max}";
    }
    
    /// <summary>
    /// Get damage range including critical hit potential for UI display
    /// Shows: Min Normal Damage - Max Critical Damage
    /// </summary>
    public string GetDamageRangeWithCriticalString()
    {
        int minNormal = GetMinDamage();
        int maxNormal = GetMaxDamage();
        
        // Apply critical multiplier to max damage (1.5f default)
        int maxCritical = Mathf.RoundToInt(maxNormal * criticalDamage);
        
        if (minNormal == maxCritical)
            return minNormal.ToString();
        else
            return $"{minNormal}-{maxCritical}";
    }
    
    /// <summary>
    /// Get average damage for comparison purposes
    /// </summary>
    public float GetAverageDamage()
    {
        return (GetMinDamage() + GetMaxDamage()) / 2f;
    }
    
    /// <summary>
    /// Get damage range string with temporary might value for preview
    /// Used by AttributesUpgradePanel for real-time preview
    /// </summary>
    public string GetDamageRangeWithCriticalString(int tempMight)
    {
        // Calculate temporary might bonus
        float baseDamageValue = baseDamage.GetBaseValue();
        float damageMultiplier = Mathf.Pow(1 + DAMAGE_GROWTH, tempMight) - 1;
        float tempMightBonus = baseDamageValue * damageMultiplier;
        
        // Get current equipment bonus (excluding might bonus)
        float currentMightBonus = baseDamage.GetValue() - baseDamage.GetBaseValue();
        float equipmentOnlyBonus = baseDamage.GetValue() - baseDamage.GetBaseValue() - currentMightBonus;
        
        // Actually, let's simplify: get all non-attribute bonuses
        float currentTotalBonus = baseDamage.GetValue() - baseDamage.GetBaseValue();
        float currentMightBonusCalculated = baseDamageValue * (Mathf.Pow(1 + DAMAGE_GROWTH, Might) - 1);
        float equipmentAndOtherBonuses = currentTotalBonus - currentMightBonusCalculated;
        
        // Get equipped weapon
        WeaponData weapon = EquipmentManager.Instance?.GetCurrentMainWeapon();
        
        if (weapon != null)
        {
            // Calculate min damage with temp might value + equipment bonuses
            int weaponMinDamage = weapon.GetTotalMinDamage();
            int totalMinDamage = weaponMinDamage + Mathf.RoundToInt(tempMightBonus + equipmentAndOtherBonuses);
            
            // Calculate max damage with temp might value + equipment bonuses
            int weaponMaxDamage = weapon.GetTotalMaxDamage();
            int totalMaxDamage = weaponMaxDamage + Mathf.RoundToInt(tempMightBonus + equipmentAndOtherBonuses);
            
            // Apply critical multiplier to max damage
            int maxCritical = Mathf.RoundToInt(totalMaxDamage * criticalDamage);
            
            // Format the range string
            if (totalMinDamage == maxCritical)
                return totalMinDamage.ToString();
            else
                return $"{totalMinDamage}-{maxCritical}";
        }
        else
        {
            // Fallback to base damage system
            float totalDamage = baseDamageValue + tempMightBonus + equipmentAndOtherBonuses;
            int normalDamage = Mathf.RoundToInt(totalDamage);
            int maxCritical = Mathf.RoundToInt(totalDamage * criticalDamage);
            
            if (normalDamage == maxCritical)
                return normalDamage.ToString();
            else
                return $"{normalDamage}-{maxCritical}";
        }
    }
    
    #endregion
    
    // Calculates just the damage bonus for a specific might level (for preview)
    public new float CalculateDamageBonusForMight(int mightLevel)
    {
        return base.CalculateDamageBonusForMight(mightLevel);
    }
    
    // Calculates just the damage bonus for a specific mind level (for preview)
    public float CalculateDamageBonusForMind(int mindLevel)
    {
        float mindMultiplier = Mathf.Pow(1 + SPEED_GROWTH, mindLevel) - 1;
        return _baseDamageValue * mindMultiplier;
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
            
            // Her levelde 3 skill point ver
            availableSkillPoints += 3;
            
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
    
    public void UpdateLevelUI()
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
        else
        {
            // Try to find level text if it's null
            RefreshUIReferences();
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
        else
        {
            // Try to find gold text if it's null
            RefreshUIReferences();
        }
    }
    
    // Gold handling methods
    public void AddGold(int amount)
    {
        if (amount <= 0) return;
        
        gold += amount;
        UpdateLevelUI();
        SaveGoldData();
        
        
    }
    
    public bool SpendGold(int amount)
    {
        if (amount <= 0) return true;
        if (gold < amount) return false;
        
        gold -= amount;
        UpdateLevelUI();
        SaveGoldData();
        
        
        return true;
    }
    
    private void SaveGoldData()
    {
        PlayerPrefs.SetInt("PlayerGold", gold);
        PlayerPrefs.Save();
    }
    
    // Refresh UI references if they are lost after scene loading
    private void RefreshUIReferences()
    {
        // Try to find UI components by name if they're null
        if (levelText == null)
        {
            GameObject levelObj = GameObject.Find("LevelText");
            if (levelObj == null) levelObj = GameObject.Find("Level_Text");
            if (levelObj == null) levelObj = GameObject.Find("PlayerLevel");
            if (levelObj != null)
            {
                levelText = levelObj.GetComponent<TextMeshProUGUI>();
                Debug.Log("PlayerStats: Found levelText reference");
            }
        }
        
        if (goldText == null)
        {
            GameObject goldObj = GameObject.Find("GoldText");
            if (goldObj == null) goldObj = GameObject.Find("Gold_Text");
            if (goldObj == null) goldObj = GameObject.Find("PlayerGold");
            if (goldObj != null)
            {
                goldText = goldObj.GetComponent<TextMeshProUGUI>();
                Debug.Log("PlayerStats: Found goldText reference");
            }
        }
        
        if (experienceBar == null)
        {
            GameObject expObj = GameObject.Find("ExperienceBar");
            if (expObj == null) expObj = GameObject.Find("ExpBar");
            if (expObj == null) expObj = GameObject.Find("Experience_Fill");
            if (expObj != null)
            {
                experienceBar = expObj.GetComponent<Image>();
                Debug.Log("PlayerStats: Found experienceBar reference");
            }
        }
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
            _defense = PlayerPrefs.GetInt("PlayerDefense", 0);
            _luck = PlayerPrefs.GetInt("PlayerLuck", 0);
            _mind = PlayerPrefs.GetInt("PlayerMind", 0);
            
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
        
       
        
        // Save changes
        SaveStatsData();
    }
    
    public void IncreaseMind()
    {
        if (availableSkillPoints <= 0 || _mind >= _maxAttributeLevel) return;
        int oldMind = _mind;
        _mind++;
        availableSkillPoints--;
        ApplyAttributeBonuses();
        UpdateLevelUI();
       
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
    
    // Method to reduce skill points externally
    public void ReduceSkillPoint()
    {
        if (availableSkillPoints > 0)
        {
            availableSkillPoints--;
            UpdateLevelUI();
        }
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
        PlayerPrefs.SetInt("PlayerDefense", _defense);
        PlayerPrefs.SetInt("PlayerLuck", _luck);
        PlayerPrefs.SetInt("PlayerMind", _mind);
        
        // Save skill points
        PlayerPrefs.SetInt("PlayerSkillPoints", availableSkillPoints);
        PlayerPrefs.Save();
        
       
    }

    // Reset all attributes and get skill points back
    public void ResetAllAttributes()
    {
        // Calculate total spent points
        int totalSpentPoints = _vitality + _might + _defense + _luck + _mind;
        
        // Restore skill points
        availableSkillPoints += totalSpentPoints;
        
        // Reset attribute values
        _vitality = 0;
        _might = 0;
        _defense = 0;
        _luck = 0;
        _mind = 0;
        
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
        
    
    }
    
    // Kritik vuruş kontrolü yapan metot
    public new bool IsCriticalHit()
    {
        return base.IsCriticalHit();
    }

    // Override all attribute properties to use the private fields
    protected override int vitality { get => _vitality; set => _vitality = value; }
    protected override int might { get => _might; set => _might = value; }
    protected override int defense { get => _defense; set => _defense = value; }
    protected override int luck { get => _luck; set => _luck = value; }
    protected override int mind { get => _mind; set => _mind = value; }
}