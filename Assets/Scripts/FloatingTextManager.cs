using UnityEngine;
using TMPro;

public class FloatingTextManager : MonoBehaviour
{
    public static FloatingTextManager Instance { get; private set; }
    
    [SerializeField] private GameObject floatingTextPrefab;
    [SerializeField] private Transform canvasTransform; // Canvas to parent the text to
    
    [Header("Text Settings")]
    [SerializeField] private Color damageColor = Color.red;
    [SerializeField] private Color critDamageColor = new Color(1.0f, 0.5f, 0f); // Orange
    
    private Canvas parentCanvas;
    private Camera mainCamera;
    
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
                Debug.LogWarning("FloatingTextManager: No main camera found!");
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
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas != null)
            {
                canvasTransform = canvas.transform;
                parentCanvas = canvas;
            }
            else
            {
                // No canvas found, create one
                Debug.LogWarning("FloatingTextManager: No canvas found, creating one.");
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
                Debug.LogError("FloatingTextManager: The prefab does not have a FloatingText component!");
            }
            
            TextMeshProUGUI tmpComponent = floatingTextPrefab.GetComponent<TextMeshProUGUI>();
            if (tmpComponent == null)
            {
                tmpComponent = floatingTextPrefab.GetComponentInChildren<TextMeshProUGUI>();
                if (tmpComponent == null)
                {
                    Debug.LogError("FloatingTextManager: The prefab does not have a TextMeshPro component!");
                }
            }
        }
    }
    
    public void ShowDamageText(float damage, Vector3 position, bool isCritical = false)
    {
        if (floatingTextPrefab == null)
        {
            Debug.LogError("FloatingTextManager: No floatingTextPrefab assigned!");
            return;
        }
        
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogError("FloatingTextManager: No camera found!");
                return;
            }
        }
        
        try
        {
            // Round damage to integer for cleaner display
            int displayDamage = Mathf.RoundToInt(damage);
            
            // Convert world position to screen position
            Vector3 screenPos = mainCamera.WorldToScreenPoint(position);
            
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
                textObj = Instantiate(floatingTextPrefab, position, Quaternion.identity);
            }
            
            // Get the FloatingText component and set text
            FloatingText floatingText = textObj.GetComponent<FloatingText>();
            if (floatingText != null)
            {
                floatingText.SetText(displayDamage.ToString());
                floatingText.SetColor(isCritical ? critDamageColor : damageColor);
            }
            else
            {
                Debug.LogError("FloatingTextManager: Created object doesn't have a FloatingText component!");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"FloatingTextManager: Error creating floating text: {e.Message}");
        }
    }
    
    public void ShowCustomText(string text, Vector3 position, Color color)
    {
        if (floatingTextPrefab == null)
        {
            Debug.LogError("FloatingTextManager: No floatingTextPrefab assigned!");
            return;
        }
        
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogError("FloatingTextManager: No camera found!");
                return;
            }
        }
        
        try
        {
            // Convert world position to screen position
            Vector3 screenPos = mainCamera.WorldToScreenPoint(position);
            
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
                textObj = Instantiate(floatingTextPrefab, position, Quaternion.identity);
            }
            
            // Get the FloatingText component and set text
            FloatingText floatingText = textObj.GetComponent<FloatingText>();
            if (floatingText != null)
            {
                floatingText.SetText(text);
                floatingText.SetColor(color);
            }
            else
            {
                Debug.LogError("FloatingTextManager: Created object doesn't have a FloatingText component!");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"FloatingTextManager: Error creating floating text: {e.Message}");
        }
    }
} 