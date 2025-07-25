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
    
    // Weapon unlock tracking
    private HashSet<int> unlockedWeaponIndices = new HashSet<int>();
    
    // Public getter for UI access
    public int GetCurrentSecondaryWeaponIndex() => currentSecondaryWeaponIndex;
    public int GetCurrentPrimaryWeaponIndex() => currentPrimaryWeaponIndex >= 0 ? currentPrimaryWeaponIndex : startingWeaponIndex;
    
    private Player player;
    private PlayerStats playerStats;
    
    void Start()
    {
        player = GetComponent<Player>();
        playerStats = GetComponent<PlayerStats>();
        
        InitializeAllWeapons();
        
        // İlk giriş kontrolü - sadece ilk girişte başlangıç silahları equip et
        if (EquipmentManager.IsFirstTimePlayer())
        {
            // Sadece starting weapon'ı unlock et ve aktif et
            UnlockWeapon(startingWeaponIndex);
            ActivatePrimaryWeapon(startingWeaponIndex);
            
            // İlk girişte secondary weapon yok!
            currentSecondaryWeaponIndex = -1;
            
            // İlk setup tamamlandı, mark as started
            EquipmentManager.MarkGameAsStarted();
            
            // İlk equip durumunu kaydet
            SaveWeaponState();
        }
        else
        {
            LoadWeaponState();
        }
        
    }

   public void EquipSecondaryWeapon(int index)
    {
        // Eğer gönderilen index geçersizse veya secondary silah değilse veya unlock edilmemişse ilk geçerli secondary'yi bul
        if (index < 0 || index >= weapons.Length || weapons[index] == null || !IsSecondaryWeapon(weapons[index]) || !IsWeaponUnlocked(index))
        {
            index = GetFirstUnlockedSecondaryWeaponIndex();
        }

        // Hâlâ bulunamadıysa secondary weapon yok demektir
        if (index == -1)
        {
            currentSecondaryWeaponIndex = -1;
            
            // Tüm secondary silahları deaktif et
            DisableAllSecondaryWeapons();
            
            // Notify EquipmentManager that no secondary weapon is equipped
            if (EquipmentManager.Instance != null)
            {
                EquipmentManager.Instance.UnequipItem(EquipmentSlot.SecondaryWeapon);
            }
            
            // Save weapon state after unequipping secondary weapon
            OnWeaponEquipped();
            return;
        }

        currentSecondaryWeaponIndex = index;
 
        // Only affect secondary weapons - disable all secondary weapons first
        DisableAllSecondaryWeapons();
        
        if (IsSecondaryWeapon(weapons[index]) && IsWeaponUnlocked(index))
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
            
            // Save weapon state after equipping secondary weapon
            OnWeaponEquipped();
        }
        else
        {
            Debug.LogError($"[PlayerWeaponManager] Cannot equip weapon at index {index} - not secondary or not unlocked");
        }
    }
    
    // Dizideki ilk unlock edilmiş secondary silah index'ini döndür
    private int GetFirstUnlockedSecondaryWeaponIndex()
    {
        for (int i = 0; i < weapons.Length; i++)
        {
            if (weapons[i] != null && IsSecondaryWeapon(weapons[i]) && IsWeaponUnlocked(i))
            {
                return i;
            }
        }
        return -1;
    }
    
    public void RefreshWeaponVisibility()
    {
        ActivatePrimaryWeapon(GetCurrentPrimaryWeaponIndex());
        EquipSecondaryWeapon(GetCurrentSecondaryWeaponIndex());
    }
    
    private void InitializeAllWeapons()
    {
        // Tüm silahları deaktif et
        for (int i = 0; i < weapons.Length; i++)
        {
            if (weapons[i] != null)
            {
                weapons[i].gameObject.SetActive(false);
            }
        }
        
        // Unlock durumlarını yükle
        LoadUnlockStates();
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
        
        // Save weapon state after activation
        OnWeaponEquipped();
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
               weapon is HammerSwordStateMachine||
               weapon is IceHammerStateMachine;
    }
     
    private bool IsSecondaryWeapon(WeaponStateMachine weapon)
    {
        return weapon is BoomerangWeaponStateMachine || 
               weapon is SpellbookWeaponStateMachine ||
               weapon is ShieldStateMachine;
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
            else if (weapons[i] is IceHammerStateMachine)
            {
                types[i] = WeaponType.IceHammer;
            }
        }
        return types;
    }
    
    /// <summary>
    /// Save current weapon state to PlayerPrefs
    /// </summary>
    public void SaveWeaponState()
    {
        // Save primary weapon index
        PlayerPrefs.SetInt("CurrentPrimaryWeaponIndex", currentPrimaryWeaponIndex);
        
        // Save secondary weapon index
        PlayerPrefs.SetInt("CurrentSecondaryWeaponIndex", currentSecondaryWeaponIndex);
        
        // Save current weapon types for EquipmentManager compatibility
        if (currentPrimaryWeaponIndex >= 0 && currentPrimaryWeaponIndex < weapons.Length && weapons[currentPrimaryWeaponIndex] != null)
        {
            WeaponType primaryType = GetWeaponTypeFromIndex(currentPrimaryWeaponIndex);
            PlayerPrefs.SetInt("EquippedMainWeaponType", (int)primaryType);
        }
        
        if (currentSecondaryWeaponIndex >= 0 && currentSecondaryWeaponIndex < weapons.Length && weapons[currentSecondaryWeaponIndex] != null)
        {
            WeaponType secondaryType = GetWeaponTypeFromIndex(currentSecondaryWeaponIndex);
            PlayerPrefs.SetInt("EquippedSecondaryWeaponType", (int)secondaryType);
        }
        
        PlayerPrefs.Save();
    }
    
    /// <summary>
    /// Load weapon state from PlayerPrefs
    /// </summary>
    public void LoadWeaponState()
    {
        // Load primary weapon index
        int savedPrimaryIndex = PlayerPrefs.GetInt("CurrentPrimaryWeaponIndex", startingWeaponIndex);
        
        // Load secondary weapon index
        int savedSecondaryIndex = PlayerPrefs.GetInt("CurrentSecondaryWeaponIndex", -1);
        
        // Validate loaded indices
        if (savedPrimaryIndex >= 0 && savedPrimaryIndex < weapons.Length && weapons[savedPrimaryIndex] != null && IsPrimaryWeapon(weapons[savedPrimaryIndex]))
        {
            ActivatePrimaryWeapon(savedPrimaryIndex);
        }
        else
        {
            Debug.LogWarning($"[PlayerWeaponManager] Invalid primary weapon index {savedPrimaryIndex}, using default");
            ActivatePrimaryWeapon(startingWeaponIndex);
        }
        
        if (savedSecondaryIndex >= 0 && savedSecondaryIndex < weapons.Length && weapons[savedSecondaryIndex] != null && IsSecondaryWeapon(weapons[savedSecondaryIndex]) && IsWeaponUnlocked(savedSecondaryIndex))
        {
            EquipSecondaryWeapon(savedSecondaryIndex);
        }
        else
        {
            // No valid unlocked secondary weapon saved, equip first available unlocked
            int firstUnlockedSecondaryIndex = GetFirstUnlockedSecondaryWeaponIndex();
            if (firstUnlockedSecondaryIndex != -1)
            {
                EquipSecondaryWeapon(firstUnlockedSecondaryIndex);
            }
            else
            {
                // Hiç unlock edilmiş secondary weapon yok
                currentSecondaryWeaponIndex = -1;
            }
        }
    }
    
    /// <summary>
    /// Get WeaponType from weapon index
    /// </summary>
    private WeaponType GetWeaponTypeFromIndex(int index)
    {
        if (index < 0 || index >= weapons.Length || weapons[index] == null)
            return WeaponType.Sword; // fallback
        
        var weapon = weapons[index];
        if (weapon is SwordWeaponStateMachine)
            return WeaponType.Sword;
        else if (weapon is BurningSwordStateMachine)
            return WeaponType.BurningSword;
        else if (weapon is HammerSwordStateMachine)
            return WeaponType.Hammer;
        else if (weapon is IceHammerStateMachine)
            return WeaponType.IceHammer;
        else if (weapon is BoomerangWeaponStateMachine)
            return WeaponType.Boomerang;
        else if (weapon is SpellbookWeaponStateMachine)
            return WeaponType.Spellbook;
        else if (weapon is ShieldStateMachine)
            return WeaponType.Shield;
        
        return WeaponType.Sword; // fallback
    }
    
    /// <summary>
    /// Called when equipment changes - save the new state
    /// </summary>
    public void OnWeaponEquipped()
    {
        SaveWeaponState();
        
        // Also update EquipmentManager
        if (EquipmentManager.Instance != null)
        {
            EquipmentManager.Instance.SaveEquipment();
        }
    }
    
   
    public void UnlockWeapon(int weaponIndex)
    {
        if (weaponIndex >= 0 && weaponIndex < weapons.Length && weapons[weaponIndex] != null)
        {
            unlockedWeaponIndices.Add(weaponIndex);
            SaveUnlockStates();
        }
    }
    
    
    public bool IsWeaponUnlocked(int weaponIndex)
    {
        return unlockedWeaponIndices.Contains(weaponIndex);
    }
    
    /// <summary>
    /// Get all unlocked weapon indices
    /// </summary>
    public HashSet<int> GetUnlockedWeaponIndices()
    {
        return new HashSet<int>(unlockedWeaponIndices);
    }
    
    /// <summary>
    /// Disable all secondary weapons
    /// </summary>
    private void DisableAllSecondaryWeapons()
    {
        for (int i = 0; i < weapons.Length; i++)
        {
            if (weapons[i] != null && IsSecondaryWeapon(weapons[i]))
            {
                weapons[i].gameObject.SetActive(false);
            }
        }
    }
    
    /// <summary>
    /// Save weapon unlock states to PlayerPrefs
    /// </summary>
    private void SaveUnlockStates()
    {
        // Save unlock states as comma-separated string
        string unlockedIndices = string.Join(",", unlockedWeaponIndices);
        PlayerPrefs.SetString("UnlockedWeapons", unlockedIndices);
        PlayerPrefs.Save();
    }
    
    /// <summary>
    /// Load weapon unlock states from PlayerPrefs
    /// </summary>
    private void LoadUnlockStates()
    {
        unlockedWeaponIndices.Clear();
        
        string unlockedIndices = PlayerPrefs.GetString("UnlockedWeapons", "");
        
        if (!string.IsNullOrEmpty(unlockedIndices))
        {
            string[] indices = unlockedIndices.Split(',');
            foreach (string indexStr in indices)
            {
                if (int.TryParse(indexStr, out int index))
                {
                    unlockedWeaponIndices.Add(index);
                }
            }
        }
    }
}