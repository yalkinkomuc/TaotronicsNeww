using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class CollectiblesPanel : BaseUIPanel
{
    [Header("Collectibles UI")]
    [SerializeField] private Transform collectiblesGrid;
    [SerializeField] private GameObject collectiblePrefab;
    
    private List<GameObject> collectibleUIItems = new List<GameObject>();
    
    private void Start()
    {
        RefreshCollectibles();
    }
    
    private void OnEnable()
    {
        RefreshCollectibles();
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
        }
        
        collectibleUIItems.Add(uiItem);
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