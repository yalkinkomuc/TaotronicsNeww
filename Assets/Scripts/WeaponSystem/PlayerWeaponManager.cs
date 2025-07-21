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
        
        player = GetComponent<Player>();
        playerStats = GetComponent<PlayerStats>();
        
        InitializeAllWeapons();
        
        ActivatePrimaryWeapon(startingWeaponIndex);
        
        currentSecondaryWeaponIndex = GetFirstSecondaryWeaponIndex();
        if (currentSecondaryWeaponIndex != -1)
        {
            EquipSecondaryWeapon(currentSecondaryWeaponIndex);
        }
        
        if (BlacksmithManager.Instance != null && playerStats != null)
        {
            BlacksmithManager.Instance.ApplyWeaponUpgrades(playerStats);
        }
        
        if (currentSecondaryWeaponIndex != -1 && currentSecondaryWeaponIndex < weapons.Length)
        {
            OnSecondaryWeaponChanged?.Invoke(currentSecondaryWeaponIndex, weapons[currentSecondaryWeaponIndex]);
        }
    }

   public void EquipSecondaryWeapon(int index)
    {
        if (index < 0 || index >= weapons.Length || weapons[index] == null)
        {
            Debug.Log($"Invalid secondary weapon index: {index}");
            return;
        }
        
        currentSecondaryWeaponIndex = index;

        // Only affect secondary weapons - disable all secondary weapons first
        for (int i = 0; i < weapons.Length; i++)
        {
            if (weapons[i] != null && IsSecondaryWeapon(weapons[i]))
            {
                weapons[i].gameObject.SetActive(false);
            }
        }
        
        if (IsSecondaryWeapon(weapons[index]))
        {
            weapons[index].gameObject.SetActive(true);
            
            if (player != null)
            {
                if (weapons[index] is BoomerangWeaponStateMachine)
                {
                    player.UpdateLastActiveWeapon(WeaponState.ThrowBoomerang);
                }
                else if (weapons[index] is SpellbookWeaponStateMachine)
                {
                    player.UpdateLastActiveWeapon(WeaponState.Spell1);
                }
            }
        }
        else
        {
            Debug.LogError($"Weapon at index {index} is not a secondary weapon!");
        }
        UpdateUISlots();
    }
    
    private void UpdateUISlots()
    {
        if (AdvancedInventoryUI.Instance != null)
        {
            AdvancedInventoryUI.Instance.UpdateEquipmentSlots();
        }
        
        OnSecondaryWeaponChanged?.Invoke(currentSecondaryWeaponIndex, weapons[currentSecondaryWeaponIndex]);
    }
    public void RefreshWeaponVisibility()
    {
        ActivatePrimaryWeapon(GetCurrentPrimaryWeaponIndex());
        EquipSecondaryWeapon(currentSecondaryWeaponIndex);
    }
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
    public void ActivatePrimaryWeapon(int primaryIndex)
    {
        if (primaryIndex < 0 || primaryIndex >= weapons.Length || weapons[primaryIndex] == null)
        {
            Debug.LogError($"Invalid primary weapon index: {primaryIndex}");
            return;
        }
        
        DisableAllPrimaryWeapons();
        
        weapons[primaryIndex].gameObject.SetActive(true);
        
        currentPrimaryWeaponIndex = primaryIndex;
        
        Debug.Log($"Activated primary weapon: {weapons[primaryIndex].name}");
    }
    
   
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
     
    private bool IsSecondaryWeapon(WeaponStateMachine weapon)
    {
        return weapon is BoomerangWeaponStateMachine || 
               weapon is SpellbookWeaponStateMachine;
    }
    
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
        
        int currentIndexInList = secondaryIndices.IndexOf(currentSecondaryWeaponIndex);
      
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
   
    public WeaponType[] GetEquippedWeaponTypes()
    {
        WeaponType[] types = new WeaponType[weapons.Length];
        
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