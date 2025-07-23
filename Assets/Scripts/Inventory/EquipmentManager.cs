using UnityEngine;
using System.Collections.Generic;
using System;

public class EquipmentManager : MonoBehaviour
{
    public static EquipmentManager Instance { get; private set; }
    
    [Header("Equipment Slots")]
    [SerializeField] private WeaponData currentWeapon;
    [SerializeField] private ArmorData currentArmor;
    [SerializeField] private EquipmentData currentSecondaryWeapon; // WeaponData veya SecondaryWeaponData olabilir
    [SerializeField] private AccessoryData currentAccessory;
    
    [Header("Rune Slots")]
    [SerializeField] private RuneData[] equippedRunes = new RuneData[6];
    private Dictionary<int, int> runeEnhancementLevels = new Dictionary<int, int>(); // slotIndex -> enhancementLevel
    
    [Header("Player Stats")]
    [SerializeField] private PlayerStats playerStats;
    
    // Events for UI updates
    public static event Action<EquipmentSlot, EquipmentData> OnEquipmentChanged;
    public static event Action<int, RuneData> OnRuneChanged;
    public static event Action OnStatsUpdated;

    // PlayerPrefs keys for saving equipped items
    private const string PREF_MAIN_WEAPON_TYPE = "EquippedMainWeaponType";
    private const string PREF_SECONDARY_WEAPON_TYPE = "EquippedSecondaryWeaponType";

    private Dictionary<StatType, int> totalStats = new Dictionary<StatType, int>();
    
    // RuneSaveManager coordination
    // private bool runesLoadingCompleted = false; // Artık RuneSaveManager tarafından yönetiliyor
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            //DontDestroyOnLoad(gameObject);
            InitializeStats();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        // Find player stats if not assigned
        if (playerStats == null)
        {
            playerStats = FindFirstObjectByType<PlayerStats>();
        }
        
        // Initialize weapon references from existing systems
        InitializeWeaponReferences();
        
        // Listen to weapon switching events
        // PlayerWeaponManager.OnSecondaryWeaponChanged += OnPlayerSecondaryWeaponChanged;
        //
        CalculateAllStats();
        
