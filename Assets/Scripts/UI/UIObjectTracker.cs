using UnityEngine;

public class UIObjectTracker : MonoBehaviour
{
    private bool wasActive;
    private bool isInitialized = false;
    
    private void Start()
    {
        // Initialization complete - now we can start tracking
        isInitialized = true;
        
        // Check current state after initialization, but delay it slightly
        StartCoroutine(DelayedInitialCheck());
    }
    
    private System.Collections.IEnumerator DelayedInitialCheck()
    {
        // Wait a frame to ensure all systems are initialized
        yield return null;
        
        // Check current state after everything is ready
        if (gameObject.activeInHierarchy)
        {
            wasActive = true;
            if (UIInputBlocker.instance != null)
            {
                UIInputBlocker.instance.OnPanelVisibilityChanged(gameObject, true);
            }
        }
    }
    
    private void OnEnable()
    {
        // Only notify if we're initialized and this is a real state change
        if (isInitialized && UIInputBlocker.instance != null)
        {
            wasActive = true;
            UIInputBlocker.instance.OnPanelVisibilityChanged(gameObject, true);
        }
    }
    
    private void OnDisable()
    {
        // Panel deaktif olduğunda UIInputBlocker'a bildir
        if (UIInputBlocker.instance != null && wasActive && isInitialized)
        {
            wasActive = false;
            UIInputBlocker.instance.OnPanelVisibilityChanged(gameObject, false);
        }
    }
} 