using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// UI-independent rune persistence manager
/// Managers klasörünün altında, DontDestroyOnLoad ile çalışır
/// </summary>
public class RuneSaveManager : MonoBehaviour
{
    public static RuneSaveManager Instance { get; private set; }
    
    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = true;
    
    private bool hasLoadedOnce = false; // Sadece 1 kere yüklesin
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            if (enableDebugLogs)
                Debug.Log("[RuneSaveManager] Initialized - DontDestroyOnLoad active");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        // Rune değişikliklerini dinle ve kaydet
        EquipmentManager.OnRuneChanged += HandleRuneChange;
    }

    private void OnDisable()
    {
        EquipmentManager.OnRuneChanged -= HandleRuneChange;
    }

    private void HandleRuneChange(int slotIndex, RuneData rune)
    {
        // Sadece yükleme tamamlandıktan sonra kaydetmeyi tetikle
        if (hasLoadedOnce)
        {
            SaveEquippedRunes();
        }
    }
    
    private void Start()
    {
        // Oyun başladığında rune'ları yükle
        StartCoroutine(LoadRunesOnGameStart());
    }
    
    /// <summary>
    /// Oyun başladığında rune'ları yükle
    /// </summary>
    private IEnumerator LoadRunesOnGameStart()
    {
        // EquipmentManager'ın hazır olmasını bekle
        yield return new WaitUntil(() => EquipmentManager.Instance != null);
        
        if (enableDebugLogs)
            Debug.Log("[RuneSaveManager] EquipmentManager found, loading runes...");
        
        LoadRunes();
        
        // UI'ların hazır olmasını bekle ve rune'ları UI'ya bildir
        yield return StartCoroutine(NotifyUIWhenReady());
    }
    
    /// <summary>
    /// Rune'ları PlayerPrefs'den yükle (sadece oyun başında 1 kere)
    /// </summary>
    private void LoadRunes()
    {
        if (hasLoadedOnce)
        {
            if (enableDebugLogs)
                Debug.Log("[RuneSaveManager] Already loaded runes once, skipping...");
            return;
        }
        
        if (EquipmentManager.Instance == null)
        {
            Debug.LogWarning("[RuneSaveManager] EquipmentManager.Instance is null - cannot load runes");
            return;
        }
        
        hasLoadedOnce = true;
        
        if (enableDebugLogs)
            Debug.Log("[RuneSaveManager] Loading runes from PlayerPrefs...");
        
        var equipmentManager = EquipmentManager.Instance;
        
        // Create arrays to hold loaded data
        RuneData[] loadedRunes = new RuneData[6]; // Assuming 6 rune slots
        Dictionary<int, int> loadedEnhancements = new Dictionary<int, int>();
        
        // Load runes from PlayerPrefs
        for (int i = 0; i < loadedRunes.Length; i++)
        {
            if (PlayerPrefs.HasKey($"EquippedRune_{i}"))
            {
                string runeName = PlayerPrefs.GetString($"EquippedRune_{i}");
                int enhancementLevel = PlayerPrefs.GetInt($"EquippedRune_{i}_Enhancement", 0);
                
                RuneData runeData = LoadRuneFromResources(runeName);
                if (runeData != null)
                {
                    loadedRunes[i] = runeData;
                    
                    if (enhancementLevel > 0)
                    {
                        loadedEnhancements[i] = enhancementLevel;
                    }
                    
                    if (enableDebugLogs)
                        Debug.Log($"[RuneSaveManager] Loaded rune at slot {i}: {runeName} (+{enhancementLevel})");
                }
                else
                {
                    Debug.LogWarning($"[RuneSaveManager] Could not load rune: {runeName}");
                }
            }
        }
        
        // Set loaded runes to EquipmentManager
        equipmentManager.SetLoadedRunes(loadedRunes, loadedEnhancements);
        
        // İlk girişse initial save yap (rune'lar yüklendikten sonra)
        if (EquipmentManager.IsFirstTimePlayer())
        {
            if (enableDebugLogs)
                Debug.Log("[RuneSaveManager] First time player - saving initial equipment state after rune load");
            
            // Hem ekipmanları hem de rune'ları kaydet
            EquipmentManager.Instance.SaveEquipment();
            SaveEquippedRunes(); // Rune'ları da ilk defa kaydet

            EquipmentManager.MarkGameAsStarted();
        }
        
        if (enableDebugLogs)
            Debug.Log("[RuneSaveManager] Runes loaded and stats recalculated");
    }
    
    /// <summary>
    /// UI'lar hazır olduğunda rune'ları bildir
    /// </summary>
    private IEnumerator NotifyUIWhenReady()
    {
        // UI'ların hazır olmasını bekle
        float maxWaitTime = 3.0f;
        float waitedTime = 0f;
        
        while (waitedTime < maxWaitTime)
        {
            if (AdvancedInventoryUI.Instance != null)
            {
                if (enableDebugLogs)
                    Debug.Log("[RuneSaveManager] UI ready, notifying about loaded runes...");
                break;
            }
            
            yield return new WaitForSeconds(0.2f);
            waitedTime += 0.2f;
        }
        
        if (AdvancedInventoryUI.Instance == null)
        {
            Debug.LogWarning("[RuneSaveManager] UI still not ready after waiting");
            yield break;
        }
        
        // UI'ya rune'ları bildir - EquipmentManager'daki public method'u kullan
        if (EquipmentManager.Instance != null)
        {
            EquipmentManager.Instance.NotifyUIAboutAllRunes();
        }
        
        if (enableDebugLogs)
            Debug.Log("[RuneSaveManager] UI notification completed");
    }
    
    /// <summary>
    /// Resources klasöründen rune'u yükle
    /// </summary>
    private RuneData LoadRuneFromResources(string runeName)
    {
        if (string.IsNullOrEmpty(runeName)) return null;
        
        // Önce direkt Resources/Items klasöründen dene
        RuneData directRune = Resources.Load<RuneData>($"Items/{runeName}");
        if (directRune != null)
        {
            return directRune;
        }
        
        // Resources'daki tüm RuneData'ları yükle ve ismine göre ara
        RuneData[] allRunes = Resources.LoadAll<RuneData>("Items");
        
        foreach (var rune in allRunes)
        {
            if (rune.itemName == runeName)
            {
                return rune;
            }
        }
        
        Debug.LogWarning($"[RuneSaveManager] Rune '{runeName}' not found in Resources/Items folder!");
        return null;
    }
    
    /// <summary>
    /// Takılı olan tüm rune'ları PlayerPrefs'e kaydeder.
    /// </summary>
    public void SaveEquippedRunes()
    {
        if (EquipmentManager.Instance == null)
        {
            Debug.LogWarning("[RuneSaveManager] EquipmentManager not found, cannot save runes.");
            return;
        }

        if (enableDebugLogs)
            Debug.Log($"[RuneSaveManager] 💾 SAVING RUNES...");
            
        for (int i = 0; i < 6; i++) // Assuming 6 rune slots
        {
            RuneData rune = EquipmentManager.Instance.GetEquippedRune(i);
            if (rune != null)
            {
                int enhancementLevel = EquipmentManager.Instance.GetRuneEnhancementLevel(i);
                PlayerPrefs.SetString($"EquippedRune_{i}", rune.itemName);
                PlayerPrefs.SetInt($"EquippedRune_{i}_Enhancement", enhancementLevel);
                if (enableDebugLogs)
                    Debug.Log($"[RuneSaveManager] ✅ SAVED rune at slot {i}: '{rune.itemName}' with enhancement +{enhancementLevel}");
            }
            else
            {
                PlayerPrefs.DeleteKey($"EquippedRune_{i}");
                PlayerPrefs.DeleteKey($"EquippedRune_{i}_Enhancement");
            }
        }
        
        PlayerPrefs.Save();
        if (enableDebugLogs)
            Debug.Log("[RuneSaveManager] Runes saved to PlayerPrefs.");
    }

    /// <summary>
    /// Debug: Print all equipped runes
    /// </summary>
    [ContextMenu("Debug: Print Equipped Runes")]
    public void DebugPrintEquippedRunes()
    {
        if (EquipmentManager.Instance != null)
        {
            EquipmentManager.Instance.DebugPrintEquippedRunes();
        }
    }
} 