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
            vfxPool.Add(vfxData.vfxId, vfx);
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
                
                // Y ekseninde döndür
                vfx.transform.rotation = hitFromLeft ? 
                    Quaternion.Euler(0f, 180f, 0f) : 
                    Quaternion.Euler(0f, 0f, 0f);
            }
            else
            {
                // Player bulunamazsa varsayılan rotasyon
                vfx.transform.rotation = Quaternion.identity;
            }
                
            if (parent != null && parent.gameObject != null && parent.gameObject.activeInHierarchy)
                vfx.transform.SetParent(parent);
                
            vfx.SetActive(true);
        }
    }

    public void OnVFXComplete(GameObject vfx)
    {
        vfx.SetActive(false);
        vfx.transform.SetParent(transform);
    }
} 