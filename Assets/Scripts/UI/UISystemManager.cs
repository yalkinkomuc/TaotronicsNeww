using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

public class UISystemManager : MonoBehaviour
{
    public static UISystemManager instance;
    
    [Header("Canvas Settings")]
    [SerializeField] private Vector2 referenceResolution = new Vector2(1920, 1080);
    [SerializeField] private CanvasScaler.ScreenMatchMode screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
    [SerializeField] private float matchWidthOrHeight = 0.5f;
    
    [Header("Debug")]
    [SerializeField] private bool logDebugInfo = false;
    
    private EventSystem eventSystem;
    private Canvas[] allCanvases;
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            
            // UI sistemini hemen başlat
            InitializeUISystem();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void InitializeUISystem()
    {
        // EventSystem'i kontrol et ve oluştur
        EnsureEventSystemExists();
        
        // Canvas ayarlarını düzelt
        StartCoroutine(SetupCanvasesAfterFrame());
    }
    
    private void EnsureEventSystemExists()
    {
        eventSystem = EventSystem.current;
        
        if (eventSystem == null)
        {
            Debug.Log("UISystemManager: EventSystem not found, creating one...");
            
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystem = eventSystemObj.AddComponent<EventSystem>();
            eventSystemObj.AddComponent<StandaloneInputModule>();
            
            DontDestroyOnLoad(eventSystemObj);
            
            if (logDebugInfo)
                Debug.Log("UISystemManager: EventSystem created successfully");
        }
        else
        {
            if (logDebugInfo)
                Debug.Log("UISystemManager: EventSystem found");
        }
    }
    
    private IEnumerator SetupCanvasesAfterFrame()
    {
        // Bir frame bekle ki sahne tamamen yüklensin
        yield return new WaitForEndOfFrame();
        
        // Tüm Canvas'ları bul
        allCanvases = FindObjectsOfType<Canvas>();
        
        foreach (Canvas canvas in allCanvases)
        {
            SetupCanvas(canvas);
        }
        
        // PixelPerfectCamera frustum hatası düzeltmesi
        FixPixelPerfectCameraIssues();
        
        if (logDebugInfo)
            Debug.Log($"UISystemManager: Configured {allCanvases.Length} canvases and fixed PixelPerfectCamera issues");
    }
    
    private void SetupCanvas(Canvas canvas)
    {
        if (canvas == null) return;
        
        // Canvas ayarlarını optimize et
        if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            // CanvasScaler ekle veya düzelt
            CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler == null)
            {
                scaler = canvas.gameObject.AddComponent<CanvasScaler>();
            }
            
            // Optimal ayarlar
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = referenceResolution;
            scaler.screenMatchMode = screenMatchMode;
            scaler.matchWidthOrHeight = matchWidthOrHeight;
            scaler.referencePixelsPerUnit = 100f;
            
            // GraphicRaycaster kontrolü
            GraphicRaycaster raycaster = canvas.GetComponent<GraphicRaycaster>();
            if (raycaster == null)
            {
                raycaster = canvas.gameObject.AddComponent<GraphicRaycaster>();
            }
            
