using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Linq;

public class UI_EquipmentSlot : MonoBehaviour, IPointerClickHandler
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
    
    private void Start()
    {
        UpdateSlotDisplay();
    }
    
    public void Initialize(EquipmentSlot equipmentSlot)
    {
        slotType = equipmentSlot;
        UpdateSlotDisplay();
    }
    
    public void UpdateSlotDisplay()
    {
        currentEquipment = GetEquippedItem();
        
        if (slotType == EquipmentSlot.SecondaryWeapon)
        {
            Debug.Log($"[SecondaryWeapon] UpdateSlotDisplay - Equipment: {currentEquipment?.itemName ?? "null"}");
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
        // Use EquipmentManager as the single source of truth
        if (EquipmentManager.Instance != null)
        {
            return EquipmentManager.Instance.GetCurrentMainWeapon();
        }
        
        return null; // No EquipmentManager available
    }
    
    private WeaponData GetEquippedSecondaryWeapon()
    {
        // Use EquipmentManager as the single source of truth
        if (EquipmentManager.Instance != null)
        {
            return EquipmentManager.Instance.GetCurrentSecondaryWeapon();
        }
        
        return null; // No EquipmentManager available
    }
    
    private ArmorData GetEquippedArmor()
    {
        // TODO: Get from EquipmentManager when implemented
        // For now return null (empty slot)
        return null;
    }
    
    private AccessoryData GetEquippedAccessory()
    {
        // TODO: Get from EquipmentManager when implemented
        // For now return null (empty slot)
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
        // Always open equipment selection panel regardless of slot state
        Debug.Log($"Clicked on {slotType} slot - Opening selection panel");
        OpenEquipmentSelectionPanel();
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
        Debug.Log($"Equipment selected: {selectedEquipment.itemName} for slot {slotType}");
        
        // For now, just update the display
        // TODO: Integrate with EquipmentManager when it's ready
        currentEquipment = selectedEquipment;
        UpdateSlotDisplay();
    }
} 