using UnityEngine;

// UYARI: Bu dosya eski kodlar için uyumluluk sağlamak üzere tutulmuştur.
// Yeni kodlarda UIObjectTracker kullanmanız önerilir.
public class UICanvasTracker : MonoBehaviour
{
    private Canvas myCanvas;
    private bool wasActive;
    
    private void Awake()
    {
        myCanvas = GetComponent<Canvas>();
    }
    
    private void OnEnable()
    {
        // Canvas aktif olduğunda UIInputBlocker'a bildir
        if (UIInputBlocker.instance != null && myCanvas != null)
        {
            wasActive = true;
            // Yeni metot adını kullan
            UIInputBlocker.instance.OnPanelVisibilityChanged(gameObject, true);
        }
    }
    
    private void OnDisable()
    {
        // Canvas deaktif olduğunda UIInputBlocker'a bildir
        if (UIInputBlocker.instance != null && myCanvas != null && wasActive)
        {
            wasActive = false;
            // Yeni metot adını kullan
            UIInputBlocker.instance.OnPanelVisibilityChanged(gameObject, false);
        }
    }
} 