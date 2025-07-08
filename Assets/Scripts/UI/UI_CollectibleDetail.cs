using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_CollectibleDetail : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image collectibleIcon;
    [SerializeField] private TextMeshProUGUI collectibleNameText;
    
    private CollectibleData collectibleData;
    
    public void Initialize(CollectibleData collectible)
    {
        collectibleData = collectible;
        UpdateDisplay();
    }
    
    private void UpdateDisplay()
    {
        if (collectibleData == null) return;
        
        // Update icon
        if (collectibleIcon != null)
        {
            collectibleIcon.sprite = collectibleData.icon;
        }
        
        // Update name
        if (collectibleNameText != null)
        {
            collectibleNameText.text = collectibleData.itemName;
        }
    }
    

    
    public void OnCollectibleClicked()
    {
        if (collectibleData != null)
        {
            Debug.Log($"Clicked collectible: {collectibleData.itemName}");
            // TODO: Future functionality can be added here
        }
    }
    

    
    public CollectibleData GetCollectibleData()
    {
        return collectibleData;
    }
} 