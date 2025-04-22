using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CheckpointSelectionScreen : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button restButton;
    [SerializeField] private Button upgradeButton;
    [SerializeField] private Button closeButton;
    
    [Header("References")]
    public UpgradePanel upgradePanel;

    private void Awake()
    {
        if (Checkpoint.persistentSelectionScreen == null)
        {
            Checkpoint.persistentSelectionScreen = this;
            DontDestroyOnLoad(gameObject);
            
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
        if (restButton != null)
            restButton.onClick.AddListener(OnRestButtonClicked);
        
        if (upgradeButton != null)
            upgradeButton.onClick.AddListener(OnUpgradeButtonClicked);
        
        if (closeButton != null)
            closeButton.onClick.AddListener(ClosePanel);
    }
    
    private void OnRestButtonClicked()
    {
        StartCoroutine(ReloadSceneWithTransition());
    }
    
    private void OnUpgradeButtonClicked()
    {
        gameObject.SetActive(false);
        
        if (UIInputBlocker.instance != null)
            UIInputBlocker.instance.RemovePanel(gameObject);
        
        if (upgradePanel != null)
        {
            Player player = PlayerManager.instance?.player;
            if (player != null)
            {
                PlayerStats playerStats = player.GetComponent<PlayerStats>();
                if (playerStats != null)
                    upgradePanel.Show(playerStats);
            }
        }
    }
    
    public void ShowPanel()
    {
        Canvas canvas = GetComponent<Canvas>();
        if (canvas != null)
            canvas.sortingOrder = 100;
        
        if (UIInputBlocker.instance != null)
            UIInputBlocker.instance.AddPanel(gameObject);
        
        gameObject.SetActive(true);
        
        // Butonların erişilebilirliğini kontrol et
        if (restButton != null) restButton.interactable = true;
        if (upgradeButton != null) upgradeButton.interactable = true;
        if (closeButton != null) closeButton.interactable = true;
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
            // Kaydedilmiş stat değerlerini yükle
            if (PlayerPrefs.HasKey("PlayerMaxHealth") && PlayerPrefs.HasKey("PlayerMaxMana"))
            {
                // Stat değerlerini al
                float savedMaxHealth = PlayerPrefs.GetFloat("PlayerMaxHealth");
                float savedMaxMana = PlayerPrefs.GetFloat("PlayerMaxMana");
                float savedBaseDamage = PlayerPrefs.GetFloat("PlayerBaseDamage");
                int savedLevel = PlayerPrefs.GetInt("PlayerLevel", 1);
                int savedExperience = PlayerPrefs.GetInt("PlayerExperience", 0);
                int savedExperienceToNextLevel = PlayerPrefs.GetInt("PlayerExperienceToNextLevel", 100);
                int savedSkillPoints = PlayerPrefs.GetInt("PlayerSkillPoints", 0);
                
                // Seviyeyi ayarla
                if (savedLevel > 1)
                    playerStats.SetLevel(savedLevel);
                
                // Deneyim ve skill point değerlerini ayarla
                System.Type type = playerStats.GetType();
                type.GetField("experience", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                    .SetValue(playerStats, savedExperience);
                    
                type.GetField("experienceToNextLevel", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                    .SetValue(playerStats, savedExperienceToNextLevel);
                    
                type.GetField("availableSkillPoints", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                    .SetValue(playerStats, savedSkillPoints);
                
                // Değerleri karşılaştırıp farklıysa güncelle
                ApplyStatDifference(playerStats.maxHealth, savedMaxHealth);
                ApplyStatDifference(playerStats.maxMana, savedMaxMana);
                ApplyStatDifference(playerStats.baseDamage, savedBaseDamage);
            }
            
            // Can ve manayı doldur
            playerStats.currentHealth = playerStats.maxHealth.GetValue();
            playerStats.currentMana = playerStats.maxMana.GetValue();
            
            // Health Bar'ı güncelle
            if (player.healthBar != null)
                player.healthBar.UpdateHealthBar(playerStats.currentHealth, playerStats.maxHealth.GetValue());
            
            // UI'ı güncelle
            System.Type playerStatsType = playerStats.GetType();
            var updateLevelUIMethod = playerStatsType.GetMethod("UpdateLevelUI", System.Reflection.BindingFlags.Instance | 
                                                  System.Reflection.BindingFlags.NonPublic);
            if (updateLevelUIMethod != null)
                updateLevelUIMethod.Invoke(playerStats, null);
            
            // Değişiklikleri kaydet
            SavePlayerPosition();
        }
        else if (player.stats != null)
        {
            // Basit iyileştirme
            player.stats.currentHealth = player.stats.maxHealth.GetValue();
            player.stats.currentMana = player.stats.maxMana.GetValue();
            
            if (player.healthBar != null)
                player.healthBar.UpdateHealthBar(player.stats.currentHealth, player.stats.maxHealth.GetValue());
        }
    }
    
    // Stat değerlerini karşılaştırıp fark varsa uygula
    private void ApplyStatDifference(Stat stat, float savedValue)
    {
        float currentValue = stat.GetValue();
        if (savedValue > currentValue)
        {
            float diff = savedValue - currentValue;
            stat.AddModifier(diff, StatModifierType.Equipment);
        }
    }
    
    // Sahneyi yeniden yükle
    private System.Collections.IEnumerator ReloadSceneWithTransition()
    {
        RestPlayer();
        gameObject.SetActive(false);
        
        if (UIInputBlocker.instance != null)
            UIInputBlocker.instance.RemovePanel(gameObject);
        
        // Geçiş efekti oluştur
        GameObject transitionEffectPrefab = Resources.Load<GameObject>("SceneTransitionEffect");
        if (transitionEffectPrefab != null)
        {
            Instantiate(transitionEffectPrefab);
        }
        else
        {
            GameObject transitionObject = new GameObject("SceneTransitionEffect");
            transitionObject.AddComponent<SceneTransitionEffect>();
        }
        
        yield return new WaitForSeconds(0.5f);
        
        Scene currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        UnityEngine.SceneManagement.SceneManager.LoadScene(currentScene.name);
    }

    private void OnDisable()
    {
        if (UIInputBlocker.instance != null)
        {
            UIInputBlocker.instance.RemovePanel(gameObject);
            UIInputBlocker.instance.EnableGameplayInput(true);
        }
    }
} 