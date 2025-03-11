using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BossHealthBar : MonoBehaviour
{
    [SerializeField] private Slider healthSlider;
    [SerializeField] private TextMeshProUGUI bossNameText;
    [SerializeField] private Image fillImage;
    [SerializeField] private Gradient healthGradient;
    
    private CharacterStats bossStats;
    private int maxHealth;
    
    public void SetupBossHealth(CharacterStats stats, string bossName)
    {
        bossStats = stats;
        maxHealth = stats.maxHealth.GetValue();
        
        // Slider'ı ayarla
        healthSlider.maxValue = maxHealth;
        healthSlider.value = stats.currentHealth;
        
        // Boss adını ayarla
        if (bossNameText != null)
            bossNameText.text = bossName;
            
        // Health bar'ı aktif et
        gameObject.SetActive(true);
        
        UpdateHealthBar();
    }
    
    private void Update()
    {
        if (bossStats != null)
        {
            UpdateHealthBar();
        }
    }
    
    private void UpdateHealthBar()
    {
        healthSlider.value = bossStats.currentHealth;
        
        // Renk gradientini uygula
        if (fillImage != null && healthGradient != null)
        {
            fillImage.color = healthGradient.Evaluate(healthSlider.normalizedValue);
        }
    }
    
    public void HideBossHealthBar()
    {
        gameObject.SetActive(false);
    }
} 