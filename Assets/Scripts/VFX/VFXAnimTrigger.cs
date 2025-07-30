using UnityEngine;

/// <summary>
/// Anim trigger ile VFX'i destroy etmek için eski sistem uyumluluğu
/// Yeni sistemde VFXDestroyer component'ini kullanın
/// </summary>
public class VFXAnimTrigger : MonoBehaviour
{
    [Header("Destroy Settings")]
    [SerializeField] private float destroyDelay = 0f;

    // Animation Event ile çağrılacak
    public void CompleteVFX()
    {
        // Yeni sistemde VFXDestroyer kullan
        var destroyer = GetComponent<VFXDestroyer>();
        if (destroyer != null)
        {
            destroyer.DestroyVFX();
        }
        else
        {
            // VFXDestroyer yoksa ekle ve destroy et
            destroyer = gameObject.AddComponent<VFXDestroyer>();
            destroyer.DestroyVFX();
        }
    }

    // Eski sistem uyumluluğu için
    public void OnVFXComplete()
    {
        CompleteVFX();
    }
} 