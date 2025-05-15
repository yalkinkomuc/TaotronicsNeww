using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AttributesUpgradePanel : MonoBehaviour
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
    [SerializeField] private TextMeshProUGUI mindCurrentValue; // Mind = Agility
    [SerializeField] private TextMeshProUGUI defenseCurrentValue;
    [SerializeField] private TextMeshProUGUI luckCurrentValue;
    
    [Header("Attribute New Values")]
    [SerializeField] private TextMeshProUGUI vitalityNewValue;
    [SerializeField] private TextMeshProUGUI mightNewValue;
    [SerializeField] private TextMeshProUGUI mindNewValue; // Mind = Agility
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
    [SerializeField] private TextMeshProUGUI attackValueText;
    [SerializeField] private TextMeshProUGUI speedValueText;
    [SerializeField] private TextMeshProUGUI defenseValueText;
    [SerializeField] private TextMeshProUGUI critRateValueText;
    
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
    private int tempAgility;
    private int tempDefense;
    private int tempLuck;
    private int tempSkillPoints;
    
    // Singleton pattern
    public static AttributesUpgradePanel instance;
    
    private void Awake()
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
            Debug.LogError("PlayerStats is null in AttributesUpgradePanel.Show()");
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
        tempAgility = playerStats.Agility;
        tempDefense = playerStats.Defense;
        tempLuck = playerStats.Luck;
        tempSkillPoints = playerStats.AvailableSkillPoints;
        
        Debug.Log($"Loaded attributes: Vit={tempVitality}, Might={tempMight}, Mind={tempAgility}, Def={tempDefense}, Luck={tempLuck}, SP={tempSkillPoints}");
    }
    
    private void UpdateAttributeUI()
    {
        if (playerStats == null) return;
        
        // Update current values
        if (vitalityCurrentValue) vitalityCurrentValue.text = playerStats.Vitality.ToString();
        if (mightCurrentValue) mightCurrentValue.text = playerStats.Might.ToString();
        if (mindCurrentValue) mindCurrentValue.text = playerStats.Agility.ToString();
        if (defenseCurrentValue) defenseCurrentValue.text = playerStats.Defense.ToString();
        if (luckCurrentValue) luckCurrentValue.text = playerStats.Luck.ToString();
        
        // Update new values
        if (vitalityNewValue) vitalityNewValue.text = tempVitality.ToString();
        if (mightNewValue) mightNewValue.text = tempMight.ToString();
        if (mindNewValue) mindNewValue.text = tempAgility.ToString();
        if (defenseNewValue) defenseNewValue.text = tempDefense.ToString();
        if (luckNewValue) luckNewValue.text = tempLuck.ToString();
        
        // Update points display
        if (pointsAvailableText) pointsAvailableText.text = tempSkillPoints.ToString();
    }
    
    private void UpdateStatPreview()
    {
        if (playerStats == null) return;
        
        // Calculate preview values based on temporary attributes
        float healthValue = CalculateHealth(tempVitality);
        float attackValue = CalculateAttack(tempMight);
        float speedValue = CalculateSpeed(tempAgility);
        float defenseValue = CalculateDefense(tempDefense);
        float critValue = CalculateCritRate(tempLuck);
        
        // Update UI
        if (healthValueText) healthValueText.text = healthValue.ToString("F0");
        if (attackValueText) attackValueText.text = attackValue.ToString("F1");
        if (speedValueText) speedValueText.text = speedValue.ToString("F0");
        if (defenseValueText) defenseValueText.text = defenseValue.ToString("F0");
        if (critRateValueText) critRateValueText.text = (critValue * 100).ToString("F1") + "%";
    }
    
    private float CalculateHealth(int vitality)
    {
        // Base health + vitality bonus
        float baseHealth = playerStats.maxHealth.GetBaseValue();
        float vitalityBonus = vitality * 30f; // HEALTH_PER_VITALITY constant
        
        return baseHealth + vitalityBonus;
    }
    
    private float CalculateAttack(int might)
    {
        // Base damage + might bonus
        float baseDamage = playerStats.baseDamage.GetBaseValue();
        float mightBonus = might * 2.5f; // DAMAGE_PER_MIGHT constant
        
        return baseDamage + mightBonus;
    }
    
    private float CalculateSpeed(int agility)
    {
        // Base speed + agility bonus
        return 300f + (agility * 6f); // Base speed + agility bonus
    }
    
    private float CalculateDefense(int defense)
    {
        // Defense calculation
        return defense * 3f; // DEFENSE_PER_POINT constant
    }
    
    private float CalculateCritRate(int luck)
    {
        // Critical rate calculation
        return luck * 0.01f; // CRIT_CHANCE_PER_LUCK constant (1% per point)
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
            case 2: // Mind (Agility)
                tempAgility++;
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
            case 2: // Mind (Agility)
                if (tempAgility > playerStats.Agility)
                {
                    tempAgility--;
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
        int agilityDiff = tempAgility - playerStats.Agility;
        int defenseDiff = tempDefense - playerStats.Defense;
        int luckDiff = tempLuck - playerStats.Luck;
        
        Debug.Log($"Applying changes: Vit +{vitalityDiff}, Might +{mightDiff}, Mind +{agilityDiff}, Def +{defenseDiff}, Luck +{luckDiff}");
        
        // Apply increases to actual player stats
        for (int i = 0; i < vitalityDiff; i++) playerStats.IncreaseVitality();
        for (int i = 0; i < mightDiff; i++) playerStats.IncreaseMight();
        for (int i = 0; i < agilityDiff; i++) playerStats.IncreaseAgility();
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
        }
    }
    
    public void OnAttributeButtonClicked(int index)
    {
        selectedAttributeIndex = index;
        UpdateSelectionIndicator();
    }
}