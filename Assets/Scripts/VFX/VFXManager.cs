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
    }

    [SerializeField] private List<VFXData> vfxList = new List<VFXData>();
    private Dictionary<string, GameObject> vfxPool = new Dictionary<string, GameObject>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject);
            InitializeVFXPool();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeVFXPool()
    {
        foreach (var vfxData in vfxList)
        {
            GameObject vfx = Instantiate(vfxData.vfxPrefab);
            vfx.transform.SetParent(transform);
            vfx.gameObject.SetActive(false);
            
            // Particle System varsa ayarları yap
            SetupParticleSystem(vfx);
            
            vfxPool.Add(vfxData.vfxId, vfx);
        }
    }

    private void SetupParticleSystem(GameObject vfx)
    {
        ParticleSystem particleSystem = vfx.GetComponent<ParticleSystem>();
        if (particleSystem != null)
        {
            // Auto Destroy kapalı olsun (pool için)
            var main = particleSystem.main;
            main.stopAction = ParticleSystemStopAction.Callback;
            
            // Particle System bittiğinde OnVFXComplete çağır
            var particleSystemCallback = vfx.GetComponent<ParticleSystemCallback>();
            if (particleSystemCallback == null)
            {
                particleSystemCallback = vfx.AddComponent<ParticleSystemCallback>();
            }
            particleSystemCallback.vfxManager = this;
        }
    }

    public void PlayVFX(string vfxId, Vector3 position, Transform parent = null)
    {
        if (vfxPool.TryGetValue(vfxId, out GameObject vfx))
        {
            // VFX GameObject'inin hala geçerli olup olmadığını kontrol et
            if (vfx == null)
            {
                Debug.LogWarning($"VFX with ID '{vfxId}' is null in pool!");
                return;
            }
            
            vfx.transform.position = position;
            
            // Vuran player'ın pozisyonuna göre VFX'i döndür
            Player player = PlayerManager.instance?.player;
            if (player != null && player.gameObject != null && player.gameObject.activeInHierarchy)
            {
                bool hitFromLeft = player.transform.position.x < position.x;
                
                // Y ekseninde döndür (tersine çevrildi)
                vfx.transform.rotation = hitFromLeft ? 
                    Quaternion.Euler(0f, 0f, 0f) : 
                    Quaternion.Euler(0f, 180f, 0f);
            }
            else
            {
                // Player bulunamazsa varsayılan rotasyon
                vfx.transform.rotation = Quaternion.identity;
            }
                
            // Parent bağlantısını kaldırdık - VFX artık düşmanın child'ı olmayacak
            
            // Particle System varsa reset et ve oynat
            ParticleSystem particleSystem = vfx.GetComponent<ParticleSystem>();
            if (particleSystem != null)
            {
                particleSystem.Stop();
                particleSystem.Clear();
                particleSystem.Play();
            }
                
            vfx.SetActive(true);
        }
    }

    public void OnVFXComplete(GameObject vfx)
    {
        vfx.SetActive(false);
        vfx.transform.SetParent(transform);
    }
}

// Particle System callback için yardımcı component
public class ParticleSystemCallback : MonoBehaviour
{
    public VFXManager vfxManager;
    private ParticleSystem particleSystem;

    private void Awake()
    {
        particleSystem = GetComponent<ParticleSystem>();
    }

    private void OnParticleSystemStopped()
    {
        if (vfxManager != null)
        {
            vfxManager.OnVFXComplete(gameObject);
        }
    }
} 