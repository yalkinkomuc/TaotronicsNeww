using UnityEngine;

/// <summary>
/// Yeni VFX sisteminin kullanım örnekleri
/// </summary>
public class VFXExamples : MonoBehaviour
{
    [Header("VFX Settings")]
    [SerializeField] private string hitVFXId = "HitEffect";
    [SerializeField] private string bloodVFXId = "BloodEffect";
    [SerializeField] private string magicVFXId = "MagicEffect";

    /// <summary>
    /// Hit VFX oynat - GameObject tipinde, anim trigger ile destroy
    /// </summary>
    public void PlayHitVFX(Vector3 position)
    {
        GameObject vfx = VFXManager.Instance.PlayGameObjectVFX(hitVFXId, position);
        
        // VFX'e VFXDestroyer component'i ekle (eğer yoksa)
        if (vfx != null && vfx.GetComponent<VFXDestroyer>() == null)
        {
            vfx.AddComponent<VFXDestroyer>();
        }
    }

    /// <summary>
    /// Blood VFX oynat - Particle effect, otomatik destroy
    /// </summary>
    public void PlayBloodVFX(Vector3 position)
    {
        VFXManager.Instance.PlayParticleEffect(bloodVFXId, position);
    }

    /// <summary>
    /// Magic VFX oynat - GameObject tipinde, belirli süre sonra destroy
    /// </summary>
    public void PlayMagicVFX(Vector3 position)
    {
        GameObject vfx = VFXManager.Instance.PlayGameObjectVFX(magicVFXId, position);
        
        if (vfx != null)
        {
            var destroyer = vfx.GetComponent<VFXDestroyer>();
            if (destroyer == null)
            {
                destroyer = vfx.AddComponent<VFXDestroyer>();
            }
            
            // 3 saniye sonra destroy et
            destroyer.DestroyAfterTime(3f);
        }
    }

    /// <summary>
    /// Genel VFX oynat - VFX tipine göre otomatik ayarlar
    /// </summary>
    public void PlayVFX(string vfxId, Vector3 position)
    {
        VFXManager.Instance.PlayVFX(vfxId, position);
    }

    // Test için
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            PlayHitVFX(transform.position);
        }
        
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            PlayBloodVFX(transform.position);
        }
        
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            PlayMagicVFX(transform.position);
        }
    }
} 