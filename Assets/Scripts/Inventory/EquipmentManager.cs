using UnityEngine;
using System.Collections.Generic;
using System;

public class EquipmentManager : MonoBehaviour
{
    public static EquipmentManager Instance { get; private set; }
    
    [Header("Equipment Slots")]
    [SerializeField] private EquipmentData currentWeapon;
    [SerializeField] private EquipmentData currentArmor;
    [SerializeField] private EquipmentData currentSecondaryWeapon;
    [SerializeField] private EquipmentData currentAccessory;
    
    [Header("Rune Slots")]
    [SerializeField] private RuneData[] equippedRunes = new RuneData[6];
    
    [Header("Player Stats")]
    [SerializeField] private PlayerStats playerStats;
    
    // Events for UI updates
    public static event Action<EquipmentSlot, EquipmentData> OnEquipmentChanged;
    public static event Action<int, RuneData> OnRuneChanged;
    public static event Action OnStatsUpdated;
    
    private Dictionary<StatType, int> totalStats = new Dictionary<StatType, int>();
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
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
        
        CalculateAllStats();
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
                currentWeapon = equipment;
                break;
            case EquipmentSlot.Armor:
                currentArmor = equipment;
                break;
            case EquipmentSlot.SecondaryWeapon:
                currentSecondaryWeapon = equipment;
                break;
            case EquipmentSlot.Accessory:
                currentAccessory = equipment;
                break;
        }
        
        CalculateAllStats();
        OnEquipmentChanged?.Invoke(equipment.equipmentSlot, equipment);
        
        Debug.Log($"Equipped {equipment.itemName} in {equipment.equipmentSlot} slot");
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
        
        Debug.Log($"Unequipped {unequippedItem.itemName} from {slot} slot");
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
            UnequipRune(slotIndex);
        }
        
        equippedRunes[slotIndex] = rune;
        CalculateAllStats();
        OnRuneChanged?.Invoke(slotIndex, rune);
        
        Debug.Log($"Equipped rune {rune.itemName} in slot {slotIndex}");
        return true;
    }
    
    public RuneData UnequipRune(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= equippedRunes.Length)
            return null;
        
        RuneData unequippedRune = equippedRunes[slotIndex];
        if (unequippedRune == null) return null;
        
        equippedRunes[slotIndex] = null;
        CalculateAllStats();
        OnRuneChanged?.Invoke(slotIndex, null);
        
        Debug.Log($"Unequipped rune {unequippedRune.itemName} from slot {slotIndex}");
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
        foreach (var rune in equippedRunes)
        {
            if (rune != null)
            {
                AddRuneStats(rune);
            }
        }
        
        OnStatsUpdated?.Invoke();
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
    
    private void AddRuneStats(RuneData rune)
    {
        if (rune == null) return;
        
        if (totalStats.ContainsKey(rune.statModifier.statType))
        {
            totalStats[rune.statModifier.statType] += rune.GetTotalStatValue();
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
    
    #region Save/Load
    
    public void SaveEquipment()
    {
        // TODO: Implement save system for equipment
        Debug.Log("Equipment saved");
    }
    
    public void LoadEquipment()
    {
        // TODO: Implement load system for equipment
        Debug.Log("Equipment loaded");
    }
    
    #endregion
} 