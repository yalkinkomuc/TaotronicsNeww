using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class UI_RuneSlot : MonoBehaviour, IPointerClickHandler
{
    [Header("UI References")]
    [SerializeField] private Image runeIcon;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image runeTypeIndicator; // Shows rune type color
    [SerializeField] private Button openRuneInventoryButton; // Inspector'dan atanacak
    
    [Header("Slot Configuration")]
    [SerializeField] private Sprite emptySlotSprite;
    [SerializeField] private Color equippedColor = Color.white;
    [SerializeField] private Color emptyColor = new Color(1, 1, 1, 0.3f);
    
    private int slotIndex;
    private RuneData currentRune;
    
    public void Initialize(int index)
    {
        slotIndex = index;
        UpdateRune(currentRune);
    }
    
    public void UpdateRune(RuneData rune)
    {
        currentRune = rune;
        
        if (rune != null)
        {
            // Show equipped rune
            runeIcon.sprite = rune.icon;
            runeIcon.color = equippedColor;
            
           
            
            // Show rune type indicator
            if (runeTypeIndicator != null)
            {
                runeTypeIndicator.color = GetRuneTypeColor(rune.runeType);
                runeTypeIndicator.gameObject.SetActive(true);
            }
          
        }
        else
        {
            // Show empty slot
            runeIcon.sprite = emptySlotSprite;
            runeIcon.color = emptyColor;
            
            
            
            // Hide rune type indicator
            if (runeTypeIndicator != null)
            {
                runeTypeIndicator.gameObject.SetActive(false);
            }
            
            
        }
        
    }
    private Color GetRuneTypeColor(RuneType runeType)
    {
        return runeType switch
        {
            RuneType.Vitality => Color.red,           // Health/Vitality
            RuneType.Strength => new Color(1f, 0.5f, 0f), // Orange - Physical damage
            RuneType.Dexterity => Color.yellow,       // Attack speed/critical
            RuneType.Intelligence => Color.blue,      // Magical damage
            RuneType.Defense => Color.gray,           // Armor/resistance
            RuneType.Luck => Color.green,             // Critical chance/rare drops
            RuneType.Utility => Color.magenta,        // Special effects
            _ => Color.white
        };
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
        if (currentRune != null)
        {
            // Unequip the rune
            if (EquipmentManager.Instance != null)
            {
                RuneData unequippedRune = EquipmentManager.Instance.UnequipRune(slotIndex);
                if (unequippedRune != null && Inventory.instance != null)
                {
                    // Add back to inventory
                    Inventory.instance.AddItem(unequippedRune);
                }
            }
        }
    }
    
    // Tooltip functionality (if needed later)
    public void ShowTooltip()
    {
        if (currentRune != null)
        {
            // TODO: Show rune tooltip
            Debug.Log($"Rune: {currentRune.GetTooltip()}");
        }
    }
    
    public void HideTooltip()
    {
        // TODO: Hide rune tooltip
    }
    
    public bool IsEmpty()
    {
        return currentRune == null;
    }
    
    public RuneType? GetRuneType()
    {
        return currentRune?.runeType;
    }

    private void Awake()
    {
        UpdateRune(null);
    }

    private void Start()
    {
        UpdateRune(currentRune);
        
        if (openRuneInventoryButton != null)
        {
            openRuneInventoryButton.onClick.AddListener(OpenRuneInventoryPanel);
        }
    }

    private void OpenRuneInventoryPanel()
    {
        if (AdvancedInventoryUI.Instance != null)
        {
            AdvancedInventoryUI.Instance.ToggleRuneInventoryPanel();
        }
    }
} 