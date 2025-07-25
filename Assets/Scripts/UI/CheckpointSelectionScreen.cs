using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class CheckpointSelectionScreen : BaseUIPanel
{
    [Header("Buttons")]
    [SerializeField] private Button restButton;
    [SerializeField] private Button upgradeButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button upgradeSkillsButton;
    
    [Header("References")]
    public AttributesUpgradePanel upgradePanel;
    [SerializeField] private SkillTreePanel skillTreePanel;
    

    private new  void Awake()
    {
        if (Checkpoint.persistentSelectionScreen == null)
        {
            Checkpoint.persistentSelectionScreen = this;
           // DontDestroyOnLoad(gameObject);
            
            // Canvas ayarını yap
            Canvas canvas = GetComponent<Canvas>();
            if (canvas != null)
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        }
        else if (Checkpoint.persistentSelectionScreen != this)
        {
            Destroy(gameObject);
        }
        
        // Başlangıçta gizle
        if (gameObject.activeSelf)
            gameObject.SetActive(false);
    }
    
    private void Start()
    {
        if (this == null) return;
        
        // Add button listeners
        SetupButtons();
    }

    private void SetupButtons()
    {
        if (restButton != null)
            restButton.onClick.AddListener(OnRestButtonClicked);
        
        if (upgradeButton != null)
            upgradeButton.onClick.AddListener(OnUpgradeButtonClicked);
        
        if (closeButton != null)
            closeButton.onClick.AddListener(ClosePanel);
            
        if (upgradeSkillsButton != null)
            upgradeSkillsButton.onClick.AddListener(OnUpgradeSkillsButtonClicked);
    }

    private void OnRestButtonClicked()
    {
        StartSceneTransition();
    }
    
    private void OnUpgradeButtonClicked()
    {
        gameObject.SetActive(false);
        
        // Önce paneli bul (instance üzerinden veya doğrudan referans)
        if (upgradePanel == null)
        {
            upgradePanel = AttributesUpgradePanel.instance;
            
            if (upgradePanel == null)
            {
                upgradePanel = FindFirstObjectByType<AttributesUpgradePanel>();
                
                if (upgradePanel == null)
                {
                    Debug.LogError("AttributesUpgradePanel bulunamadı!");
                    return;
                }
            }
        }
        
        // Panel bulundu, şimdi oyuncuyu bul
        Player player = PlayerManager.instance?.player;
        if (player == null)
        {
            Debug.LogError("Player bulunamadı!");
            return;
        }
        
        // PlayerStats'i al
        PlayerStats playerStats = player.GetComponent<PlayerStats>();
        if (playerStats == null)
        {
            Debug.LogError("PlayerStats bulunamadı!");
            return;
        }
        
        upgradePanel.gameObject.SetActive(true);
        upgradePanel.Show(playerStats);
        
        // UI Input Blocker ekleme işlemi Attributes panel tarafından yapılıyor
    }
    
    private void OnUpgradeSkillsButtonClicked()
    {
        gameObject.SetActive(false);
        
        if (UIInputBlocker.instance != null)
            UIInputBlocker.instance.RemovePanel(gameObject);
        
        if (skillTreePanel != null)
        {
            skillTreePanel.OpenPanel();
        }
        else
        {
            SkillTreePanel foundPanel = FindFirstObjectByType<SkillTreePanel>();
            if (foundPanel != null)
            {
                skillTreePanel = foundPanel;
                skillTreePanel.OpenPanel();
                if (UIInputBlocker.instance != null)
                    UIInputBlocker.instance.AddPanel(gameObject);
            }
            else
            {
                Debug.LogWarning("Skill Tree Panel bulunamadı! Lütfen inspector'da referansı atayın.");
            }
        }
    }
    
    public new void ShowPanel()
    {
        Canvas canvas = GetComponent<Canvas>();
        if (canvas != null)
            canvas.sortingOrder = 100;
        
        // Panel'i göster
        gameObject.SetActive(true);
        
        // Input blocking'i manuel olarak yap (BaseUIPanel otomatik yapmayabilir Player hazır değilse)
        StartCoroutine(EnsureInputBlocking());
        
        // Butonların erişilebilirliğini kontrol et
        if (restButton != null) restButton.interactable = true;
        if (upgradeButton != null) upgradeButton.interactable = true;
        if (closeButton != null) closeButton.interactable = true;
        if (upgradeSkillsButton != null) upgradeSkillsButton.interactable = true;
    }
    
    // Input blocking'in çalıştığından emin ol
    private System.Collections.IEnumerator EnsureInputBlocking()
    {
        int retryCount = 0;
        
        while (retryCount < 10) // 1 saniye boyunca deneyeceğiz
        {
            if (UIInputBlocker.instance != null)
            {
                Player player = PlayerManager.instance?.player;
                if (player != null && player.playerInput != null)
                {
                    UIInputBlocker.instance.AddPanel(gameObject);
                    UIInputBlocker.instance.DisableGameplayInput();
                    yield break; // Success, exit
                }
            }
            
            yield return new WaitForSeconds(0.1f);
            retryCount++;
        }
        
        Debug.LogError("CheckpointSelectionScreen: Failed to setup input blocking - Player not ready!");
    }
    
    public void ClosePanel()
    {
        gameObject.SetActive(false);
        
        if (UIInputBlocker.instance != null)
        {
            UIInputBlocker.instance.RemovePanel(gameObject);
            UIInputBlocker.instance.EnableGameplayInput(true);
        }
    }
    
    // Oyuncunun konumunu ve stat değerlerini checkpoint olarak kaydet
    private void SavePlayerPosition()
    {
        Player player = PlayerManager.instance?.player;
        if (player == null) return;
        
        // Checkpoint bilgilerini kaydet
        PlayerPrefs.SetInt("CheckpointActivated", 1);
        PlayerPrefs.SetFloat("CheckpointX", player.transform.position.x);
        PlayerPrefs.SetFloat("CheckpointY", player.transform.position.y);
        
        // Sahne indeksini kaydet
        int currentSceneIndex = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
        PlayerPrefs.SetInt("CheckpointSceneIndex", currentSceneIndex);
        
        // Oyuncu stat değerlerini kaydet
        PlayerStats playerStats = player.GetComponent<PlayerStats>();
        if (playerStats != null)
        {
            // Temel değerleri kaydet
            PlayerPrefs.SetInt("PlayerLevel", playerStats.GetLevel());
            PlayerPrefs.SetFloat("PlayerMaxHealth", playerStats.maxHealth.GetValue());
            PlayerPrefs.SetFloat("PlayerMaxMana", playerStats.maxMana.GetValue());
            PlayerPrefs.SetFloat("PlayerBaseDamage", playerStats.baseDamage.GetValue());
            PlayerPrefs.SetInt("PlayerSkillPoints", playerStats.AvailableSkillPoints);

            // Deneyim değerlerini kaydet
            System.Type type = playerStats.GetType();
            int experience = (int)type.GetField("experience", System.Reflection.BindingFlags.Instance | 
                                             System.Reflection.BindingFlags.NonPublic).GetValue(playerStats);
            int experienceToNextLevel = (int)type.GetField("experienceToNextLevel", System.Reflection.BindingFlags.Instance | 
                                                        System.Reflection.BindingFlags.NonPublic).GetValue(playerStats);
            
            PlayerPrefs.SetInt("PlayerExperience", experience);
            PlayerPrefs.SetInt("PlayerExperienceToNextLevel", experienceToNextLevel);
        }

        // Persist equipped weapons through EquipmentManager
        if (EquipmentManager.Instance != null)
        {
            EquipmentManager.Instance.SaveEquipment();
        }
        
        PlayerPrefs.Save();
    }
    
    // Oyuncunun can ve mana değerlerini doldur
    private void RestPlayer()
    {
        Player player = PlayerManager.instance?.player;
        if (player == null) return;
        
        PlayerStats playerStats = player.GetComponent<PlayerStats>();
        if (playerStats != null)
        {
           
            float oldMaxHealth = playerStats.maxHealth.GetValue();
            
            // SADECE seviyeyi, deneyimi ve skill puanlarını yükle, stat değerlerini modifiye etme
            if (PlayerPrefs.HasKey("PlayerLevel") && PlayerPrefs.HasKey("PlayerExperience"))
            {
                int savedLevel = PlayerPrefs.GetInt("PlayerLevel", 1);
                int savedExperience = PlayerPrefs.GetInt("PlayerExperience", 0);
                int savedExperienceToNextLevel = PlayerPrefs.GetInt("PlayerExperienceToNextLevel", 100);
                int savedSkillPoints = PlayerPrefs.GetInt("PlayerSkillPoints", 0);
                
                // Seviyeyi ayarla (diğer stat değerlerini değiştirmeden)
                System.Type type = playerStats.GetType();
                
                // Seviyeyi reflection ile ayarla
                type.GetField("level", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                    .SetValue(playerStats, savedLevel);
                
                // Deneyim ve skill point değerlerini ayarla
                type.GetField("experience", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                    .SetValue(playerStats, savedExperience);
                    
                type.GetField("experienceToNextLevel", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                    .SetValue(playerStats, savedExperienceToNextLevel);
                    
                type.GetField("availableSkillPoints", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                    .SetValue(playerStats, savedSkillPoints);
            }
            
            // Şu anki max health değerini al (modifiye etmeden)
            float maxHealthValue = playerStats.maxHealth.GetValue();
            
            // Current health'i direkt MAX health'e eşitle (Full can yap)
            playerStats.SetHealth(maxHealthValue);
            playerStats.currentMana = playerStats.maxMana.GetValue();
            
            // Health Bar'ı güncelle
            if (player.healthBar != null)
                player.healthBar.UpdateHealthBar(playerStats.currentHealth, playerStats.maxHealth.GetValue());
            else
            {
                // Force reinitialize health bar if reference is lost
                HealthBar healthBar = player.GetComponent<HealthBar>();
                if (healthBar != null)
                {
                    healthBar.ForceReinitialize();
                }
            }
            
            // Force mana bar update as well
            ManaBar manaBar = player.GetComponent<ManaBar>();
            if (manaBar != null)
            {
                manaBar.UpdateManaBar(playerStats.currentMana, playerStats.maxMana.GetValue());
            }
            
            // UI'ı güncelle
            playerStats.UpdateLevelUI();
            
           
            
            // Değişiklikleri kaydet
            SavePlayerPosition();
        }
        else if (player.stats != null)
        {
            // Basit iyileştirme
            float roundedMaxHealth = Mathf.Round(player.stats.maxHealth.GetValue());
            float roundedMaxMana = Mathf.Round(player.stats.maxMana.GetValue());
            
            // Can ve manayı doldur (Current health MAX health'e eşitle)
            player.stats.SetHealth(roundedMaxHealth);
            player.stats.currentMana = roundedMaxMana;
            
            if (player.healthBar != null)
                player.healthBar.UpdateHealthBar(player.stats.currentHealth, player.stats.maxHealth.GetValue());
            
        }
    }
    
    // Bu kod SceneTransitionTrigger sınıfından direkt kopyalandı ve uyarlandı
    private void StartSceneTransition()
    {
        // Önce oyuncuyu iyileştir
        RestPlayer();
        
        // UI paneli kapat
        gameObject.SetActive(false);
        if (UIInputBlocker.instance != null)
            UIInputBlocker.instance.RemovePanel(gameObject);
    
        // Oyuncu silahlarını geçici olarak gizle
        Player player = PlayerManager.instance?.player;
        if (player != null)
        {
            player.HideWeapons();
            
            // Hareketi durdur, kilitlenme olmaması için
            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
            }
        }
        
        
        // Rest butonundan dönüş olduğunu belirt - checkpoint pozisyonuna yerleşmesi için
        PlayerPrefs.SetInt("UseCheckpointRespawn", 1);
        PlayerPrefs.Save();
        
        // Şu anki sahne index'ini al
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        
            SceneManager.LoadScene(currentSceneIndex);
       
      
            
       
    }
} 