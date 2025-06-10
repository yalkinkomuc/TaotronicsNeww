using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class BlacksmithManager : MonoBehaviour
{
    public static BlacksmithManager Instance;
    
    [Header("Weapon Database")]
    public List<WeaponData> weaponDatabase = new List<WeaponData>();
    
    // Dictionary to store currently active weapon upgrades
    private Dictionary<string, WeaponData> activeWeapons = new Dictionary<string, WeaponData>();
    
    private void Awake()
    {
        // Basit singleton pattern
        if (Instance == null)
        {
            Instance = this;
            
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        // weaponDatabase null olup olmadığını kontrol et
        if (weaponDatabase == null)
        {
            weaponDatabase = new List<WeaponData>();
        }
        
        // Silahları tanımla (her zaman)
        InitializeDefaultWeapons();
        
        // Kaydedilmiş verileri yükle
        LoadWeaponData();
        

    }
    
    private void InitializeDefaultWeapons()
    {
        // Önce listeyi temizle
        weaponDatabase.Clear();
        
        // Kılıç
        WeaponData sword = new WeaponData("sword", "Kılıç", WeaponType.Sword, 10f, 100);
        sword.upgradeDamageIncrement = 5f; 
        weaponDatabase.Add(sword);
        
        // Bumerang
        WeaponData boomerang = new WeaponData("boomerang", "Bumerang", WeaponType.Boomerang, 8f, 120);
        boomerang.upgradeDamageIncrement = 4f; 
        weaponDatabase.Add(boomerang);
        
        // Büyü Kitabı
        WeaponData spellbook = new WeaponData("spellbook", "Büyü Kitabı", WeaponType.Spellbook, 6f, 150);
        spellbook.upgradeDamageIncrement = 3f;
        weaponDatabase.Add(spellbook);
        

    }
    
    private void AddWeaponWithIcon(string id, string name, WeaponType type, float baseDamage, int cost)
    {
        WeaponData weapon = new WeaponData(id, name, type, baseDamage, cost);
        
        // Resources klasöründen ikonu yüklemeyi dene
        string iconPath = $"WeaponIcons/{id}";
        Sprite icon = Resources.Load<Sprite>(iconPath);
        
        if (icon != null)
        {
            weapon.weaponIcon = icon;
        }
        
        weaponDatabase.Add(weapon);
    }
    
    // Get a list of all weapons - Null kontrolü eklendi
    public List<WeaponData> GetAllWeapons()
    {
        // weaponDatabase null kontrolü
        if (weaponDatabase == null)
        {
            return new List<WeaponData>();
        }
        
        // Veritabanı boş ise, default silahları yükle
        if (weaponDatabase.Count == 0)
        {
            InitializeDefaultWeapons();
        }
        
        // Bu fonksiyon çağrıldığında, silahların hepsinin aktif olmasını sağla
        if (weaponDatabase.Count > 0 && activeWeapons.Count == 0)
        {
            LoadWeaponData();
        }
        
        return weaponDatabase;
    }
    
    // Get a specific weapon by ID - Null kontrolü eklendi
    public WeaponData GetWeapon(string weaponId)
    {
        // Eğer activeWeapons boş ise, yeniden yüklemeyi dene
        if (activeWeapons.Count == 0)
        {
            LoadWeaponData();
        }
        
        return (activeWeapons != null && activeWeapons.ContainsKey(weaponId)) ? 
               activeWeapons[weaponId] : null;
    }
    
    // Get weapons of a specific type
    public List<WeaponData> GetWeaponsByType(WeaponType type)
    {
        return weaponDatabase.Where(w => w.weaponType == type).ToList();
    }
    
    // Try to upgrade a weapon
    public bool UpgradeWeapon(string weaponId, PlayerStats playerStats)
    {
        if (!activeWeapons.ContainsKey(weaponId))
            return false;
            
        WeaponData weapon = activeWeapons[weaponId];
        
        // Check if the weapon can be upgraded
        if (!weapon.CanUpgrade())
            return false;
            
        // Calculate upgrade cost
        int upgradeCost = weapon.GetNextUpgradeCost();
        
        // Check if player has enough gold
        if (playerStats.gold < upgradeCost)
            return false;
            
        // Deduct gold
        playerStats.SpendGold(upgradeCost);
        
        // Upgrade the weapon
        weapon.level++;
        
        // Apply the upgrade effect
        ApplyWeaponUpgrades(playerStats);
        
        // Save the changes
        SaveWeaponData();
        
        return true;
    }
    
    // Apply all weapon upgrades to the player stats
    public void ApplyWeaponUpgrades(PlayerStats playerStats)
    {
        if (playerStats == null)
            return;
            
        // First remove all existing equipment modifiers
        playerStats.baseDamage.RemoveAllModifiersOfType(StatModifierType.Equipment);
        playerStats.boomerangDamage.RemoveAllModifiersOfType(StatModifierType.Equipment);
        playerStats.spellbookDamage.RemoveAllModifiersOfType(StatModifierType.Equipment);
        
        // Calculate total damage bonus from all upgraded weapons
        float totalDamageBonus = 0f;
        float boomerangDamageBonus = 0f;
        float spellbookDamageBonus = 0f;
        
        foreach (var weapon in activeWeapons.Values)
        {
            float weaponBonus = weapon.GetCurrentDamageBonus();
            totalDamageBonus += weaponBonus;
            
            // If this is the boomerang weapon, add its bonus to boomerang damage
            if (weapon.weaponType == WeaponType.Boomerang)
            {
                boomerangDamageBonus += weaponBonus;
            }
            
            // If this is the spellbook weapon, add its bonus to spellbook damage
            if (weapon.weaponType == WeaponType.Spellbook)
            {
                spellbookDamageBonus += weaponBonus;
            }
        }
        
        // Apply total damage bonus if it's greater than 0
        if (totalDamageBonus > 0)
        {
            playerStats.baseDamage.AddModifier(totalDamageBonus, StatModifierType.Equipment);
        }
        
        // Apply boomerang-specific damage bonus
        if (boomerangDamageBonus > 0)
        {
            playerStats.boomerangDamage.AddModifier(boomerangDamageBonus, StatModifierType.Equipment);
        }
        
        // Apply spellbook-specific damage bonus
        if (spellbookDamageBonus > 0)
        {
            playerStats.spellbookDamage.AddModifier(spellbookDamageBonus, StatModifierType.Equipment);
        }
        

    }
    
    // Save weapon data to PlayerPrefs
    private void SaveWeaponData()
    {
        foreach (var weapon in activeWeapons.Values)
        {
            PlayerPrefs.SetInt($"Weapon_{weapon.weaponId}_Level", weapon.level);
        }
        PlayerPrefs.Save();
        

    }
    
    // Load weapon data from PlayerPrefs
    private void LoadWeaponData()
    {
        if (weaponDatabase == null || weaponDatabase.Count == 0)
        {
            return;
        }
        
        activeWeapons.Clear();
        
        foreach (var weapon in weaponDatabase)
        {
            if (weapon == null)
            {
                continue;
            }
            
            // Create a copy of the weapon data
            WeaponData weaponCopy = new WeaponData(
                weapon.weaponId, 
                weapon.weaponName, 
                weapon.weaponType, 
                weapon.baseDamageBonus, 
                weapon.baseUpgradeCost
            );
            
            // Copy additional properties
            weaponCopy.maxLevel = weapon.maxLevel;
            weaponCopy.upgradeDamageIncrement = weapon.upgradeDamageIncrement;
            weaponCopy.upgradeCostMultiplier = weapon.upgradeCostMultiplier;
            
            // Ikon ataması
            if (weapon.weaponIcon != null)
            {
                weaponCopy.weaponIcon = weapon.weaponIcon;
            }
            else
            {
                // Eğer orijinal WeaponData'da ikon yoksa, Resources'dan tekrar yüklemeyi dene
                string iconPath = $"WeaponIcons/{weapon.weaponId}";
                Sprite icon = Resources.Load<Sprite>(iconPath);
                if (icon != null)
                {
                    weaponCopy.weaponIcon = icon;
                }
            }
            
            // Load saved level
            if (PlayerPrefs.HasKey($"Weapon_{weapon.weaponId}_Level"))
            {
                weaponCopy.level = PlayerPrefs.GetInt($"Weapon_{weapon.weaponId}_Level");
            }
            
            // Add to active weapons dictionary
            activeWeapons.Add(weaponCopy.weaponId, weaponCopy);
        }
    }
} 