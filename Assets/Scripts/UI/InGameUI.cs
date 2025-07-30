using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class InGameUI : MonoBehaviour
{
    [Header("UI Debug")]
    [SerializeField] private bool enableUIDebug = false;
    [SerializeField] private KeyCode debugKey = KeyCode.F12;
    
    public static InGameUI instance;

    [Obsolete("Obsolete")]
    private void Awake()
    {

        if (instance == null)
        {
            instance = this;
        }
        
    }

    public void CloseInGameUI()
    {
        gameObject.SetActive(false);
    }

    public void OpenInGameUI()
    {
        gameObject.SetActive(true);
    }

    private void Update()
    {
        // UI debug tuşu kontrolü
        if (enableUIDebug && Input.GetKeyDown(debugKey))
        {
            DebugUISystem();
        }
    }
    
    // ReSharper disable Unity.PerformanceAnalysis
    [Obsolete("Obsolete")]
    private void DebugUISystem()
    {
        Debug.Log("=== UI SYSTEM DEBUG ===");
        
        // UISystemManager bilgilerini göster
        if (UISystemManager.instance != null)
        {
            UISystemManager.instance.LogUISystemInfo();
        }
        else
        {
            Debug.LogWarning("UISystemManager bulunamadı!");
        }
        
        // BaseUIPanel'lerin durumunu kontrol et
        BaseUIPanel[] allPanels = FindObjectsOfType<BaseUIPanel>();
        Debug.Log($"Aktif BaseUIPanel sayısı: {allPanels.Length}");
        
        foreach (BaseUIPanel panel in allPanels)
        {
            if (panel.gameObject.activeInHierarchy)
            {
                bool isSafe = panel.IsInSafePosition();
                Debug.Log($"Panel '{panel.name}': Aktif, Güvenli pozisyon: {isSafe}");
                
                if (!isSafe)
                {
                    Debug.LogWarning($"Panel '{panel.name}' güvenli pozisyonda değil! Düzeltiliyor...");
                    panel.ForceToSafePosition();
                }
            }
        }
        
        // Canvas'ları kontrol et
        Canvas[] allCanvases = FindObjectsOfType<Canvas>();
        Debug.Log($"Toplam Canvas sayısı: {allCanvases.Length}");
        
        foreach (Canvas canvas in allCanvases)
        {
            RectTransform rectTransform = canvas.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                Vector3[] corners = new Vector3[4];
                rectTransform.GetWorldCorners(corners);
                
                Debug.Log($"Canvas '{canvas.name}': " +
                         $"RenderMode={canvas.renderMode}, " +
                         $"SortingOrder={canvas.sortingOrder}, " +
                         $"Size={rectTransform.rect.size}");
            }
        }
    }
}
