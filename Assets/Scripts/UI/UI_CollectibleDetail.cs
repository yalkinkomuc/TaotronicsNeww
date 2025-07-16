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
        if (collectibleData == null) 
        {
            Debug.LogError("CollectibleData is NULL!");
            return;
        }
        
        // Update icon
        if (collectibleIcon != null && collectibleData.icon != null)
        {
            collectibleIcon.sprite = collectibleData.icon;
        }
        else
        {
            Debug.LogError($"Icon problem - collectibleIcon: {(collectibleIcon != null ? "OK" : "NULL")}, data.icon: {(collectibleData.icon != null ? "OK" : "NULL")}");
        }
        
        // Update name
        if (collectibleNameText != null && !string.IsNullOrEmpty(collectibleData.itemName))
        {
            collectibleNameText.text = collectibleData.itemName;
        }
        else
        {
            Debug.LogError($"Name problem - nameText: {(collectibleNameText != null ? "OK" : "NULL")}, itemName: '{collectibleData.itemName}'");
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