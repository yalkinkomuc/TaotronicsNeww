using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using System.Collections;

public class SkillTreePanel : BaseUIPanel
{
    [Header("Skill Panel References")]
    [SerializeField] private GameObject skillScreenPanel;
    [SerializeField] private Button unlockSkillButton;

    [Header("Skill Screen UI Elements")]
    [SerializeField] private Button resetButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private TextMeshProUGUI skillNameText; // Sol üstteki skill adı
    [SerializeField] private TextMeshProUGUI skillDescriptionText; // Sol üstteki skill açıklaması
    [SerializeField] private TextMeshProUGUI centerSkillNameText; // Ortadaki büyük yazı
    [SerializeField] private TextMeshProUGUI shardCountText;
    
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
    
    // Skill mapping to SkillManager skill IDs
    private Dictionary<Button, string> buttonToSkillID = new Dictionary<Button, string>();
    private Dictionary<string, int> skillCosts = new Dictionary<string, int>();
    
    private new void Awake()
    {
        // Panel'i önce kapat
        gameObject.SetActive(false);
        
        // Mapping'i initialize et
        InitializeSkillMapping();
        // Button setup'ı OnEnable()'da yapacağız
    }
    
    private void Start()
    {
        // Start'ta sadece UI'ı güncelle
        UpdateShardCount();
        ResetUI();
    }
    
    protected override void OnEnable()
    {
        
        // UIInputBlocker'a ekle (sadece aktif olduğunda)
        if (UIInputBlocker.instance != null)
        {
            UIInputBlocker.instance.AddPanel(gameObject);
            if (skillScreenPanel != null)
                UIInputBlocker.instance.AddPanel(skillScreenPanel);
        }
        

        
        SetupButtons(); // HER AÇILIŞTA BUTTON'LARI SETUP ET!
        UpdateShardCount();
        UpdateAllSkillButtons();
    }
    
    private void InitializeSkillMapping()
    {
        // Map buttons to skill IDs from SkillManager (only if buttons are assigned)
        if (fireSkill1Button != null) buttonToSkillID[fireSkill1Button] = "FireSpell";
        if (fireSkill2Button != null) buttonToSkillID[fireSkill2Button] = "fireball_spell";
        if (iceSkill1Button != null) buttonToSkillID[iceSkill1Button] = "ice_shard";
        if (earthSkill1Button != null) buttonToSkillID[earthSkill1Button] = "earth_push";
        if (electricSkill1Button != null) buttonToSkillID[electricSkill1Button] = "electric_dash";
        if (airSkill1Button != null) buttonToSkillID[airSkill1Button] = "air_push";
        if (voidSkill1Button != null) buttonToSkillID[voidSkill1Button] = "void_skill";
        
        // Set skill costs (matching SkillTreePanel's old costs)
        skillCosts["FireSpell"] = 30;
        skillCosts["fireball_spell"] = 80;
        skillCosts["ice_shard"] = 25;
        skillCosts["earth_push"] = 35;
        skillCosts["electric_dash"] = 40;
        skillCosts["air_push"] = 20;
        skillCosts["void_skill"] = 60;
        
        // Future skills (not implemented yet) - only if buttons are assigned
        if (fireSkill3Button != null) buttonToSkillID[fireSkill3Button] = "fire_level3";
        if (iceSkill2Button != null) buttonToSkillID[iceSkill2Button] = "ice_level2";
        if (iceSkill3Button != null) buttonToSkillID[iceSkill3Button] = "ice_level3";
        if (earthSkill2Button != null) buttonToSkillID[earthSkill2Button] = "earth_level2";
        if (earthSkill3Button != null) buttonToSkillID[earthSkill3Button] = "earth_level3";
        if (electricSkill2Button != null) buttonToSkillID[electricSkill2Button] = "electric_level2";
        if (electricSkill3Button != null) buttonToSkillID[electricSkill3Button] = "electric_level3";
        if (airSkill2Button != null) buttonToSkillID[airSkill2Button] = "air_level2";
        if (airSkill3Button != null) buttonToSkillID[airSkill3Button] = "air_level3";
        if (voidSkill2Button != null) buttonToSkillID[voidSkill2Button] = "void_level2";
        if (voidSkill3Button != null) buttonToSkillID[voidSkill3Button] = "void_level3";
        
        skillCosts["fire_level3"] = 150;
        skillCosts["ice_level2"] = 75;
        skillCosts["ice_level3"] = 140;
        skillCosts["earth_level2"] = 85;
        skillCosts["earth_level3"] = 160;
        skillCosts["electric_level2"] = 90;
        skillCosts["electric_level3"] = 170;
        skillCosts["air_level2"] = 70;
        skillCosts["air_level3"] = 130;
        skillCosts["void_level2"] = 100;
        skillCosts["void_level3"] = 200;
    }
    
