using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkillTreePanel : MonoBehaviour
{
    [Header("Skill Panel References")]
    [SerializeField] private GameObject skillScreenPanel; // SkillScreenPanel objesi
    [SerializeField] private Button unlockSkillButton; // Button'a tıklayınca SkillScreenPanel açılacak

    [Header("Skill Screen UI Elements")]
    [SerializeField] private Button resetButton;
    [SerializeField] private Button applyButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI shardCountText;
    
    [Header("Skill Slots")]
    [SerializeField] private Button xxxSkillButton; // XXX-Skill butonu
    [SerializeField] private Button voidSkillButton; // Void_Skill butonu
    
    // Seçili skill
    private string selectedSkillID;
    
    private void Awake()
    {
        // Panel aktif değilse gizle
        if (skillScreenPanel != null && skillScreenPanel.activeSelf)
        {
            skillScreenPanel.SetActive(false);
        }
        
        // UI Input Blocker'a kaydet
        if (UIInputBlocker.instance != null)
        {
            UIInputBlocker.instance.AddPanel(gameObject);
            if (skillScreenPanel != null)
                UIInputBlocker.instance.AddPanel(skillScreenPanel);
        }
    }
    
    private void Start()
    {
        // Buton olaylarını ayarla
        SetupButtons();
        
        // Shard sayısını güncelle
        UpdateShardCount();
    }
    
    private void SetupButtons()
    {
        // SkillTreePanel butonu
        if (unlockSkillButton != null)
        {
            unlockSkillButton.onClick.AddListener(OpenSkillPanel);
        }
        
        // SkillScreen butonları
        if (resetButton != null)
        {
            resetButton.onClick.AddListener(ResetSelection);
        }
        
        if (applyButton != null)
        {
            applyButton.onClick.AddListener(UnlockSelectedSkill);
            applyButton.interactable = false; // Başlangıçta devre dışı
        }
        
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseSkillPanel);
        }
        
        // Skill butonları
        if (xxxSkillButton != null)
        {
            xxxSkillButton.onClick.AddListener(() => SelectSkill("xxx_skill"));
        }
        
        if (voidSkillButton != null)
        {
            voidSkillButton.onClick.AddListener(() => SelectSkill("void_skill"));
        }
    }
    
    // Panel açma
    public void OpenPanel()
    {
        gameObject.SetActive(true);
    }
    
    // Skill Panel'i aç
    private void OpenSkillPanel()
    {
        if (skillScreenPanel != null)
        {
            skillScreenPanel.SetActive(true);
            UpdateShardCount();
            UpdateSkillStatus();
            ResetSelection(); // Seçimi sıfırla
            
            // Input'u devre dışı bırak
            if (UIInputBlocker.instance != null)
            {
                UIInputBlocker.instance.DisableGameplayInput();
            }
        }
    }
    
    // Skill Panel'i kapat
    private void CloseSkillPanel()
    {
        if (skillScreenPanel != null)
        {
            skillScreenPanel.SetActive(false);
            
            // Input'u yeniden etkinleştir
            if (UIInputBlocker.instance != null)
            {
                UIInputBlocker.instance.EnableGameplayInput(true);
            }
        }
    }
    
    // Tüm paneli kapat
    public void ClosePanel()
    {
        CloseSkillPanel();
        gameObject.SetActive(false);
    }
    
    // Skill seçimi
    private void SelectSkill(string skillID)
    {
        selectedSkillID = skillID;
        Debug.Log($"Skill seçildi: {skillID}");
        
        // Başlığı güncelle
        if (titleText != null)
        {
            titleText.text = "Acquire " + (skillID == "void_skill" ? "Void Skill" : "XXX Skill") + ".";
        }
        
        // Butonların seçili durumunu güncelle
        if (xxxSkillButton != null)
        {
            // XXX-Skill için outline etrafını göster/gizle
            Transform outline = xxxSkillButton.transform.Find("SelectedOutline");
            if (outline != null)
                outline.gameObject.SetActive(skillID == "xxx_skill");
        }
        
        if (voidSkillButton != null)
        {
            // Void_Skill için outline etrafını göster/gizle
            Transform outline = voidSkillButton.transform.Find("SelectedOutline");
            if (outline != null)
                outline.gameObject.SetActive(skillID == "void_skill");
        }
        
        // Apply butonunu güncelle
        UpdateApplyButton();
    }
    
    // Seçimi sıfırla
    private void ResetSelection()
    {
        selectedSkillID = null;
        
        // Başlığı sıfırla
        if (titleText != null)
        {
            titleText.text = "Acquaire a new skill.";
        }
        
        // Outlineları kapat
        if (xxxSkillButton != null)
        {
            Transform outline = xxxSkillButton.transform.Find("SelectedOutline");
            if (outline != null)
                outline.gameObject.SetActive(false);
        }
        
        if (voidSkillButton != null)
        {
            Transform outline = voidSkillButton.transform.Find("SelectedOutline");
            if (outline != null)
                outline.gameObject.SetActive(false);
        }
        
        // Apply butonunu güncelle
        if (applyButton != null)
        {
            applyButton.interactable = false;
        }
    }
    
    // Seçili skill'i aç
    private void UnlockSelectedSkill()
    {
        if (string.IsNullOrEmpty(selectedSkillID) || SkillManager.Instance == null)
            return;
        
        // Maliyet hesapla (xxx_skill ve void_skill her ikisi de 50 shard)
        int cost = 50; 
        
        // Skill'i aç
        bool success = SkillManager.Instance.UnlockSkill(selectedSkillID, cost);
        
        if (success)
        {
            Debug.Log($"Skill açıldı: {selectedSkillID}");
            
            // UI'ı güncelle
            UpdateShardCount();
            UpdateSkillStatus();
            UpdateApplyButton();
        }
        else
        {
            Debug.LogWarning($"Skill açılamadı: {selectedSkillID}");
        }
    }
    
    // Shard sayısını güncelle
    private void UpdateShardCount()
    {
        if (shardCountText != null && SkillManager.Instance != null)
        {
            shardCountText.text = SkillManager.Instance.GetShardCount().ToString();
        }
    }
    
    // Becerilerin durumunu güncelle
    private void UpdateSkillStatus()
    {
        if (SkillManager.Instance == null)
            return;
            
        // XXX-Skill için kilit durumunu güncelle
        if (xxxSkillButton != null)
        {
            bool isUnlocked = SkillManager.Instance.IsSkillUnlocked("xxx_skill");
            
            // Locked/Unlocked ikonlarını güncelle
            Transform lockedIcon = xxxSkillButton.transform.Find("LockedIcon");
            Transform unlockedIcon = xxxSkillButton.transform.Find("UnlockedIcon");
            
            if (lockedIcon != null)
                lockedIcon.gameObject.SetActive(!isUnlocked);
                
            if (unlockedIcon != null)
                unlockedIcon.gameObject.SetActive(isUnlocked);
        }
        
        // Void_Skill için kilit durumunu güncelle
        if (voidSkillButton != null)
        {
            bool isUnlocked = SkillManager.Instance.IsSkillUnlocked("void_skill");
            
            // Locked/Unlocked ikonlarını güncelle
            Transform lockedIcon = voidSkillButton.transform.Find("LockedIcon");
            Transform unlockedIcon = voidSkillButton.transform.Find("UnlockedIcon");
            
            if (lockedIcon != null)
                lockedIcon.gameObject.SetActive(!isUnlocked);
                
            if (unlockedIcon != null)
                unlockedIcon.gameObject.SetActive(isUnlocked);
        }
    }
    
    // Apply butonunun durumunu güncelle
    private void UpdateApplyButton()
    {
        if (applyButton == null || SkillManager.Instance == null || string.IsNullOrEmpty(selectedSkillID))
        {
            if (applyButton != null)
                applyButton.interactable = false;
            return;
        }
        
        // Seçili skill açık mı ve yeterli shard var mı?
        bool isUnlocked = SkillManager.Instance.IsSkillUnlocked(selectedSkillID);
        bool hasEnoughShards = SkillManager.Instance.GetShardCount() >= 50; // xxx_skill ve void_skill her ikisi de 50
        
        // Apply butonu, skill açık değilse ve yeterli shard varsa aktif
        applyButton.interactable = !isUnlocked && hasEnoughShards;
    }
    
    // Panel açıldığında verileri güncelle
    private void OnEnable()
    {
        UpdateShardCount();
        UpdateSkillStatus();
    }
} 