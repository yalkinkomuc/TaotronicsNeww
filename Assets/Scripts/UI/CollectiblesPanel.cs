using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class CollectiblesPanel : BaseUIPanel
{
    [Header("Collectibles UI")]
    [SerializeField] private Transform collectiblesGrid;
    [SerializeField] private GameObject collectiblePrefab;
    
    [Header("Lore Display")]
    [SerializeField] private TextMeshProUGUI loreText;
    [SerializeField] private GameObject lorePanel;
    
    private List<GameObject> collectibleUIItems = new List<GameObject>();
    private CollectibleData selectedCollectible;
    
    private void Start()
    {
        RefreshCollectibles();
        ShowDefaultLore();
    }
    
    private new void OnEnable()
    {
        RefreshCollectibles();
        ShowDefaultLore();
    }
    
    public void RefreshCollectibles()
    {
        if (Inventory.instance == null || collectiblesGrid == null || collectiblePrefab == null) 
            return;
            
        // Clear existing UI items
        ClearCollectibleUI();
        
        // Get all collectibles from inventory
        var collectibles = Inventory.instance.GetCollectibles();
        
        // Create UI item for each collectible
        foreach (var collectible in collectibles)
        {
            CreateCollectibleUI(collectible);
        }
    }
    
    private void CreateCollectibleUI(CollectibleData collectible)
    {
        GameObject uiItem = Instantiate(collectiblePrefab, collectiblesGrid);
        
        // Setup the UI item with collectible data
        CollectibleUIItem uiComponent = uiItem.GetComponent<CollectibleUIItem>();
        if (uiComponent != null)
        {
            uiComponent.Setup(collectible);
            
            // Add click event to show lore
            Button itemButton = uiItem.GetComponent<Button>();
            if (itemButton == null)
            {
                itemButton = uiItem.AddComponent<Button>();
            }
            
            itemButton.onClick.RemoveAllListeners();
            itemButton.onClick.AddListener(() => ShowCollectibleLore(collectible));
        }
        
        collectibleUIItems.Add(uiItem);
    }
    
    public void ShowCollectibleLore(CollectibleData collectible)
    {
        selectedCollectible = collectible;
        
        if (loreText != null && collectible != null)
        {
            if (!string.IsNullOrEmpty(collectible.loreText))
            {
                loreText.text = $"<b>{collectible.itemName}</b>\n\n<i>\"{collectible.loreText}\"</i>";
                
                if (!string.IsNullOrEmpty(collectible.discoveryLocation))
                {
                    loreText.text += $"\n\n<color=grey>Discovered in: {collectible.discoveryLocation}</color>";
                }
            }
            else
            {
                loreText.text = $"<b>{collectible.itemName}</b>\n\nNo lore available for this item.";
            }
        }
        
        // Show lore panel if it exists
        if (lorePanel != null)
        {
            lorePanel.SetActive(true);
        }
    }
    
    private void ShowDefaultLore()
    {
        if (loreText != null)
        {
            loreText.text = "Select a collectible to view its lore and history...";
        }
        
        // Hide lore panel initially if it exists
        if (lorePanel != null)
        {
            lorePanel.SetActive(false);
        }
    }
    
    private void ClearCollectibleUI()
    {
        foreach (var item in collectibleUIItems)
        {
            if (item != null)
            {
                DestroyImmediate(item);
            }
        }
        collectibleUIItems.Clear();
    }
} 