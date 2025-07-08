using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class UI_MaterialDisplay : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image materialIcon;
    [SerializeField] private TextMeshProUGUI countText;
    
    [Header("Configuration")]
    [SerializeField] private Sprite defaultMaterialSprite;
    [SerializeField] private Color hasItemsColor = Color.white;
    [SerializeField] private Color noItemsColor = new Color(1, 1, 1, 0.3f);
    
    private MaterialType materialType;
    private int currentCount = 0;
    private Sprite cachedMaterialSprite;
    
    public void SetMaterialType(MaterialType type)
    {
        materialType = type;
        
        // Cache the material sprite at initialization
        cachedMaterialSprite = LoadMaterialSprite();
        
        // Set initial display with 0 count
        UpdateDisplay();
    }
    
    public void UpdateCount()
    {
        if (Inventory.instance == null)
        {
            currentCount = 0;
            UpdateDisplay();
            return;
        }
        
        // Count all upgrade materials of this type
        currentCount = 0;
        
        foreach (var inventoryItem in Inventory.instance.inventoryItems)
        {
            if (inventoryItem.data is UpgradeMaterialData material && 
                material.materialType == materialType)
            {
                currentCount += inventoryItem.stackSize;
            }
        }
        
        UpdateDisplay();
    }
    
    private void UpdateDisplay()
    {
        // Update count text
        if (countText != null)
        {
            countText.text = currentCount.ToString();
        }
        
        // Update material icon
        if (materialIcon != null)
        {
            // Use cached sprite
            if (cachedMaterialSprite != null)
            {
                materialIcon.sprite = cachedMaterialSprite;
            }
            else
            {
                materialIcon.sprite = defaultMaterialSprite;
            }
            
            // Set color based on whether we have any
            materialIcon.color = currentCount > 0 ? hasItemsColor : noItemsColor;
        }
        

    }
    
    private Sprite LoadMaterialSprite()
    {
        // Load material sprite from Resources at initialization
        var allMaterials = Resources.LoadAll<UpgradeMaterialData>("Items");
        var foundMaterial = allMaterials.FirstOrDefault(m => m.materialType == materialType);
        
        return foundMaterial?.icon;
    }
    

    
    public MaterialType GetMaterialType()
    {
        return materialType;
    }
    
    public int GetCount()
    {
        return currentCount;
    }
    
    // Optional: Add click functionality to show material details
    public void OnMaterialClicked()
    {
        Debug.Log($"{materialType}: {currentCount} in inventory");
        // TODO: Could show a detailed material info panel
    }
} 