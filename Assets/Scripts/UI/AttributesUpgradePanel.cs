using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AttributesUpgradePanel : BaseUIPanel
{
    [Header("UI Elements")]
    [SerializeField] private GameObject attributeUpgradeUI;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button upgradeButton;
    [SerializeField] private Button resetButton;
    
    [Header("Attribute Selection")]
    [SerializeField] private Image selectionIndicator;
    [SerializeField] private int selectedAttributeIndex = 0;
    
    [Header("Attribute Current Values")]
    [SerializeField] private TextMeshProUGUI vitalityCurrentValue;
    [SerializeField] private TextMeshProUGUI mightCurrentValue;
    [SerializeField] private TextMeshProUGUI mindCurrentValue;
    [SerializeField] private TextMeshProUGUI defenseCurrentValue;
    [SerializeField] private TextMeshProUGUI luckCurrentValue;
    
    [Header("Attribute New Values")]
    [SerializeField] private TextMeshProUGUI vitalityNewValue;
    [SerializeField] private TextMeshProUGUI mightNewValue;
    [SerializeField] private TextMeshProUGUI mindNewValue;
    [SerializeField] private TextMeshProUGUI defenseNewValue;
    [SerializeField] private TextMeshProUGUI luckNewValue;
    
    [Header("Attribute Buttons")]
    [SerializeField] private Button vitalityButton;
    [SerializeField] private Button mightButton;
    [SerializeField] private Button mindButton;
    [SerializeField] private Button defenseButton;
    [SerializeField] private Button luckButton;
    
    [Header("Stat Preview Fields")]
    [SerializeField] private TextMeshProUGUI healthValueText;
    [SerializeField] private TextMeshProUGUI manaValueText;
    [SerializeField] private TextMeshProUGUI attackValueText;
    [SerializeField] private TextMeshProUGUI speedValueText;
    [SerializeField] private TextMeshProUGUI defenseValueText;
    [SerializeField] private TextMeshProUGUI critRateValueText;
    [SerializeField] private TextMeshProUGUI magicPowerValueText;
    
    [Header("Points Display")]
    [SerializeField] private TextMeshProUGUI pointsAvailableText;
    [SerializeField] private TextMeshProUGUI levelText;
    
    // Reference to all attribute buttons
    private List<Button> attributeButtons = new List<Button>();
    
    private PlayerStats playerStats;
    private bool isVisible = false;
    
    // Temporary values for preview
    private int tempVitality;
    private int tempMight;
    private int tempMind;
    private int tempDefense;
    private int tempLuck;
    private int tempSkillPoints;
    
    // Singleton pattern
    public static AttributesUpgradePanel instance;
    
    private new void Awake()
    {
        // Set singleton instance
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        // Initialize UI
        if (attributeUpgradeUI)
            attributeUpgradeUI.SetActive(false);
            
        if (closeButton)
            closeButton.onClick.AddListener(ClosePanel);
            
        if (upgradeButton)
            upgradeButton.onClick.AddListener(ApplyChanges);
            
        if (resetButton)
            resetButton.onClick.AddListener(ResetAttributes);
        
        // Add buttons to list for easier access
        attributeButtons.Clear();
        if (vitalityButton) attributeButtons.Add(vitalityButton);
        if (mightButton) attributeButtons.Add(mightButton);
        if (mindButton) attributeButtons.Add(mindButton);
        if (defenseButton) attributeButtons.Add(defenseButton);
        if (luckButton) attributeButtons.Add(luckButton);
        
        // Add click listeners
        for (int i = 0; i < attributeButtons.Count; i++)
        {
            int index = i; // Local copy for closure
            attributeButtons[i].onClick.AddListener(() => OnAttributeButtonClicked(index));
        }
    }
    
    private void Update()
    {
        if (!isVisible) return;
        
        // Handle keyboard input for attribute selection and modification
        HandleAttributeSelection();
        HandleAttributeModification();
        
        // Check for confirmation or reset
        if (Input.GetKeyDown(KeyCode.F))
        {
            ApplyChanges();
            ClosePanel();
            
        }
        else if (Input.GetKeyDown(KeyCode.R))
        {
            ResetAttributes();
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            CancelChanges();
            ClosePanel();
        }
    }
    
    public void Show(PlayerStats stats)
    {
        playerStats = stats;
        
        if (playerStats == null)
        {
            return;
        }
        
        // Load current attribute values
        LoadCurrentAttributes();
        
        // Update UI
        UpdateAttributeUI();
        UpdateStatPreview();
        
        // Show panel
        if (attributeUpgradeUI)
            attributeUpgradeUI.SetActive(true);
        isVisible = true;
        
        // Block game input
        UIInputBlocker inputBlocker = FindInputBlocker();
        if (inputBlocker != null)
        {
            inputBlocker.AddPanel(gameObject);
            inputBlocker.EnableGameplayInput(false);
            Debug.Log("AttributesUpgradePanel: Gameplay input DISABLED");
        }
        else
        {
            Debug.LogError("AttributesUpgradePanel: UIInputBlocker not found! Gameplay input NOT disabled!");
        }
        
        // Default selection
        selectedAttributeIndex = 0;
        UpdateSelectionIndicator();
        
        // Update level display
        if (levelText) levelText.text = playerStats.GetLevel().ToString();
        
        Debug.Log($"AttributesUpgradePanel opened - Level: {playerStats.GetLevel()}, SP: {playerStats.AvailableSkillPoints}");
    }
    
    private UIInputBlocker FindInputBlocker()
    {
        if (UIInputBlocker.instance != null)
            return UIInputBlocker.instance;
            
        UIInputBlocker foundBlocker = FindFirstObjectByType<UIInputBlocker>();
        return foundBlocker;
    }
    
    private void LoadCurrentAttributes()
    {
        if (playerStats == null)
        {
            Debug.LogError("PlayerStats is null in LoadCurrentAttributes");
            return;
        }
        
        tempVitality = playerStats.Vitality;
        tempMight = playerStats.Might;
        tempMind = playerStats.Mind;
        tempDefense = playerStats.Defense;
        tempLuck = playerStats.Luck;
        tempSkillPoints = playerStats.AvailableSkillPoints;
        
        Debug.Log($"Loaded attributes: Vit={tempVitality}, Might={tempMight}, Mind={tempMind}, Def={tempDefense}, Luck={tempLuck}, SP={tempSkillPoints}");
    }
    
    private void UpdateAttributeUI()
    {
        if (playerStats == null) return;
        
        // Update current values
        if (vitalityCurrentValue) vitalityCurrentValue.text = playerStats.Vitality.ToString();
        if (mightCurrentValue) mightCurrentValue.text = playerStats.Might.ToString();
        if (mindCurrentValue) mindCurrentValue.text = playerStats.Mind.ToString();
        if (defenseCurrentValue) defenseCurrentValue.text = playerStats.Defense.ToString();
        if (luckCurrentValue) luckCurrentValue.text = playerStats.Luck.ToString();
        
        // Update new values
        if (vitalityNewValue) vitalityNewValue.text = tempVitality.ToString();
        if (mightNewValue) mightNewValue.text = tempMight.ToString();
        if (mindNewValue) mindNewValue.text = tempMind.ToString();
        if (defenseNewValue) defenseNewValue.text = tempDefense.ToString();
        if (luckNewValue) luckNewValue.text = tempLuck.ToString();
        
        // Update points display
        if (pointsAvailableText) pointsAvailableText.text = tempSkillPoints.ToString();
    }
    
    private void UpdateStatPreview()
    {
        if (playerStats == null) return;
        
        // Health preview: vitality arttıkça anlık güncellensin
        float healthValue = CalculateHealth(tempVitality);
        float elementalPower = CalculateElementalMultiplier(tempMind);
        float defenseValue = CalculateDefense(tempDefense);
        float critValue = CalculateCritRate(tempLuck);
        
        // Calculate attack damage range string using PlayerStats method (same as AdvancedInventoryUI)
        string attackRangeString = playerStats.GetDamageRangeWithCriticalString(tempMight);
        
        // Update UI
        if (healthValueText) healthValueText.text = healthValue.ToString("F0");
        if (manaValueText) manaValueText.text = "-";
        if (attackValueText) attackValueText.text = attackRangeString;
        if (speedValueText) speedValueText.text = "-";
        if (defenseValueText) defenseValueText.text = defenseValue.ToString("F0");
        if (critRateValueText) critRateValueText.text = (critValue * 100).ToString("F1") + "%";
        if (magicPowerValueText) magicPowerValueText.text = $"{((elementalPower-1f)*100f):F0}%";
    }
    
    private float CalculateHealth(int vitality)
    {
        if (playerStats == null)
            return 0f;
        
        // Base health
        float baseHealth = playerStats.maxHealth.GetBaseValue();
        // Vitality bonus
        float healthMultiplier = Mathf.Pow(1 + 0.08f, vitality) - 1; // HEALTH_GROWTH = 0.08f
        float bonus = baseHealth * healthMultiplier;
        // Sadece base health + vitality bonusu
        return baseHealth + bonus;
    }
    

    
    private float CalculateDefense(int defense)
    {
        // Hasar azaltma formülü: Her 1 defans puanı için %1 hasar azaltma
        // (Aynı formülün hem gösterimde hem de gerçek hasarda kullanıldığından emin ol)
        float reduction = Mathf.Min(defense, 80);
        return reduction;
    }
    
    private float CalculateCritRate(int luck)
    {
        // Kritik şansı doğrusal artış (her puan %1)
        return luck * 0.01f; // CRIT_CHANCE_PER_LUCK = 0.01f
    }
    
    private float CalculateElementalMultiplier(int mind)
    {
        // Elemental multiplier calculation
        return 1f + (mind * 0.01f); // ELEMENTAL_MULTIPLIER_PER_MIND = 0.01f
    }
    

    
    private void HandleAttributeSelection()
    {
        // Move selection up/down with arrow keys
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            selectedAttributeIndex--;
            if (selectedAttributeIndex < 0)
                selectedAttributeIndex = attributeButtons.Count - 1;
                
            UpdateSelectionIndicator();
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            selectedAttributeIndex++;
            if (selectedAttributeIndex >= attributeButtons.Count)
                selectedAttributeIndex = 0;
                
            UpdateSelectionIndicator();
        }
    }
    
    private void HandleAttributeModification()
    {
        // Increase/decrease selected attribute with left/right arrow keys
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            IncreaseSelectedAttribute();
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            DecreaseSelectedAttribute();
        }
    }
    
    private void IncreaseSelectedAttribute()
    {
        if (tempSkillPoints <= 0) return;
        
        switch (selectedAttributeIndex)
        {
            case 0: // Vitality
                tempVitality++;
                break;
            case 1: // Might
                tempMight++;
                break;
            case 2: // Mind
                tempMind++;
                break;
            case 3: // Defense
                tempDefense++;
                break;
            case 4: // Luck
                tempLuck++;
                break;
        }
        
        tempSkillPoints--;
        UpdateAttributeUI();
        UpdateStatPreview();
    }
    
    private void DecreaseSelectedAttribute()
    {
        bool decreased = false;
        
        switch (selectedAttributeIndex)
        {
            case 0: // Vitality
                if (tempVitality > playerStats.Vitality)
                {
                    tempVitality--;
                    decreased = true;
                }
                break;
            case 1: // Might
                if (tempMight > playerStats.Might)
                {
                    tempMight--;
                    decreased = true;
                }
                break;
            case 2: // Mind
                if (tempMind > playerStats.Mind)
                {
                    tempMind--;
                    decreased = true;
                }
                break;
            case 3: // Defense
                if (tempDefense > playerStats.Defense)
                {
                    tempDefense--;
                    decreased = true;
                }
                break;
            case 4: // Luck
                if (tempLuck > playerStats.Luck)
                {
                    tempLuck--;
                    decreased = true;
                }
                break;
        }
        
        if (decreased)
        {
            tempSkillPoints++;
            UpdateAttributeUI();
            UpdateStatPreview();
        }
    }
    
    private void UpdateSelectionIndicator()
    {
        if (selectionIndicator == null || attributeButtons.Count <= selectedAttributeIndex || selectedAttributeIndex < 0) return;
        
        // Move indicator to selected button
        RectTransform buttonRect = attributeButtons[selectedAttributeIndex].GetComponent<RectTransform>();
        if (buttonRect != null)
        {
            selectionIndicator.rectTransform.position = buttonRect.position;
            selectionIndicator.gameObject.SetActive(true);
        }
    }
    
    private void ApplyChanges()
    {
        if (playerStats == null) return;
        
        // Calculate differences
        int vitalityDiff = tempVitality - playerStats.Vitality;
        int mightDiff = tempMight - playerStats.Might;
        int mindDiff = tempMind - playerStats.Mind;
        int defenseDiff = tempDefense - playerStats.Defense;
        int luckDiff = tempLuck - playerStats.Luck;
        
        Debug.Log($"Applying changes: Vit +{vitalityDiff}, Might +{mightDiff}, Mind +{mindDiff}, Def +{defenseDiff}, Luck +{luckDiff}");
        
        // Apply increases to actual player stats
        // Each method already properly updates health percentage and health bar UI
        for (int i = 0; i < vitalityDiff; i++) playerStats.IncreaseVitality();
        for (int i = 0; i < mightDiff; i++) playerStats.IncreaseMight();
        for (int i = 0; i < mindDiff; i++) playerStats.IncreaseMind();
        for (int i = 0; i < defenseDiff; i++) playerStats.IncreaseDefense();
        for (int i = 0; i < luckDiff; i++) playerStats.IncreaseLuck();
        
        // Close panel after applying changes
        ClosePanel();
    }
    
    private void ResetAttributes()
    {
        if (playerStats == null) return;
        
        // Reset player attributes
        playerStats.ResetAllAttributes();
        
        // Update temporary values
        LoadCurrentAttributes();
        UpdateAttributeUI();
        UpdateStatPreview();
        
        Debug.Log("Attributes reset successfully");
    }
    
    private void CancelChanges()
    {
        // Just load current values without applying changes
        LoadCurrentAttributes();
        UpdateAttributeUI();
        UpdateStatPreview();
        
        Debug.Log("Changes canceled");
    }
    
    public void ClosePanel()
    {
        if (attributeUpgradeUI)
            attributeUpgradeUI.SetActive(false);
        isVisible = false;
        
        // Unblock game input
        UIInputBlocker inputBlocker = FindInputBlocker();
        if (inputBlocker != null)
        {
            inputBlocker.RemovePanel(gameObject);
            inputBlocker.EnableGameplayInput(true);
            Debug.Log("AttributesUpgradePanel: Gameplay input ENABLED");
        }
        else
        {
            Debug.LogError("AttributesUpgradePanel: UIInputBlocker not found! Gameplay input NOT enabled!");
        }
    }
    
    public void OnAttributeButtonClicked(int index)
    {
        selectedAttributeIndex = index;
        UpdateSelectionIndicator();
    }
}