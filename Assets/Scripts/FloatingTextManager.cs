using UnityEngine;
using TMPro;

public class FloatingTextManager : MonoBehaviour
{
    public static FloatingTextManager Instance { get; private set; }
    
    [SerializeField] private GameObject floatingTextPrefab;
    [SerializeField] private Transform canvasTransform; // Canvas to parent the text to
    
    [Header("Hasar Metin Ayarları")]
    [SerializeField] private Color damageColor = Color.red; // Varsayılan kırmızı
    [SerializeField] private Color magicDamageColor = new Color(0.3f, 0.3f, 1f); // Mavi
    
    [Header("Kombo Hasar Renkleri")]
    [SerializeField] private Color firstComboColor = Color.yellow; // İlk vuruş rengi
    [SerializeField] private Color secondComboColor = new Color(1.0f, 0.5f, 0f); // İkinci vuruş rengi (turuncu)
    [SerializeField] private Color thirdComboColor = Color.red; // Üçüncü vuruş rengi
    
    [Header("Pozisyon Ayarları")]
    [SerializeField] private float positionVariance = 0.5f; // Rastgele pozisyon aralığı
    [SerializeField] private float minTextSpacing = 0.5f; // Minimum metin aralığı
    
    // Son oluşturulan metin pozisyonunu sakla
    private Vector3 lastTextPosition = Vector3.zero;
    private int consecutiveTexts = 0; // Ardışık oluşturulan metin sayısı
    
    private Canvas parentCanvas;
    private Camera mainCamera;
    
    // Zaman damgası - üst üste oluşturulan metinleri tespit için
    private float lastTextTime = 0;
    private const float TEXT_GROUPING_THRESHOLD = 0.2f; // Saniye
    
