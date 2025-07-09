using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private Image healthBarFill;
    [SerializeField] private float smoothSpeed = 10f;
    [SerializeField] private TextMeshProUGUI healthText; // Health değerini göstermek için metin alanı

    private CharacterStats stats;
    private float targetFillAmount;
    private float currentFillAmount;
    private bool isInitialized = false;

    private void Start()
    {
        StartCoroutine(InitializeHealthBarWithRetry());
    }
    
    private void OnEnable()
    {
        // Component etkinleştirildiğinde veya yüklendiğinde çalışır
        if (!isInitialized)
        {
            StartCoroutine(InitializeHealthBarWithRetry());
        }
    }
    
    // Retry mechanism for initialization
    private IEnumerator InitializeHealthBarWithRetry()
    {
        int attempts = 0;
        int maxAttempts = 5;
        
        while (attempts < maxAttempts && !isInitialized)
        {
            attempts++;
            
            // Wait a bit before each attempt (except the first one)
            if (attempts > 1)
            {
                yield return new WaitForSeconds(0.2f);
            }
            
            Debug.Log($"HealthBar: Initialization attempt {attempts}/{maxAttempts}");
            
            if (InitializeHealthBar())
            {
                isInitialized = true;
                Debug.Log("HealthBar: Successfully initialized");
                break;
            }
        }
        
        if (!isInitialized)
        {
            Debug.LogWarning("HealthBar: Failed to initialize after maximum attempts");
        }
    }
    
    private bool InitializeHealthBar()
    {
        // Find CharacterStats reference
        if (stats == null)
        {
            stats = GetComponent<CharacterStats>();
            if (stats == null)
            {
                stats = GetComponentInParent<CharacterStats>();
            }
            // Eğer hala null ise PlayerManager'dan player'ı bul
            if (stats == null && PlayerManager.instance?.player != null)
            {
                stats = PlayerManager.instance.player.GetComponent<CharacterStats>();
                Debug.Log("HealthBar: Found stats from PlayerManager");
            }
        }

        if (stats == null)
        {
            Debug.LogWarning("HealthBar: CharacterStats not found");
            return false;
        }

        // Try to find UI components if not assigned
        if (healthBarFill == null)
        {
            // Try multiple strategies to find the health bar fill
            healthBarFill = FindHealthBarFill();
            if (healthBarFill == null)
            {
                Debug.LogWarning("HealthBar: healthBarFill not found");
                return false;
            }
        }

        // Initialize values and update UI
        if (stats != null && healthBarFill != null)
        {
            float roundedCurrentHealth = Mathf.Round(stats.currentHealth);
            float roundedMaxHealth = Mathf.Round(stats.maxHealth.GetValue());
            
            targetFillAmount = roundedCurrentHealth / roundedMaxHealth;
            currentFillAmount = targetFillAmount;
            healthBarFill.fillAmount = currentFillAmount;
            healthBarFill.color = Color.red;
            
            // Metin alanını güncelle
            UpdateHealthText(roundedCurrentHealth, roundedMaxHealth);
            
            Debug.Log($"HealthBar: Initialized with Health {roundedCurrentHealth}/{roundedMaxHealth}");
            return true;
        }

        return false;
    }
    
    // Multiple strategies to find health bar fill image
    private Image FindHealthBarFill()
    {
        // Strategy 1: Look for common names
        string[] possibleNames = { "HealthBarFill", "HealthBar_Fill", "Health_Fill", "HealthFill", "Fill" };
        
        foreach (string name in possibleNames)
        {
            GameObject obj = GameObject.Find(name);
            if (obj != null)
            {
                Image img = obj.GetComponent<Image>();
                if (img != null)
                {
                    Debug.Log($"HealthBar: Found fill image by name: {name}");
                    return img;
                }
            }
        }
        
        // Strategy 2: Look in children for Fill images
        Image[] childImages = GetComponentsInChildren<Image>();
        foreach (Image img in childImages)
        {
            if (img.name.ToLower().Contains("fill") || img.name.ToLower().Contains("health"))
            {
                Debug.Log($"HealthBar: Found fill image in children: {img.name}");
                return img;
            }
        }
        
        // Strategy 3: Look in parent for Fill images
        if (transform.parent != null)
        {
            Image[] parentImages = GetComponentsInParent<Image>();
            foreach (Image img in parentImages)
            {
                if (img.name.ToLower().Contains("fill") && img.name.ToLower().Contains("health"))
                {
                    Debug.Log($"HealthBar: Found fill image in parent: {img.name}");
                    return img;
                }
            }
        }
        
        return null;
    }

    private void LateUpdate()
    {
        if (stats == null || healthBarFill == null) 
        {
            // Try to reinitialize if references are lost
            if (!isInitialized)
            {
                StartCoroutine(InitializeHealthBarWithRetry());
            }
            return;
        }

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
        if (healthBarFill == null) 
        {
            Debug.LogWarning("HealthBar: Trying to update but healthBarFill is null. Attempting to find it...");
            healthBarFill = FindHealthBarFill();
            if (healthBarFill == null) return;
        }
        
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
    
    // InGameUI'dan referansları set et
    public void SetHealthBarFill(Image fill, TextMeshProUGUI text)
    {
        healthBarFill = fill;
        healthText = text;
        isInitialized = true; // Mark as initialized since references are explicitly set
        Debug.Log($"HealthBar: References set externally - Fill: {fill != null}, Text: {text != null}");
        
        // Hemen güncelle
        if (stats != null && healthBarFill != null)
        {
            UpdateHealthBar(stats.currentHealth, stats.maxHealth.GetValue());
        }
    }
    
    // Force re-initialization (can be called externally if needed)
    public void ForceReinitialize()
    {
        isInitialized = false;
        stats = null;
        StartCoroutine(InitializeHealthBarWithRetry());
    }
} 