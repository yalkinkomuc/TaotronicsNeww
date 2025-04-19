using UnityEngine;

public class UIObjectTracker : MonoBehaviour
{
    private bool wasActive;
    
    private void OnEnable()
    {
        // Panel aktif olduğunda UIInputBlocker'a bildir
        if (UIInputBlocker.instance != null)
        {
            wasActive = true;
            UIInputBlocker.instance.OnPanelVisibilityChanged(gameObject, true);
        }
    }
    
    private void OnDisable()
    {
        // Panel deaktif olduğunda UIInputBlocker'a bildir
        if (UIInputBlocker.instance != null && wasActive)
        {
            wasActive = false;
            UIInputBlocker.instance.OnPanelVisibilityChanged(gameObject, false);
        }
    }
} 