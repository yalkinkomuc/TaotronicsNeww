using UnityEngine;
using System;
using System.Collections.Generic;

public class PlayerWeaponManager : MonoBehaviour
{
    // Static event for weapon switching
    public static event Action<int, WeaponStateMachine> OnSecondaryWeaponChanged;
    
    public WeaponStateMachine[] weapons;
    private int currentSecondaryWeaponIndex = -1; // Will be set to first secondary weapon found

    [SerializeField] public int startingWeaponIndex = 3; // Burning Sword ile başla
    private int currentPrimaryWeaponIndex = -1; // Aktif primary weapon index
    
    // Public getter for UI access
    public int GetCurrentSecondaryWeaponIndex() => currentSecondaryWeaponIndex;
    public int GetCurrentPrimaryWeaponIndex() => currentPrimaryWeaponIndex >= 0 ? currentPrimaryWeaponIndex : startingWeaponIndex;
    
    
    
    private Player player;
    private PlayerStats playerStats;

    void Start()
    {
        if (weapons.Length < 2)
        {
            return;
        }
        
        // Get player references
        player = GetComponent<Player>();
        playerStats = GetComponent<PlayerStats>();

        // Initialize all weapons (disable all first)
        InitializeAllWeapons();
        
        // Activate starting primary weapon
        ActivatePrimaryWeapon(startingWeaponIndex);
        
        // Find and activate first secondary weapon
        currentSecondaryWeaponIndex = GetFirstSecondaryWeaponIndex();
        if (currentSecondaryWeaponIndex != -1)
        {
            EquipSecondaryWeapon(currentSecondaryWeaponIndex);
        }
        
        // Initialize weapon upgrades
        if (BlacksmithManager.Instance != null && playerStats != null)
        {
            BlacksmithManager.Instance.ApplyWeaponUpgrades(playerStats);
        }
        
        // Fire initial event for UI
        if (currentSecondaryWeaponIndex != -1 && currentSecondaryWeaponIndex < weapons.Length)
        {
            OnSecondaryWeaponChanged?.Invoke(currentSecondaryWeaponIndex, weapons[currentSecondaryWeaponIndex]);
        }
    }

    void Update()
    {
          
    }

   public void EquipSecondaryWeapon(int index)
    {
        if (index < 0 || index >= weapons.Length || weapons[index] == null)
        {
            Debug.LogError($"Invalid secondary weapon index: {index}");
            return;
        }
        
        // Yeni secondary weapon indeksini güncelle
        currentSecondaryWeaponIndex = index;

        // Only affect secondary weapons - disable all secondary weapons first
        for (int i = 0; i < weapons.Length; i++)
        {
            if (weapons[i] != null && IsSecondaryWeapon(weapons[i]))
            {
                weapons[i].gameObject.SetActive(false);
            }
        }
        
        // Activate the selected secondary weapon (only if it's actually a secondary weapon)
        if (IsSecondaryWeapon(weapons[index]))
        {
            weapons[index].gameObject.SetActive(true);
            
            // Player üzerindeki lastActiveWeaponState'i güncelle
            if (player != null)
            {
                // Eğer aktif edilen silah boomerang ise
                if (weapons[index] is BoomerangWeaponStateMachine)
                {
                    player.UpdateLastActiveWeapon(WeaponState.ThrowBoomerang);
                }
                // Eğer aktif edilen silah spellbook ise
                else if (weapons[index] is SpellbookWeaponStateMachine)
                {
                    player.UpdateLastActiveWeapon(WeaponState.Spell1);
                }
            }
            
            Debug.Log($"Equipped secondary weapon: {weapons[index].name}");
        }
        else
        {
            Debug.LogError($"Weapon at index {index} is not a secondary weapon!");
        }
        
        // Update UI when weapon changes
        UpdateUISlots();
    }
    
    private void UpdateUISlots()
    {
        if (AdvancedInventoryUI.Instance != null)
        {
            AdvancedInventoryUI.Instance.UpdateEquipmentSlots();
        }
        
        // Fire event for UI updates
        OnSecondaryWeaponChanged?.Invoke(currentSecondaryWeaponIndex, weapons[currentSecondaryWeaponIndex]);
    }
    

    
    // Method to restore weapon visibility - called after HideWeapons/ShowWeapons
    public void RefreshWeaponVisibility()
    {
        // Refresh primary weapon
        ActivatePrimaryWeapon(GetCurrentPrimaryWeaponIndex());
        
        // Refresh the secondary weapons visibility
        EquipSecondaryWeapon(currentSecondaryWeaponIndex);
    }
    
