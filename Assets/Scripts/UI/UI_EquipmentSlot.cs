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
        // Handle slot click - open equipment selection or show details
        Debug.Log($"Clicked on {slotType} slot");
        
        if (currentEquipment != null)
        {
            // Show equipment details or context menu
            Debug.Log($"Equipped: {currentEquipment.itemName}");
            // TODO: Show tooltip or context menu
        }
        else
        {
            // Open equipment selection for this slot type
            Debug.Log($"Open equipment selection for {slotType}");
            OpenEquipmentSelectionPanel();
        }
    }
    
    private void OpenEquipmentSelectionPanel()
    {
        if (UI_EquipmentSelectionPanel.Instance != null)
        {
            Vector3 worldPosition = transform.position;
            UI_EquipmentSelectionPanel.Instance.ShowSelectionPanel(
                slotType, 
                worldPosition, 
                OnEquipmentSelected
            );
        }
        else
        {
            Debug.LogWarning("UI_EquipmentSelectionPanel.Instance not found!");
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