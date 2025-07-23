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
    private System.Action<EquipmentData> onEquipmentSelected;
    
    private void Start()
    {
        UpdateSlotDisplay();
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
        // Use EquipmentManager cached value as the single source of truth
        if (EquipmentManager.Instance != null)
        {
            return EquipmentManager.Instance.GetEquippedItem(EquipmentSlot.MainWeapon) as WeaponData;
        }
        
        return null; // No EquipmentManager available
    }
    
    private EquipmentData GetEquippedSecondaryWeapon()
    {
        // Use EquipmentManager cached value as the single source of truth
        if (EquipmentManager.Instance != null)
        {
            return EquipmentManager.Instance.GetEquippedItem(EquipmentSlot.SecondaryWeapon);
        }
        
        return null; // No EquipmentManager available
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
        onEquipmentSelected?.Invoke(selectedEquipment);
    }
} 