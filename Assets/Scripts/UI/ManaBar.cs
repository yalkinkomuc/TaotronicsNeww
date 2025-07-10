using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ManaBar : MonoBehaviour
{
    [SerializeField] private Image manaBarFill;
    [SerializeField] private float smoothSpeed = 10f;

    private CharacterStats stats;
    private float targetFillAmount;
    private float currentFillAmount;
    private bool isInitialized = false;

    private void Awake()
    {
        StartCoroutine(InitializeComponentsWithRetry());
    }

    private void Start()
    {
        StartCoroutine(InitializeComponentsWithRetry());
        
        if (stats != null && manaBarFill != null)
        {
            currentFillAmount = stats.currentMana / stats.maxMana.GetValue();
            targetFillAmount = currentFillAmount;
            manaBarFill.fillAmount = currentFillAmount;
            manaBarFill.color = Color.blue;
        }
    }
    
    private void OnEnable()
    {
        // Component etkinleştirildiğinde veya yüklendiğinde çalışır
        if (!isInitialized)
        {
            StartCoroutine(InitializeComponentsWithRetry());
        }
    }
    
    // Retry mechanism for initialization
    private IEnumerator InitializeComponentsWithRetry()
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
            
           // Debug.Log($"ManaBar: Initialization attempt {attempts}/{maxAttempts}");
            
            if (InitializeComponents())
            {
                isInitialized = true;
               // Debug.Log("ManaBar: Successfully initialized");
                break;
            }
        }
        
        if (!isInitialized)
        {
            Debug.LogWarning("ManaBar: Failed to initialize after maximum attempts");
        }
    }
    
    private bool InitializeComponents()
    {
        bool foundStats = false;
        bool foundFill = false;
        
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
                Debug.Log("ManaBar: Found stats from PlayerManager");
            }
        }

        if (stats == null)
        {
            Debug.LogWarning("ManaBar: CharacterStats not found");
        }
        else
        {
            foundStats = true;
        }

        // Try to find UI components if not assigned
        if (manaBarFill == null)
        {
            manaBarFill = FindManaBarFill();
            if (manaBarFill == null)
            {
                Debug.LogWarning("ManaBar: manaBarFill not found");
            }
            else
            {
                foundFill = true;
            }
        }
        else
        {
            foundFill = true;
        }
        
        return foundStats && foundFill;
    }
    
    // Multiple strategies to find mana bar fill image
    private Image FindManaBarFill()
    {
        // Strategy 1: Look for common names
        string[] possibleNames = { "ManaBarFill", "ManaBar_Fill", "Mana_Fill", "ManaFill", "Fill" };
        
        foreach (string name in possibleNames)
        {
            GameObject obj = GameObject.Find(name);
            if (obj != null)
            {
                Image img = obj.GetComponent<Image>();
                if (img != null)
                {
                    //Debug.Log($"ManaBar: Found fill image by name: {name}");
                    return img;
                }
            }
        }
        
        // Strategy 2: Look in children for Fill images
        Image[] childImages = GetComponentsInChildren<Image>();
        foreach (Image img in childImages)
        {
            if (img.name.ToLower().Contains("fill") || img.name.ToLower().Contains("mana"))
            {
                //Debug.Log($"ManaBar: Found fill image in children: {img.name}");
                return img;
            }
        }
        
        // Strategy 3: Look in parent for Fill images
        if (transform.parent != null)
        {
            Image[] parentImages = GetComponentsInParent<Image>();
            foreach (Image img in parentImages)
            {
                if (img.name.ToLower().Contains("fill") && img.name.ToLower().Contains("mana"))
                {
                   // Debug.Log($"ManaBar: Found fill image in parent: {img.name}");
                    return img;
                }
            }
        }
        
        return null;
    }

    private void Update()
    {
        if (stats == null || manaBarFill == null) 
        {
            // Try to reinitialize if references are lost
            if (!isInitialized)
            {
                StartCoroutine(InitializeComponentsWithRetry());
            }
            return;
        }

        targetFillAmount = Mathf.Clamp01(stats.currentMana / stats.maxMana.GetValue());
        currentFillAmount = Mathf.Lerp(currentFillAmount, targetFillAmount, Time.deltaTime * smoothSpeed);
        manaBarFill.fillAmount = currentFillAmount;
    }

    public void UpdateManaBar(float currentMana, float maxMana)
    {
        if (manaBarFill == null) 
        {
           // Debug.LogWarning("ManaBar: Trying to update but manaBarFill is null. Attempting to find it...");
            manaBarFill = FindManaBarFill();
            if (manaBarFill == null) return;
        }
        
        targetFillAmount = Mathf.Clamp01(currentMana / maxMana);
        manaBarFill.fillAmount = Mathf.Lerp(manaBarFill.fillAmount, targetFillAmount, Time.deltaTime * smoothSpeed * 3f);
        
        //Debug.Log($"ManaBar updated: {currentMana}/{maxMana} = {targetFillAmount}");
    }
    
    // InGameUI'dan referansları set et
    public void SetManaBarFill(Image fill)
    {
        manaBarFill = fill;
        isInitialized = true; // Mark as initialized since references are explicitly set
       // Debug.Log($"ManaBar: References set externally - Fill: {fill != null}");
        
        // Hemen güncelle
        if (stats != null && manaBarFill != null)
        {
            UpdateManaBar(stats.currentMana, stats.maxMana.GetValue());
        }
    }
    
    // Force re-initialization (can be called externally if needed)
    public void ForceReinitialize()
    {
        isInitialized = false;
        stats = null;
        StartCoroutine(InitializeComponentsWithRetry());
    }
} 