            if (logDebugInfo)
                Debug.Log($"UISystemManager: Canvas '{canvas.name}' configured");
        }
    }
    
    // Yeni sahne yüklendiğinde çağrılır
    private void OnEnable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    private void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        // Yeni sahnede UI sistemini yeniden kur
        StartCoroutine(ReinitializeUISystemForNewScene());
    }
    
    private IEnumerator ReinitializeUISystemForNewScene()
    {
        // Sahnenin tamamen yüklenmesi için bekle
        yield return new WaitForSeconds(0.2f);
        
        // EventSystem'in hala mevcut olduğunu kontrol et
        EnsureEventSystemExists();
        
        // Yeni Canvas'ları ayarla
        yield return SetupCanvasesAfterFrame();
        
        // Sahne geçişinde PixelPerfectCamera'ları da kontrol et
        FixPixelPerfectCameraIssues();
        
        if (logDebugInfo)
            Debug.Log($"UISystemManager: UI system reinitialized for scene: {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}");
    }
    
    // UI frustum hatasını önlemek için güvenli pozisyon kontrolü
    public static bool IsPositionSafeForUI(Vector3 screenPosition)
    {
        // Ekran sınırları içinde mi kontrol et
        return screenPosition.x >= 0 && screenPosition.x <= Screen.width &&
               screenPosition.y >= 0 && screenPosition.y <= Screen.height;
    }
    
    // UI elemanlarını güvenli pozisyona taşı
    public static Vector3 ClampToSafeUIPosition(Vector3 screenPosition)
    {
        Vector3 safePosition = screenPosition;
        
        safePosition.x = Mathf.Clamp(safePosition.x, 0, Screen.width);
        safePosition.y = Mathf.Clamp(safePosition.y, 0, Screen.height);
        
        return safePosition;
    }
    
    // PixelPerfectCamera frustum hatası düzeltmesi
    private void FixPixelPerfectCameraIssues()
    {
        // Tüm PixelPerfectCamera'ları bul
        UnityEngine.U2D.PixelPerfectCamera[] pixelPerfectCameras = FindObjectsOfType<UnityEngine.U2D.PixelPerfectCamera>();
        
        foreach (var pixelCamera in pixelPerfectCameras)
        {
            // PixelPerfectCamera'nın reference resolution'ını kontrol et
            int currentRefX = pixelCamera.refResolutionX;
            int currentRefY = pixelCamera.refResolutionY;
            
            // Eğer reference resolution ekran çözünürlüğü ile uyumsuzsa düzelt
            float aspectRatio = (float)Screen.width / Screen.height;
            float targetAspectRatio = (float)currentRefX / currentRefY;
            
            // Aspect ratio farkı %20'den fazlaysa reference resolution'ı güncelle
            if (Mathf.Abs(aspectRatio - targetAspectRatio) > 0.2f)
            {
                // Reference resolution'ı current ekran çözünürlüğüne uyumlu hale getir
                // Ama pixel art için uygun bir değer kullan
                int newRefX = Mathf.RoundToInt(referenceResolution.x * 0.5f); // 960
                int newRefY = Mathf.RoundToInt(referenceResolution.y * 0.5f); // 540
                
                pixelCamera.refResolutionX = newRefX;
                pixelCamera.refResolutionY = newRefY;
                
                Debug.LogWarning($"PixelPerfectCamera '{pixelCamera.name}' reference resolution updated from {currentRefX}x{currentRefY} to {newRefX}x{newRefY} to prevent frustum errors");
                
                if (logDebugInfo)
                {
                    Debug.Log($"PixelPerfectCamera: Screen aspect ratio: {aspectRatio:F2}, Previous target aspect: {targetAspectRatio:F2}, New target aspect: {(float)newRefX / newRefY:F2}");
                }
            }
            
            // CropFrame'i false yap ki tam ekranı kullansın
            if (pixelCamera.cropFrameX || pixelCamera.cropFrameY)
            {
                pixelCamera.cropFrameX = false;
                pixelCamera.cropFrameY = false;
                
                if (logDebugInfo)
                {
                    Debug.Log($"PixelPerfectCamera '{pixelCamera.name}': Crop frame disabled to use full screen");
                }
            }
            
            // StretchFill'i true yap ki ekran tam doldurulsun
            if (!pixelCamera.stretchFill)
            {
                pixelCamera.stretchFill = true;
                
                if (logDebugInfo)
                {
                    Debug.Log($"PixelPerfectCamera '{pixelCamera.name}': Stretch fill enabled");
                }
            }
        }
        
        if (pixelPerfectCameras.Length > 0 && logDebugInfo)
        {
            Debug.Log($"UISystemManager: Fixed {pixelPerfectCameras.Length} PixelPerfectCamera(s) to prevent frustum errors");
        }
    }
    
    // Debug bilgileri
    public void LogUISystemInfo()
    {
        Debug.Log("=== UI System Info ===");
        Debug.Log($"Screen Resolution: {Screen.width}x{Screen.height}");
        Debug.Log($"Reference Resolution: {referenceResolution}");
        Debug.Log($"EventSystem exists: {EventSystem.current != null}");
        
        allCanvases = FindObjectsOfType<Canvas>();
        Debug.Log($"Active Canvases: {allCanvases.Length}");
        
        foreach (Canvas canvas in allCanvases)
        {
            CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
            Debug.Log($"Canvas '{canvas.name}': RenderMode={canvas.renderMode}, " +
                     $"SortingOrder={canvas.sortingOrder}, " +
                     $"HasScaler={scaler != null}");
        }
        
        // PixelPerfectCamera bilgilerini de göster
        UnityEngine.U2D.PixelPerfectCamera[] pixelCameras = FindObjectsOfType<UnityEngine.U2D.PixelPerfectCamera>();
        if (pixelCameras.Length > 0)
        {
            Debug.Log($"PixelPerfectCameras found: {pixelCameras.Length}");
            foreach (var pixelCam in pixelCameras)
            {
                Debug.Log($"PixelPerfectCamera '{pixelCam.name}': RefResolution={pixelCam.refResolutionX}x{pixelCam.refResolutionY}, " +
                         $"CropFrame={pixelCam.cropFrameX || pixelCam.cropFrameY}, StretchFill={pixelCam.stretchFill}");
            }
        }
    }
} 