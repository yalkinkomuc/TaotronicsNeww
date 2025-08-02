using System;
using UnityEngine;
using System.Collections.Generic;

public class VFXManager : MonoBehaviour
{
    public static VFXManager Instance { get; private set; }

    [System.Serializable]
    public class VFXData
    {
        public string vfxId;
        public GameObject vfxPrefab;
        public VFXType vfxType = VFXType.GameObject;
        public float autoDestroyTime = 2f; // Particle effect'ler için
    }

    public enum VFXType
    {
        GameObject,     // Normal GameObject - anim trigger ile destroy
        ParticleEffect  // Particle System - otomatik destroy
    }

    [SerializeField] private List<VFXData> vfxList = new List<VFXData>();
    private Dictionary<string, VFXData> vfxDataDict = new Dictionary<string, VFXData>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializeVFXData();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeVFXData()
    {
        foreach (var vfxData in vfxList)
        {
            if (!string.IsNullOrEmpty(vfxData.vfxId))
            {
                vfxDataDict[vfxData.vfxId] = vfxData;
            }
        }
    }

    /// <summary>
    /// VFX oynat - GameObject tipinde anim trigger ile destroy edilir
    /// </summary>
    public GameObject PlayVFX(string vfxId, Vector3 position, Transform parent = null)
    {
        if (!vfxDataDict.TryGetValue(vfxId, out VFXData vfxData))
        {
            Debug.LogWarning($"VFX with ID '{vfxId}' not found!");
            return null;
        }

        GameObject vfx = Instantiate(vfxData.vfxPrefab, position, Quaternion.identity);
        
        // Parent ayarla
        if (parent != null)
        {
            vfx.transform.SetParent(parent);
        }

        // Player pozisyonuna göre rotasyon ayarla
        SetVFXRotation(vfx, position);

        // VFX tipine göre ayarları yap
        if (vfxData.vfxType == VFXType.ParticleEffect)
        {
            SetupParticleEffect(vfx, vfxData.autoDestroyTime);
        }

        return vfx;
    }

    /// <summary>
    /// Particle effect oynat - otomatik destroy
    /// </summary>
    public void PlayParticleEffect(string vfxId, Vector3 position, Transform parent = null)
    {
        if (!vfxDataDict.TryGetValue(vfxId, out VFXData vfxData))
        {
            Debug.LogWarning($"VFX with ID '{vfxId}' not found!");
            return;
        }

        GameObject vfx = Instantiate(vfxData.vfxPrefab, position, Quaternion.identity);
        
        // Parent ayarla
        if (parent != null)
        {
            vfx.transform.SetParent(parent);
        }

        // Player pozisyonuna göre rotasyon ayarla
        SetVFXRotation(vfx, position);

        // Particle System ayarları
        SetupParticleEffect(vfx, vfxData.autoDestroyTime);
    }

    /// <summary>
    /// GameObject VFX oynat - anim trigger ile destroy edilir
    /// </summary>
    public GameObject PlayGameObjectVFX(string vfxId, Vector3 position, Transform parent = null)
    {
        if (!vfxDataDict.TryGetValue(vfxId, out VFXData vfxData))
        {
            Debug.LogWarning($"VFX with ID '{vfxId}' not found!");
            return null;
        }

        GameObject vfx = Instantiate(vfxData.vfxPrefab, position, Quaternion.identity);
        
        // Parent ayarla
        if (parent != null)
        {
            vfx.transform.SetParent(parent);
        }

        // Player pozisyonuna göre rotasyon ayarla
        SetVFXRotation(vfx, position);

        return vfx;
    }

    private void SetVFXRotation(GameObject vfx, Vector3 position)
    {
        // Player'ın pozisyonuna göre VFX'i döndür
        Player player = PlayerManager.instance?.player;
        if (player != null && player.gameObject != null && player.gameObject.activeInHierarchy)
        {
            bool hitFromLeft = player.transform.position.x < position.x;
            
            // Y ekseninde döndür
            vfx.transform.rotation = hitFromLeft ? 
                Quaternion.Euler(0f, 0f, 0f) : 
                Quaternion.Euler(0f, 180f, 0f);
        }
        else
        {
            // Player bulunamazsa varsayılan rotasyon
            vfx.transform.rotation = Quaternion.identity;
        }
    }

    private void SetupParticleEffect(GameObject vfx, float destroyTime)
    {
        ParticleSystem particleSystem = vfx.GetComponent<ParticleSystem>();
        if (particleSystem != null)
        {
            // Particle System'i reset et ve oynat
            particleSystem.Stop();
            particleSystem.Clear();
            particleSystem.Play();

            // Otomatik destroy için component ekle
            var autoDestroy = vfx.GetComponent<ParticleAutoDestroy>();
            if (autoDestroy == null)
            {
                autoDestroy = vfx.AddComponent<ParticleAutoDestroy>();
            }
            autoDestroy.destroyTime = destroyTime;
        }
        else
        {
            // Particle System yoksa normal destroy
            Destroy(vfx, destroyTime);
        }
    }

    /// <summary>
    /// VFX'i manuel olarak destroy et (anim trigger için)
    /// </summary>
    public void DestroyVFX(GameObject vfx)
    {
        if (vfx != null)
        {
            Destroy(vfx);
        }
    }
}

/// <summary>
/// Particle effect'ler için otomatik destroy component'i
/// </summary>
public class ParticleAutoDestroy : MonoBehaviour
{
    public float destroyTime = 2f;
    private new ParticleSystem particleSystem;
    private float timer;

    private void Awake()
    {
        particleSystem = GetComponent<ParticleSystem>();
        timer = 0f;
    }

    private void Update()
    {
        timer += Time.deltaTime;
        
        // Belirtilen süre sonra veya particle system bittikten sonra destroy et
        if (timer >= destroyTime || (particleSystem != null && !particleSystem.IsAlive()))
        {
            Destroy(gameObject);
        }
    }
} 