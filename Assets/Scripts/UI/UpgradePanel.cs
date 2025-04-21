using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradePanel : MonoBehaviour
{
    [Header("Stat Buttons")]
    [SerializeField] private Button hpButton;
    [SerializeField] private Button manaButton;
    [SerializeField] private Button damageButton;
    
    [Header("Action Buttons")]
    [SerializeField] private Button applyButton;
    [SerializeField] private Button resetButton;
    [SerializeField] private Button closeButton;
    
    [Header("UI Text")]
    [SerializeField] private TextMeshProUGUI skillPointText;
    [SerializeField] private TextMeshProUGUI titleText;
    
    private PlayerStats playerStats;
    
    // Geçici değişiklikler
    private int tempHpUpgrades = 0;
    private int tempManaUpgrades = 0;
    private int tempDamageUpgrades = 0;
    private int usedSkillPoints = 0;
    
    private void Awake()
    {
        // DontDestroyOnLoad ile korunmasını sağlayalım, ama parent'ı değiştirmeyelim
        if (Checkpoint.persistentUpgradePanel == null)
        {
            Checkpoint.persistentUpgradePanel = this;
            DontDestroyOnLoad(gameObject);
            
            // Canvas ayarlarını düzenle
            Canvas canvas = GetComponent<Canvas>();
            if (canvas != null)
            {
                // Canvas'ı Screen Space - Overlay moduna ayarla
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                
                // Sorting order değiştirmeyelim
                // canvas.sortingOrder = 110; // Bu satırı kaldırdık
            }
        }
        else if (Checkpoint.persistentUpgradePanel != this)
        {
            // Eğer zaten bir instance varsa ve bu o değilse, bu instance'ı yok et
            Destroy(gameObject);
        }
        
        // Başlangıçta gizli olsun
        if (gameObject.activeSelf)
        {
            gameObject.SetActive(false);
        }
    }
    
    private void Start()
    {
        // Eğer bu nesne Awake'de yok edilmediyse devam et
        if (this == null) return;
        
        // Buton olaylarını ayarla
        if (hpButton != null)
            hpButton.onClick.AddListener(OnHpButtonClicked);
            
        if (manaButton != null)
            manaButton.onClick.AddListener(OnManaButtonClicked);
            
        if (damageButton != null)
            damageButton.onClick.AddListener(OnDamageButtonClicked);
            
        if (applyButton != null)
            applyButton.onClick.AddListener(OnApplyButtonClicked);
            
        if (resetButton != null)
            resetButton.onClick.AddListener(OnResetButtonClicked);
            
        if (closeButton != null)
            closeButton.onClick.AddListener(OnCloseButtonClicked);
    }
    
   private void Initialize(PlayerStats stats)
    {
        playerStats = stats;
        ResetTemporaryChanges();
        UpdateUI();
    }
    
    private void ResetTemporaryChanges()
    {
        tempHpUpgrades = 0;
        tempManaUpgrades = 0;
        tempDamageUpgrades = 0;
        usedSkillPoints = 0;
    }
    
    private void UpdateUI()
    {
        if (playerStats == null)
            return;
            
        // Kalan skill point'leri göster
        if (skillPointText != null)
        {
            int remainingPoints = playerStats.AvailableSkillPoints - usedSkillPoints;
            skillPointText.text = "Remaining Skill Point : " + remainingPoints;
        }
        
        // Skill point'leri tükendiyse butonları devre dışı bırak
        bool hasSkillPoints = (playerStats.AvailableSkillPoints - usedSkillPoints) > 0;
        
        if (hpButton != null)
            hpButton.interactable = hasSkillPoints;
            
        if (manaButton != null)
            manaButton.interactable = hasSkillPoints;
            
        if (damageButton != null)
            damageButton.interactable = hasSkillPoints;
            
        // Action butonlarını kontrol et
        if (applyButton != null)
            applyButton.interactable = (usedSkillPoints > 0);
            
        if (resetButton != null)
            resetButton.interactable = (usedSkillPoints > 0);
    }
    
    // Buton olayları
    private void OnHpButtonClicked()
    {
        if (playerStats == null || (playerStats.AvailableSkillPoints - usedSkillPoints) <= 0)
            return;
            
        tempHpUpgrades++;
        usedSkillPoints++;
        UpdateUI();
    }
    
    private void OnManaButtonClicked()
    {
        if (playerStats == null || (playerStats.AvailableSkillPoints - usedSkillPoints) <= 0)
            return;
            
        tempManaUpgrades++;
        usedSkillPoints++;
        UpdateUI();
    }
    
    private void OnDamageButtonClicked()
    {
        if (playerStats == null || (playerStats.AvailableSkillPoints - usedSkillPoints) <= 0)
            return;
            
        tempDamageUpgrades++;
        usedSkillPoints++;
        UpdateUI();
    }
    
    private void OnApplyButtonClicked()
    {
        if (playerStats == null || usedSkillPoints <= 0)
            return;
            
        // HP artışlarını uygula
        for (int i = 0; i < tempHpUpgrades; i++)
        {
            playerStats.IncreaseMaxHealth();
        }
        
        // Mana artışlarını uygula
        for (int i = 0; i < tempManaUpgrades; i++)
        {
            playerStats.IncreaseMaxMana();
        }
        
        // Hasar artışlarını uygula
        for (int i = 0; i < tempDamageUpgrades; i++)
        {
            playerStats.IncreaseDamage();
        }
        
        // Değişiklikleri sıfırla
        ResetTemporaryChanges();
        
        // UI'ı güncelle
        UpdateUI();
        
        // Eğer tüm skill point'ler kullanıldıysa paneli kapat
        if (playerStats.AvailableSkillPoints <= 0)
        {
            gameObject.SetActive(false);
        }
    }
    
    private void OnResetButtonClicked()
    {
        ResetTemporaryChanges();
        UpdateUI();
    }
    
    private void OnCloseButtonClicked()
    {
        // Eğer değişiklik yapıldıysa sıfırla
        if (usedSkillPoints > 0)
        {
            ResetTemporaryChanges();
        }
        
        // Paneli kapat
        gameObject.SetActive(false);
    }
    
    // Panel görünürlüğünü ayarla
    public void Show(PlayerStats stats)
    {
        Initialize(stats);
        
        // Canvas'ı aktif olduğundan emin ol
        Canvas canvas = GetComponent<Canvas>();
        if (canvas != null)
        {
            // Canvas'ı Screen Space - Overlay moduna getir
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            
            // En üstte görünmesi için yüksek sorting order ver
            canvas.sortingOrder = 110; // Selection Screen'den daha yüksek
        }
        
        // Eğer bu Canvasta GraphicRaycaster yoksa ekle
        GraphicRaycaster raycaster = GetComponent<GraphicRaycaster>();
        if (raycaster == null)
        {
            raycaster = gameObject.AddComponent<GraphicRaycaster>();
        }
        
        // Objeyi aktifleştirmeden önce UIInputBlocker'a paneli ekle
        if (UIInputBlocker.instance != null)
        {
            UIInputBlocker.instance.AddPanel(gameObject);
        }
        
        // UI scale'inin doğru olduğunu kontrol et
        transform.localScale = Vector3.one;
        
        // Paneli göster
        gameObject.SetActive(true);
        
        // Butonların erişilebilir olduğunu kontrol et
        if (hpButton != null) hpButton.interactable = true;
        if (manaButton != null) manaButton.interactable = true;
        if (damageButton != null) damageButton.interactable = true;
        if (applyButton != null) applyButton.interactable = (usedSkillPoints > 0);
        if (resetButton != null) resetButton.interactable = (usedSkillPoints > 0);
        if (closeButton != null) closeButton.interactable = true;
    }
    
    public void Hide()
    {
        gameObject.SetActive(false);
    }
} 