using UnityEngine;
using System;
using System.Collections.Generic;

public class PlayerWeaponManager : MonoBehaviour
{
    // Static event for weapon switching - NO LONGER NEEDED
    // public static event Action<int, WeaponStateMachine> OnSecondaryWeaponChanged;
    
    public WeaponStateMachine[] weapons;
    private int currentSecondaryWeaponIndex = -1; // Will be set to first secondary weapon found

    [SerializeField] public int startingWeaponIndex = 3; // Burning Sword ile başla
    private int currentPrimaryWeaponIndex = -1; // Aktif primary weapon index
    
    // Public getter for UI access
    public int GetCurrentSecondaryWeaponIndex() => currentSecondaryWeaponIndex;
    public int GetCurrentPrimaryWeaponIndex() => currentPrimaryWeaponIndex >= 0 ? currentPrimaryWeaponIndex : startingWeaponIndex;
    
    private Player player;
    private PlayerStats playerStats;

    private void Awake()
    {
        Debug.Log(currentSecondaryWeaponIndex);
        Debug.Log(GetCurrentSecondaryWeaponIndex());
        
        
    }

    private void Update()
    {
        Debug.Log(currentSecondaryWeaponIndex);
        Debug.Log(GetCurrentSecondaryWeaponIndex());
        int testIndex = PlayerPrefs.GetInt("EquippedSecondaryWeaponType", -1);
        Debug.Log(testIndex);
    }

    void Start()
    {
        player = GetComponent<Player>();
        playerStats = GetComponent<PlayerStats>();
        int saved = PlayerPrefs.GetInt("EquippedSecondaryWeaponType", -1);
        Debug.Log($"START: Okunan secondary type = {(WeaponType)saved}");
        
        
        
        
        InitializeAllWeapons();
        
        ActivatePrimaryWeapon(startingWeaponIndex);
        
        if (currentSecondaryWeaponIndex != -1)
        {
            EquipSecondaryWeapon(currentSecondaryWeaponIndex);
        }
        
        if (BlacksmithManager.Instance != null && playerStats != null)
        {
            BlacksmithManager.Instance.ApplyWeaponUpgrades(playerStats);
        }
        
    }

   public void EquipSecondaryWeapon(int index)
    {
        // Eğer gönderilen index geçersizse veya secondary silah değilse ilk geçerli secondary'yi bul
        if (index < 0 || index >= weapons.Length || weapons[index] == null || !IsSecondaryWeapon(weapons[index]))
        {
            index = GetFirstSecondaryWeaponIndex();
        }

        // Hâlâ bulunamadıysa çık
        if (index == -1)
        {
            Debug.LogWarning("No secondary weapon found to equip!");
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
            
            // Equip via EquipmentManager so that single OnEquipmentChanged event kullanılır
            if (EquipmentManager.Instance != null)
            {
                EquipmentManager.Instance.EquipSecondaryByStateMachine(weapons[index]);
            }

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
                else if (weapons[index] is ShieldStateMachine)
                {
                    player.UpdateLastActiveWeapon(WeaponState.Idle);
                }
            }
        }
        else
        {
            Debug.LogError($"Weapon at index {index} is not a secondary weapon!");
        }
        // UpdateUISlots(); - NO LONGER NEEDED, EquipSecondaryByStateMachine handles it.
    }

    // Dizideki ilk secondary silah index'ini döndür
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
    
    // private void UpdateUISlots() - NO LONGER NEEDED
    // {
    //     if (AdvancedInventoryUI.Instance != null)
    //     {
    //         AdvancedInventoryUI.Instance.UpdateEquipmentSlots();
    //     }
        
    //     OnSecondaryWeaponChanged?.Invoke(currentSecondaryWeaponIndex, weapons[currentSecondaryWeaponIndex]);
    // }
    public void RefreshWeaponVisibility()
    {
        ActivatePrimaryWeapon(GetCurrentPrimaryWeaponIndex());
        EquipSecondaryWeapon(GetCurrentSecondaryWeaponIndex());
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
               weapon is SpellbookWeaponStateMachine ||
               weapon is ShieldStateMachine;
    }
    
  
    
    // Get first secondary weapon index
   
   
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
            else if (weapons[i] is ShieldStateMachine)
            {
                types[i] = WeaponType.Shield;
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

    // Helper method to find weapon index by WeaponType
    private int GetWeaponIndexByType(WeaponType type)
    {
        for (int i = 0; i < weapons.Length; i++)
        {
            if (weapons[i] == null) continue;

            // Sadece secondary silahlar taransın
            if (!IsSecondaryWeapon(weapons[i])) continue;

            if ((type == WeaponType.Boomerang && weapons[i] is BoomerangWeaponStateMachine) ||
                (type == WeaponType.Spellbook && weapons[i] is SpellbookWeaponStateMachine) ||
                (type == WeaponType.Shield && weapons[i] is ShieldStateMachine))
            {
                return i;
            }
        }
        return -1;
    }
}