    private void SetupButtons()
    {
        
        if (unlockSkillButton != null)
        {
            unlockSkillButton.onClick.RemoveAllListeners();
            unlockSkillButton.onClick.AddListener(OpenSkillPanel);
            unlockSkillButton.interactable = true; // ZORLA AKTİF YAP
        }
        
        if (resetButton != null)
        {
            resetButton.onClick.RemoveAllListeners();
            resetButton.onClick.AddListener(ResetUI);
            resetButton.interactable = true; // ZORLA AKTİF YAP
        }
        
        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(ClosePanel);
            closeButton.interactable = true; // ZORLA AKTİF YAP
        }
        
        // Setup skill buttons - SADECE CLICK EVENT'LERİ
        Debug.Log($"buttonToSkillID dictionary has {buttonToSkillID.Count} entries");
        
        foreach (var kvp in buttonToSkillID)
        {
            Button button = kvp.Key;
            string skillID = kvp.Value;
            
            // FİRESKİLL1 İÇİN ÖZEL LOG!
            if (skillID == "FireSpell")
            {
                Debug.Log($"PROCESSING FIRESKILL! Button is null: {button == null}");
                if (button != null)
                {
                    Debug.Log($"FireSkill button name: {button.name}");
                    Debug.Log($"FireSkill button active: {button.gameObject.activeInHierarchy}");
                }
            }
            
            if (button != null)
            {
                Debug.Log($"Setting up button for skill: {skillID}");
                // Önce eski event'leri temizle
                button.onClick.RemoveAllListeners();
                // ZORLA HER BUTTON'U AKTİF YAP
                button.interactable = true;
                
                // Click to show skill info
                button.onClick.AddListener(() => {
                    Debug.Log($"BUTTON CLICKED! SkillID: {skillID}");
                    ShowSkillInfo(skillID);
                });
                
                // FİRESKİLL İÇİN EKSTRA DEBUG!
                if (skillID == "FireSpell")
                {
                    Debug.Log("FIRESKILL BUTTON EVENT ADDED!");
                    button.onClick.AddListener(() => {
                        Debug.Log("FIRESKILL EXTRA EVENT TRIGGERED!");
                    });
                }
                
                // TEST İÇİN: EventTrigger ile PointerClick test et
                EventTrigger trigger = button.GetComponent<EventTrigger>();
                if (trigger == null)
                {
                    trigger = button.gameObject.AddComponent<EventTrigger>();
                }
                
                // Eski trigger'ları temizle
                trigger.triggers.Clear();
                
                // PointerClick eventi ekle
                EventTrigger.Entry pointerClick = new EventTrigger.Entry();
                pointerClick.eventID = EventTriggerType.PointerClick;
                pointerClick.callback.AddListener((data) => {
                    Debug.Log($"EVENTTRIGGER CLICKED! SkillID: {skillID}");
                    ShowSkillInfo(skillID);
                });
                trigger.triggers.Add(pointerClick);
                
                // HOVER ANİMASYONU EKLE!
                // Pointer Enter Event (Mouse hover başladığında)
                EventTrigger.Entry pointerEnter = new EventTrigger.Entry();
                pointerEnter.eventID = EventTriggerType.PointerEnter;
                pointerEnter.callback.AddListener((data) => {
                    // Button'u büyüt
                    StartCoroutine(ScaleButton(button.transform, Vector3.one * 1.1f, 0.2f));
                });
                trigger.triggers.Add(pointerEnter);
                
                // Pointer Exit Event (Mouse hover bittiğinde)
                EventTrigger.Entry pointerExit = new EventTrigger.Entry();
                pointerExit.eventID = EventTriggerType.PointerExit;
                pointerExit.callback.AddListener((data) => {
                    // Button'u normal boyutuna döndür
                    StartCoroutine(ScaleButton(button.transform, Vector3.one, 0.2f));
                });
                trigger.triggers.Add(pointerExit);
                
                // BASILI TUTMA SİSTEMİ EKLE!
                AddHoldToUnlockEvents(button, skillID);
                
                Debug.Log($"Button {skillID} is now interactable: {button.interactable}");
            }
            else
            {
                Debug.LogWarning($"Button is null for skillID: {skillID}");
            }
        }
        
