using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class UI_CollectiblesPanel : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Transform collectibleListParent;
    [SerializeField] private GameObject collectibleItemPrefab;
    
    [Header("Collection Info")]
    [SerializeField] private TextMeshProUGUI totalCollectedText;
    [SerializeField] private TextMeshProUGUI totalAvailableText;
    [SerializeField] private Slider overallProgressSlider;
    
    private List<UI_CollectibleDetail> collectibleItems = new List<UI_CollectibleDetail>();
    
    private void Start()
    {
        // Panel açıldığında otomatik refresh yapmıyoruz
        // Sadece inventory değiştiğinde refresh olacak
        UpdateOverallProgress();
    }
    
    public void RefreshCollectiblesDisplay()
    {
        CreateCollectiblesList();
        UpdateOverallProgress();
    }
    
    private void CreateCollectiblesList()
    {
        // Clear existing collectible items
        foreach (var item in collectibleItems)
        {
            if (item != null && item.gameObject != null)
                Destroy(item.gameObject);
        }
        collectibleItems.Clear();
        
        if (Inventory.instance == null) return;
        
        // Get ONLY owned collectibles from inventory
        var ownedCollectibles = Inventory.instance.inventoryItems
            .Where(item => item.data is CollectibleData)
            .Select(item => item.data as CollectibleData)
            .ToList();
        
        // Create UI only for owned collectibles
        foreach (var collectible in ownedCollectibles)
        {
            GameObject itemObj = Instantiate(collectibleItemPrefab, collectibleListParent);
            UI_CollectibleDetail collectibleDetail = itemObj.GetComponent<UI_CollectibleDetail>();
            
            if (collectibleDetail != null)
            {
                collectibleDetail.Initialize(collectible);
                collectibleItems.Add(collectibleDetail);
            }
            else
            {
                Debug.LogError("UI_CollectibleDetail component NOT FOUND on prefab!");
            }
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
} 