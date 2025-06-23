using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class UI_CollectiblesPanel : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Transform collectionsParent;
    [SerializeField] private GameObject collectionSetPrefab;
    [SerializeField] private Transform collectibleDetailsParent;
    [SerializeField] private GameObject collectibleDetailPrefab;
    
    [Header("Collection Info")]
    [SerializeField] private TextMeshProUGUI totalCollectedText;
    [SerializeField] private TextMeshProUGUI totalAvailableText;
    [SerializeField] private Slider overallProgressSlider;
    
    [Header("Selected Collection Display")]
    [SerializeField] private TextMeshProUGUI selectedCollectionNameText;
    [SerializeField] private TextMeshProUGUI selectedCollectionProgressText;
    [SerializeField] private Image selectedCollectionIcon;
    [SerializeField] private TextMeshProUGUI selectedCollectionDescription;
    
    private List<UI_CollectionSet> collectionSets = new List<UI_CollectionSet>();
    private List<UI_CollectibleDetail> collectibleDetails = new List<UI_CollectibleDetail>();
    private string currentSelectedCollection = "";
    
    private void Start()
    {
        RefreshCollectionsDisplay();
    }
    
    public void RefreshCollectionsDisplay()
    {
        CreateCollectionSets();
        UpdateOverallProgress();
        
        // Show first collection by default
        if (collectionSets.Count > 0)
        {
            SelectCollection(collectionSets[0].GetSetName());
        }
    }
    
    private void CreateCollectionSets()
    {
        // Clear existing collection sets
        foreach (var set in collectionSets)
        {
            if (set != null && set.gameObject != null)
                Destroy(set.gameObject);
        }
        collectionSets.Clear();
        
        if (Inventory.instance == null) return;
        
        // Get all collectibles and group by set
        var collectibles = Inventory.instance.inventoryItems
            .Where(item => item.data is CollectibleData)
            .Select(item => item.data as CollectibleData)
            .ToList();
        
        // Group by set name
        var collectionGroups = collectibles
            .Where(c => !string.IsNullOrEmpty(c.setName))
            .GroupBy(c => c.setName)
            .ToList();
        
        // Add collections that have no items yet (from Resources)
        var allCollectibles = Resources.LoadAll<CollectibleData>("Items");
        var allSets = allCollectibles
            .Where(c => !string.IsNullOrEmpty(c.setName))
            .GroupBy(c => c.setName)
            .Where(g => !collectionGroups.Any(cg => cg.Key == g.Key))
            .ToList();
        
        // Create UI for existing collections
        foreach (var group in collectionGroups)
        {
            CreateCollectionSetUI(group.Key, group.ToList(), allCollectibles.Where(c => c.setName == group.Key).ToArray());
        }
        
        // Create UI for missing collections
        foreach (var group in allSets)
        {
            CreateCollectionSetUI(group.Key, new List<CollectibleData>(), group.ToArray());
        }
    }
    
    private void CreateCollectionSetUI(string setName, List<CollectibleData> ownedItems, CollectibleData[] allItemsInSet)
    {
        GameObject setObj = Instantiate(collectionSetPrefab, collectionsParent);
        UI_CollectionSet collectionSet = setObj.GetComponent<UI_CollectionSet>();
        
        if (collectionSet != null)
        {
            collectionSet.Initialize(setName, ownedItems, allItemsInSet);
            collectionSet.OnCollectionSelected += SelectCollection;
            collectionSets.Add(collectionSet);
        }
    }
    
    private void UpdateOverallProgress()
    {
        if (Inventory.instance == null) return;
        
        // Count total collectibles owned vs available
        var ownedCollectibles = Inventory.instance.inventoryItems
            .Where(item => item.data is CollectibleData)
            .Count();
        
        var allCollectibles = Resources.LoadAll<CollectibleData>("Items");
        var totalAvailable = allCollectibles.Length;
        
        // Update UI
        if (totalCollectedText != null)
            totalCollectedText.text = ownedCollectibles.ToString();
        
        if (totalAvailableText != null)
            totalAvailableText.text = totalAvailable.ToString();
        
        if (overallProgressSlider != null)
        {
            overallProgressSlider.value = totalAvailable > 0 ? (float)ownedCollectibles / totalAvailable : 0f;
        }
    }
    
    public void SelectCollection(string setName)
    {
        currentSelectedCollection = setName;
        ShowCollectionDetails(setName);
        
        // Update collection set visual states
        foreach (var set in collectionSets)
        {
            set.SetSelected(set.GetSetName() == setName);
        }
    }
    
    private void ShowCollectionDetails(string setName)
    {
        // Clear existing details
        foreach (var detail in collectibleDetails)
        {
            if (detail != null && detail.gameObject != null)
                Destroy(detail.gameObject);
        }
        collectibleDetails.Clear();
        
        // Get all items in this collection
        var allItemsInSet = Resources.LoadAll<CollectibleData>("Items")
            .Where(c => c.setName == setName)
            .ToArray();
        
        // Get owned items in this collection
        var ownedItems = new List<CollectibleData>();
        if (Inventory.instance != null)
        {
            ownedItems = Inventory.instance.inventoryItems
                .Where(item => item.data is CollectibleData collectible && collectible.setName == setName)
                .Select(item => item.data as CollectibleData)
                .ToList();
        }
        
        // Update collection header info
        if (selectedCollectionNameText != null)
            selectedCollectionNameText.text = setName;
        
        if (selectedCollectionProgressText != null)
            selectedCollectionProgressText.text = $"{ownedItems.Count} / {allItemsInSet.Length}";
        
        // Create detail entries for each item in the set
        foreach (var collectible in allItemsInSet)
        {
            GameObject detailObj = Instantiate(collectibleDetailPrefab, collectibleDetailsParent);
            UI_CollectibleDetail detail = detailObj.GetComponent<UI_CollectibleDetail>();
            
            if (detail != null)
            {
                bool isOwned = ownedItems.Any(owned => owned.itemName == collectible.itemName);
                detail.Initialize(collectible, isOwned);
                collectibleDetails.Add(detail);
            }
        }
    }
    
    public void OnCollectionSetClicked(string setName)
    {
        SelectCollection(setName);
    }
} 