        Debug.Log($"Total buttons set up: {buttonToSkillID.Count}");
    }
    
    private void ShowSkillInfo(string skillID)
    {
        Debug.Log($"ShowSkillInfo called for skillID: {skillID}");
        
        if (SkillManager.Instance == null) 
        {
            Debug.LogWarning("SkillManager.Instance is null!");
            return;
        }
        
        var skillInfo = SkillManager.Instance.GetSkillInfo(skillID);
        if (skillInfo != null)
        {
            Debug.Log($"Found skillInfo for {skillID}: {skillInfo.skillName}");
            
            // Sol üstteki skill cost (eski skillNameText)
            if (skillNameText != null)
            {
                int cost = skillCosts.ContainsKey(skillID) ? skillCosts[skillID] : 50;
                skillNameText.text = $"Required Skill Shard: {cost}";
            }
            else
                Debug.LogWarning("skillNameText is null!");
            
            // Sol üstteki skill description
            if (skillDescriptionText != null)
                skillDescriptionText.text = skillInfo.description;
            else
                Debug.LogWarning("skillDescriptionText is null!");
            
            // Ortadaki büyük skill name
            if (centerSkillNameText != null)
                centerSkillNameText.text = skillInfo.skillName;
            else
                Debug.LogWarning("centerSkillNameText is null!");
        }
        else
        {
            Debug.Log($"SkillInfo not found for {skillID}, using future skill data");
            
            // Future skill için varsayılan bilgiler
            string skillName = GetFutureSkillName(skillID);
            string description = "Coming Soon...";
            
            if (skillNameText != null)
            {
                int cost = skillCosts.ContainsKey(skillID) ? skillCosts[skillID] : 50;
                skillNameText.text = $"Required Skill Shard: {cost}";
            }
            
            if (skillDescriptionText != null)
                skillDescriptionText.text = description;
            
            if (centerSkillNameText != null)
                centerSkillNameText.text = skillName;
        }
        
        UpdateAllSkillButtons();
    }
    
    private string GetFutureSkillName(string skillID)
    {
        switch (skillID)
        {
            case "fire_level3": return "Fire Storm";
            case "ice_level2": return "Ice Spear";
            case "ice_level3": return "Blizzard";
            case "earth_level2": return "Stone Wall";
            case "earth_level3": return "Earthquake";
            case "electric_level2": return "Lightning Bolt";
            case "electric_level3": return "Thunder Storm";
            case "air_level2": return "Wind Blade";
            case "air_level3": return "Tornado";
            case "void_level2": return "Void Slash";
            case "void_level3": return "Reality Rift";
            default: return "Unknown Skill";
        }
    }
    
    private void UnlockSkill(string skillID)
    {
        if (SkillManager.Instance == null) return;
        
        // Check if already unlocked
        if (SkillManager.Instance.IsSkillUnlocked(skillID))
        {
            Debug.Log($"Skill {skillID} is already unlocked!");
            return;
        }
        
        // Check if we have enough shards
        int cost = skillCosts.ContainsKey(skillID) ? skillCosts[skillID] : 50;
        if (SkillManager.Instance.GetShardCount() < cost)
        {
            Debug.Log($"Not enough shards to unlock {skillID}. Need: {cost}, Have: {SkillManager.Instance.GetShardCount()}");
            return;
        }
        
        // Unlock the skill
        bool success = SkillManager.Instance.UnlockSkill(skillID, cost);
        
        if (success)
        {
            Debug.Log($"Successfully unlocked skill: {skillID}");
            UpdateShardCount();
            UpdateAllSkillButtons();
            ShowSkillInfo(skillID); // Refresh the info display
        }
        else
        {
            Debug.LogWarning($"Failed to unlock skill: {skillID}");
        }
    }
    
    private void ResetUI()
    {
        if (skillNameText != null)
            skillNameText.text = "Required Skill Shard: -";
        
        if (skillDescriptionText != null)
            skillDescriptionText.text = "Click on a skill to see its details.";
        
        if (centerSkillNameText != null)
            centerSkillNameText.text = "Skill Tree";
    }
    
    public void OpenPanel()
    {
        // Checkpoint UI'ı kapat!
        CheckpointSelectionScreen checkpointScreen = FindAnyObjectByType<CheckpointSelectionScreen>();
        if (checkpointScreen != null)
        {
            Debug.Log("Closing CheckpointSelectionScreen!");
            checkpointScreen.gameObject.SetActive(false);
        }
        
        gameObject.SetActive(true);
        UpdateShardCount();
        UpdateAllSkillButtons();
        ResetUI();
        
        if (UIInputBlocker.instance != null)
        {
            UIInputBlocker.instance.DisableGameplayInput();
        }
    }
    
    private void OpenSkillPanel()
    {
        // Bu metod artık gerekli değil, OpenPanel() kullan
        OpenPanel();
    }
    
    public void ClosePanel()
    {
        if (UIInputBlocker.instance != null)
        {
            UIInputBlocker.instance.EnableGameplayInput(true);
        }
        
        gameObject.SetActive(false);
        
        // Checkpoint UI'ı tekrar aç!
        CheckpointSelectionScreen checkpointScreen = FindAnyObjectByType<CheckpointSelectionScreen>();
        if (checkpointScreen != null)
        {
            Debug.Log("Reopening CheckpointSelectionScreen!");
            checkpointScreen.gameObject.SetActive(true);
        }
    }
    
   
    
    private void UpdateShardCount()
    {
        if (shardCountText != null && SkillManager.Instance != null)
        {
            shardCountText.text = SkillManager.Instance.GetShardCount().ToString();
        }
    }
    
    private void UpdateAllSkillButtons()
    {
        if (SkillManager.Instance == null) return;
        
        foreach (var kvp in buttonToSkillID)
        {
            Button button = kvp.Key;
            string skillID = kvp.Value;
            
            if (button != null)
            {
                bool isUnlocked = SkillManager.Instance.IsSkillUnlocked(skillID);
                
                // BUTTON'I HER ZAMAN AKTİF BIRAK!
                button.interactable = true;
                
                // Update button visual based on unlock status
                var buttonImage = button.GetComponent<Image>();
                if (buttonImage != null)
                {
                    Color currentColor = buttonImage.color;
                    
                    if (isUnlocked)
                    {
                        // Unlocked - bright green tint
                        currentColor = Color.green;
                        currentColor.a = 1f;
                    }
                    else
                    {
                        // Not unlocked - normal color
                        currentColor = Color.white;
                        currentColor.a = 1f;
                    }
                    
                    buttonImage.color = currentColor;
                }
                
                Debug.Log($"UpdateAllSkillButtons: {skillID} interactable = {button.interactable}");
            }
        }
    }

    private IEnumerator ScaleButton(Transform buttonTransform, Vector3 targetScale, float duration)
    {
        float elapsedTime = 0f;
        Vector3 startScale = buttonTransform.localScale;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);
            buttonTransform.localScale = Vector3.Lerp(startScale, targetScale, t);
            yield return null;
        }

        buttonTransform.localScale = targetScale;
    }

    private void AddHoldToUnlockEvents(Button button, string skillID)
    {
        bool isHolding = false;
        float holdTime = 0f;
        float requiredHoldTime = 2.0f; // 2 saniye basılı tut
        
        EventTrigger trigger = button.GetComponent<EventTrigger>();
        if (trigger == null)
        {
            trigger = button.gameObject.AddComponent<EventTrigger>();
        }
        
        // Pointer Down Event (basılı tutma başlangıcı)
        EventTrigger.Entry pointerDown = new EventTrigger.Entry();
        pointerDown.eventID = EventTriggerType.PointerDown;
        pointerDown.callback.AddListener((data) => {
            isHolding = true;
            holdTime = 0f;
            Debug.Log($"Started holding button for skill: {skillID}");
        });
        trigger.triggers.Add(pointerDown);
        
        // Pointer Up Event (basılı tutma sonu)
        EventTrigger.Entry pointerUp = new EventTrigger.Entry();
        pointerUp.eventID = EventTriggerType.PointerUp;
        pointerUp.callback.AddListener((data) => {
            if (isHolding)
            {
                Debug.Log($"Stopped holding button for skill: {skillID} after {holdTime:F1}s");
                isHolding = false;
                holdTime = 0f;
            }
        });
        trigger.triggers.Add(pointerUp);
        
        // Hold timer coroutine
        StartCoroutine(HoldTimerCoroutine());
        
        System.Collections.IEnumerator HoldTimerCoroutine()
        {
            while (true)
            {
                if (isHolding)
                {
                    holdTime += Time.deltaTime;
                    
                    if (holdTime >= requiredHoldTime)
                    {
                        Debug.Log($"Hold time reached! Unlocking skill: {skillID}");
                        UnlockSkill(skillID);
                        isHolding = false;
                        holdTime = 0f;
                    }
                }
                yield return null;
            }
        }
    }
} 