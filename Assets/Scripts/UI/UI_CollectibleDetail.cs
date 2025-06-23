using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_CollectibleDetail : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image collectibleIcon;
    [SerializeField] private TextMeshProUGUI collectibleNameText;
    [SerializeField] private TextMeshProUGUI collectibleDescriptionText;
    [SerializeField] private TextMeshProUGUI discoveryLocationText;
    [SerializeField] private GameObject ownedIndicator;
    [SerializeField] private GameObject notOwnedOverlay;
    [SerializeField] private Image rarityBackground;
    
    [Header("Visual States")]
    [SerializeField] private Color ownedColor = Color.white;
    [SerializeField] private Color notOwnedColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);
    
    private CollectibleData collectibleData;
    private bool isOwned;
    
    public void Initialize(CollectibleData collectible, bool owned)
    {
        collectibleData = collectible;
        isOwned = owned;
        
        UpdateDisplay();
    }
    
    private void UpdateDisplay()
    {
        if (collectibleData == null) return;
        
        // Update icon
        if (collectibleIcon != null)
        {
            collectibleIcon.sprite = collectibleData.icon;
            collectibleIcon.color = isOwned ? ownedColor : notOwnedColor;
        }
        
        // Update name (show ??? if not owned)
        if (collectibleNameText != null)
        {
            collectibleNameText.text = isOwned ? collectibleData.itemName : "???";
        }
        
        // Update description (show ??? if not owned)
        if (collectibleDescriptionText != null)
        {
            collectibleDescriptionText.text = isOwned ? collectibleData.description : "Not yet discovered";
        }
        
        // Update discovery location
        if (discoveryLocationText != null)
        {
            if (isOwned && !string.IsNullOrEmpty(collectibleData.discoveryLocation))
            {
                discoveryLocationText.text = $"Found in: {collectibleData.discoveryLocation}";
                discoveryLocationText.gameObject.SetActive(true);
            }
            else
            {
                discoveryLocationText.gameObject.SetActive(false);
            }
        }
        
        // Update owned indicator
        if (ownedIndicator != null)
        {
            ownedIndicator.SetActive(isOwned);
        }
        
        // Update not owned overlay
        if (notOwnedOverlay != null)
        {
            notOwnedOverlay.SetActive(!isOwned);
        }
        
        // Update rarity background
        if (rarityBackground != null && isOwned)
        {
            rarityBackground.color = GetRarityColor(collectibleData.rarity);
        }
        else if (rarityBackground != null)
        {
            rarityBackground.color = Color.gray;
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
    
    public void OnCollectibleClicked()
    {
        if (isOwned && collectibleData != null)
        {
            // Show full tooltip or details
            Debug.Log($"Collectible Details:\n{collectibleData.GetTooltip()}");
            // TODO: Could open a detailed view panel
        }
        else
        {
            Debug.Log("This collectible has not been discovered yet.");
        }
    }
    
    public bool IsOwned()
    {
        return isOwned;
    }
    
    public CollectibleData GetCollectibleData()
    {
        return collectibleData;
    }
} 