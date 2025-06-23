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
        // Return color based on material type
        return materialType switch
        {
            MaterialType.Leather => new Color(0.6f, 0.4f, 0.2f),        // Brown
            MaterialType.Iron => Color.gray,                             // Gray
            MaterialType.Steel => new Color(0.7f, 0.7f, 0.8f),         // Light gray
            MaterialType.Mithril => new Color(0.8f, 0.9f, 1f),         // Light blue
            MaterialType.Adamantine => new Color(0.2f, 0.2f, 0.3f),    // Dark blue
            MaterialType.Crystal => new Color(1f, 0.9f, 1f),           // Light pink
            MaterialType.Gem => new Color(1f, 0.2f, 0.6f),             // Pink
            MaterialType.Essence => new Color(0.6f, 0.3f, 0.9f),       // Purple
            MaterialType.Rune_Fragment => new Color(0.3f, 0.6f, 0.9f), // Blue
            MaterialType.Dragon_Scale => new Color(0.9f, 0.1f, 0.1f),  // Red
            MaterialType.Phoenix_Feather => new Color(1f, 0.5f, 0f),   // Orange
            MaterialType.Void_Shard => new Color(0.1f, 0.1f, 0.1f),    // Almost black
            _ => Color.gray
        };
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