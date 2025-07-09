using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class InGameUI : MonoBehaviour
{
    public static InGameUI instance;
    
    [Header("Health/Mana Bars")]
    [SerializeField] private HealthBar healthBar;
    [SerializeField] private ManaBar manaBar;
    
    [Header("UI References")]
    [SerializeField] private Image healthBarFill;
    [SerializeField] private Image manaBarFill;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private Image expBar;
    
    // Retry parameters for finding references
    private int maxRetryAttempts = 10;
    private float retryInterval = 0.5f;
    
    private void Awake()
    {
        // Singleton pattern
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("InGameUI: Singleton instance created");
            
            // Subscribe to scene loading events
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoadedEvent;
        }
        else
        {
            Debug.LogWarning("InGameUI: Duplicate instance destroyed");
            Destroy(gameObject);
            return;
        }
    }
    
    private void Start()
    {
        // Sahne yüklendikten sonra UI referanslarını yeniden bul
        StartCoroutine(RefreshUIReferencesWithRetry());
    }
    
    private void OnEnable()
    {
        // Sahne değişikliklerinde tetiklenir
        if (instance == this) // Only if this is the singleton instance
        {
            StartCoroutine(RefreshUIReferencesWithRetry());
        }
    }
    
    private void OnDestroy()
    {
        // Unsubscribe from events
        if (instance == this)
        {
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoadedEvent;
        }
    }
    
    // Called when a scene is loaded
    private void OnSceneLoadedEvent(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        Debug.Log($"InGameUI: Scene loaded - {scene.name}");
        // Wait a bit for scene to fully initialize before refreshing references
        StartCoroutine(RefreshUIReferencesWithRetry(1f));
    }
    
    // Retry mechanism for finding UI references
    private IEnumerator RefreshUIReferencesWithRetry(float initialDelay = 0f)
    {
        if (initialDelay > 0)
        {
            yield return new WaitForSeconds(initialDelay);
        }
        
        int attempts = 0;
        bool allReferencesFound = false;
        
        while (attempts < maxRetryAttempts && !allReferencesFound)
        {
            attempts++;
            Debug.Log($"InGameUI: Attempting to refresh UI references (attempt {attempts}/{maxRetryAttempts})");
            
            allReferencesFound = RefreshUIReferences();
            
            if (!allReferencesFound)
            {
                yield return new WaitForSeconds(retryInterval);
            }
        }
        
        if (!allReferencesFound)
        {
            Debug.LogWarning("InGameUI: Could not find all UI references after maximum retry attempts");
        }
        else
        {
            Debug.Log("InGameUI: All UI references successfully found and connected");
        }
    }
    
    public bool RefreshUIReferences()
    {
        Debug.Log("InGameUI: Refreshing UI references...");
        bool allFound = true;
        
        // HealthBar ve ManaBar scriptlerini bul
        if (healthBar == null)
        {
            healthBar = FindObjectOfType<HealthBar>();
            if (healthBar == null)
            {
                // Try to find on Player specifically
                Player player = PlayerManager.instance?.player;
                if (player != null)
                {
                    healthBar = player.GetComponent<HealthBar>();
                }
            }
        }
        
        if (manaBar == null)
        {
            manaBar = FindObjectOfType<ManaBar>();
            if (manaBar == null)
            {
                // Try to find on Player specifically
                Player player = PlayerManager.instance?.player;
                if (player != null)
                {
                    manaBar = player.GetComponent<ManaBar>();
                }
            }
        }
            
        // UI elementlerini isimle bul - with fallback strategies
        if (healthBarFill == null)
        {
            healthBarFill = FindImageByName("HealthBarFill", "HealthBar_Fill", "Health_Fill");
        }
        
        if (manaBarFill == null)
        {
            manaBarFill = FindImageByName("ManaBarFill", "ManaBar_Fill", "Mana_Fill");
        }
        
        if (healthText == null)
        {
            healthText = FindTextByName("HealthText", "Health_Text", "HealthBarText");
        }
        
        if (expBar == null)
        {
            expBar = FindImageByName("ExpBar", "ExperienceBar", "Experience_Fill");
        }
            
        // Check if critical references are found
        if (healthBar == null || healthBarFill == null)
        {
            Debug.LogWarning("InGameUI: Missing critical health bar references");
            allFound = false;
        }
        
        if (manaBar == null || manaBarFill == null)
        {
            Debug.LogWarning("InGameUI: Missing critical mana bar references");
            allFound = false;
        }
        
        // HealthBar ve ManaBar'a referansları ver (sadece bulunduysa)
        if (healthBar != null && healthBarFill != null)
        {
            healthBar.SetHealthBarFill(healthBarFill, healthText);
            Debug.Log("InGameUI: Health bar references successfully connected");
        }
        
        if (manaBar != null && manaBarFill != null)
        {
            manaBar.SetManaBarFill(manaBarFill);
            Debug.Log("InGameUI: Mana bar references successfully connected");
        }
            
        Debug.Log($"InGameUI: References status - HealthBar: {healthBar != null}, ManaBar: {manaBar != null}, HealthFill: {healthBarFill != null}, ManaFill: {manaBarFill != null}");
        
        return allFound;
    }
    
    // Helper method to find Image components by multiple possible names
    private Image FindImageByName(params string[] possibleNames)
    {
        foreach (string name in possibleNames)
        {
            GameObject obj = GameObject.Find(name);
            if (obj != null)
            {
                Image img = obj.GetComponent<Image>();
                if (img != null)
                {
                    Debug.Log($"InGameUI: Found Image component: {name}");
                    return img;
                }
            }
        }
        
        // Fallback: search in all Image components for names containing keywords
        Image[] allImages = FindObjectsOfType<Image>();
        foreach (Image img in allImages)
        {
            foreach (string name in possibleNames)
            {
                if (img.name.Contains(name.Replace("_", "").Replace("Bar", "").Replace("Fill", "")))
                {
                    Debug.Log($"InGameUI: Found Image component by partial match: {img.name}");
                    return img;
                }
            }
        }
        
        return null;
    }
    
    // Helper method to find TextMeshProUGUI components by multiple possible names
    private TextMeshProUGUI FindTextByName(params string[] possibleNames)
    {
        foreach (string name in possibleNames)
        {
            GameObject obj = GameObject.Find(name);
            if (obj != null)
            {
                TextMeshProUGUI text = obj.GetComponent<TextMeshProUGUI>();
                if (text != null)
                {
                    Debug.Log($"InGameUI: Found Text component: {name}");
                    return text;
                }
            }
        }
        
        return null;
    }
    
    // Public method to force refresh (can be called externally)
    public void ForceRefreshReferences()
    {
        StartCoroutine(RefreshUIReferencesWithRetry());
    }
    
    // Sahne geçişinden sonra çağrılacak
    public void OnSceneLoaded()
    {
        ForceRefreshReferences();
    }
}
