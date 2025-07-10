using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TabNavigationHints : MonoBehaviour
{
    [Header("Navigation Hint UI")]
    [SerializeField] private GameObject hintsPanel;
    [SerializeField] private TextMeshProUGUI leftHintText;
    [SerializeField] private TextMeshProUGUI rightHintText;
    [SerializeField] private Image leftHintIcon;
    [SerializeField] private Image rightHintIcon;
    
    [Header("Platform Icons")]
    [SerializeField] private Sprite pcIcon_Q;
    [SerializeField] private Sprite pcIcon_E;
    [SerializeField] private Sprite gamepadIcon_LB;
    [SerializeField] private Sprite gamepadIcon_RB;
    
    [Header("Settings")]
    
    [SerializeField] private float fadeInDuration = 0.3f;
    [SerializeField] private float fadeOutDuration = 0.2f;
    
    private CanvasGroup canvasGroup;
    private bool isVisible = false;
    private Coroutine fadeCoroutine;
    
    private void Awake()
    {
        // Get or add CanvasGroup for fading
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        
        // Initially hidden
        SetVisibility(false, true);
    }
    
    private void Start()
    {
        // Subscribe to tab events
        TabManager.OnTabChanged += OnTabChanged;
        
      
    }
    
    private void OnDestroy()
    {
        // Unsubscribe from events
        TabManager.OnTabChanged -= OnTabChanged;
    }
    
    private void OnTabChanged(int tabIndex, string tabName)
    {
        
        
        // Show hints briefly when tab changes
        ShowHintsBriefly();
    }
    
   
    
    public void SetVisibility(bool visible, bool immediate = false)
    {
        if (isVisible == visible) return;
        
        isVisible = visible;
        
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
        
        if (immediate)
        {
            canvasGroup.alpha = visible ? 1f : 0f;
            canvasGroup.interactable = visible;
            canvasGroup.blocksRaycasts = visible;
        }
        else
        {
            fadeCoroutine = StartCoroutine(FadeToAlpha(visible ? 1f : 0f, visible ? fadeInDuration : fadeOutDuration));
        }
    }
    
    public void ShowHintsBriefly(float duration = 2f)
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
        
        fadeCoroutine = StartCoroutine(ShowBriefly(duration));
    }
    
    private System.Collections.IEnumerator FadeToAlpha(float targetAlpha, float duration)
    {
        float startAlpha = canvasGroup.alpha;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            yield return null;
        }
        
        canvasGroup.alpha = targetAlpha;
        canvasGroup.interactable = targetAlpha > 0f;
        canvasGroup.blocksRaycasts = targetAlpha > 0f;
        
        fadeCoroutine = null;
    }
    
    private System.Collections.IEnumerator ShowBriefly(float duration)
    {
        // Fade in
        yield return StartCoroutine(FadeToAlpha(1f, fadeInDuration));
        
        // Wait
        yield return new WaitForSeconds(duration);
        
        // Fade out
        yield return StartCoroutine(FadeToAlpha(0f, fadeOutDuration));
        
        fadeCoroutine = null;
    }
    
    private IPlayerInput GetPlayerInput()
    {
        // Get player input from PlayerManager or find Player
        if (PlayerManager.instance != null && PlayerManager.instance.player != null)
        {
            return PlayerManager.instance.player.playerInput;
        }
        
        // Fallback: find player in scene
        Player player = FindFirstObjectByType<Player>();
        return player?.playerInput;
    }
    
    #region Public API
    
  
    
    public void SetFadeDurations(float fadeIn, float fadeOut)
    {
        fadeInDuration = fadeIn;
        fadeOutDuration = fadeOut;
    }
    
    #endregion
} 