    // Initialize all weapons (disable everything first)
    private void InitializeAllWeapons()
    {
        for (int i = 0; i < weapons.Length; i++)
        {
            if (weapons[i] != null)
            {
                weapons[i].gameObject.SetActive(false);
            }
        }
    }
    
    // Activate primary weapon and disable other primary weapons
    public void ActivatePrimaryWeapon(int primaryIndex)
    {
        if (primaryIndex < 0 || primaryIndex >= weapons.Length || weapons[primaryIndex] == null)
        {
            Debug.LogError($"Invalid primary weapon index: {primaryIndex}");
            return;
        }
        
        // Disable all primary weapons first
        DisableAllPrimaryWeapons();
        
        // Activate the selected primary weapon
        weapons[primaryIndex].gameObject.SetActive(true);
        
        // Aktif primary weapon index'ini güncelle
        currentPrimaryWeaponIndex = primaryIndex;
        
        Debug.Log($"Activated primary weapon: {weapons[primaryIndex].name}");
    }
    
    // Disable all primary weapons (indices that are primary weapons)
    private void DisableAllPrimaryWeapons()
    {
        for (int i = 0; i < weapons.Length; i++)
        {
            if (weapons[i] != null && IsPrimaryWeapon(weapons[i]))
            {
                weapons[i].gameObject.SetActive(false);
            }
        }
    }
    
    // Check if weapon is a primary weapon
    private bool IsPrimaryWeapon(WeaponStateMachine weapon)
    {
        return weapon is SwordWeaponStateMachine || 
               weapon is BurningSwordStateMachine || 
               weapon is HammerSwordStateMachine;
    }
    
    // Check if weapon is a secondary weapon
    private bool IsSecondaryWeapon(WeaponStateMachine weapon)
    {
        return weapon is BoomerangWeaponStateMachine || 
               weapon is SpellbookWeaponStateMachine;
    }
    
    // Get next secondary weapon index for cycling
    private int GetNextSecondaryWeaponIndex()
    {
        // Find all secondary weapon indices
        List<int> secondaryIndices = new List<int>();
        for (int i = 0; i < weapons.Length; i++)
        {
            if (weapons[i] != null && IsSecondaryWeapon(weapons[i]))
            {
                secondaryIndices.Add(i);
            }
        }
        
        if (secondaryIndices.Count == 0) return -1;
        
        // Find current index in the list
        int currentIndexInList = secondaryIndices.IndexOf(currentSecondaryWeaponIndex);
        
        // Get next index (cycle back to 0 if at end)
        int nextIndexInList = (currentIndexInList + 1) % secondaryIndices.Count;
        
        return secondaryIndices[nextIndexInList];
    }
    
    // Get first secondary weapon index
    private int GetFirstSecondaryWeaponIndex()
    {
        for (int i = 0; i < weapons.Length; i++)
        {
            if (weapons[i] != null && IsSecondaryWeapon(weapons[i]))
            {
                return i;
            }
        }
        return -1;
    }
    
    // Get the current active weapons for blacksmith system
    public WeaponType[] GetEquippedWeaponTypes()
    {
        WeaponType[] types = new WeaponType[weapons.Length];
        
        // Determine weapon types
        for (int i = 0; i < weapons.Length; i++)
        {
            if (weapons[i] is SwordWeaponStateMachine)
            {
                types[i] = WeaponType.Sword;
            }
            else if (weapons[i] is BoomerangWeaponStateMachine)
            {
                types[i] = WeaponType.Boomerang;
            }
            else if (weapons[i] is SpellbookWeaponStateMachine)
            {
                types[i] = WeaponType.Spellbook;
            }
            else if (weapons[i] is BurningSwordStateMachine)
            {
                types[i] = WeaponType.BurningSword;
            }
            else if (weapons[i] is HammerSwordStateMachine)
            {
                types[i] = WeaponType.Hammer;
            }
        }
        
        return types;
    }
}