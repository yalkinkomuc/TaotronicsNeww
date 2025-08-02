using UnityEngine;

/// <summary>
/// Anim trigger ile VFX'i destroy etmek için component
/// </summary>
public class VFXDestroyer : MonoBehaviour
{
    [Header("VFX Destroy Settings")]
    [SerializeField] private bool destroyOnAnimationEvent = true;
    [SerializeField] private float destroyDelay = 0f;
    
    public void DestroyVFX()
    {
        if (destroyDelay > 0)
        {
            Invoke(nameof(DestroyImmediate), destroyDelay);
        }
        else
        {
            DestroyImmediate();
        }
    }

    /// <summary>
    /// Manuel olarak VFX'i destroy et
    /// </summary>
    public void DestroyVFXManual()
    {
        DestroyVFX();
    }

    private void DestroyImmediate()
    {
        // VFXManager üzerinden destroy et (eğer varsa)
        if (VFXManager.Instance != null)
        {
            VFXManager.Instance.DestroyVFX(gameObject);
        }
        else
        {
            // VFXManager yoksa direkt destroy et
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Belirtilen süre sonra otomatik destroy
    /// </summary>
    public void DestroyAfterTime(float time)
    {
        Invoke(nameof(DestroyImmediate), time);
    }
} 