using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Notepad_UI : BaseUIPanel
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI contentText;
    
    [SerializeField] private Image noteBackground;
    
    
    
    
    // Singleton pattern
    public static Notepad_UI Instance { get; private set; }
    
    private CanvasGroup canvasGroup;
    
    protected override void Awake()
    {
        base.Awake();
        
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        
        // Başlangıçta gizli
        gameObject.SetActive(false);
    }
    
    public void ShowNote(string title, string content, Color? noteColor = null, Sprite icon = null)
    {
        // UI elementlerini güncelle
        if (titleText != null)
            titleText.text = title;
            
        if (contentText != null)
            contentText.text = content;
            
        // Renk ayarla
        if (noteColor.HasValue && noteBackground != null)
        {
            noteBackground.color = noteColor.Value;
        }
        
        // UI'yi göster
        ShowPanel();
    }
    
    public void CloseNote()
    {
      
        if (canvasGroup != null)
        {
            LeanTween.cancel(canvasGroup.gameObject);
        }
        
      
        HidePanel();
    }
    
    // ESC tuşuna basıldığında animasyonlu kapatma
    protected override void OnEscapePressed()
    {
        
        // Eğer animasyon devam ediyorsa durdur
        if (canvasGroup != null)
        {
            LeanTween.cancel(canvasGroup.gameObject);
        }
        CloseNote();
    }
    

    
    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
