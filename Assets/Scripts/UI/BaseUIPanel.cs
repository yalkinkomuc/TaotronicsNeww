using UnityEngine;

public class BaseUIPanel : MonoBehaviour
{
    protected virtual void OnEnable()
    {
        // Sadece Player ve PlayerInput hazırsa input blocking yap
        if (UIInputBlocker.instance != null && IsPlayerReady())
        {
            UIInputBlocker.instance.AddPanel(gameObject);
        }
        else if (UIInputBlocker.instance != null)
        {
            // Player henüz hazır değil, manuel olarak input blocking yapmayı bekle
        }
    }

    protected virtual void OnDisable()
    {
        if (UIInputBlocker.instance != null)
        {
            UIInputBlocker.instance.RemovePanel(gameObject);
        }
    }
    
    // Player ve PlayerInput'un hazır olup olmadığını kontrol et
    private bool IsPlayerReady()
    {
        Player player = PlayerManager.instance?.player;
        return player != null && player.playerInput != null;
    }
} 