    private void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Cache the camera reference
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogWarning("FloatingTextManager: Ana kamera bulunamadı!");
            }
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    private void Start()
    {
        // If no canvas specified, try to find one
        if (canvasTransform == null)
        {
            Canvas canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas != null)
            {
                canvasTransform = canvas.transform;
                parentCanvas = canvas;
            }
            else
            {
                // No canvas found, create one
                Debug.LogWarning("FloatingTextManager: Canvas bulunamadı, yeni bir canvas oluşturuluyor.");
                GameObject canvasObj = new GameObject("FloatingTextCanvas");
                Canvas newCanvas = canvasObj.AddComponent<Canvas>();
                newCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
                canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
                canvasTransform = canvasObj.transform;
                parentCanvas = newCanvas;
                DontDestroyOnLoad(canvasObj);
            }
        }
        else
        {
            parentCanvas = canvasTransform.GetComponent<Canvas>();
        }
        
        // Verify the prefab has the FloatingText component
        if (floatingTextPrefab != null)
        {
            FloatingText testComponent = floatingTextPrefab.GetComponent<FloatingText>();
            if (testComponent == null)
            {
                Debug.LogError("FloatingTextManager: Prefab'da FloatingText bileşeni bulunamadı!");
            }
            
            TextMeshProUGUI tmpComponent = floatingTextPrefab.GetComponent<TextMeshProUGUI>();
            if (tmpComponent == null)
            {
                tmpComponent = floatingTextPrefab.GetComponentInChildren<TextMeshProUGUI>();
                if (tmpComponent == null)
                {
                    Debug.LogError("FloatingTextManager: Prefab'da TextMeshProUGUI bileşeni bulunamadı!");
                }
            }
        }
    }
    
    // Sihir hasarı için metod
    public void ShowMagicDamageText(float damage, Vector3 position)
    {
        ShowDamageTextWithColor(damage, position, magicDamageColor);
    }
    
    // Ana hasar gösterme metodu (renk parametreli)
    private void ShowDamageTextWithColor(float damage, Vector3 position, Color color, string overrideText = null)
    {
        if (floatingTextPrefab == null)
        {
            Debug.LogError("FloatingTextManager: floatingTextPrefab atanmamış!");
            return;
        }
        
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogError("FloatingTextManager: Kamera bulunamadı!");
                return;
            }
        }
        
        try
        {
            // Metni üretme zamanını kontrol et (üst üste gelmeyi önlemek için)
            bool isGroupedText = Time.time - lastTextTime < TEXT_GROUPING_THRESHOLD;
            lastTextTime = Time.time;
            
            if (isGroupedText)
            {
                consecutiveTexts++;
            }
            else
            {
                consecutiveTexts = 0;
            }
            
            // Metni göstermek için pozisyonu ayarla
            Vector3 adjustedPosition = AdjustTextPosition(position, isGroupedText);
            
            // Round damage to integer for cleaner display
            int displayDamage = Mathf.RoundToInt(damage);
            
            // Convert world position to screen position
            Vector3 screenPos = mainCamera.WorldToScreenPoint(adjustedPosition);
            
            // If screen position is behind the camera (negative z), don't show
            if (screenPos.z < 0)
            {
                return;
            }
            
            // Create the floating text object
            GameObject textObj;
            
            if (parentCanvas != null && parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                // For Screen Space Overlay canvas
                textObj = Instantiate(floatingTextPrefab, screenPos, Quaternion.identity, canvasTransform);
            }
            else if (parentCanvas != null && parentCanvas.renderMode == RenderMode.ScreenSpaceCamera)
            {
                // For Screen Space Camera canvas, convert from screen to canvas position
                Vector2 viewportPos = mainCamera.ScreenToViewportPoint(screenPos);
                Vector2 canvasPos = new Vector2(
                    (viewportPos.x * canvasTransform.GetComponent<RectTransform>().rect.width) - (canvasTransform.GetComponent<RectTransform>().rect.width * 0.5f),
                    (viewportPos.y * canvasTransform.GetComponent<RectTransform>().rect.height) - (canvasTransform.GetComponent<RectTransform>().rect.height * 0.5f)
                );
                
                textObj = Instantiate(floatingTextPrefab, canvasTransform);
                textObj.GetComponent<RectTransform>().anchoredPosition = canvasPos;
            }
            else
            {
                // For World Space or if no canvas, create at world position
                textObj = Instantiate(floatingTextPrefab, adjustedPosition, Quaternion.identity);
            }
            
            // Get the FloatingText component and set text
            FloatingText floatingText = textObj.GetComponent<FloatingText>();
            if (floatingText != null)
            {
                // Hasar değerini doğrudan ayarla (boyutlandırma için)
                floatingText.SetDamageValue(damage);
                
                // Varsa özel metni kullan, yoksa hasarı göster
                string textToShow = overrideText ?? displayDamage.ToString();
                floatingText.SetText(textToShow);
                floatingText.SetColor(color);
            }
            else
            {
                Debug.LogError("FloatingTextManager: Oluşturulan nesne FloatingText bileşenine sahip değil!");
            }
            
            // Son metin pozisyonunu kaydet
            lastTextPosition = adjustedPosition;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"FloatingTextManager: Yüzen metin oluşturulurken hata: {e.Message}");
        }
    }
    
    // Metinlerin üst üste gelmesini önlemek için pozisyon ayarlama
    private Vector3 AdjustTextPosition(Vector3 basePosition, bool isGrouped)
    {
        // İlk metin veya yeni grup ise
        if (!isGrouped || consecutiveTexts == 0)
        {
            // Biraz rastgele offset ekle
            return basePosition + new Vector3(
                Random.Range(-positionVariance, positionVariance),
                Random.Range(0, positionVariance), // Sadece yukarı doğru kayma
                0
            );
        }
        
        // Metin grubu içindeyse, önceki metinden belli mesafe uzaklıkta konumlandır
        Vector3 directionFromLast = new Vector3(
            Mathf.Cos(consecutiveTexts * 0.7f), // Daire şeklinde dağılım
            Mathf.Sin(consecutiveTexts * 0.7f),
            0
        ).normalized;
        
        return lastTextPosition + (directionFromLast * minTextSpacing);
    }
    
    // Normal hasar metni - Standart renk kullanır
    public void ShowDamageText(float damage, Vector3 position)
    {
        ShowDamageTextWithColor(damage, position, damageColor);
    }
    
    // Kombo numarasına göre hasar metni göster
    public void ShowComboDamageText(float damage, Vector3 position, int comboCount)
    {
        Color comboColor;
        
        switch(comboCount)
        {
            case 0:
                comboColor = firstComboColor; // İlk vuruş - sarı
                break;
            case 1:
                comboColor = secondComboColor; // İkinci vuruş - turuncu
                break;
            case 2:
                comboColor = thirdComboColor; // Üçüncü vuruş - kırmızı
                break;
            default:
                comboColor = damageColor;
                break;
        }
        
        ShowDamageTextWithColor(damage, position, comboColor);
    }
    
    public void ShowCustomText(string text, Vector3 position, Color color)
    {
        if (floatingTextPrefab == null)
        {
            Debug.LogError("FloatingTextManager: floatingTextPrefab atanmamış!");
            return;
        }
        
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogError("FloatingTextManager: Kamera bulunamadı!");
                return;
            }
        }
        
        try
        {
            // Metni üretme zamanını kontrol et (üst üste gelmeyi önlemek için)
            bool isGroupedText = Time.time - lastTextTime < TEXT_GROUPING_THRESHOLD;
            lastTextTime = Time.time;
            
            if (isGroupedText)
            {
                consecutiveTexts++;
            }
            else
            {
                consecutiveTexts = 0;
            }
            
            // Metni göstermek için pozisyonu ayarla
            Vector3 adjustedPosition = AdjustTextPosition(position, isGroupedText);
            
            // Convert world position to screen position
            Vector3 screenPos = mainCamera.WorldToScreenPoint(adjustedPosition);
            
            // If screen position is behind the camera (negative z), don't show
            if (screenPos.z < 0)
            {
                return;
            }
            
            // Create the floating text object
            GameObject textObj;
            
            if (parentCanvas != null && parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                // For Screen Space Overlay canvas
                textObj = Instantiate(floatingTextPrefab, screenPos, Quaternion.identity, canvasTransform);
            }
            else if (parentCanvas != null && parentCanvas.renderMode == RenderMode.ScreenSpaceCamera)
            {
                // For Screen Space Camera canvas, convert from screen to canvas position
                Vector2 viewportPos = mainCamera.ScreenToViewportPoint(screenPos);
                Vector2 canvasPos = new Vector2(
                    (viewportPos.x * canvasTransform.GetComponent<RectTransform>().rect.width) - (canvasTransform.GetComponent<RectTransform>().rect.width * 0.5f),
                    (viewportPos.y * canvasTransform.GetComponent<RectTransform>().rect.height) - (canvasTransform.GetComponent<RectTransform>().rect.height * 0.5f)
                );
                
                textObj = Instantiate(floatingTextPrefab, canvasTransform);
                textObj.GetComponent<RectTransform>().anchoredPosition = canvasPos;
            }
            else
            {
                // For World Space or if no canvas, create at world position
                textObj = Instantiate(floatingTextPrefab, adjustedPosition, Quaternion.identity);
            }
            
            // Get the FloatingText component and set text
            FloatingText floatingText = textObj.GetComponent<FloatingText>();
            if (floatingText != null)
            {
                // Sayısal değeri kontrol et, varsa hasar değeri olarak ayarla
                if (float.TryParse(text, out float value))
                {
                    floatingText.SetDamageValue(value);
                }
                
                floatingText.SetText(text);
                floatingText.SetColor(color);
            }
            else
            {
                Debug.LogError("FloatingTextManager: Oluşturulan nesne FloatingText bileşenine sahip değil!");
            }
            
            // Son metin pozisyonunu kaydet
            lastTextPosition = adjustedPosition;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"FloatingTextManager: Yüzen metin oluşturulurken hata: {e.Message}");
        }
    }
} 