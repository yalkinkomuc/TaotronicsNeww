using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class UI_EquipmentSlot : MonoBehaviour, IPointerClickHandler
{
    [Header("UI References")]
    [SerializeField] private Image equipmentIcon;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private TextMeshProUGUI enhancementText;
    [SerializeField] private GameObject emptySlotIndicator;
    
    [Header("Slot Configuration")]
    [SerializeField] private Sprite emptySlotSprite;
    [SerializeField] private Color equippedColor = Color.white;
    [SerializeField] private Color emptyColor = new Color(1, 1, 1, 0.3f);
    
    private EquipmentSlot slotType;
    private EquipmentData currentEquipment;
    
    public void Initialize(EquipmentSlot equipmentSlot)
    {
        slotType = equipmentSlot;
        UpdateEquipment(null);
    }
    
    public void UpdateEquipment(EquipmentData equipment)
    {
        currentEquipment = equipment;
        
        if (equipment != null)
        {
            // Show equipped item
            equipmentIcon.sprite = equipment.icon;
            equipmentIcon.color = equippedColor;
            
            // Show enhancement level if enhanced
            if (enhancementText != null)
            {
                if (equipment.enhancementLevel > 0)
                {
                    enhancementText.text = $"+{equipment.enhancementLevel}";
                    enhancementText.gameObject.SetActive(true);
                }
                else
                {
                    enhancementText.gameObject.SetActive(false);
                }
            }
            
            // Hide empty slot indicator
            if (emptySlotIndicator != null)
            {
                emptySlotIndicator.SetActive(false);
            }
        }
        else
        {
            // Show empty slot
            equipmentIcon.sprite = emptySlotSprite;
            equipmentIcon.color = emptyColor;
            
            // Hide enhancement text
            if (enhancementText != null)
            {
                enhancementText.gameObject.SetActive(false);
            }
            
            // Show empty slot indicator
            if (emptySlotIndicator != null)
            {
                emptySlotIndicator.SetActive(true);
            }
        }
        
        UpdateSlotVisuals();
    }
    
    private void UpdateSlotVisuals()
    {
        // Update background based on slot type and equipment rarity
        if (backgroundImage != null)
        {
            if (currentEquipment != null)
            {
                // Set background color based on item rarity
                backgroundImage.color = GetRarityColor(currentEquipment.rarity);
            }
            else
            {
                // Default background for empty slot
                backgroundImage.color = Color.gray;
            }
        }
    }
    
    private Color GetRarityColor(ItemRarity rarity)
    {
        return rarity switch
        {
            ItemRarity.Common => Color.white,
            ItemRarity.Uncommon => Color.green,
            ItemRarity.Rare => Color.blue,
            ItemRarity.Epic => new Color(0.6f, 0.2f, 0.8f), // Purple
            ItemRarity.Legendary => Color.yellow,
            ItemRarity.Mythic => Color.red,
            _ => Color.white
        };
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            // Left click: Show equipment selection panel
            ShowEquipmentSelectionPanel();
        }
        else if (eventData.button == PointerEventData.InputButton.Right && currentEquipment != null)
        {
            // Right click: Unequip current item
            UnequipCurrentItem();
        }
    }
    
    private void ShowEquipmentSelectionPanel()
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
    }
    
    private void OnEquipmentSelected(EquipmentData selectedEquipment)
    {
        if (EquipmentManager.Instance != null && selectedEquipment != null)
        {
            // Equip the selected item
            bool equipped = EquipmentManager.Instance.EquipItem(selectedEquipment);
            if (equipped && Inventory.instance != null)
            {
                // Remove from inventory
                Inventory.instance.RemoveItem(selectedEquipment);
            }
        }
    }
    
    private void UnequipCurrentItem()
    {
        if (currentEquipment != null && EquipmentManager.Instance != null)
        {
            EquipmentData unequippedItem = EquipmentManager.Instance.UnequipItem(slotType);
            if (unequippedItem != null && Inventory.instance != null)
            {
                // Add back to inventory
                Inventory.instance.AddItem(unequippedItem);
            }
        }
    }
    
    // Tooltip functionality (if needed later)
    public void ShowTooltip()
    {
        if (currentEquipment != null)
        {
            // TODO: Show equipment tooltip
            Debug.Log($"Equipment: {currentEquipment.GetTooltip()}");
        }
    }
    
    public void HideTooltip()
    {
        // TODO: Hide equipment tooltip
    }
} 