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
        Debug.Log($"Initialize called with collectible: {(collectible != null ? collectible.name : "NULL")}");
        UpdateDisplay();
    }
    
    private void UpdateDisplay()
    {
        Debug.Log($"UpdateDisplay called - collectibleData: {(collectibleData != null ? collectibleData.name : "NULL")}");
        
        if (collectibleData == null) 
        {
            Debug.LogError("CollectibleData is NULL!");
            return;
        }
        
        Debug.Log($"CollectibleData found: {collectibleData.itemName}");
        Debug.Log($"Icon: {(collectibleData.icon != null ? collectibleData.icon.name : "NULL")}");
        
        // Update icon
        if (collectibleIcon != null && collectibleData.icon != null)
        {
            collectibleIcon.sprite = collectibleData.icon;
            Debug.Log($"Icon set to: {collectibleData.icon.name}");
        }
        else
        {
            Debug.LogError($"Icon problem - collectibleIcon: {(collectibleIcon != null ? "OK" : "NULL")}, data.icon: {(collectibleData.icon != null ? "OK" : "NULL")}");
        }
        
        // Update name
        if (collectibleNameText != null && !string.IsNullOrEmpty(collectibleData.itemName))
        {
            collectibleNameText.text = collectibleData.itemName;
            Debug.Log($"Name set to: {collectibleData.itemName}");
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