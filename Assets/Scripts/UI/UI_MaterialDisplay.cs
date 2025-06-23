using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class UI_MaterialDisplay : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image materialIcon;
    [SerializeField] private TextMeshProUGUI countText;
    [SerializeField] private Image backgroundImage;
    
    [Header("Configuration")]
    [SerializeField] private Sprite defaultMaterialSprite;
    [SerializeField] private Color hasItemsColor = Color.white;
    [SerializeField] private Color noItemsColor = new Color(1, 1, 1, 0.3f);
    
    private MaterialType materialType;
    private int currentCount = 0;
    
    public void SetMaterialType(MaterialType type)
    {
        materialType = type;
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
            // Try to find a material of this type to get its icon
            Sprite materialSprite = GetMaterialSprite();
            
            if (materialSprite != null)
            {
                materialIcon.sprite = materialSprite;
            }
            else
            {
                materialIcon.sprite = defaultMaterialSprite;
            }
            
            // Set color based on whether we have any
            materialIcon.color = currentCount > 0 ? hasItemsColor : noItemsColor;
        }
        
        // Update background based on material rarity/type
        if (backgroundImage != null)
        {
            backgroundImage.color = GetMaterialTypeColor();
        }
    }
    
    private Sprite GetMaterialSprite()
    {
        if (Inventory.instance == null) return null;
        
        // Find the first material of this type and get its icon
        foreach (var inventoryItem in Inventory.instance.inventoryItems)
        {
            if (inventoryItem.data is UpgradeMaterialData material && 
                material.materialType == materialType)
            {
                return material.icon;
            }
        }
        
        // If not found in inventory, try to load from Resources
        var allMaterials = Resources.LoadAll<UpgradeMaterialData>("Items");
        var foundMaterial = allMaterials.FirstOrDefault(m => m.materialType == materialType);
        
        return foundMaterial?.icon;
    }
    
    private Color GetMaterialTypeColor()
    {
        // Single color for all material types
        return Color.gray;
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