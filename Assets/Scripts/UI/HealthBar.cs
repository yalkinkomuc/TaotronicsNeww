using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private Image healthBarFill;
    [SerializeField] private float smoothSpeed = 10f;

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
            targetFillAmount = stats.currentHealth / stats.maxHealth.GetValue();
            currentFillAmount = targetFillAmount;
            healthBarFill.fillAmount = currentFillAmount;
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

        // LateUpdate kullanarak diğer health değişikliklerinden sonra güncelleyelim
        targetFillAmount = stats.currentHealth / stats.maxHealth.GetValue();
        currentFillAmount = Mathf.Lerp(currentFillAmount, targetFillAmount, Time.deltaTime * smoothSpeed);
        healthBarFill.fillAmount = currentFillAmount;
    }

    public void UpdateHealthBar(float currentHealth, float maxHealth)
    {
        if (healthBarFill == null) return;
        
        targetFillAmount = currentHealth / maxHealth;
        // Hızlı değişimler için currentFillAmount'ı da hemen güncelle
        currentFillAmount = targetFillAmount;
        healthBarFill.fillAmount = currentFillAmount;
        
        Debug.Log($"Health Bar güncellendi: {currentHealth}/{maxHealth} = {targetFillAmount}");
    }
} 