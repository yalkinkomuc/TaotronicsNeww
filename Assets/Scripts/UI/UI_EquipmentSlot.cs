using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Linq;

public class UI_EquipmentSlot : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI References")]
    [SerializeField] private Image equipmentIcon;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private TextMeshProUGUI equipmentNameText;
    [SerializeField] private GameObject emptySlotIndicator;
    
    [Header("Slot Configuration")]
    [SerializeField] private EquipmentSlot slotType;
    [SerializeField] private Sprite emptySlotSprite;
    [SerializeField] private Color equippedColor = Color.white;
    [SerializeField] private Color emptyColor = new Color(1, 1, 1, 0.3f);
    
    
    
    // Public property for external access
    public EquipmentSlot SlotType => slotType;
    
    private EquipmentData currentEquipment;
    private System.Action<EquipmentData> onEquipmentSelected;
    private bool isHovering = false;
    
    private void Start()
    {
        // Subscribe to equipment change events
        EquipmentManager.OnEquipmentChanged += OnEquipmentChanged;
        
        UpdateSlotDisplay();
    }
    
    private void OnDestroy()
    {
        // Unsubscribe from events
        EquipmentManager.OnEquipmentChanged -= OnEquipmentChanged;
    }
    
    private void OnEquipmentChanged(EquipmentSlot changedSlot, EquipmentData equipment)
    {
        // Only update if this slot is the one that changed
        if (changedSlot == slotType)
        {
            currentEquipment = equipment;
            UpdateSlotDisplay();
        }
    }
    
    public void Initialize(EquipmentSlot equipmentSlot)
    {
        slotType = equipmentSlot;
        UpdateSlotDisplay();
    }
    
    public void SetWeaponData(WeaponData weaponData)
    {
        currentEquipment = weaponData;
        UpdateSlotDisplay();
    }
    
    public void SetEquipmentData(EquipmentData equipmentData)
    {
        currentEquipment = equipmentData;
        UpdateSlotDisplay();
    }
    
    public void SetSelectionCallback(System.Action<EquipmentData> callback)
    {
        onEquipmentSelected = callback;
    }
    
    public void UpdateSlotDisplay()
    {
        // Only get equipped item if currentEquipment is not set (for regular equipment slots)
        if (currentEquipment == null)
        {
            currentEquipment = GetEquippedItem();
        }
        
        if (currentEquipment != null)
        {
            ShowEquippedItem();
        }
        else
        {
            ShowEmptySlot();
        }
    }
    
    private void ShowEquippedItem()
    {
        // Show item icon
        if (equipmentIcon != null)
        {
            equipmentIcon.sprite = currentEquipment.icon;
            equipmentIcon.color = equippedColor;
            equipmentIcon.gameObject.SetActive(true);
        }
        
        // Show item name
        if (equipmentNameText != null)
        {
            equipmentNameText.text = currentEquipment.itemName;
            equipmentNameText.gameObject.SetActive(true);
        }
        
        // Hide empty indicator
        if (emptySlotIndicator != null)
        {
            emptySlotIndicator.SetActive(false);
        }
    }
    
    private void ShowEmptySlot()
    {
        // Show empty slot visual
        if (equipmentIcon != null)
        {
            equipmentIcon.sprite = emptySlotSprite;
            equipmentIcon.color = emptyColor;
            equipmentIcon.gameObject.SetActive(true);
        }
        
        // Show slot type name
        if (equipmentNameText != null)
        {
            equipmentNameText.text = GetSlotTypeName();
            equipmentNameText.gameObject.SetActive(true);
        }
        
        // Show empty indicator
        if (emptySlotIndicator != null)
        {
            emptySlotIndicator.SetActive(true);
        }
    }
    
    private EquipmentData GetEquippedItem()
    {
        if (EquipmentManager.Instance == null)
        {
            return null;
        }
        
        // Get equipped item based on slot type
        return slotType switch
        {
            EquipmentSlot.MainWeapon => GetEquippedWeapon(),
            EquipmentSlot.SecondaryWeapon => GetEquippedSecondaryWeapon(),
            EquipmentSlot.Armor => GetEquippedArmor(),
            EquipmentSlot.Accessory => GetEquippedAccessory(),
            _ => null
        };
    }
    
    private WeaponData GetEquippedWeapon()
    {
        // Get from EquipmentManager
        if (EquipmentManager.Instance != null)
        {
            return EquipmentManager.Instance.GetEquippedItem(EquipmentSlot.MainWeapon) as WeaponData;
        }
        
        return null;
    }
    
    private EquipmentData GetEquippedSecondaryWeapon()
    {
        // Get from EquipmentManager - this should return null if no secondary weapon is equipped
        if (EquipmentManager.Instance != null)
        {
            return EquipmentManager.Instance.GetEquippedItem(EquipmentSlot.SecondaryWeapon);
        }
        
        return null;
    }
    
    private ArmorData GetEquippedArmor()
    {
        if (EquipmentManager.Instance != null)
        {
            return EquipmentManager.Instance.GetEquippedItem(EquipmentSlot.Armor) as ArmorData;
        }
        return null;
    }
    
    private AccessoryData GetEquippedAccessory()
    {
        if (EquipmentManager.Instance != null)
        {
            return EquipmentManager.Instance.GetEquippedItem(EquipmentSlot.Accessory) as AccessoryData;
        }
        return null;
    }
    
    private string GetSlotTypeName()
    {
        return slotType switch
        {
            EquipmentSlot.MainWeapon => "Weapon",
            EquipmentSlot.SecondaryWeapon => "Secondary",
            EquipmentSlot.Armor => "Armor",
            EquipmentSlot.Accessory => "Accessory",
            _ => "Empty"
        };
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        // If we have a selection callback, this is a selection slot
        if (onEquipmentSelected != null)
        {
            onEquipmentSelected?.Invoke(currentEquipment);
        }
        else
        {
            OpenEquipmentSelectionPanel();
        }
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        // Prevent multiple triggers
        if (isHovering) return;
        
        // Don't show tooltip if this is a selection slot (has callback)
        if (onEquipmentSelected != null)
        {
            return; // This is a selection slot, don't show tooltip
        }
        
        isHovering = true;
        Debug.Log("OnPointerEnter triggered on EquipmentSlot!");
        
        // Check if mouse is over tooltip panel (prevent event conflicts)
        if (GlobalTooltipManager.Instance != null && 
            GlobalTooltipManager.Instance.IsMouseOverTooltip())
        {
            Debug.Log("Mouse is over tooltip panel, ignoring event");
            isHovering = false;
            return; // Don't show tooltip if mouse is over tooltip panel
        }
        
        // Show tooltip even if slot is empty (show slot type info)
        if (GlobalTooltipManager.Instance != null)
        {
            if (currentEquipment != null)
            {
                Debug.Log($"Showing tooltip for equipment: {currentEquipment.itemName}");
                GlobalTooltipManager.Instance.ShowTooltip(currentEquipment, transform.position);
            }
            else
            {
                Debug.Log("Slot is empty, no tooltip to show");
            }
        }
        else
        {
            Debug.LogError("GlobalTooltipManager.Instance is null!");
        }
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
        Debug.Log("OnPointerExit triggered on EquipmentSlot!");
        
        // Hide global tooltip
        if (GlobalTooltipManager.Instance != null)
        {
            GlobalTooltipManager.Instance.HideTooltip();
        }
    }
    
    /// <summary>
    /// Force reset hover state when panel closes
    /// </summary>
    public void ResetHoverState()
    {
        isHovering = false;
        Debug.Log("Hover state reset for EquipmentSlot!");
    }
    
    private void OpenEquipmentSelectionPanel()
    {
        // This will trigger lazy loading if needed
        UI_EquipmentSelectionPanel panel = UI_EquipmentSelectionPanel.Instance;
        if (panel != null)
        {
            // Get the slot's screen position instead of world position
            Vector3 screenPosition = RectTransformUtility.WorldToScreenPoint(Camera.main, transform.position);
            panel.ShowSelectionPanel(
                slotType, 
                screenPosition, 
                OnEquipmentSelected
            );
        }
        else
        {
            Debug.LogError("Failed to load UI_EquipmentSelectionPanel!");
        }
    }
    
    private void OnEquipmentSelected(EquipmentData selectedEquipment)
    {
        // TODO: Integrate with EquipmentManager when it's ready
        currentEquipment = selectedEquipment;
        UpdateSlotDisplay();
        
        // Reset hover state to ensure tooltip works after equipment change
        isHovering = false;
        
        // Update tooltip if it's currently showing
        if (GlobalTooltipManager.Instance != null && currentEquipment != null)
        {
            GlobalTooltipManager.Instance.ShowTooltip(currentEquipment, transform.position);
        }
        
        onEquipmentSelected?.Invoke(selectedEquipment);
    }
    
} 