        // NOTE: Initial equipment save moved to after rune loading to prevent clearing loaded runes
        // NOTE: Rune loading is now handled by RuneSaveManager to avoid timing issues
    }
    
    private void InitializeStats()
    {
        // Initialize all stat types to 0
        foreach (StatType statType in Enum.GetValues(typeof(StatType)))
        {
            totalStats[statType] = 0;
        }
    }
    
    #region Equipment Management
    
    public bool EquipItem(EquipmentData equipment)
    {
        if (equipment == null) return false;

        // Validate item-slot compatibility
        if (!IsItemCompatibleWithSlot(equipment, equipment.equipmentSlot))
        {
            Debug.LogWarning($"[EquipmentManager] Cannot equip {equipment.itemName} to {equipment.equipmentSlot} – incompatible type.");
            return false;
        }

        // Check if player meets level requirement
        if (playerStats != null && playerStats.GetLevel() < equipment.requiredLevel)
        {
            Debug.LogWarning($"Player level {playerStats.GetLevel()} is too low for {equipment.itemName} (requires {equipment.requiredLevel})");
            return false;
        }
        
        EquipmentData previousEquipment = GetEquippedItem(equipment.equipmentSlot);
        
        // Unequip previous item if exists
        if (previousEquipment != null)
        {
            UnequipItem(equipment.equipmentSlot);
        }
        
        // Equip new item
        switch (equipment.equipmentSlot)
        {
            case EquipmentSlot.MainWeapon:
                currentWeapon = equipment as WeaponData;
                break;
            case EquipmentSlot.Armor:
                currentArmor = equipment as ArmorData;
                break;
            case EquipmentSlot.SecondaryWeapon:
                currentSecondaryWeapon = equipment; // Artık EquipmentData, cast gerek yok
                
                // PlayerPrefs.Save(); // SaveEquipment içinde yapılacak
                break;
            case EquipmentSlot.Accessory:
                currentAccessory = equipment as AccessoryData;
                break;
        }

        // Persist equipment changes immediately
        SaveEquipment();
        
        CalculateAllStats();
        OnEquipmentChanged?.Invoke(equipment.equipmentSlot, equipment);
        return true;
    }
    
    public EquipmentData UnequipItem(EquipmentSlot slot)
    {
        EquipmentData unequippedItem = GetEquippedItem(slot);
        
        if (unequippedItem == null) return null;
        
        switch (slot)
        {
            case EquipmentSlot.MainWeapon:
                currentWeapon = null;
                break;
            case EquipmentSlot.Armor:
                currentArmor = null;
                break;
            case EquipmentSlot.SecondaryWeapon:
                currentSecondaryWeapon = null;
                break;
            case EquipmentSlot.Accessory:
                currentAccessory = null;
                break;
        }
        
        CalculateAllStats();
        OnEquipmentChanged?.Invoke(slot, null);
        return unequippedItem;
    }
    
    public EquipmentData GetEquippedItem(EquipmentSlot slot)
    {
        return slot switch
        {
            EquipmentSlot.MainWeapon => currentWeapon,
            EquipmentSlot.Armor => currentArmor,
            EquipmentSlot.SecondaryWeapon => currentSecondaryWeapon,
            EquipmentSlot.Accessory => currentAccessory,
            _ => null
        };
    }
    
    #endregion
    
    #region Rune Management
    
    public bool EquipRune(RuneData rune, int slotIndex)
    {
        if (rune == null || slotIndex < 0 || slotIndex >= equippedRunes.Length)
            return false;
        
        // Unequip previous rune if exists
        if (equippedRunes[slotIndex] != null)
        {
            UnequipRune(slotIndex); // Bu zaten event'i tetikler, ekstra işleme gerek yok
        }
        
        equippedRunes[slotIndex] = rune;
        
        // Enhancement level'ı kaydet (rune'un mevcut enhancement level'ı)
        runeEnhancementLevels[slotIndex] = rune.enhancementLevel;
        
        CalculateAllStats();
        OnRuneChanged?.Invoke(slotIndex, rune);
        return true;
    }
    
    public RuneData UnequipRune(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= equippedRunes.Length)
            return null;
        
        RuneData unequippedRune = equippedRunes[slotIndex];
        if (unequippedRune == null) return null;
        
        equippedRunes[slotIndex] = null;
        
        // Enhancement level'ı dictionary'den temizle
        if (runeEnhancementLevels.ContainsKey(slotIndex))
        {
            runeEnhancementLevels.Remove(slotIndex);
        }
        
        CalculateAllStats();
        OnRuneChanged?.Invoke(slotIndex, null);
        
        return unequippedRune;
    }
    
    public RuneData GetEquippedRune(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= equippedRunes.Length)
            return null;
        
        return equippedRunes[slotIndex];
    }
    
    public bool IsRuneSlotEmpty(int slotIndex)
    {
        return GetEquippedRune(slotIndex) == null;
    }
    
    /// <summary>
    /// Belirli bir rune slot'unun enhancement level'ını döndür
    /// </summary>
    public int GetRuneEnhancementLevel(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= equippedRunes.Length)
            return 0;
        
        return runeEnhancementLevels.ContainsKey(slotIndex) ? runeEnhancementLevels[slotIndex] : 0;
    }
    
    /// <summary>
    /// Debug: Mevcut equipped rune'ları logla
    /// </summary>
    [ContextMenu("Debug: Print Equipped Runes")]
    public void DebugPrintEquippedRunes()
    {
           for (int i = 0; i < equippedRunes.Length; i++)
        {
            if (equippedRunes[i] != null)
            {
                int enhancement = GetRuneEnhancementLevel(i);
                
            }
        }
    }
    
    public void NotifyUIAboutAllRunes()
    {
        for (int i = 0; i < equippedRunes.Length; i++)
        {
            if (equippedRunes[i] != null)
            {
                OnRuneChanged?.Invoke(i, equippedRunes[i]);
            }
        }
    }
    
    public void SetLoadedRunes(RuneData[] loadedRunes, Dictionary<int, int> loadedEnhancements)
    {
        if (loadedRunes == null || loadedEnhancements == null) return;
        
        for (int i = 0; i < equippedRunes.Length; i++)
        {
            equippedRunes[i] = null;
        }
        runeEnhancementLevels.Clear();
        
        for (int i = 0; i < loadedRunes.Length && i < equippedRunes.Length; i++)
        {
            equippedRunes[i] = loadedRunes[i];
        }
        
        foreach (var kvp in loadedEnhancements)
        {
            runeEnhancementLevels[kvp.Key] = kvp.Value;
        }
        
        // Recalculate stats
        CalculateAllStats();
        
    }
    
    #endregion
    
    #region Stat Calculation
    
    private void CalculateAllStats()
    {
        // Reset all stats
        InitializeStats();
        
        // Add stats from equipment
        AddEquipmentStats(currentWeapon);
        AddEquipmentStats(currentArmor);
        AddEquipmentStats(currentSecondaryWeapon);
        AddEquipmentStats(currentAccessory);
        
        // Add stats from runes
        for (int i = 0; i < equippedRunes.Length; i++)
        {
            if (equippedRunes[i] != null)
            {
                AddRuneStats(equippedRunes[i], i);
            }
        }
        
        OnStatsUpdated?.Invoke();
        
        // Statları PlayerStats'a uygula
        if (playerStats != null)
        {
            playerStats.ApplyEquipmentStats(GetAllStats());
        }
    }
    
    private void AddEquipmentStats(EquipmentData equipment)
    {
        if (equipment == null) return;
        
        foreach (var modifier in equipment.statModifiers)
        {
            if (totalStats.ContainsKey(modifier.statType))
            {
                totalStats[modifier.statType] += equipment.GetTotalStatValue(modifier.statType);
            }
        }
    }
    
    private void AddRuneStats(RuneData rune, int slotIndex)
    {
        if (rune == null) return;
        
        if (totalStats.ContainsKey(rune.statModifier.statType))
        {
            // Enhancement level'ını dictionary'den al
            int enhancementLevel = runeEnhancementLevels.ContainsKey(slotIndex) ? runeEnhancementLevels[slotIndex] : 0;
            
            // Stat hesapla (enhancement level ile birlikte)
            int baseValue = rune.statModifier.baseValue;
            int enhancementBonus = Mathf.RoundToInt(baseValue * (enhancementLevel * 0.1f));
            int totalValue = baseValue + enhancementBonus;
            
            totalStats[rune.statModifier.statType] += totalValue;
        }
    }
    
    public int GetTotalStat(StatType statType)
    {
        return totalStats.ContainsKey(statType) ? totalStats[statType] : 0;
    }
    
    public Dictionary<StatType, int> GetAllStats()
    {
        return new Dictionary<StatType, int>(totalStats);
    }
    
    #endregion
    
    #region Weapon Integration
    
    private void InitializeWeaponReferences()
    {
        
        LoadEquipment();

       
        PlayerWeaponManager weaponManager = FindFirstObjectByType<PlayerWeaponManager>();
        
        if (weaponManager != null && weaponManager.weapons != null && weaponManager.weapons.Length > 0)
        {
            // Get the currently active primary weapon based on startingWeaponIndex
            int primaryIndex = weaponManager.startingWeaponIndex;
            if (primaryIndex >= 0 && primaryIndex < weaponManager.weapons.Length && weaponManager.weapons[primaryIndex] != null)
            {
                WeaponStateMachine primaryWeapon = weaponManager.weapons[primaryIndex];
                
                // Determine weapon type and find corresponding WeaponData
                WeaponType primaryWeaponType = GetWeaponTypeFromStateMachine(primaryWeapon);
                
                if (BlacksmithManager.Instance != null)
                {
                    var weapons = BlacksmithManager.Instance.GetAllWeapons();
                    if (weapons != null)
                    {
                        var primaryWeaponData = weapons.Find(w => w.weaponType == primaryWeaponType);
                        if (primaryWeaponData != null && currentWeapon == null)
                        {
                            currentWeapon = primaryWeaponData;
                        }
                    }
                }
            }
        }
        
        // If no primary weapon found through PlayerWeaponManager, fallback to default logic
        if (currentWeapon == null && BlacksmithManager.Instance != null)
        {
            var weapons = BlacksmithManager.Instance.GetAllWeapons();
            if (weapons != null && weapons.Count > 0)
            {
                // Try to find Sword first (default fallback)
                var sword = weapons.Find(w => w.weaponType == WeaponType.Sword);
                if (sword != null)
                {
                    currentWeapon = sword;
                }
                
            }
        }
    }
    
    /// <summary>
    /// Get WeaponType from a WeaponStateMachine
    /// </summary>
    private WeaponType GetWeaponTypeFromStateMachine(WeaponStateMachine stateMachine)
    {
        if (stateMachine == null) return WeaponType.Sword; // Default fallback
        
        // Determine weapon type based on state machine type
        if (stateMachine is SwordWeaponStateMachine)
        {
            return WeaponType.Sword;
        }
        else if (stateMachine is BurningSwordStateMachine)
        {
            return WeaponType.BurningSword;
        }
        else if (stateMachine is HammerSwordStateMachine)
        {
            return WeaponType.Hammer;
        }
        else if (stateMachine is BoomerangWeaponStateMachine)
        {
            return WeaponType.Boomerang;
        }
        else if (stateMachine is SpellbookWeaponStateMachine)
        {
            return WeaponType.Spellbook;
        }
        else if (stateMachine is ShieldStateMachine)
        {
            return WeaponType.Shield;
        }
        
        return WeaponType.Sword; // Default fallback
    }
    
    private EquipmentData GetEquipmentDataFromStateMachine(WeaponStateMachine stateMachine)
    {
        if (stateMachine == null || BlacksmithManager.Instance == null) return null;

        // Determine weapon type based on state machine type
        if (stateMachine is ShieldStateMachine)
        {
            return BlacksmithManager.Instance.secondaryWeaponDataBase?.Find(s => s.secondaryWeaponType == SecondaryWeaponType.Shield);
        }
        
        // For other types, they are likely in the main weapon database
        var allWeapons = BlacksmithManager.Instance.GetAllWeapons();
        if (allWeapons == null) return null;
        
        if (stateMachine is BoomerangWeaponStateMachine)
        {
            return allWeapons.Find(w => w.weaponType == WeaponType.Boomerang);
        }
        else if (stateMachine is SpellbookWeaponStateMachine)
        {
            return allWeapons.Find(w => w.weaponType == WeaponType.Spellbook);
        }
        else if (stateMachine is SwordWeaponStateMachine)
        {
            return allWeapons.Find(w => w.weaponType == WeaponType.Sword);
        }
        else if (stateMachine is BurningSwordStateMachine)
        {
            return allWeapons.Find(w => w.weaponType == WeaponType.BurningSword);
        }
        else if (stateMachine is HammerSwordStateMachine)
        {
            return allWeapons.Find(w => w.weaponType == WeaponType.Hammer);
        }
        
        return null;
    }
    
    public WeaponData GetCurrentMainWeapon()
    {
        // Real-time olarak PlayerWeaponManager'dan aktif weapon'ı al
        PlayerWeaponManager weaponManager = FindFirstObjectByType<PlayerWeaponManager>();
        if (weaponManager != null && weaponManager.weapons != null)
        {
            // Aktif primary weapon index'ini al
            int currentPrimaryIndex = weaponManager.GetCurrentPrimaryWeaponIndex();
            
            if (currentPrimaryIndex >= 0 && currentPrimaryIndex < weaponManager.weapons.Length)
            {
                WeaponStateMachine activeWeapon = weaponManager.weapons[currentPrimaryIndex];
                if (activeWeapon != null)
                {
                    // StateMachine'den WeaponType belirle
                    WeaponType weaponType = GetWeaponTypeFromStateMachine(activeWeapon);
                    
                    // BlacksmithManager'dan WeaponData'yı al
                    if (BlacksmithManager.Instance != null)
                    {
                        var weapons = BlacksmithManager.Instance.GetAllWeapons();
                        var weaponData = weapons?.Find(w => w.weaponType == weaponType);
                        if (weaponData != null)
                        {
                            return weaponData;
                        }
                    }
                }
            }
        }
        return currentWeapon as WeaponData;
    }
    
    
    public EquipmentData GetCurrentSecondaryWeapon()
    {
        return currentSecondaryWeapon;
    }
    
    public WeaponData GetCurrentSecondaryWeaponAsWeaponData()
    {
        // Real-time olarak PlayerWeaponManager'dan aktif secondary weapon'ı al
        PlayerWeaponManager weaponManager = FindFirstObjectByType<PlayerWeaponManager>();
        if (weaponManager != null && weaponManager.weapons != null)
        {
            // Aktif secondary weapon index'ini al
            int currentSecondaryIndex = weaponManager.GetCurrentSecondaryWeaponIndex();
            
            if (currentSecondaryIndex >= 0 && currentSecondaryIndex < weaponManager.weapons.Length)
            {
                WeaponStateMachine activeSecondaryWeapon = weaponManager.weapons[currentSecondaryIndex];
                if (activeSecondaryWeapon != null)
                {
                    // StateMachine'den WeaponType belirle
                    WeaponType weaponType = GetWeaponTypeFromStateMachine(activeSecondaryWeapon);
                    
                    // BlacksmithManager'dan SecondaryWeaponData veya WeaponData'yı al
                    if (BlacksmithManager.Instance != null)
                    {
                        // Önce secondary weapon data'da ara
                        if (BlacksmithManager.Instance.secondaryWeaponDataBase != null)
                        {
                            SecondaryWeaponType secType = weaponType switch
                            {
                                WeaponType.Boomerang => SecondaryWeaponType.Boomerang,
                                WeaponType.Spellbook => SecondaryWeaponType.Spellbook,
                                WeaponType.Shield => SecondaryWeaponType.Shield,
                                _ => SecondaryWeaponType.Boomerang
                            };
                            
                            var secondaryWeaponData = BlacksmithManager.Instance.secondaryWeaponDataBase
                                .Find(s => s.secondaryWeaponType == secType);
                            
                            if (secondaryWeaponData != null)
                            {
                                // SecondaryWeaponData'yı WeaponData'ya cast edemeyiz
                                // Bunun yerine weapon database'den eşleşen WeaponData'yı bulalım
                                var weapons = BlacksmithManager.Instance.GetAllWeapons();
                                var weaponDatas = weapons?.Find(w => w.weaponType == weaponType);
                                if (weaponDatas != null)
                                {
                                    return weaponDatas;
                                }
                            }
                        }
                        
                        // Fallback: weapon database'den ara
                        var allWeapons = BlacksmithManager.Instance.GetAllWeapons();
                        var weaponData = allWeapons?.Find(w => w.weaponType == weaponType);
                        if (weaponData != null)
                        {
                            return weaponData;
                        }
                    }
                }
            }
        }
        
        // Fallback: cached value
        return currentSecondaryWeapon as WeaponData;
    }
    private WeaponType GetWeaponTypeFromSecondaryType(SecondaryWeaponType secondaryType)
    {
        return secondaryType switch
        {
            SecondaryWeaponType.Spellbook => WeaponType.Spellbook,
            SecondaryWeaponType.Boomerang => WeaponType.Boomerang,
            SecondaryWeaponType.Shield => WeaponType.Shield,
            _ => WeaponType.Spellbook // Default fallback
        };
    }
    
    #endregion
    
    #region Save/Load

    /// <summary>
    /// Persist currently equipped items to PlayerPrefs.
    /// </summary>
    public void SaveEquipment()
    {
        // MAIN WEAPON
        if (currentWeapon != null)
        {
            PlayerPrefs.SetInt(PREF_MAIN_WEAPON_TYPE, (int)currentWeapon.weaponType);
        }
        else
        {
            PlayerPrefs.DeleteKey(PREF_MAIN_WEAPON_TYPE);
        }

        // SECONDARY WEAPON
        if (currentSecondaryWeapon != null)
        {
            WeaponType secType;
            if (currentSecondaryWeapon is WeaponData weaponData)
            {
                secType = weaponData.weaponType;
            }
            else if (currentSecondaryWeapon is SecondaryWeaponData secData)
            {
                secType = MapSecondaryToWeaponType(secData.secondaryWeaponType);
            }
            else
            {
                secType = WeaponType.Boomerang; // fallback
            }
            PlayerPrefs.SetInt(PREF_SECONDARY_WEAPON_TYPE, (int)secType);
        }
        else
        {
            PlayerPrefs.DeleteKey(PREF_SECONDARY_WEAPON_TYPE);
        }

        // ARMOR (for future implementation)
        if (currentArmor != null)
        {
            PlayerPrefs.SetString("EquippedArmor", currentArmor.itemName);
        }
        else
        {
            PlayerPrefs.DeleteKey("EquippedArmor");
        }

        // ACCESSORY (for future implementation)
        if (currentAccessory != null)
        {
            PlayerPrefs.SetString("EquippedAccessory", currentAccessory.itemName);
        }
        else
        {
            PlayerPrefs.DeleteKey("EquippedAccessory");
        }

        PlayerPrefs.Save();
    }
    
    public void LoadEquipment()
    {
        if (BlacksmithManager.Instance == null) 
        {
            Debug.LogWarning("[EquipmentManager] BlacksmithManager.Instance is null - cannot load equipment");
            return;
        }

        var weapons = BlacksmithManager.Instance.GetAllWeapons();
        if (weapons == null || weapons.Count == 0) 
        {
            Debug.LogWarning("[EquipmentManager] No weapons found in BlacksmithManager - cannot load equipment");
            return;
        }

        // MAIN WEAPON
        if (PlayerPrefs.HasKey(PREF_MAIN_WEAPON_TYPE))
        {
            int savedMainType = PlayerPrefs.GetInt(PREF_MAIN_WEAPON_TYPE, -1);
            var mainWeaponData = weapons.Find(w => (int)w.weaponType == savedMainType);
            if (mainWeaponData != null)
            {
                currentWeapon = mainWeaponData;
            }
        }

        // SECONDARY WEAPON
        if (PlayerPrefs.HasKey(PREF_SECONDARY_WEAPON_TYPE))
        {
            int savedSecondaryType = PlayerPrefs.GetInt(PREF_SECONDARY_WEAPON_TYPE, -1);
            var secondaryWeaponData = weapons.Find(w => (int)w.weaponType == savedSecondaryType);
            if (secondaryWeaponData != null)
            {
                currentSecondaryWeapon = secondaryWeaponData;
            }
        }
    }
   
    #endregion
    
    private bool IsItemCompatibleWithSlot(EquipmentData equipment, EquipmentSlot targetSlot)
    {
        switch (targetSlot)
        {
            case EquipmentSlot.MainWeapon:
                if (equipment is WeaponData weapon)
                {
                    return IsPrimaryWeaponType(weapon.weaponType);
                }
                return false;

            case EquipmentSlot.SecondaryWeapon:
                if (equipment is WeaponData secWeapon)
                {
                    return IsSecondaryWeaponType(secWeapon.weaponType);
                }
                // Allow SecondaryWeaponData scriptable objects as well
                return equipment is SecondaryWeaponData;

            case EquipmentSlot.Armor:
                return equipment is ArmorData;

            case EquipmentSlot.Accessory:
                return equipment is AccessoryData;

            default:
                return false;
        }
    }

    private bool IsPrimaryWeaponType(WeaponType type)
    {
        return type == WeaponType.Sword || type == WeaponType.BurningSword || type == WeaponType.Hammer;
    }

    private bool IsSecondaryWeaponType(WeaponType type)
    {
        return type == WeaponType.Boomerang || type == WeaponType.Spellbook || type == WeaponType.Shield;
    }

    private WeaponType MapSecondaryToWeaponType(SecondaryWeaponType sec)
    {
        return sec switch
        {
            SecondaryWeaponType.Boomerang => WeaponType.Boomerang,
            SecondaryWeaponType.Spellbook => WeaponType.Spellbook,
            SecondaryWeaponType.Shield => WeaponType.Shield,
            _ => WeaponType.Boomerang
        };
    }

    public void EquipSecondaryByStateMachine(WeaponStateMachine wsm)
    {
        EquipmentData data = GetEquipmentDataFromStateMachine(wsm); // Shield için SecondaryWeaponData
        if (data != null)
            EquipItem(data);           // Böylece tek olay tetiklenir

        
        WeaponType secType = GetWeaponTypeFromStateMachine(wsm);
        PlayerPrefs.SetInt(PREF_SECONDARY_WEAPON_TYPE, (int)secType);
        PlayerPrefs.Save();
    }
    
    public static bool IsFirstTimePlayer()
    {
        return !PlayerPrefs.HasKey("GameStarted");
    }
    
    public static void MarkGameAsStarted()
    {
        PlayerPrefs.SetInt("GameStarted", 1);
        PlayerPrefs.Save();
    }
} 