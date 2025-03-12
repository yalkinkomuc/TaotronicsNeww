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
            DontDestroyOnLoad(gameObject);
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
            vfx.gameObject.SetActive(true);
            vfxPool.Add(vfxData.vfxId, vfx);
        }
    }

    public void PlayVFX(string vfxId, Vector3 position, Transform parent = null)
    {
        if (vfxPool.TryGetValue(vfxId, out GameObject vfx))
        {
            vfx.transform.position = position;
            
            // Vuran player'ın pozisyonuna göre VFX'i döndür
            Player player = PlayerManager.instance.player;
            bool hitFromLeft = player.transform.position.x < position.x;
            
            // Y ekseninde döndür
            vfx.transform.rotation = hitFromLeft ? 
                Quaternion.Euler(0f, 180f, 0f) : 
                Quaternion.Euler(0f, 0f, 0f);
                
            if (parent != null)
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