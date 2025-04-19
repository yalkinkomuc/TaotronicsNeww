using UnityEngine;
using System.Collections.Generic;

public class UIInputBlocker : MonoBehaviour
{
    public static UIInputBlocker instance;

    [Header("UI Paneller")]
    [Tooltip("İnput'u devre dışı bırakması gereken UI paneller")]
    [SerializeField] private List<GameObject> uiPanels = new List<GameObject>();
    
    [Header("UIBlocker")]
    [SerializeField] private GameObject uiBlockerPanel;
    
    // Aktif UI panel sayısı
    private int activePanelCount = 0;
    
    private void Awake()
    {
        // Singleton pattern
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        // Her panel için tracker ekle
        foreach (GameObject panel in uiPanels)
        {
            if (panel != null && panel.GetComponent<UIObjectTracker>() == null)
            {
                panel.AddComponent<UIObjectTracker>();
            }
        }
        
        // Input durumunu sıfırla
        activePanelCount = 0;
        EnableGameplayInput(true);
        
        // Başlangıç durumunu kontrol et
        CheckActivePanels();
    }
    
    // UI panelleri kontrol et
    private void CheckActivePanels()
    {
        activePanelCount = 0;
        
        // Her panel için aktif/pasif durumunu kontrol et
        foreach (GameObject panel in uiPanels)
        {
            if (panel != null && panel.activeInHierarchy)
            {
                activePanelCount++;
            }
        }
        
        // Panel varsa input'u devre dışı bırak
        if (activePanelCount > 0)
        {
            DisableGameplayInput();
        }
        else
        {
            EnableGameplayInput(true);
        }
    }
    
    // Bir panel'in görünürlüğü değiştiğinde çağrılır
    public void OnPanelVisibilityChanged(GameObject panel, bool isVisible)
    {
        if (isVisible)
        {
            activePanelCount++;
            if (activePanelCount == 1) // İlk panel aktifleştiğinde
            {
                DisableGameplayInput();
            }
        }
        else
        {
            if (activePanelCount > 0)
                activePanelCount--;
            
            if (activePanelCount == 0) // Tüm paneller kapandığında
            {
                EnableGameplayInput(true); // Zorla etkinleştir
            }
        }
        
        // Debug bilgisi
        Debug.Log($"Panel görünürlüğü değişti: {(isVisible ? "Gösterildi" : "Gizlendi")}. Aktif panel sayısı: {activePanelCount}");
    }
    
    // Panel listesine panel ekle
    public void AddPanel(GameObject panel)
    {
        if (!uiPanels.Contains(panel))
        {
            uiPanels.Add(panel);
            
            if (panel.GetComponent<UIObjectTracker>() == null)
            {
                panel.AddComponent<UIObjectTracker>();
            }
            
            // Eğer panel aktifse input'u devre dışı bırak
            if (panel.activeInHierarchy)
            {
                OnPanelVisibilityChanged(panel, true);
            }
        }
    }
    
    // Panel listesinden panel çıkar
    public void RemovePanel(GameObject panel)
    {
        if (uiPanels.Contains(panel))
        {
            // Eğer aktifse sayaçtan düş
            if (panel.activeInHierarchy)
            {
                OnPanelVisibilityChanged(panel, false);
            }
            else
            {
                // Panel aktif değilse de sayaçtan düş (güvenlik için)
                if (activePanelCount > 0)
                    activePanelCount--;
            }
            
            uiPanels.Remove(panel);
            
            // Debug bilgisi
            Debug.Log($"Panel kaldırıldı. Kalan aktif panel sayısı: {activePanelCount}");
        }
    }
    
    // UI blocker'ı göster/gizle
    private void SetUIBlockerVisibility(bool isVisible)
    {
        if (uiBlockerPanel != null)
        {
            uiBlockerPanel.SetActive(isVisible);
        }
    }
    
    // SADECE OYUN GİRDİLERİNİ devre dışı bırak, UI girdilerini ETKİLEME
    public void DisableGameplayInput()
    {
        Debug.Log("UI aktif: Oyun girdileri devre dışı bırakıldı");
        
        // Player girdilerini devre dışı bırak
        Player player = FindObjectOfType<Player>();
        if (player != null)
        {
            IPlayerInput playerInput = player.playerInput;
            if (playerInput != null)
            {
                // Sadece oyun input'larını devre dışı bırak
                playerInput.DisableGameplayInput();
            }
        }
        
        // UI Blocker'ı göster
        SetUIBlockerVisibility(true);
    }
    
    // SADECE OYUN GİRDİLERİNİ etkinleştir
    public void EnableGameplayInput(bool forceEnable = false)
    {
        // Eğer zorla etkinleştirme isteniyorsa veya aktif panel yoksa
        if (forceEnable || activePanelCount <= 0)
        {
            Debug.Log("UI kapatıldı: Oyun girdileri etkinleştirildi");
            
            // Player girdilerini etkinleştir
            Player player = FindObjectOfType<Player>();
            if (player != null)
            {
                IPlayerInput playerInput = player.playerInput;
                if (playerInput != null)
                {
                    // Tüm input'ları etkinleştir
                    playerInput.EnableAllInput();
                }
            }
            
            // UI Blocker'ı gizle
            SetUIBlockerVisibility(false);
        }
        else
        {
            Debug.Log($"Hala {activePanelCount} adet aktif panel var, input etkinleştirilmedi");
        }
    }
    
    // Panel aktif et
    public void ShowPanel(GameObject panel)
    {
        if (panel != null)
        {
            // Listede yoksa ekle
            if (!uiPanels.Contains(panel))
            {
                AddPanel(panel);
            }
            
            // Paneli aktif et
            panel.SetActive(true);
        }
    }
    
    // Panel pasif et
    public void HidePanel(GameObject panel)
    {
        if (panel != null)
        {
            panel.SetActive(false);
        }
    }
} 