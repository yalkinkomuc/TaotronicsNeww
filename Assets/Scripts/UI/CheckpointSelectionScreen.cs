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
        // Close selection screen
        gameObject.SetActive(false);
        
        // Remove this panel from UIInputBlocker
        if (UIInputBlocker.instance != null)
        {
            UIInputBlocker.instance.RemovePanel(gameObject);
        }
        
        // Heal player
        HealPlayer();
        
        // Start scene transition effect and reload scene
        StartCoroutine(ReloadSceneWithTransition());
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
    
    // Heal player (fill health and mana)
    private void HealPlayer()
    {
        Player player = PlayerManager.instance.player;
        if (player != null)
        {
            // Fill health and mana
            player.stats.currentHealth = player.stats.maxHealth.GetValue();
            player.stats.currentMana = player.stats.maxMana.GetValue();
            
            // Update health bar
            if (player.healthBar != null)
            {
                player.healthBar.UpdateHealthBar(player.stats.currentHealth, player.stats.maxHealth.GetValue());
            }
        }
    }
    
    // Reload scene with transition effect
    private System.Collections.IEnumerator ReloadSceneWithTransition()
    {
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