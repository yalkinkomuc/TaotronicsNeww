using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System;

public class UI_CollectionSet : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI setNameText;
    [SerializeField] private TextMeshProUGUI progressText;
    [SerializeField] private Image progressFill;
    [SerializeField] private Button selectButton;
    [SerializeField] private Image backgroundImage;
    
    [Header("Visual States")]
    [SerializeField] private Color selectedColor = Color.yellow;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color completeColor = Color.green;
    
    public event Action<string> OnCollectionSelected;
    
    private string setName;
    private int ownedCount;
    private int totalCount;
    private bool isSelected;
    
    public void Initialize(string collectionName, List<CollectibleData> ownedItems, CollectibleData[] allItems)
    {
        setName = collectionName;
        ownedCount = ownedItems.Count;
        totalCount = allItems.Length;
        
        UpdateDisplay();
        
        if (selectButton != null)
        {
            selectButton.onClick.AddListener(() => OnCollectionSelected?.Invoke(setName));
        }
    }
    
    private void UpdateDisplay()
    {
        if (setNameText != null)
            setNameText.text = setName;
        
        if (progressText != null)
            progressText.text = $"{ownedCount}/{totalCount}";
        
        if (progressFill != null)
        {
            float progress = totalCount > 0 ? (float)ownedCount / totalCount : 0f;
            progressFill.fillAmount = progress;
            
            // Change color based on completion
            if (ownedCount == totalCount)
                progressFill.color = completeColor;
            else
                progressFill.color = normalColor;
        }
        
        UpdateBackgroundColor();
    }
    
    public void SetSelected(bool selected)
    {
        isSelected = selected;
        UpdateBackgroundColor();
    }
    
    private void UpdateBackgroundColor()
    {
        if (backgroundImage != null)
        {
            if (isSelected)
                backgroundImage.color = selectedColor;
            else if (ownedCount == totalCount)
                backgroundImage.color = completeColor;
            else
                backgroundImage.color = normalColor;
        }
    }
    
    public string GetSetName()
    {
        return setName;
    }
    
    public bool IsComplete()
    {
        return ownedCount == totalCount;
    }
} 