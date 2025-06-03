using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using System.Collections;

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
    [SerializeField] private TextMeshProUGUI costText;
    
    [Header("Tooltip System")]
    [SerializeField] private GameObject tooltipPanel; // Tooltip background panel
    [SerializeField] private TextMeshProUGUI tooltipTitle; // Skill name
    [SerializeField] private TextMeshProUGUI tooltipDescription; // Skill description
    [SerializeField] private TextMeshProUGUI tooltipCost; // Skill cost
    
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
        
        // Setup tooltip (keep it simple for fixed positioning)
        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(false);
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
        // SkillManager'dan veri alıyoruz
        if (SkillManager.Instance == null)
        {
            Debug.LogError("SkillManager instance not found!");
            return;
        }
        
        Debug.Log("🚀 InitializeSkillNodes - Getting data from SkillManager");
        
        // Fire Element Skills - SkillManager'dan veri çek
        var fireSpellInfo = SkillManager.Instance.GetSkillInfo("FireSpell");
        if (fireSpellInfo != null)
        {
            skillNodes["FireSpell"] = new SkillNodeData
            {
                skillID = fireSpellInfo.skillID,
                skillName = fireSpellInfo.skillName,
                description = fireSpellInfo.description, // SkillManager'dan description
                shardCost = 30,
                prerequisiteSkillID = null, // Level 1 skill
                button = fireSkill1Button
            };
            Debug.Log($"✅ FireSpell loaded: {fireSpellInfo.skillName} - {fireSpellInfo.description}");
        }
        else
        {
            Debug.LogError("❌ FireSpell not found in SkillManager!");
        }
        
        var fireballInfo = SkillManager.Instance.GetSkillInfo("fireball_spell");
        if (fireballInfo != null)
        {
            skillNodes["fireball_spell"] = new SkillNodeData
            {
                skillID = fireballInfo.skillID,
                skillName = fireballInfo.skillName,
                description = fireballInfo.description, // SkillManager'dan description
                shardCost = 80,
                prerequisiteSkillID = "FireSpell", // Requires Fire Spell
                button = fireSkill2Button
            };
            Debug.Log($"✅ Fireball loaded: {fireballInfo.skillName} - {fireballInfo.description}");
        }
        else
        {
            Debug.LogError("❌ Fireball not found in SkillManager!");
        }
        
        // Ice Element Skills - SkillManager'dan veri çek
        var iceShardInfo = SkillManager.Instance.GetSkillInfo("ice_shard");
        if (iceShardInfo != null)
        {
            skillNodes["ice_shard"] = new SkillNodeData
            {
                skillID = iceShardInfo.skillID,
                skillName = iceShardInfo.skillName,
                description = iceShardInfo.description,
                shardCost = 25,
                prerequisiteSkillID = null,
                button = iceSkill1Button
            };
            Debug.Log($"✅ Ice Shard loaded: {iceShardInfo.skillName}");
        }
        
        // Earth Element Skills - SkillManager'dan veri çek
        var earthPushInfo = SkillManager.Instance.GetSkillInfo("earth_push");
        if (earthPushInfo != null)
        {
            skillNodes["earth_push"] = new SkillNodeData
            {
                skillID = earthPushInfo.skillID,
                skillName = earthPushInfo.skillName,
                description = earthPushInfo.description,
                shardCost = 35,
                prerequisiteSkillID = null,
                button = earthSkill1Button
            };
            Debug.Log($"✅ Earth Push loaded: {earthPushInfo.skillName}");
        }
        
        // Electric Element Skills - SkillManager'dan veri çek
        var electricDashInfo = SkillManager.Instance.GetSkillInfo("electric_dash");
        if (electricDashInfo != null)
        {
            skillNodes["electric_dash"] = new SkillNodeData
            {
                skillID = electricDashInfo.skillID,
                skillName = electricDashInfo.skillName,
                description = electricDashInfo.description,
                shardCost = 40,
                prerequisiteSkillID = null,
                button = electricSkill1Button
            };
            Debug.Log($"✅ Electric Dash loaded: {electricDashInfo.skillName}");
        }
        
        // Air Element Skills - SkillManager'dan veri çek
        var airPushInfo = SkillManager.Instance.GetSkillInfo("air_push");
        if (airPushInfo != null)
        {
            skillNodes["air_push"] = new SkillNodeData
            {
                skillID = airPushInfo.skillID,
                skillName = airPushInfo.skillName,
                description = airPushInfo.description,
                shardCost = 20,
                prerequisiteSkillID = null,
                button = airSkill1Button
            };
            Debug.Log($"✅ Air Push loaded: {airPushInfo.skillName}");
        }
        
        // Void Element Skills - SkillManager'dan veri çek
        var voidSkillInfo = SkillManager.Instance.GetSkillInfo("void_skill");
        if (voidSkillInfo != null)
        {
            skillNodes["void_skill"] = new SkillNodeData
            {
                skillID = voidSkillInfo.skillID,
                skillName = voidSkillInfo.skillName,
                description = voidSkillInfo.description,
                shardCost = 60,
                prerequisiteSkillID = null,
                button = voidSkill1Button
            };
            Debug.Log($"✅ Void Skill loaded: {voidSkillInfo.skillName}");
        }
        
        // Level 2 & 3 placeholder skills (SkillManager'da henüz yok)
        skillNodes["ice_level2"] = new SkillNodeData
        {
            skillID = "ice_level2",
            skillName = "Ice Spear",
            description = "Launches piercing ice spears. (Coming Soon)",
            shardCost = 75,
            prerequisiteSkillID = "ice_shard",
            button = iceSkill2Button
        };
        
        skillNodes["earth_level2"] = new SkillNodeData
        {
            skillID = "earth_level2",
            skillName = "Stone Wall",
            description = "Creates protective stone barriers. (Coming Soon)",
            shardCost = 85,
            prerequisiteSkillID = "earth_push",
            button = earthSkill2Button
        };
        
        skillNodes["electric_level2"] = new SkillNodeData
        {
            skillID = "electric_level2",
            skillName = "Lightning Bolt",
            description = "Strikes enemies with lightning. (Coming Soon)",
            shardCost = 90,
            prerequisiteSkillID = "electric_dash",
            button = electricSkill2Button
        };
        
        skillNodes["air_level2"] = new SkillNodeData
        {
            skillID = "air_level2",
            skillName = "Wind Blade",
            description = "Slices enemies with wind blades. (Coming Soon)",
            shardCost = 70,
            prerequisiteSkillID = "air_push",
            button = airSkill2Button
        };
        
        skillNodes["void_level2"] = new SkillNodeData
        {
            skillID = "void_level2",
            skillName = "Void Slash",
            description = "Cuts through reality itself. (Coming Soon)",
            shardCost = 100,
            prerequisiteSkillID = "void_skill",
            button = voidSkill2Button
        };
        
        Debug.Log($"🚀 Total skill nodes created: {skillNodes.Count}");
    }
    
    private void SetupButtons()
    {
        if (unlockSkillButton != null)
            unlockSkillButton.onClick.AddListener(OpenSkillPanel);
        
        if (resetButton != null)
            resetButton.onClick.AddListener(ResetSelection);
        
        // Apply button artık kullanmıyoruz - Hold to unlock sistemi
        if (applyButton != null)
        {
            applyButton.gameObject.SetActive(false); // Apply button'u gizle
        }
        
        if (closeButton != null)
            closeButton.onClick.AddListener(ClosePanel);
        
        // Setup skill buttons with hold-to-unlock events
        foreach (var skillNode in skillNodes.Values)
        {
            if (skillNode.button != null)
            {
                string skillID = skillNode.skillID; // Local variable for closure
                
                Debug.Log($"🔧 Setting up button for skill: {skillID} - {skillNode.skillName}");
                
                // Normal click için tooltip göster
                skillNode.button.onClick.AddListener(() => {
                    Debug.Log($"🔘 Button clicked for skill: {skillID}");
                    ShowTooltipForSkill(skillID);
                });
                
                // Hold-to-unlock events ekle
                AddHoldToUnlockEvents(skillNode.button, skillID);
                
                // Hover events (eski sistem)
                AddHoverEvents(skillNode.button, skillID);
            }
            else
            {
                Debug.LogWarning($"❌ Button is null for skill: {skillNode.skillID}");
            }
        }
    }
    
    private Coroutine currentHoldCoroutine = null;
    private string currentHoldSkillID = null;
    
    private void AddHoldToUnlockEvents(Button button, string skillID)
    {
        // EventTrigger ekle
        EventTrigger trigger = button.GetComponent<EventTrigger>();
        if (trigger == null)
        {
            trigger = button.gameObject.AddComponent<EventTrigger>();
        }
        
        // Mouse/Touch basılı tutma başlangıcı
        EventTrigger.Entry pointerDown = new EventTrigger.Entry();
        pointerDown.eventID = EventTriggerType.PointerDown;
        pointerDown.callback.AddListener((data) => StartHoldToUnlock(skillID));
        trigger.triggers.Add(pointerDown);
        
        // Mouse/Touch bırakma
        EventTrigger.Entry pointerUp = new EventTrigger.Entry();
        pointerUp.eventID = EventTriggerType.PointerUp;
        pointerUp.callback.AddListener((data) => StopHoldToUnlock());
        trigger.triggers.Add(pointerUp);
        
        // Mouse button'dan çıkma (cancel)
        EventTrigger.Entry pointerExit = new EventTrigger.Entry();
        pointerExit.eventID = EventTriggerType.PointerExit;
        pointerExit.callback.AddListener((data) => StopHoldToUnlock());
        trigger.triggers.Add(pointerExit);
    }
    
    private void StartHoldToUnlock(string skillID)
    {
        // Eğer skill unlock edilemiyorsa, hold başlatma
        if (!skillNodes.ContainsKey(skillID)) return;
        
        var skillData = skillNodes[skillID];
        bool isUnlocked = SkillManager.Instance.IsSkillUnlocked(skillID);
        bool canUnlock = CanUnlockSkill(skillData);
        
        if (isUnlocked || !canUnlock) return;
        
        // Önceki hold işlemini durdur
        StopHoldToUnlock();
        
        currentHoldSkillID = skillID;
        currentHoldCoroutine = StartCoroutine(HoldToUnlockCoroutine(skillID));
        
        Debug.Log($"🔥 Started holding to unlock: {skillID}");
    }
    
    private void StopHoldToUnlock()
    {
        if (currentHoldCoroutine != null)
        {
            StopCoroutine(currentHoldCoroutine);
            currentHoldCoroutine = null;
            Debug.Log($"🔥 Stopped holding: {currentHoldSkillID}");
            currentHoldSkillID = null;
        }
    }
    
    private System.Collections.IEnumerator HoldToUnlockCoroutine(string skillID)
    {
        float holdTime = 1.5f; // 1.5 saniye basılı tut
        float currentTime = 0f;
        
        while (currentTime < holdTime)
        {
            currentTime += Time.deltaTime;
            float progress = currentTime / holdTime;
            
            // Progress'i debug'la göster (ileride UI progress bar eklenebilir)
            if (currentTime % 0.3f < Time.deltaTime) // Her 0.3 saniyede log
            {
                Debug.Log($"🔥 Hold progress: {progress:F1} for {skillID}");
            }
            
            yield return null;
        }
        
        // Hold tamamlandı, skill'i unlock et!
        UnlockSkillDirectly(skillID);
        currentHoldCoroutine = null;
        currentHoldSkillID = null;
    }
    
    private void UnlockSkillDirectly(string skillID)
    {
        if (!skillNodes.ContainsKey(skillID)) return;
        
        var skillData = skillNodes[skillID];
        Debug.Log($"🔥 HOLD COMPLETED! Unlocking skill: {skillData.skillName}");
        
        bool success = SkillManager.Instance.UnlockSkill(skillID, skillData.shardCost);
        
        if (success)
        {
            Debug.Log($"✅ Skill unlocked via hold: {skillID}");
            UpdateShardCount();
            UpdateAllSkillStatus();
            
            // Success feedback (ses/efekt eklenebilir)
            ShowSkillUnlockedFeedback(skillData.skillName);
        }
        else
        {
            Debug.LogWarning($"❌ Failed to unlock skill via hold: {skillID}");
        }
    }
    
    private void ShowSkillUnlockedFeedback(string skillName)
    {
        // Basit feedback, ileride particle effect vs eklenebilir
        Debug.Log($"🎉 SKILL UNLOCKED: {skillName}!");
    }
    
    private void ShowTooltipForSkill(string skillID)
    {
        // Normal click'te sadece tooltip göster
        if (!skillNodes.ContainsKey(skillID) || tooltipPanel == null) 
        {
            Debug.LogWarning($"❌ ShowTooltipForSkill failed: skillID={skillID}, skillExists={skillNodes.ContainsKey(skillID)}, tooltipPanel={tooltipPanel != null}");
            return;
        }
        
        var skillData = skillNodes[skillID];
        
        Debug.Log($"📝 Showing tooltip for: {skillData.skillName} - {skillData.description}");
        
        // Update tooltip content
        if (tooltipTitle != null)
            tooltipTitle.text = skillData.skillName;
            
        if (tooltipDescription != null)
            tooltipDescription.text = skillData.description;
            
        if (tooltipCost != null)
        {
            bool isUnlocked = SkillManager.Instance != null && SkillManager.Instance.IsSkillUnlocked(skillID);
            if (isUnlocked)
                tooltipCost.text = "✅ UNLOCKED";
            else
                tooltipCost.text = $"Hold to unlock - Cost: {skillData.shardCost} Shards";
        }
        
        tooltipPanel.SetActive(true);
        Debug.Log($"✅ Tooltip panel activated for: {skillData.skillName}");
    }
    
    private void AddHoverEvents(Button button, string skillID)
    {
        // Add EventTrigger component if not exists
        EventTrigger trigger = button.GetComponent<EventTrigger>();
        if (trigger == null)
        {
            trigger = button.gameObject.AddComponent<EventTrigger>();
        }
        
        // Pointer Enter Event (Hover Start)
        EventTrigger.Entry pointerEnter = new EventTrigger.Entry();
        pointerEnter.eventID = EventTriggerType.PointerEnter;
        pointerEnter.callback.AddListener((data) => ShowTooltip(skillID, (PointerEventData)data));
        trigger.triggers.Add(pointerEnter);
        
        // Pointer Exit Event (Hover End)
        EventTrigger.Entry pointerExit = new EventTrigger.Entry();
        pointerExit.eventID = EventTriggerType.PointerExit;
        pointerExit.callback.AddListener((data) => HideTooltip());
        trigger.triggers.Add(pointerExit);
    }
    
    private void ShowTooltip(string skillID, PointerEventData eventData)
    {
        if (!skillNodes.ContainsKey(skillID) || tooltipPanel == null) return;
        
        var skillData = skillNodes[skillID];
        
        // Update tooltip content
        if (tooltipTitle != null)
            tooltipTitle.text = skillData.skillName;
            
        if (tooltipDescription != null)
            tooltipDescription.text = skillData.description;
            
        if (tooltipCost != null)
        {
            bool isUnlocked = SkillManager.Instance != null && SkillManager.Instance.IsSkillUnlocked(skillID);
            if (isUnlocked)
                tooltipCost.text = "✅ UNLOCKED";
            else
                tooltipCost.text = $"Cost: {skillData.shardCost} Shards";
        }
        
        // Show tooltip at fixed position (no dynamic positioning)
        tooltipPanel.SetActive(true);
    }
    
    private void PositionTooltip(PointerEventData eventData)
    {
        // Removed - tooltip now stays at fixed canvas position
    }
    
    private void ClampTooltipToCanvas(Canvas canvas)
    {
        // Removed - no longer needed for fixed positioning
    }
    
    private void HideTooltip()
    {
        if (tooltipPanel != null)
            tooltipPanel.SetActive(false);
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
        
        // Hide tooltip when closing panel
        HideTooltip();
    }
    
    public void ClosePanel()
    {
        CloseSkillPanel();
        gameObject.SetActive(false);
        HideTooltip();
    }
    
    private void ResetSelection()
    {
        // Hold sistem kullandığımız için selection'a gerek yok
        // Bu metod artık sadece tooltip'i kapatmak için
        HideTooltip();
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
        if (SkillManager.Instance == null || skillData.button == null) return;
        
        bool isUnlocked = SkillManager.Instance.IsSkillUnlocked(skillData.skillID);
        bool canUnlock = CanUnlockSkill(skillData);
        
        // Hold-to-unlock sisteminde button'lar her zaman tıklanabilir olmalı
        // Sadece görsel feedback değişir
        skillData.button.interactable = true;
        
        // Visual feedback with alpha
        var buttonImage = skillData.button.GetComponent<Image>();
        if (buttonImage != null)
        {
            Color currentColor = buttonImage.color;
            
            if (isUnlocked)
            {
                // Skill unlocked - full alpha, green tint
                currentColor.a = 1f;
                currentColor = Color.green * 0.8f; // Yeşilimsi ton
                currentColor.a = 1f;
            }
            else if (canUnlock)
            {
                // Skill can be unlocked - full alpha, normal color
                currentColor = Color.white;
                currentColor.a = 1f;
            }
            else
            {
                // Skill cannot be unlocked - low alpha (still clickable for tooltip)
                currentColor = Color.white;
                currentColor.a = 0.4f;
            }
            
            buttonImage.color = currentColor;
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
    
    private void OnEnable()
    {
        UpdateShardCount();
        UpdateAllSkillStatus();
    }
    
    private void OnDisable()
    {
        // Hide tooltip when panel is disabled
        HideTooltip();
    }
} 