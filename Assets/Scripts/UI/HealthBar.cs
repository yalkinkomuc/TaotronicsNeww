using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private Image healthBarFill;
    [SerializeField] private float smoothSpeed = 10f;
    [SerializeField] private TextMeshProUGUI healthText; // Health değerini göstermek için metin alanı

    private CharacterStats stats;
    private float targetFillAmount;
    private float currentFillAmount;

    private void Start()
    {
        InitializeHealthBar();
    }
    
    private void OnEnable()
    {
        // Component etkinleştirildiğinde veya yüklendiğinde çalışır
        InitializeHealthBar();
    }
    
    private void InitializeHealthBar()
    {
        if (stats == null)
        {
            stats = GetComponent<CharacterStats>();
            if (stats == null)
            {
                stats = GetComponentInParent<CharacterStats>();
            }
        }

        if (stats == null)
        {
            Debug.LogError("HealthBar: CharacterStats component'i bulunamadı!");
            return;
        }

        if (healthBarFill == null)
        {
            Debug.LogError("HealthBar: Health Bar Fill referansı atanmamış!");
            return;
        }

        // Health bar'ı güncel değerlere göre ayarla
        if (stats != null)
        {
            float roundedCurrentHealth = Mathf.Round(stats.currentHealth);
            float roundedMaxHealth = Mathf.Round(stats.maxHealth.GetValue());
            
            targetFillAmount = roundedCurrentHealth / roundedMaxHealth;
            currentFillAmount = targetFillAmount;
            healthBarFill.fillAmount = currentFillAmount;
            
            // Metin alanını güncelle
            UpdateHealthText(roundedCurrentHealth, roundedMaxHealth);
        }
        else
        {
            currentFillAmount = 1f;
            targetFillAmount = 1f;
        }
        
        healthBarFill.color = Color.red;
    }

    private void LateUpdate()
    {
        if (stats == null || healthBarFill == null) return;

        // Tam sayılara yuvarla
        float roundedCurrentHealth = Mathf.Round(stats.currentHealth);
        float roundedMaxHealth = Mathf.Round(stats.maxHealth.GetValue());
        
        // LateUpdate kullanarak diğer health değişikliklerinden sonra güncelleyelim
        targetFillAmount = roundedCurrentHealth / roundedMaxHealth;
        currentFillAmount = Mathf.Lerp(currentFillAmount, targetFillAmount, Time.deltaTime * smoothSpeed);
        healthBarFill.fillAmount = currentFillAmount;
        
        // Metin alanını güncelle
        UpdateHealthText(roundedCurrentHealth, roundedMaxHealth);
    }

    public void UpdateHealthBar(float currentHealth, float maxHealth)
    {
        if (healthBarFill == null) return;
        
        // Tam sayılara yuvarla
        float roundedCurrentHealth = Mathf.Round(currentHealth);
        float roundedMaxHealth = Mathf.Round(maxHealth);
        
        targetFillAmount = roundedCurrentHealth / roundedMaxHealth;
        // Hızlı değişimler için currentFillAmount'ı da hemen güncelle
        currentFillAmount = targetFillAmount;
        healthBarFill.fillAmount = currentFillAmount;
        
        // Metin alanını güncelle
        UpdateHealthText(roundedCurrentHealth, roundedMaxHealth);
        
        
    }
    
    // Health metin alanını güncelle
    private void UpdateHealthText(float currentHealth, float maxHealth)
    {
        if (healthText != null)
        {
            // Tam sayıya çevirerek göster
            healthText.text = $"{(int)currentHealth}/{(int)maxHealth}";
        }
    }
} 