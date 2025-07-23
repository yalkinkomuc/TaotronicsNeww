using System;
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
    [SerializeField] private int maxRetryAttempts = 3; // Maximum number of retry attempts
    private int currentRetryAttempts = 0;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            
            SetupEventListeners();
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
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
    
    private IEnumerator RetrySlotSearch()
    {
        currentRetryAttempts++;
        
        if (currentRetryAttempts > maxRetryAttempts)
        {
            Debug.LogWarning($"[EquipmentUIManager] Max retry attempts ({maxRetryAttempts}) reached. Stopping retry.");
            yield break;
        }
        
        // Wait a bit longer before retrying
        yield return new WaitForSeconds(2f);
        
        FindEquipmentSlots();
        
        // Update slots after finding them
        UpdateAllEquipmentSlots();
    }
    
    // ReSharper disable Unity.PerformanceAnalysis
    public void FindEquipmentSlots()
    {
        // Try multiple strategies to find AdvancedInventoryUI
        AdvancedInventoryUI inventoryUI = null;
        
        // Strategy 1: Try to find via Instance (if already initialized)
        if (AdvancedInventoryUI.Instance != null)
        {
            inventoryUI = AdvancedInventoryUI.Instance;
        }
        
        // Strategy 2: Try to find via FindFirstObjectByType (includes inactive objects)
        if (inventoryUI == null)
        {
            inventoryUI = FindFirstObjectByType<AdvancedInventoryUI>();
        }
        
        // Strategy 3: Try to find via FindObjectsOfType (includes inactive objects)
        if (inventoryUI == null)
        {
            AdvancedInventoryUI[] allInventoryUIs = FindObjectsOfType<AdvancedInventoryUI>(true); // true = include inactive
            if (allInventoryUIs.Length > 0)
            {
                inventoryUI = allInventoryUIs[0];
            }
        }
        
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
                        break;
                    case EquipmentSlot.SecondaryWeapon:
                        secondaryWeaponSlot = slot;
                        break;
                    case EquipmentSlot.Armor:
                        armorSlot = slot;
                        break;
                    case EquipmentSlot.Accessory:
                        accessorySlot = slot;
                        break;
                }
            }
        }
        else
        {
            Debug.LogWarning("[EquipmentUIManager] AdvancedInventoryUI not found in scene - will retry later");
            // Schedule a retry if slots are not found
            StartCoroutine(RetrySlotSearch());
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
    }
    
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
        }
        else
        {
            Debug.LogWarning($"[EquipmentUIManager] {slotName} slot reference is null");
        }
    }
    
    private void OnEquipmentChanged(EquipmentSlot slot, EquipmentData equipment)
    {
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
    
    [ContextMenu("Force Refresh All Slots")]
    public void ForceRefreshAllSlots()
    {
        if (autoFindSlots && (weaponSlot == null || secondaryWeaponSlot == null))
        {
            FindEquipmentSlots();
        }
        
        UpdateAllEquipmentSlots();
    }
    
    private void OnDestroy()
    {
        if (EquipmentManager.Instance != null)
        {
            EquipmentManager.OnEquipmentChanged -= OnEquipmentChanged;
        }
        
        if (Instance == this)
        {
            Instance = null;
        }
    }
    
} 