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
    
    private void Start()
    {
        // Add button listeners
        if (restButton != null)
        {
            restButton.onClick.AddListener(OnRestButtonClicked);
        }
        
        if (upgradeButton != null)
        {
            upgradeButton.onClick.AddListener(OnUpgradeButtonClicked);
        }
        
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(ClosePanel);
        }
    }
    
    // Called when Rest button is clicked
    private void OnRestButtonClicked()
    {
        // Önce coroutine'i başlat, sonra paneli kapat
        StartCoroutine(ReloadSceneWithTransition());
        
        // Coroutine içinde paneli kapatacağız, burada yapmıyoruz
    }
    
    // Called when Upgrade button is clicked
    private void OnUpgradeButtonClicked()
    {
        // Close selection screen
        gameObject.SetActive(false);
        
        // Remove this panel from UIInputBlocker
        if (UIInputBlocker.instance != null)
        {
            UIInputBlocker.instance.RemovePanel(gameObject);
        }
        
        // Show upgrade panel
        if (upgradePanel != null)
        {
            Player player = PlayerManager.instance.player;
            if (player != null)
            {
                PlayerStats playerStats = player.GetComponent<PlayerStats>();
                if (playerStats != null)
                {
                    upgradePanel.Show(playerStats);
                }
            }
        }
    }
    
    // Show panel
    public void ShowPanel()
    {
        gameObject.SetActive(true);
        
        // Add to UIInputBlocker
        if (UIInputBlocker.instance != null)
        {
            UIInputBlocker.instance.AddPanel(gameObject);
        }
    }
    
    // Close panel
    public void ClosePanel()
    {
        gameObject.SetActive(false);
        
        // Remove from UIInputBlocker and re-enable input
        if (UIInputBlocker.instance != null)
        {
            UIInputBlocker.instance.RemovePanel(gameObject);
            UIInputBlocker.instance.EnableGameplayInput(true);
        }
    }
    
    // Oyuncunun konumunu ve stat değerlerini checkpoint olarak kaydet
    private void SavePlayerPosition()
    {
        Player player = PlayerManager.instance.player;
        if (player != null)
        {
            // 1. Checkpoint aktif et
            PlayerPrefs.SetInt("CheckpointActivated", 1);
            
            // 2. Konumu kaydet
            PlayerPrefs.SetFloat("CheckpointX", player.transform.position.x);
            PlayerPrefs.SetFloat("CheckpointY", player.transform.position.y);
            
            // 3. Oyuncu stat değerlerini kaydet
            PlayerStats playerStats = player.GetComponent<PlayerStats>();
            if (playerStats != null)
            {
                // Seviye ve stat değerlerini kaydet
                PlayerPrefs.SetInt("PlayerLevel", playerStats.GetLevel());
                PlayerPrefs.SetFloat("PlayerMaxHealth", playerStats.maxHealth.GetValue());
                PlayerPrefs.SetFloat("PlayerMaxMana", playerStats.maxMana.GetValue());
                PlayerPrefs.SetFloat("PlayerBaseDamage", playerStats.baseDamage.GetValue());
                PlayerPrefs.SetInt("PlayerSkillPoints", playerStats.AvailableSkillPoints);
                
                Debug.Log($"Oyuncu değerleri kaydedildi: Seviye={playerStats.GetLevel()}, MaxHP={playerStats.maxHealth.GetValue()}, MaxMana={playerStats.maxMana.GetValue()}");
            }
            
            // 4. Değişiklikleri kaydet
            PlayerPrefs.Save();
            Debug.Log("Checkpoint kaydedildi: " + player.transform.position);
        }
    }
    
    // Oyuncunun canını ve manasını doldur, ve değerlerini yükle
    private void HealPlayer()
    {
        Player player = PlayerManager.instance.player;
        if (player != null)
        {
            PlayerStats playerStats = player.GetComponent<PlayerStats>();
            if (playerStats != null)
            {
                // 1. Kaydedilmiş stat değerlerini yükle (eğer varsa)
                if (PlayerPrefs.HasKey("PlayerMaxHealth") && PlayerPrefs.HasKey("PlayerMaxMana"))
                {
                    // Stat değerlerini yükle - Eğer bir upgrade yapıldıysa bu değerler farklı olacak
                    float savedMaxHealth = PlayerPrefs.GetFloat("PlayerMaxHealth");
                    float savedMaxMana = PlayerPrefs.GetFloat("PlayerMaxMana");
                    float savedBaseDamage = PlayerPrefs.GetFloat("PlayerBaseDamage");
                    int savedLevel = PlayerPrefs.GetInt("PlayerLevel", 1);
                    
                    // MaxHealth, MaxMana ve Damage değerlerini yükle
                    // Bu değerler level system veya upgrade panel tarafından değiştirilmiş olabilir
                    // Burada mevcut değerler yerine kaydedilmiş değerleri kullanıyoruz
                    
                    // Önce seviyeyi ayarla
                    if (savedLevel > 1)
                    {
                        playerStats.SetLevel(savedLevel);
                    }
                    
                    // Burada direkt modifierlara erişemediğimiz için farkları hesaplayıp ekliyoruz
                    float currentMaxHealth = playerStats.maxHealth.GetValue();
                    float currentMaxMana = playerStats.maxMana.GetValue();
                    float currentBaseDamage = playerStats.baseDamage.GetValue();
                    
                    // Eğer kaydedilen değerler mevcut değerlerden farklıysa, farkı ekle
                    if (savedMaxHealth > currentMaxHealth)
                    {
                        float healthDiff = savedMaxHealth - currentMaxHealth;
                        playerStats.maxHealth.AddModifier(healthDiff, StatModifierType.Equipment);
                        Debug.Log($"Max Health değeri güncellendi: {currentMaxHealth} -> {savedMaxHealth}");
                    }
                    
                    if (savedMaxMana > currentMaxMana)
                    {
                        float manaDiff = savedMaxMana - currentMaxMana;
                        playerStats.maxMana.AddModifier(manaDiff, StatModifierType.Equipment);
                        Debug.Log($"Max Mana değeri güncellendi: {currentMaxMana} -> {savedMaxMana}");
                    }
                    
                    if (savedBaseDamage > currentBaseDamage)
                    {
                        float damageDiff = savedBaseDamage - currentBaseDamage;
                        playerStats.baseDamage.AddModifier(damageDiff, StatModifierType.Equipment);
                        Debug.Log($"Base Damage değeri güncellendi: {currentBaseDamage} -> {savedBaseDamage}");
                    }
                }
                
                // 2. Can ve manayı doldur - Artık güncellenmiş max değerleri kullanacak
                playerStats.currentHealth = playerStats.maxHealth.GetValue();
                playerStats.currentMana = playerStats.maxMana.GetValue();
                
                // 3. Health Bar'ı güncelle
                if (player.healthBar != null)
                {
                    player.healthBar.UpdateHealthBar(playerStats.currentHealth, playerStats.maxHealth.GetValue());
                }
                
                Debug.Log($"Oyuncunun canı ve manası dolduruldu: HP={playerStats.currentHealth}/{playerStats.maxHealth.GetValue()}, Mana={playerStats.currentMana}/{playerStats.maxMana.GetValue()}");
            }
            else
            {
                // PlayerStats bulunamazsa basit iyileştirme yap
                player.stats.currentHealth = player.stats.maxHealth.GetValue();
                player.stats.currentMana = player.stats.maxMana.GetValue();
                
                if (player.healthBar != null)
                {
                    player.healthBar.UpdateHealthBar(player.stats.currentHealth, player.stats.maxHealth.GetValue());
                }
                
                Debug.Log("Oyuncunun canı ve manası dolduruldu (basit mod)");
            }
        }
    }
    
    // Reload scene with transition effect
    private System.Collections.IEnumerator ReloadSceneWithTransition()
    {
        // Önce heal işlemini yap
        HealPlayer();
        
        // Şimdi paneli kapat - Coroutine başlatıldıktan sonra güvenli
        gameObject.SetActive(false);
        
        // Remove this panel from UIInputBlocker
        if (UIInputBlocker.instance != null)
        {
            UIInputBlocker.instance.RemovePanel(gameObject);
        }
        
        // Create transition effect
        GameObject transitionEffectPrefab = Resources.Load<GameObject>("SceneTransitionEffect");
        if (transitionEffectPrefab != null)
        {
            Instantiate(transitionEffectPrefab);
        }
        else
        {
            // Manually create transition effect
            GameObject transitionObject = new GameObject("SceneTransitionEffect");
            transitionObject.AddComponent<SceneTransitionEffect>();
        }
        
        // Wait for transition animation
        yield return new WaitForSeconds(0.5f);
        
        // Reload current scene
        Scene currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        UnityEngine.SceneManagement.SceneManager.LoadScene(currentScene.name);
    }

    private void OnDisable()
    {
        // Re-enable input when panel is deactivated
        if (UIInputBlocker.instance != null)
        {
            // Remove panel from InputBlocker
            UIInputBlocker.instance.RemovePanel(gameObject);
            
            // Force enable input
            UIInputBlocker.instance.EnableGameplayInput(true);
        }
    }
} 