using UnityEngine;
using System.Collections;

/// <summary>
/// Standalone manager for equipment UI updates that works independently from inventory
/// Listens to weapon changes and updates equipment slots in real-time
/// </summary>
public class EquipmentUIManager : MonoBehaviour
{
    public static EquipmentUIManager Instance { get; private set; }
    
    [Header("Equipment Slot References")]
    [SerializeField] private UI_EquipmentSlot weaponSlot;
    [SerializeField] private UI_EquipmentSlot secondaryWeaponSlot;
    [SerializeField] private UI_EquipmentSlot armorSlot;
    [SerializeField] private UI_EquipmentSlot accessorySlot;
    
    [Header("Auto-Find Settings")]
    [SerializeField] private bool autoFindSlots = true;
    [SerializeField] private float findSlotsDelay = 1f; // Delay to find slots after scene load
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            //DontDestroyOnLoad(gameObject);
            
            // Setup event listeners immediately
            SetupEventListeners();
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }
    
    private void Start()
    {
        // Auto-find slots if enabled
        if (autoFindSlots)
        {
            StartCoroutine(DelayedSlotSearch());
        }
    }
    
    private void SetupEventListeners()
    {
        // Listen to weapon switching events
        PlayerWeaponManager.OnSecondaryWeaponChanged += OnSecondaryWeaponChanged;
        
        // Listen to equipment changes if EquipmentManager exists
        if (EquipmentManager.Instance != null)
        {
            EquipmentManager.OnEquipmentChanged += OnEquipmentChanged;
        }
    }
    
    private IEnumerator DelayedSlotSearch()
    {
        yield return new WaitForSeconds(findSlotsDelay);
        FindEquipmentSlots();
        
        // Update slots after finding them
        UpdateAllEquipmentSlots();
    }
    
    /// <summary>
    /// Automatically find equipment slots in the scene
    /// </summary>
    public void FindEquipmentSlots()
    {
        // Try to find AdvancedInventoryUI first
        AdvancedInventoryUI inventoryUI = FindFirstObjectByType<AdvancedInventoryUI>();
        
        if (inventoryUI != null)
        {
            // Get slot references from AdvancedInventoryUI using reflection or direct access
            // We'll look for UI_EquipmentSlot components in the inventory hierarchy
            UI_EquipmentSlot[] foundSlots = inventoryUI.GetComponentsInChildren<UI_EquipmentSlot>(true);
            
            foreach (var slot in foundSlots)
            {
                // Assign slots based on their configured slot type
                switch (slot.SlotType)
                {
                    case EquipmentSlot.MainWeapon:
                        weaponSlot = slot;
                        Debug.Log("[EquipmentUIManager] Found main weapon slot");
                        break;
                    case EquipmentSlot.SecondaryWeapon:
                        secondaryWeaponSlot = slot;
                        Debug.Log("[EquipmentUIManager] Found secondary weapon slot");
                        break;
                    case EquipmentSlot.Armor:
                        armorSlot = slot;
                        Debug.Log("[EquipmentUIManager] Found armor slot");
                        break;
                    case EquipmentSlot.Accessory:
                        accessorySlot = slot;
                        Debug.Log("[EquipmentUIManager] Found accessory slot");
                        break;
                }
            }
        }
        else
        {
            Debug.LogWarning("[EquipmentUIManager] AdvancedInventoryUI not found in scene");
        }
    }
    
    /// <summary>
    /// Manually assign equipment slot references
    /// </summary>
    public void SetEquipmentSlots(UI_EquipmentSlot weapon, UI_EquipmentSlot secondary, UI_EquipmentSlot armor, UI_EquipmentSlot accessory)
    {
        weaponSlot = weapon;
        secondaryWeaponSlot = secondary;
        armorSlot = armor;
        accessorySlot = accessory;
        
        Debug.Log("[EquipmentUIManager] Equipment slots manually assigned");
    }
    
    /// <summary>
    /// Update all equipment slots
    /// </summary>
    public void UpdateAllEquipmentSlots()
    {
        UpdateEquipmentSlot(weaponSlot, "Main Weapon");
        UpdateEquipmentSlot(secondaryWeaponSlot, "Secondary Weapon");
        UpdateEquipmentSlot(armorSlot, "Armor");
        UpdateEquipmentSlot(accessorySlot, "Accessory");
    }
    
    private void UpdateEquipmentSlot(UI_EquipmentSlot slot, string slotName)
    {
        if (slot != null)
        {
            slot.UpdateSlotDisplay();
            Debug.Log($"[EquipmentUIManager] Updated {slotName} slot");
        }
        else
        {
            Debug.LogWarning($"[EquipmentUIManager] {slotName} slot reference is null");
        }
    }
    
    /// <summary>
    /// Called when secondary weapon changes
    /// </summary>
    private void OnSecondaryWeaponChanged(int weaponIndex, WeaponStateMachine weaponStateMachine)
    {
        Debug.Log($"[EquipmentUIManager] Secondary weapon changed: Index {weaponIndex}, Weapon: {weaponStateMachine?.name}");
        
        // Update secondary weapon slot specifically
        UpdateEquipmentSlot(secondaryWeaponSlot, "Secondary Weapon");
        
        // Also update main weapon slot if needed
        UpdateEquipmentSlot(weaponSlot, "Main Weapon");
    }
    
    /// <summary>
    /// Called when any equipment changes (if EquipmentManager is available)
    /// </summary>
    private void OnEquipmentChanged(EquipmentSlot slot, EquipmentData equipment)
    {
        Debug.Log($"[EquipmentUIManager] Equipment changed in slot: {slot}");
        
        // Update specific slot based on the changed equipment
        switch (slot)
        {
            case EquipmentSlot.MainWeapon:
                UpdateEquipmentSlot(weaponSlot, "Main Weapon");
                break;
            case EquipmentSlot.SecondaryWeapon:
                UpdateEquipmentSlot(secondaryWeaponSlot, "Secondary Weapon");
                break;
            case EquipmentSlot.Armor:
                UpdateEquipmentSlot(armorSlot, "Armor");
                break;
            case EquipmentSlot.Accessory:
                UpdateEquipmentSlot(accessorySlot, "Accessory");
                break;
        }
    }
    
    /// <summary>
    /// Force refresh all slots (useful for debugging or manual updates)
    /// </summary>
    [ContextMenu("Force Refresh All Slots")]
    public void ForceRefreshAllSlots()
    {
        Debug.Log("[EquipmentUIManager] Force refreshing all equipment slots");
        
        // Re-find slots if needed
        if (autoFindSlots && (weaponSlot == null || secondaryWeaponSlot == null))
        {
            FindEquipmentSlots();
        }
        
        // Update all slots
        UpdateAllEquipmentSlots();
    }
    
    private void OnDestroy()
    {
        // Unsubscribe from events
        PlayerWeaponManager.OnSecondaryWeaponChanged -= OnSecondaryWeaponChanged;
        
        if (EquipmentManager.Instance != null)
        {
            EquipmentManager.OnEquipmentChanged -= OnEquipmentChanged;
        }
        
        if (Instance == this)
        {
            Instance = null;
        }
    }
    
    /// <summary>
    /// Check if all equipment slots are properly assigned
    /// </summary>
    public bool AreAllSlotsAssigned()
    {
        return weaponSlot != null && secondaryWeaponSlot != null && armorSlot != null && accessorySlot != null;
    }
    
    /// <summary>
    /// Get debug info about current slot assignments
    /// </summary>
    public string GetSlotDebugInfo()
    {
        return $"Weapon: {(weaponSlot != null ? "✓" : "✗")}, " +
               $"Secondary: {(secondaryWeaponSlot != null ? "✓" : "✗")}, " +
               $"Armor: {(armorSlot != null ? "✓" : "✗")}, " +
               $"Accessory: {(accessorySlot != null ? "✓" : "✗")}";
    }
} 