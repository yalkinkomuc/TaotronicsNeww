using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class UIInputBlocker : MonoBehaviour
{
    public static UIInputBlocker instance;

    [Header("UI Panels")]
    [Tooltip("UI panels that should disable gameplay input")]
    [SerializeField] private List<GameObject> uiPanels = new List<GameObject>();
    
    [Header("UIBlocker")]
    [SerializeField] private GameObject uiBlockerPanel;
    
    [Header("Skill System")]
    [SerializeField] private GameObject skillTreePanel;
    
    // Active UI panel count
    private int activePanelCount = 0;
    
    private void Awake()
    {
        // Singleton pattern
        if (instance == null)
        {
            instance = this;
            Debug.Log("UIInputBlocker: Singleton instance created");
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Debug.LogWarning("UIInputBlocker: Duplicate instance destroyed");
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        // Clean up null/missing references from previous scenes
        CleanupNullReferences();
        
        // Add skill tree panel to the list if assigned and valid
        if (skillTreePanel != null && IsValidGameObject(skillTreePanel) && !uiPanels.Contains(skillTreePanel))
        {
            Debug.Log("Adding SkillTreePanel to UIInputBlocker list!");
            uiPanels.Add(skillTreePanel);
        }
        else if (skillTreePanel != null && !IsValidGameObject(skillTreePanel))
        {
            Debug.LogWarning("SkillTreePanel reference is destroyed, clearing it");
            skillTreePanel = null;
        }
        
        // Find and add all UI panels in current scene
        FindAndAddCurrentScenePanels();
        
        // Add tracker to each panel
        foreach (GameObject panel in uiPanels)
        {
            if (panel != null && panel.GetComponent<UIObjectTracker>() == null)
            {
                panel.AddComponent<UIObjectTracker>();
            }
        }
        
        // Reset input state on every scene load
        activePanelCount = 0;
        
        // Force enable gameplay input after a short delay to ensure Player is ready
        StartCoroutine(RestoreInputAfterSceneLoad());
        
        // Don't check initial state immediately - do it in coroutine after Player is ready
    }
    
    // Null/Missing referansları temizle
    private void CleanupNullReferences()
    {
        Debug.Log($"UIInputBlocker: Cleaning up null references. List count before: {uiPanels.Count}");
        
        // Geriye doğru iterate et ki remove işlemi güvenli olsun
        for (int i = uiPanels.Count - 1; i >= 0; i--)
        {
            GameObject panel = uiPanels[i];
            
            // Unity'de destroyed object kontrolü
            if (panel == null || !IsValidGameObject(panel))
            {
                Debug.Log($"UIInputBlocker: Removing null/destroyed reference at index {i}");
                uiPanels.RemoveAt(i);
            }
        }
        
        Debug.Log($"UIInputBlocker: List count after cleanup: {uiPanels.Count}");
    }
    
    // GameObject'in gerçekten valid olup olmadığını kontrol et
    private bool IsValidGameObject(GameObject obj)
    {
        if (obj == null) return false;
        
        try
        {
            // Eğer object destroyed ise bu işlemler exception fırlatır veya false döner
            return obj.gameObject != null && obj.name != null;
        }
        catch
        {
            return false;
        }
    }
    
    // Mevcut sahnedeki UI panellerini bul ve ekle
    private void FindAndAddCurrentScenePanels()
    {
        Debug.Log("UIInputBlocker: Searching for UI panels in current scene...");
        
        // Yaygın UI panel isimlerini ara
        string[] commonPanelNames = {
            "AttributesUpgradePanel", "SkillTreePanel", "InventoryPanel", 
            "ChestUI", "DialoguePanel", "PauseMenu", "SettingsPanel"
        };
        
        foreach (string panelName in commonPanelNames)
        {
            GameObject panel = GameObject.Find(panelName);
            if (panel != null && IsValidGameObject(panel) && !uiPanels.Contains(panel))
            {
                Debug.Log($"UIInputBlocker: Adding panel from current scene: {panelName}");
                uiPanels.Add(panel);
            }
        }
        
        // Tüm BaseUIPanel componentlerini bul
        BaseUIPanel[] allPanels = FindObjectsOfType<BaseUIPanel>();
        foreach (BaseUIPanel panelComponent in allPanels)
        {
            GameObject panelObj = panelComponent.gameObject;
            if (panelObj != null && IsValidGameObject(panelObj) && !uiPanels.Contains(panelObj))
            {
                Debug.Log($"UIInputBlocker: Adding BaseUIPanel: {panelObj.name}");
                uiPanels.Add(panelObj);
            }
        }
        
        Debug.Log($"UIInputBlocker: Found {uiPanels.Count} total panels after scene search");
    }
    
    // Sahne yüklendikten sonra input'u restore et
    private System.Collections.IEnumerator RestoreInputAfterSceneLoad()
    {
        // Player'ın sahne değişimi sonrası hazır olmasını bekle
        float waitTime = 0f;
        while (waitTime < 1f && (PlayerManager.instance?.player?.playerInput == null))
        {
            yield return new WaitForSeconds(0.1f);
            waitTime += 0.1f;
        }
        
        // Player hazır olduğunda panel kontrolünü yap
        CheckActivePanels();
        
        Debug.Log("UIInputBlocker: Input restored after scene load");
    }
    

    
    // Check UI panels
    private void CheckActivePanels()
    {
        activePanelCount = 0;
        
        Debug.Log("UIInputBlocker: Checking active panels...");
        
        // Check active/inactive state for each panel
        for (int i = uiPanels.Count - 1; i >= 0; i--)
        {
            GameObject panel = uiPanels[i];
            
            // Null/destroyed kontrolü - eğer null veya destroyed ise listeden çıkar
            if (panel == null || !IsValidGameObject(panel))
            {
                Debug.LogWarning($"UIInputBlocker: Removing null/destroyed panel at index {i}");
                uiPanels.RemoveAt(i);
                continue;
            }
            
            if (panel.activeInHierarchy)
            {
                activePanelCount++;
                Debug.Log($"UIInputBlocker: Active panel found: {panel.name}");
            }
        }
        
        Debug.Log($"UIInputBlocker: Total active panels: {activePanelCount}");
        
        // If there are panels, disable input
        if (activePanelCount > 0)
        {
            Debug.Log("UIInputBlocker: Disabling input due to active panels");
            DisableGameplayInput();
        }
        else
        {
            // Only enable input if Player is ready
            if (PlayerManager.instance?.player?.playerInput != null)
            {
                Debug.Log("UIInputBlocker: No active panels, enabling input");
                EnableGameplayInput(true);
            }
            else
            {
                Debug.Log("UIInputBlocker: Player not ready yet, postponing input enable");
            }
        }
    }
    
    // Called when a panel's visibility changes
    public void OnPanelVisibilityChanged(GameObject panel, bool isVisible)
    {
        if (isVisible)
        {
            activePanelCount++;
            if (activePanelCount == 1) // When first panel activates
            {
                DisableGameplayInput();
            }
        }
        else
        {
            if (activePanelCount > 0)
                activePanelCount--;
            
            if (activePanelCount == 0) // When all panels are closed
            {
                EnableGameplayInput(true); // Force enable
            }
        }
        
      //  Debug.Log($"Panel visibility changed: {(isVisible ? "Shown" : "Hidden")}. Active panel count: {activePanelCount}");
    }
    
    // Add panel to the list
    public void AddPanel(GameObject panel)
    {
        if (!uiPanels.Contains(panel))
        {
            uiPanels.Add(panel);
            
            if (panel.GetComponent<UIObjectTracker>() == null)
            {
                panel.AddComponent<UIObjectTracker>();
            }
            
            // If panel is active, disable input
            if (panel.activeInHierarchy)
            {
                OnPanelVisibilityChanged(panel, true);
            }
        }
    }
    
    // Remove panel from the list
    public void RemovePanel(GameObject panel)
    {
        if (uiPanels.Contains(panel))
        {
            // If active, decrease counter
            if (panel.activeInHierarchy)
            {
                OnPanelVisibilityChanged(panel, false);
            }
            else
            {
                // If not active, still decrease counter (for safety)
                if (activePanelCount > 0)
                    activePanelCount--;
            }
            
            uiPanels.Remove(panel);
            
           // Debug.Log($"Panel removed. Remaining active panel count: {activePanelCount}");
        }
    }
    
    // Show/hide UI blocker
    private void SetUIBlockerVisibility(bool isVisible)
    {
        if (uiBlockerPanel != null)
        {
            uiBlockerPanel.SetActive(isVisible);
        }
    }
    
    // ONLY disable GAMEPLAY INPUTS, do NOT affect UI inputs
    public void DisableGameplayInput()
    {
        Debug.Log("UIInputBlocker: DisableGameplayInput called");
        
        // Disable player inputs
        Player player = PlayerManager.instance?.player;
        if (player != null)
        {
            Debug.Log($"UIInputBlocker: Player found: {player.name}");
            IPlayerInput playerInput = player.playerInput;
            if (playerInput != null)
            {
                Debug.Log("UIInputBlocker: PlayerInput found, disabling gameplay input");
                // Only disable gameplay inputs
                playerInput.DisableGameplayInput();
            }
            else
            {
                Debug.LogError("UIInputBlocker: PlayerInput is NULL!");
                // Try to retry after a delay
                StartCoroutine(RetryDisableGameplayInput());
            }
        }
        else
        {
            Debug.LogError("UIInputBlocker: Player is NULL!");
            // Try to retry after a delay
            StartCoroutine(RetryDisableGameplayInput());
        }
        
        // Show UI Blocker
        SetUIBlockerVisibility(true);
    }
    
    // Player henüz hazır değilse tekrar dene
    private System.Collections.IEnumerator RetryDisableGameplayInput()
    {
        int retryCount = 0;
        while (retryCount < 5)
        {
            yield return new WaitForSeconds(0.1f);
            retryCount++;
            Debug.Log($"UIInputBlocker: Retrying DisableGameplayInput... Attempt {retryCount}");
            
            Player player = PlayerManager.instance?.player;
            if (player != null && player.playerInput != null)
            {
                Debug.Log("UIInputBlocker: Player and PlayerInput found on retry, disabling gameplay input");
                player.playerInput.DisableGameplayInput();
                yield break; // Success, exit coroutine
            }
        }
        
        Debug.LogError("UIInputBlocker: Failed to disable gameplay input after retries!");
    }
    
    // ONLY enable GAMEPLAY INPUTS
    public void EnableGameplayInput(bool forceEnable = false)
    {
        Debug.Log($"UIInputBlocker: EnableGameplayInput called - forceEnable: {forceEnable}, activePanelCount: {activePanelCount}");
        
        // If force enable is requested or there are no active panels
        if (forceEnable || activePanelCount <= 0)
        {
            // Enable player inputs with proper null checks
            if (PlayerManager.instance == null)
            {
                Debug.LogWarning("UIInputBlocker: PlayerManager.instance is NULL - skipping input enable");
                return;
            }
            
            Player player = PlayerManager.instance.player;
            if (player == null)
            {
                Debug.LogWarning("UIInputBlocker: Player is NULL - skipping input enable");
                return;
            }
            
            Debug.Log($"UIInputBlocker: Player found: {player.name}");
            IPlayerInput playerInput = player.playerInput;
            if (playerInput != null)
            {
                Debug.Log("UIInputBlocker: PlayerInput found, enabling all input");
                // Enable all inputs
                playerInput.EnableAllInput();
            }
            else
            {
                Debug.LogWarning("UIInputBlocker: PlayerInput is NULL - Player may not be fully initialized yet");
                return;
            }
            
            // Hide UI Blocker
            SetUIBlockerVisibility(false);
        }
        else
        {
            Debug.Log($"UIInputBlocker: Still {activePanelCount} active panels, input remains disabled");
        }
    }
    
    // Activate panel
    public void ShowPanel(GameObject panel)
    {
        if (panel != null)
        {
            // Add to list if not present
            if (!uiPanels.Contains(panel))
            {
                AddPanel(panel);
            }
            
            // Activate panel
            panel.SetActive(true);
        }
    }
    
    // Deactivate panel
    public void HidePanel(GameObject panel)
    {
        if (panel != null)
        {
            panel.SetActive(false);
        }
    }
    
    // Manuel olarak tüm listeyi temizle (debug için)
    public void ClearAllPanels()
    {
        Debug.Log("UIInputBlocker: Manually clearing all panels");
        uiPanels.Clear();
        skillTreePanel = null;
        activePanelCount = 0;
        EnableGameplayInput(true);
    }
} 