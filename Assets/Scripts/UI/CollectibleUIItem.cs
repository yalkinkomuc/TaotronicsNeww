using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CollectibleUIItem : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image rarityIndicator;
    
    [Header("Rarity Colors")]
    [SerializeField] private Color commonColor = Color.gray;
    [SerializeField] private Color rareColor = Color.yellow;
    
    private CollectibleData collectibleData;
    
    public void Setup(CollectibleData data)
    {
        collectibleData = data;
        
        if (data == null) return;
        
        // Set icon
        if (iconImage != null && data.icon != null)
        {
            iconImage.sprite = data.icon;
        }
        
        // Set name
        if (nameText != null)
        {
            nameText.text = data.itemName;
        }
        
        // Set rarity indicator
        if (rarityIndicator != null)
        {
            rarityIndicator.color = data.isRareCollectible ? rareColor : commonColor;
        }
        
    }
    
    // Called when clicked (if you want click functionality)
    public void OnItemClicked()
    {
        if (collectibleData != null)
        {
            Debug.Log($"Clicked collectible: {collectibleData.itemName}");
            // Add any click behavior here if needed
        }
    }
} 