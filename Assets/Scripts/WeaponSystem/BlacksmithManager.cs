using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class BlacksmithManager : MonoBehaviour
{
    public static BlacksmithManager Instance;
    
    [Header("Weapon Database")]
    public List<WeaponData> weaponDatabase = new List<WeaponData>();
    
    // Dictionary to store currently active weapon upgrades (original ScriptableObjects)
    private Dictionary<string, WeaponData> activeWeapons = new Dictionary<string, WeaponData>();
    
    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
           // DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        // Load weapon data
        LoadWeaponData();
    }
    
    // Get a list of all weapons
    public List<WeaponData> GetAllWeapons()
    {
        if (weaponDatabase == null)
        {
            return new List<WeaponData>();
        }
        
        return weaponDatabase;
    }
    
    // Get a specific weapon by itemName
    public WeaponData GetWeapon(string weaponName)
    {
        return weaponDatabase?.FirstOrDefault(w => w.itemName == weaponName);
    }
    
    // Get weapons of a specific type
    public List<WeaponData> GetWeaponsByType(WeaponType type)
    {
        return weaponDatabase?.Where(w => w.weaponType == type).ToList() ?? new List<WeaponData>();
    }
    
    // Try to upgrade a weapon
    public bool UpgradeWeapon(string weaponName, PlayerStats playerStats)
    {
        WeaponData weapon = GetWeapon(weaponName);
        if (weapon == null)
            return false;
            
        // Check if the weapon can be upgraded
        if (!weapon.CanUpgrade())
            return false;
            
        // Calculate upgrade cost
        int upgradeCost = weapon.GetNextUpgradeCost();
        
        // Check if player has enough gold
        if (playerStats.gold < upgradeCost)
            return false;
            
        // Check upgrade materials requirement
        if (!CheckUpgradeMaterials(weapon))
        {
            Debug.Log($"Yeterli upgrade material yok! {weapon.itemName} için gerekli materyaller eksik.");
            return false;
        }
            
        // Deduct gold
        playerStats.SpendGold(upgradeCost);
        
        // Consume upgrade materials
        ConsumeUpgradeMaterials(weapon);
        
        // Upgrade the weapon (modify the ScriptableObject directly)
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
        playerStats.hammerDamage.RemoveAllModifiersOfType(StatModifierType.Equipment);
        playerStats.burningSwordDamage.RemoveAllModifiersOfType(StatModifierType.Equipment);
        
        // Calculate total damage bonus from all upgraded weapons
        float totalDamageBonus = 0f;
        float boomerangDamageBonus = 0f;
        float spellbookDamageBonus = 0f;
        float hammerDamageBonus = 0f;
        float burningSwordDamageBonus = 0f;
        
        foreach (var weapon in weaponDatabase)
        {
            if (weapon == null) continue;
            
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
            
            // If this is the hammer weapon, add its bonus to hammer damage
            if (weapon.weaponType == WeaponType.Hammer)
            {
                hammerDamageBonus += weaponBonus;
            }
            
            // If this is the burning sword weapon, add its bonus to burning sword damage
            if (weapon.weaponType == WeaponType.BurningSword)
            {
                burningSwordDamageBonus += weaponBonus;
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
        
        // Apply hammer-specific damage bonus
        if (hammerDamageBonus > 0)
        {
            playerStats.hammerDamage.AddModifier(hammerDamageBonus, StatModifierType.Equipment);
        }
        
        // Apply burning sword-specific damage bonus
        if (burningSwordDamageBonus > 0)
        {
            playerStats.burningSwordDamage.AddModifier(burningSwordDamageBonus, StatModifierType.Equipment);
        }
    }
    
    // Save weapon data to PlayerPrefs
    private void SaveWeaponData()
    {
        foreach (var weapon in weaponDatabase)
        {
            if (weapon != null)
            {
                PlayerPrefs.SetInt($"Weapon_{weapon.itemName}_Level", weapon.level);
            }
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
        
        foreach (var weapon in weaponDatabase)
        {
            if (weapon != null)
            {
                // Load saved level
                if (PlayerPrefs.HasKey($"Weapon_{weapon.itemName}_Level"))
                {
                    weapon.level = PlayerPrefs.GetInt($"Weapon_{weapon.itemName}_Level");
                }
            }
        }
    }

    // Check if player has required upgrade materials
    private bool CheckUpgradeMaterials(WeaponData weapon)
    {
        if (Inventory.instance == null)
            return false;
            
        // Get required materials based on weapon type and level
        var requiredMaterials = GetRequiredMaterials(weapon);
        
        foreach (var requirement in requiredMaterials)
        {
            int availableCount = GetMaterialCount(requirement.Key);
            if (availableCount < requirement.Value)
            {
                Debug.Log($"Eksik material: {requirement.Key} - Gerekli: {requirement.Value}, Mevcut: {availableCount}");
                return false;
            }
        }
        
        return true;
    }
    
    // Consume required upgrade materials from inventory
    private void ConsumeUpgradeMaterials(WeaponData weapon)
    {
        if (Inventory.instance == null)
            return;
            
        var requiredMaterials = GetRequiredMaterials(weapon);
        
        foreach (var requirement in requiredMaterials)
        {
            RemoveMaterialFromInventory(requirement.Key, requirement.Value);
        }
    }
    
    // Get required materials for weapon upgrade
    private Dictionary<MaterialType, int> GetRequiredMaterials(WeaponData weapon)
    {
        return weapon.GetRequiredMaterialsForUpgrade();
    }
    
    // Get count of specific material type in inventory
    private int GetMaterialCount(MaterialType materialType)
    {
        if (Inventory.instance == null)
            return 0;
            
        int totalCount = 0;
        
        foreach (var inventoryItem in Inventory.instance.inventoryItems)
        {
            if (inventoryItem.data is UpgradeMaterialData material && 
                material.materialType == materialType)
            {
                totalCount += inventoryItem.stackSize;
            }
        }
        
        return totalCount;
    }
    
    // Remove specific amount of material from inventory
    private void RemoveMaterialFromInventory(MaterialType materialType, int amount)
    {
        if (Inventory.instance == null)
            return;
            
        int remainingToRemove = amount;
        
        // Find items to remove (iterate through a copy to avoid modification during iteration)
        var itemsToCheck = new List<InventoryItem>(Inventory.instance.inventoryItems);
        
        foreach (var inventoryItem in itemsToCheck)
        {
            if (remainingToRemove <= 0)
                break;
                
            if (inventoryItem.data is UpgradeMaterialData material && 
                material.materialType == materialType)
            {
                int removeFromThisStack = Mathf.Min(remainingToRemove, inventoryItem.stackSize);
                
                for (int i = 0; i < removeFromThisStack; i++)
                {
                    Inventory.instance.RemoveItem(inventoryItem.data);
                }
                
                remainingToRemove -= removeFromThisStack;
            }
        }
        
        Debug.Log($"{amount} adet {materialType} materyali tüketildi. Kalan: {GetMaterialCount(materialType)}");
    }
    
    // Get required materials info for UI display (public method for BlacksmithUI)
    public Dictionary<MaterialType, int> GetRequiredMaterialsForDisplay(string weaponName)
    {
        WeaponData weapon = GetWeapon(weaponName);
        if (weapon == null)
            return new Dictionary<MaterialType, int>();
            
        return GetRequiredMaterials(weapon);
    }
} 