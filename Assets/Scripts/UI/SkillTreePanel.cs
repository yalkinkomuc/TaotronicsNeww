using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class SkillTreePanel : MonoBehaviour
{
    [Header("Skill Panel References")]
    [SerializeField] private GameObject skillScreenPanel;
    [SerializeField] private Button unlockSkillButton;

    [Header("Skill Screen UI Elements")]
    [SerializeField] private Button resetButton;
    [SerializeField] private Button applyButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI shardCountText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI costText;
    
    [Header("Fire Element Skills")]
    [SerializeField] private Button fireSkill1Button; // Fire Spell
    [SerializeField] private Button fireSkill2Button; // Fireball Spell
    [SerializeField] private Button fireSkill3Button; // Future Fire Skill
    
    [Header("Ice Element Skills")]
    [SerializeField] private Button iceSkill1Button; // Ice Shard
    [SerializeField] private Button iceSkill2Button; // Future Ice Skill
    [SerializeField] private Button iceSkill3Button; // Future Ice Skill
    
    [Header("Earth Element Skills")]
    [SerializeField] private Button earthSkill1Button; // Earth Push
    [SerializeField] private Button earthSkill2Button; // Future Earth Skill
    [SerializeField] private Button earthSkill3Button; // Future Earth Skill
    
    [Header("Electric Element Skills")]
    [SerializeField] private Button electricSkill1Button; // Electric Dash
    [SerializeField] private Button electricSkill2Button; // Future Electric Skill
    [SerializeField] private Button electricSkill3Button; // Future Electric Skill
    
    [Header("Air Element Skills")]
    [SerializeField] private Button airSkill1Button; // Air Push
    [SerializeField] private Button airSkill2Button; // Future Air Skill
    [SerializeField] private Button airSkill3Button; // Future Air Skill
    
    [Header("Void Element Skills")]
    [SerializeField] private Button voidSkill1Button; // Void Skill
    [SerializeField] private Button voidSkill2Button; // Future Void Skill
    [SerializeField] private Button voidSkill3Button; // Future Void Skill
    
    // Skill tree structure
    [System.Serializable]
    public class SkillNodeData
    {
        public string skillID;
        public string skillName;
        public string description;
        public int shardCost;
        public string prerequisiteSkillID; // Hangi skill açık olmalı
        public Button button;
    }
    
    private Dictionary<string, SkillNodeData> skillNodes = new Dictionary<string, SkillNodeData>();
    private string selectedSkillID;
    
    private void Awake()
    {
        if (gameObject.activeSelf)
        {
            gameObject.SetActive(false);
        }
        
        if (UIInputBlocker.instance != null)
        {
            UIInputBlocker.instance.AddPanel(gameObject);
            if (skillScreenPanel != null)
                UIInputBlocker.instance.AddPanel(skillScreenPanel);
        }
        
        InitializeSkillNodes();
    }
    
    private void Start()
    {
        SetupButtons();
        UpdateShardCount();
    }
    
    private void InitializeSkillNodes()
    {
        // Fire Element Skills
        skillNodes["FireSpell"] = new SkillNodeData
        {
            skillID = "FireSpell",
            skillName = "Fire Spell",
            description = "Continuous fire spell that burns enemies over time.",
            shardCost = 30,
            prerequisiteSkillID = null, // Level 1 skill, no prerequisite
            button = fireSkill1Button
        };
        
        skillNodes["fireball_spell"] = new SkillNodeData
        {
            skillID = "fireball_spell", 
            skillName = "Fireball Spell",
            description = "Shoots a powerful fireball projectile.",
            shardCost = 50,
            prerequisiteSkillID = "FireSpell", // Requires Fire Spell
            button = fireSkill2Button
        };
        
        // Ice Element Skills
        skillNodes["ice_shard"] = new SkillNodeData
        {
            skillID = "ice_shard",
            skillName = "Ice Shard",
            description = "Creates ice shards that emerge from the ground.",
            shardCost = 25,
            prerequisiteSkillID = null,
            button = iceSkill1Button
        };
        
        // Earth Element Skills
        skillNodes["earth_push"] = new SkillNodeData
        {
            skillID = "earth_push",
            skillName = "Earth Push", 
            description = "Creates an earth wave that pushes enemies.",
            shardCost = 35,
            prerequisiteSkillID = null,
            button = earthSkill1Button
        };
        
        // Electric Element Skills
        skillNodes["electric_dash"] = new SkillNodeData
        {
            skillID = "electric_dash",
            skillName = "Electric Dash",
            description = "Lightning-fast dash that damages enemies.",
            shardCost = 40,
            prerequisiteSkillID = null,
            button = electricSkill1Button
        };
        
        // Air Element Skills
        skillNodes["air_push"] = new SkillNodeData
        {
            skillID = "air_push",
            skillName = "Air Push",
            description = "Creates a gust of wind that pushes enemies.",
            shardCost = 20,
            prerequisiteSkillID = null,
            button = airSkill1Button
        };
        
        // Void Element Skills
        skillNodes["void_skill"] = new SkillNodeData
        {
            skillID = "void_skill",
            skillName = "Void Disappear",
            description = "Become invisible and intangible for a short time.",
            shardCost = 60,
            prerequisiteSkillID = null,
            button = voidSkill1Button
        };
    }
    
    private void SetupButtons()
    {
        if (unlockSkillButton != null)
            unlockSkillButton.onClick.AddListener(OpenSkillPanel);
        
        if (resetButton != null)
            resetButton.onClick.AddListener(ResetSelection);
        
        if (applyButton != null)
        {
            applyButton.onClick.AddListener(UnlockSelectedSkill);
            applyButton.interactable = false;
        }
        
        if (closeButton != null)
            closeButton.onClick.AddListener(ClosePanel);
        
        // Setup skill buttons
        foreach (var skillNode in skillNodes.Values)
        {
            if (skillNode.button != null)
            {
                string skillID = skillNode.skillID; // Local variable for closure
                skillNode.button.onClick.AddListener(() => SelectSkill(skillID));
            }
        }
    }
    
    public void OpenPanel()
    {
        gameObject.SetActive(true);
    }
    
    private void OpenSkillPanel()
    {
        if (skillScreenPanel != null)
        {
            skillScreenPanel.SetActive(true);
            UpdateShardCount();
            UpdateAllSkillStatus();
            ResetSelection();
            
            if (UIInputBlocker.instance != null)
            {
                UIInputBlocker.instance.AddPanel(skillScreenPanel.gameObject);
                UIInputBlocker.instance.DisableGameplayInput();
            }
        }
    }
    
    private void CloseSkillPanel()
    {
        if (skillScreenPanel != null)
        {
            skillScreenPanel.gameObject.SetActive(false);
            
            if (UIInputBlocker.instance != null)
            {
                UIInputBlocker.instance.RemovePanel(skillScreenPanel.gameObject);
                UIInputBlocker.instance.EnableGameplayInput(true);
            }
        }
    }
    
    public void ClosePanel()
    {
        CloseSkillPanel();
        gameObject.SetActive(false);
    }
    
    private void SelectSkill(string skillID)
    {
        if (!skillNodes.ContainsKey(skillID))
            return;
            
        selectedSkillID = skillID;
        var skillData = skillNodes[skillID];
        
        // Update UI texts
        if (titleText != null)
            titleText.text = $"Acquire {skillData.skillName}";
            
        if (descriptionText != null)
            descriptionText.text = skillData.description;
            
        if (costText != null)
            costText.text = $"Cost: {skillData.shardCost} Shards";
        
        // Update button selection outlines
        UpdateSelectionOutlines();
        
        // Update apply button
        UpdateApplyButton();
    }
    
    private void UpdateSelectionOutlines()
    {
        foreach (var skillNode in skillNodes.Values)
        {
            if (skillNode.button != null)
            {
                Transform outline = skillNode.button.transform.Find("SelectedOutline");
                if (outline != null)
                    outline.gameObject.SetActive(skillNode.skillID == selectedSkillID);
            }
        }
    }
    
    private void ResetSelection()
    {
        selectedSkillID = null;
        
        if (titleText != null)
            titleText.text = "Acquire a new skill.";
            
        if (descriptionText != null)
            descriptionText.text = "Select a skill to see its description.";
            
        if (costText != null)
            costText.text = "";
        
        UpdateSelectionOutlines();
        
        if (applyButton != null)
            applyButton.interactable = false;
    }
    
    private void UnlockSelectedSkill()
    {
        if (string.IsNullOrEmpty(selectedSkillID) || SkillManager.Instance == null)
            return;
            
        if (!skillNodes.ContainsKey(selectedSkillID))
            return;
        
        var skillData = skillNodes[selectedSkillID];
        bool success = SkillManager.Instance.UnlockSkill(selectedSkillID, skillData.shardCost);
        
        if (success)
        {
            Debug.Log($"Skill unlocked: {selectedSkillID}");
            UpdateShardCount();
            UpdateAllSkillStatus();
            UpdateApplyButton();
        }
        else
        {
            Debug.LogWarning($"Failed to unlock skill: {selectedSkillID}");
        }
    }
    
    private void UpdateShardCount()
    {
        if (shardCountText != null && SkillManager.Instance != null)
        {
            shardCountText.text = SkillManager.Instance.GetShardCount().ToString();
        }
    }
    
    private void UpdateAllSkillStatus()
    {
        if (SkillManager.Instance == null)
            return;
        
        foreach (var skillNode in skillNodes.Values)
        {
            if (skillNode.button != null)
            {
                UpdateSkillButtonStatus(skillNode);
            }
        }
    }
    
    private void UpdateSkillButtonStatus(SkillNodeData skillData)
    {
        bool isUnlocked = SkillManager.Instance.IsSkillUnlocked(skillData.skillID);
        bool canUnlock = CanUnlockSkill(skillData);
        
        // Update locked/unlocked icons
        Transform lockedIcon = skillData.button.transform.Find("LockedIcon");
        Transform unlockedIcon = skillData.button.transform.Find("UnlockedIcon");
        
        if (lockedIcon != null)
            lockedIcon.gameObject.SetActive(!isUnlocked);
            
        if (unlockedIcon != null)
            unlockedIcon.gameObject.SetActive(isUnlocked);
        
        // Update button interactability
        skillData.button.interactable = !isUnlocked && canUnlock;
        
        // Optional: Add visual feedback for prerequisite not met
        if (!isUnlocked && !canUnlock)
        {
            // Could add a "prerequisite not met" visual here
            var image = skillData.button.GetComponent<Image>();
            if (image != null)
                image.color = new Color(0.5f, 0.5f, 0.5f, 0.7f); // Greyed out
        }
        else if (!isUnlocked)
        {
            var image = skillData.button.GetComponent<Image>();
            if (image != null)
                image.color = Color.white; // Normal color
        }
    }
    
    private bool CanUnlockSkill(SkillNodeData skillData)
    {
        // Check if prerequisite skill is unlocked
        if (!string.IsNullOrEmpty(skillData.prerequisiteSkillID))
        {
            if (!SkillManager.Instance.IsSkillUnlocked(skillData.prerequisiteSkillID))
                return false;
        }
        
        // Check if player has enough shards
        return SkillManager.Instance.GetShardCount() >= skillData.shardCost;
    }
    
    private void UpdateApplyButton()
    {
        if (applyButton == null || SkillManager.Instance == null || string.IsNullOrEmpty(selectedSkillID))
        {
            if (applyButton != null)
                applyButton.interactable = false;
            return;
        }
        
        if (!skillNodes.ContainsKey(selectedSkillID))
        {
            applyButton.interactable = false;
            return;
        }
        
        var skillData = skillNodes[selectedSkillID];
        bool isUnlocked = SkillManager.Instance.IsSkillUnlocked(selectedSkillID);
        bool canUnlock = CanUnlockSkill(skillData);
        
        applyButton.interactable = !isUnlocked && canUnlock;
    }
    
    private void OnEnable()
    {
        UpdateShardCount();
        UpdateAllSkillStatus();
    }
} 