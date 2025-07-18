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
            
            //DontDestroyOnLoad(gameObject);
        }
        else
        {
            
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
          
            uiPanels.Add(skillTreePanel);
        }
        else if (skillTreePanel != null && !IsValidGameObject(skillTreePanel))
        {
            
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
       
        
        // Geriye doğru iterate et ki remove işlemi güvenli olsun
        for (int i = uiPanels.Count - 1; i >= 0; i--)
        {
            GameObject panel = uiPanels[i];
            
            // Unity'de destroyed object kontrolü
            if (panel == null || !IsValidGameObject(panel))
            {
               
                uiPanels.RemoveAt(i);
            }
        }
        
        
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
        
        
        // Yaygın UI panel isimlerini ara
        string[] commonPanelNames = {
            "AttributesUpgradePanel", "SkillTreePanel", "InventoryPanel", 
            "ChestUI", "DialoguePanel", "PauseMenu", "SettingsPanel",
            "CollectiblesPanel", "UI_CollectiblesPanel"
        };
        
        foreach (string panelName in commonPanelNames)
        {
            GameObject panel = GameObject.Find(panelName);
            if (panel != null && IsValidGameObject(panel) && !uiPanels.Contains(panel))
            {
                
                uiPanels.Add(panel);
            }
        }
        
        // Tüm BaseUIPanel componentlerini bul (InGameUI elementlerini hariç tut)
        string[] excludedInGameUINames = { 
            "HealthBar", "ManaBar", "BossHealthBar", 
            "SkillScreenPanel", "TabManager", "InGameUI"
        };
        
        BaseUIPanel[] allPanels = FindObjectsByType<BaseUIPanel>(FindObjectsSortMode.None);
        foreach (BaseUIPanel panelComponent in allPanels)
        {
            GameObject panelObj = panelComponent.gameObject;
            
            // InGameUI elementlerini exclude et
            bool isExcluded = false;
            foreach (string excludedName in excludedInGameUINames)
            {
                if (panelObj.name.Contains(excludedName))
                {
                    isExcluded = true;
                    
                    break;
                }
            }
            
            if (!isExcluded && panelObj != null && IsValidGameObject(panelObj) && !uiPanels.Contains(panelObj))
            {
                
                uiPanels.Add(panelObj);
            }
        }
        
        
    }
    
    // Sahne yüklendikten sonra input'u restore et
    private IEnumerator RestoreInputAfterSceneLoad()
    {
        // Wait for all systems to initialize first
        yield return new WaitForSeconds(0.5f);
        
        // Player'ın sahne değişimi sonrası hazır olmasını bekle
        float waitTime = 0f;
        while (waitTime < 2f && (PlayerManager.instance?.player?.playerInput == null))
        {
            yield return new WaitForSeconds(0.1f);
            waitTime += 0.1f;
        }
        
        // Player hazır olduğunda panel kontrolünü yap
        CheckActivePanels();
        
       
    }
    

    
    // Check UI panels
    private void CheckActivePanels()
    {
        activePanelCount = 0;
        
        
        
        // Check active/inactive state for each panel
        for (int i = uiPanels.Count - 1; i >= 0; i--)
        {
            GameObject panel = uiPanels[i];
            
            // Null/destroyed kontrolü - eğer null veya destroyed ise listeden çıkar
            if (panel == null || !IsValidGameObject(panel))
            {
               
                uiPanels.RemoveAt(i);
                continue;
            }
            
            if (panel.activeInHierarchy)
            {
                activePanelCount++;
              
            }
        }
        
        
        
        // If there are panels, disable input (but only if Player is ready)
        if (activePanelCount > 0)
        {
            if (PlayerManager.instance?.player?.playerInput != null)
            {
                
                DisableGameplayInput();
            }
            else
            {
                Debug.Log("UIInputBlocker: Active panels found but Player not ready, skipping input disable");
            }
        }
        else
        {
            // Only enable input if Player is ready
            if (PlayerManager.instance?.player?.playerInput != null)
            {
               
                EnableGameplayInput(true);
            }
            else
            {
                
                // Force enable after delay if no Player found
                StartCoroutine(ForceEnableInputAfterDelay());
            }
        }
    }
    
    private IEnumerator ForceEnableInputAfterDelay()
    {
        yield return new WaitForSeconds(2f);
        
        // If still no active panels and Player still not ready, force enable
        if (activePanelCount == 0 && PlayerManager.instance?.player?.playerInput == null)
        {
            
            SetUIBlockerVisibility(false);
        }
    }
    
        // Called when a panel's visibility changes
    public void OnPanelVisibilityChanged(GameObject panel, bool isVisible)
    {
        // Double check the actual visibility state to prevent false triggers
        bool actuallyVisible = panel != null && panel.activeInHierarchy;
        
        if (isVisible && actuallyVisible)
        {
            activePanelCount++;
            if (activePanelCount == 1) // When first panel activates
            {
                // Only disable input if Player is ready
                if (PlayerManager.instance?.player?.playerInput != null)
                {
                    DisableGameplayInput();
                }
                else
                {
                    Debug.Log("UIInputBlocker: Panel activated but Player not ready, skipping input disable");
                }
            }
        }
        else if (!isVisible || !actuallyVisible)
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
        if (panel == null)
        {
            
            return;
        }
        

        
        // Check if panel should be excluded
        string[] excludedNames = { 
            "SkillScreenPanel", "TabManager", "InGameUI", 
            "HealthBar", "ManaBar", "BossHealthBar"
        };
        
        foreach (string excludedName in excludedNames)
        {
            if (panel.name.Contains(excludedName))
            {
               
                return;
            }
        }
        
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
        else
        {
            Debug.Log($"UIInputBlocker: Panel already in list, skipping: {panel.name}");
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
               
                StartCoroutine(RetryDisableGameplayInput());
            }
        }
        else
        {
            StartCoroutine(RetryDisableGameplayInput());
        }
        
        // Show UI Blocker
        SetUIBlockerVisibility(true);
        // Oyun duraklat
        Time.timeScale = 0f;
    }
    
    // Player henüz hazır değilse tekrar dene
    private IEnumerator RetryDisableGameplayInput()
    {
        int retryCount = 0;
        while (retryCount < 5)
        {
            yield return new WaitForSeconds(0.1f);
            retryCount++;
            
            // Only log on first and last attempts to reduce log spam
            if (retryCount == 1 || retryCount == 5)
            {
                Debug.Log($"UIInputBlocker: Retrying DisableGameplayInput... Attempt {retryCount}/5");
            }
            
            Player player = PlayerManager.instance?.player;
            if (player != null && player.playerInput != null)
            {
               
               
                yield break; // Success, exit coroutine
            }
        }
        
        // Only log as warning since this might be expected during initialization
        
    }
    
    // ONLY enable GAMEPLAY INPUTS
    public void EnableGameplayInput(bool forceEnable = false)
    {
       
        
        // If force enable is requested or there are no active panels
        if (forceEnable || activePanelCount <= 0)
        {
            // Enable player inputs with proper null checks
            if (PlayerManager.instance == null)
            {
                return;
            }
            
            Player player = PlayerManager.instance.player;
            if (player == null)
            {
                return;
            }
            
            
            IPlayerInput playerInput = player.playerInput;
            if (playerInput != null)
            {
                Debug.Log("UIInputBlocker: PlayerInput found, enabling all input");
                // Enable all inputs
                playerInput.EnableGameplayInput();
            }
            else
            {
                return;
            }
            
            // Hide UI Blocker
            SetUIBlockerVisibility(false);
            // Oyun devam etsin
            Time.timeScale = 1f;
        }
        else
        {
            Debug.Log($"UIInputBlocker: Still {activePanelCount} active panels, input remains disabled");
        }
    }
    
  
   
} 