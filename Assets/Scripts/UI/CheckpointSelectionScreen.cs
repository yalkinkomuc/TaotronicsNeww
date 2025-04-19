using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CheckpointSelectionScreen : MonoBehaviour
{
    [Header("Butonlar")]
    [SerializeField] private Button restButton;
    [SerializeField] private Button upgradeButton;
    [SerializeField] private Button closeButton;
    
    [Header("Referanslar")]
    public UpgradePanel upgradePanel;
    
    private void Start()
    {
        // Buton listenerlarını ekle
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
    
    // Rest butonuna tıklandığında çağrılır
    private void OnRestButtonClicked()
    {
        // Seçim ekranını kapat
        gameObject.SetActive(false);
        
        // UI Input Blocker'dan bu paneli çıkar
        if (UIInputBlocker.instance != null)
        {
            UIInputBlocker.instance.RemovePanel(gameObject);
        }
        
        // Oyuncuyu iyileştir
        HealPlayer();
        
        // Sahne geçiş efektini başlat ve sahneyi yeniden yükle
        StartCoroutine(ReloadSceneWithTransition());
    }
    
    // Upgrade butonuna tıklandığında çağrılır
    private void OnUpgradeButtonClicked()
    {
        // Seçim ekranını kapat
        gameObject.SetActive(false);
        
        // UI Input Blocker'dan bu paneli çıkar
        if (UIInputBlocker.instance != null)
        {
            UIInputBlocker.instance.RemovePanel(gameObject);
        }
        
        // Upgrade panelini göster
        if (upgradePanel != null)
        {
            Player player = FindObjectOfType<Player>();
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
    
    // Paneli göster
    public void ShowPanel()
    {
        gameObject.SetActive(true);
        
        // UI Input Blocker'a ekle
        if (UIInputBlocker.instance != null)
        {
            UIInputBlocker.instance.AddPanel(gameObject);
        }
    }
    
    // Paneli kapat
    public void ClosePanel()
    {
        gameObject.SetActive(false);
        
        // UI Input Blocker'dan çıkar ve input'u geri etkinleştir
        if (UIInputBlocker.instance != null)
        {
            UIInputBlocker.instance.RemovePanel(gameObject);
            UIInputBlocker.instance.EnableGameplayInput(true);
            Debug.Log("CheckpointSelectionScreen kapandı: Oyun girdileri etkinleştirildi");
        }
    }
    
    // Oyuncuyu iyileştir (can ve mana doldur)
    private void HealPlayer()
    {
        Player player = FindObjectOfType<Player>();
        if (player != null)
        {
            // Can ve mana doldur
            player.stats.currentHealth = player.stats.maxHealth.GetValue();
            player.stats.currentMana = player.stats.maxMana.GetValue();
            
            // Health bar'ı güncelle
            if (player.healthBar != null)
            {
                player.healthBar.UpdateHealthBar(player.stats.currentHealth, player.stats.maxHealth.GetValue());
            }
        }
    }
    
    // Sahne geçiş efekti ile sahneyi yeniden yükle
    private System.Collections.IEnumerator ReloadSceneWithTransition()
    {
        // Geçiş efekti oluştur
        GameObject transitionEffectPrefab = Resources.Load<GameObject>("SceneTransitionEffect");
        if (transitionEffectPrefab != null)
        {
            Instantiate(transitionEffectPrefab);
        }
        else
        {
            // Manuel olarak geçiş efekti oluştur
            GameObject transitionObject = new GameObject("SceneTransitionEffect");
            transitionObject.AddComponent<SceneTransitionEffect>();
        }
        
        // Geçiş animasyonu için bekle
        yield return new WaitForSeconds(0.5f);
        
        // Mevcut sahneyi yeniden yükle
        Scene currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        UnityEngine.SceneManagement.SceneManager.LoadScene(currentScene.name);
    }

    private void OnDisable()
    {
        // Panel deaktif olduğunda input'u geri etkinleştir
        if (UIInputBlocker.instance != null)
        {
            // Paneli Input Blocker'dan çıkar
            UIInputBlocker.instance.RemovePanel(gameObject);
            
            // Input'u zorla etkinleştir
            UIInputBlocker.instance.EnableGameplayInput(true);
            Debug.Log("CheckpointSelectionScreen deaktif edildi: Oyun girdileri etkinleştirildi");
        }
    }
} 