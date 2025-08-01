using UnityEngine;
using System.Collections;

public class BaseUIPanel : MonoBehaviour
{
    [Header("UI Safety")]
    [SerializeField] private bool enforceScreenBounds = true;
    [SerializeField] private bool delayedInputBlocking = true;
    [SerializeField] private float inputBlockingDelay = 0.1f;
    
    private RectTransform rectTransform;
    private Canvas parentCanvas;
    private InGameUI[] inGameUIElements;
    
    protected virtual void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        parentCanvas = GetComponentInParent<Canvas>();
        
        // UI pozisyonunu güvenli hale getir
        if (enforceScreenBounds)
        {
            EnsureSafeUIPosition();
        }
    }
    
    protected virtual void OnEnable()
    {
        // UI pozisyon güvenliği
        if (enforceScreenBounds)
        {
            StartCoroutine(EnsureSafePositionAfterFrame());
        }
        
        // Input blocking'i gecikmeli yap
        if (delayedInputBlocking)
        {
            StartCoroutine(DelayedInputBlocking());
        }
        else
        {
            // Hemen input blocking yap
            HandleInputBlocking();
        }
        
        // InGameUI elementlerini gizle
        HideInGameUIElements();
    }

    protected virtual void OnDisable()
    {
        if (UIInputBlocker.instance != null)
        {
            UIInputBlocker.instance.RemovePanel(gameObject);
        }
        
        // InGameUI elementlerini tekrar göster
        ShowInGameUIElements();
    }
    
    // InGameUI elementlerini gizle
    private void HideInGameUIElements()
    {
        inGameUIElements = FindObjectsByType<InGameUI>(FindObjectsSortMode.None);
        foreach (InGameUI ui in inGameUIElements)
        {
            if (ui != null && ui.gameObject.activeInHierarchy)
            {
                ui.gameObject.SetActive(false);
            }
        }
    }
    
    // InGameUI elementlerini tekrar göster
    private void ShowInGameUIElements()
    {
        if (inGameUIElements != null)
        {
            foreach (InGameUI ui in inGameUIElements)
            {
                if (ui != null)
                {
                    ui.gameObject.SetActive(true);
                }
            }
        }
    }
    
    private IEnumerator DelayedInputBlocking()
    {
        yield return new WaitForSeconds(inputBlockingDelay);
        HandleInputBlocking();
    }
    
    private void HandleInputBlocking()
    {
        // Sadece Player ve PlayerInput hazırsa input blocking yap
        if (UIInputBlocker.instance != null)
        {
            UIInputBlocker.instance.AddPanel(gameObject);
        }
        else if (UIInputBlocker.instance != null)
        {
            // Player henüz hazır değil, manuel olarak input blocking yapmayı bekle
            StartCoroutine(WaitForPlayerAndBlock());
        }
    }
    
    private IEnumerator WaitForPlayerAndBlock()
    {
        float waitTime = 0f;
        const float maxWaitTime = 2f;
        
        while (waitTime < maxWaitTime)
        {
            yield return new WaitForSeconds(0.1f);
            waitTime += 0.1f;
        }
        
        // Player hazır olduğunda input blocking yap
        if ( UIInputBlocker.instance != null)
        {
            UIInputBlocker.instance.AddPanel(gameObject);
        }
    }
    
    // UI pozisyonunu ekran sınırları içinde tut
    private void EnsureSafeUIPosition()
    {
        if (rectTransform == null || parentCanvas == null) return;
        
        // Canvas overlay modunda değilse kontrol etme
        if (parentCanvas.renderMode != RenderMode.ScreenSpaceOverlay) return;
        
        // RectTransform pozisyonunu kontrol et
        Vector3[] corners = new Vector3[4];
        rectTransform.GetWorldCorners(corners);
        
        // Screen space'e çevir
        Camera cam = parentCanvas.worldCamera ?? Camera.main;
        if (cam == null) return;
        
        bool needsAdjustment = false;
        for (int i = 0; i < corners.Length; i++)
        {
            Vector3 screenPos = cam.WorldToScreenPoint(corners[i]);
            if (!UISystemManager.IsPositionSafeForUI(screenPos))
            {
                needsAdjustment = true;
                break;
            }
        }
        
        if (needsAdjustment)
        {
            // Pozisyonu güvenli hale getir
            Vector3 currentPos = rectTransform.anchoredPosition;
            
            // Ekran merkezine yakın bir pozisyona taşı
            Vector2 safePosition = Vector2.zero;
            rectTransform.anchoredPosition = safePosition;
            
            Debug.LogWarning($"BaseUIPanel: '{gameObject.name}' was repositioned to prevent screen bounds error. " +
                           $"Original pos: {currentPos}, New pos: {safePosition}");
        }
    }
    
    private IEnumerator EnsureSafePositionAfterFrame()
    {
        yield return new WaitForEndOfFrame();
        EnsureSafeUIPosition();
    }
    
    // Player ve PlayerInput'un hazır olup olmadığını kontrol et
   
    
    // UI elemanının güvenli pozisyonda olup olmadığını kontrol et
    public bool IsInSafePosition()
    {
        if (rectTransform == null || parentCanvas == null) return true;
        
        Vector3[] corners = new Vector3[4];
        rectTransform.GetWorldCorners(corners);
        
        Camera cam = parentCanvas.worldCamera ?? Camera.main;
        if (cam == null) return true;
        
        foreach (Vector3 corner in corners)
        {
            Vector3 screenPos = cam.WorldToScreenPoint(corner);
            if (!UISystemManager.IsPositionSafeForUI(screenPos))
            {
                return false;
            }
        }
        
        return true;
    }
    
    // Manuel olarak güvenli pozisyona taşı
    public void ForceToSafePosition()
    {
        if (rectTransform != null)
        {
            rectTransform.anchoredPosition = Vector2.zero;
            Debug.Log($"BaseUIPanel: '{gameObject.name}' was forced to safe position (center)");
        }
    }
    
    // Panel gösterme metodu
    public virtual void ShowPanel()
    {
        gameObject.SetActive(true);
    }
    
    // Panel gizleme metodu
    public virtual void HidePanel()
    {
        gameObject.SetActive(false);
    }
    
    // ESC tuşu ile kapatma desteği
    protected virtual void Update()
    {
        // ESC tuşu ile kapatma (yeni input sistemi)
        if (UserInput.WasEscapePressed && gameObject.activeInHierarchy)
        {
            OnEscapePressed();
        }
    }
    
    // ESC tuşuna basıldığında çağrılacak metod (override edilebilir)
    protected virtual void OnEscapePressed()
    {
        HidePanel();
    }
} 