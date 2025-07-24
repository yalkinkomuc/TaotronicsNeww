using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneManager : MonoBehaviour
{
    public static SceneManager instance { get; private set; }
    
    [Header("Scene Transition")]
    [SerializeField] private Color fadeColor = Color.black;
    [SerializeField] private GameObject transitionEffectPrefab;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            
            
            // Aktif checkpoint kontrolü
            if (PlayerPrefs.GetInt("CheckpointActivated", 0) == 1)
            {
                int checkpointSceneIndex = PlayerPrefs.GetInt("CheckpointSceneIndex", 0);
                int currentSceneIndex = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
                
                if (checkpointSceneIndex != currentSceneIndex)
                {
                    UnityEngine.SceneManagement.SceneManager.LoadScene(checkpointSceneIndex);
                }
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        // Kamera ve spawn işlemlerini coroutine ile sırayla yap
        StartCoroutine(HandlePlayerSpawnAndCamera());
    }
    
    private IEnumerator HandlePlayerSpawnAndCamera()
    {
        // Sahnenin tamamen yüklenmesini bekle
        yield return new WaitForEndOfFrame();
        
        Player player = PlayerManager.instance?.player;
        if (player == null)
        {
            Debug.LogWarning("Player not found in scene!");
            yield break;
        }

        // Spawn işlemlerini yap
        HandlePlayerSpawning(player);
        
        // Kameranın güncellenmesi için kısa bir süre bekle
        yield return new WaitForSeconds(0.1f);
        
    }
    
    private void HandlePlayerSpawning(Player player)
    {
        // Check if player is valid before spawning
        if (player == null || player.gameObject == null)
        {
            Debug.LogError("SceneManager: Cannot spawn null player!");
            return;
        }
        
        // İsimle belirtilen spawn noktasını kontrol et
        if (PlayerPrefs.GetInt("UseNamedSpawnPoint", 0) == 1)
        {
            string spawnPointName = PlayerPrefs.GetString("TargetSpawnPointName", "");
            if (!string.IsNullOrEmpty(spawnPointName))
            {
                // İsimle belirtilen spawn noktasını bul
                GameObject namedSpawnPoint = GameObject.Find(spawnPointName);
                if (namedSpawnPoint != null)
                {
                    SpawnPlayerAt(player, namedSpawnPoint.transform.position);
                    
                    // Named spawn point bayrağını sıfırla
                    ClearSpawnPrefs("UseNamedSpawnPoint");
                    
                    return;
                }
                else
                {
                    Debug.LogWarning($"Spawn noktası bulunamadı: {spawnPointName}");
                }
            }
        }
        
        // Özel spawn pozisyonu kontrolü (kapıdan geçişleri kontrol etmek için)
        if (PlayerPrefs.GetInt("UseCustomSpawn", 0) == 1)
        {
            // Kapıdan geçiş - özel konumu kullan
            HandleCustomSpawnPosition(player);
            return;
        }
        
        // Rest butonundan yeniden canlanma kontrolü
        if (PlayerPrefs.GetInt("UseCheckpointRespawn", 0) == 1)
        {
            // Rest butonundan sonra yeniden yükleme - checkpoint konumunu kullan
            if (PlayerPrefs.GetInt("CheckpointActivated", 0) == 1)
            {
                float x = PlayerPrefs.GetFloat("CheckpointX");
                float y = PlayerPrefs.GetFloat("CheckpointY");
                Vector2 checkpointPosition = new Vector2(x, y);
                
                SpawnPlayerAt(player, checkpointPosition);
                
                // Bayrağı sıfırla
                PlayerPrefs.SetInt("UseCheckpointRespawn", 0);
                PlayerPrefs.Save();
                return;
            }
        }
        
        // Checkpoint aktifse VE aynı sahnedeysek, her zaman checkpoint konumundan başla
        if (PlayerPrefs.GetInt("CheckpointActivated", 0) == 1)
        {
            int checkpointSceneIndex = PlayerPrefs.GetInt("CheckpointSceneIndex", 0);
            int currentSceneIndex = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;

            if (currentSceneIndex == checkpointSceneIndex)
            {
                float x = PlayerPrefs.GetFloat("CheckpointX");
                float y = PlayerPrefs.GetFloat("CheckpointY");
                Vector2 checkpointPosition = new Vector2(x, y);
                
                SpawnPlayerAt(player, checkpointPosition);

                // Eğer ölümden geliniyorsa bayrağı sıfırla; yoksa zaten 0/1 fark etmez
                PlayerPrefs.SetInt("PlayerDied", 0);
                PlayerPrefs.Save();
                return;
            }
        }
        
        // Diğer durumlar için varsayılan spawn noktasına yerleştir
        GameObject spawnPoint = GameObject.FindGameObjectWithTag("PlayerSpawnPoint");
        
        if (spawnPoint != null)
        {
            SpawnPlayerAt(player, spawnPoint.transform.position);
        }
        else
        {
            // Varsayılan spawn noktası yoksa sahne ortasında spawn et
            SpawnPlayerAt(player, Vector3.zero);
            Debug.LogWarning("PlayerSpawnPoint not found, spawning at origin");
        }
    }
    
    // Oyuncuyu belirtilen pozisyonda spawn et
    private void SpawnPlayerAt(Player player, Vector3 position)
    {
        player.gameObject.SetActive(true);
        player.transform.position = position;
        
        // Physics'i sıfırla
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
        
        player.ResetPlayerFacing();
        player.ShowWeapons();
    }
   
    
    // Özel kapı geçişleri için spawn pozisyonunu ayarla
    private void HandleCustomSpawnPosition(Player player)
    {
        float offsetX = PlayerPrefs.GetFloat("PlayerSpawnX", 0);
        float offsetY = PlayerPrefs.GetFloat("PlayerSpawnY", 0);
        
        // Sahnenin sınırlarını bul
        SceneBoundary boundary = FindFirstObjectByType<SceneBoundary>();
        if (boundary != null)
        {
            float spawnX = 0, spawnY = 0;
            
            // X koordinatını belirle
            if (offsetX > 0) // Sağ taraftan giriş (sol kapıdan)
            {
                spawnX = boundary.leftBoundary + 2f; 
                
                // Y koordinatını koru
                if (PlayerPrefs.GetInt("PreserveY", 0) == 1)
                {
                    spawnY = PlayerPrefs.GetFloat("PlayerRelativeY", 0);
                }
            }
            else if (offsetX < 0) // Sol taraftan giriş (sağ kapıdan)
            {
                spawnX = boundary.rightBoundary - 2f;
                
                // Y koordinatını koru
                if (PlayerPrefs.GetInt("PreserveY", 0) == 1)
                {
                    spawnY = PlayerPrefs.GetFloat("PlayerRelativeY", 0);
                }
            }
            else if (offsetY > 0) // Üst taraftan giriş (alt kapıdan)
            {
                spawnY = boundary.bottomBoundary + 3f;
                
                // X koordinatını koru
                if (PlayerPrefs.GetInt("PreserveX", 0) == 1)
                {
                    spawnX = PlayerPrefs.GetFloat("PlayerRelativeX", 0);
                }
            }
            else if (offsetY < 0) // Alt taraftan giriş (üst kapıdan)
            {
                spawnY = boundary.topBoundary - 2f;
                
                // X koordinatını koru
                if (PlayerPrefs.GetInt("PreserveX", 0) == 1)
                {
                    spawnX = PlayerPrefs.GetFloat("PlayerRelativeX", 0);
                }
            }
            
            // Zemin seviyesinden biraz yukarıda spawn olması için ek Y offseti
            // Alt kapıdan girişler için daha fazla Y offset ekle
            if (offsetY > 0) // Üst taraftan giriş (alt kapıdan) 
            {
                spawnY += 1.5f; // Ek yükseklik ekle - yerin altında spawn olmasın
            }
            
            // Oyuncuyu konumlandır
            SpawnPlayerAt(player, new Vector2(spawnX, spawnY));
        }
        else
        {
            // Sınır yoksa varsayılan spawn noktasını kullan
            GameObject spawnPoint = GameObject.FindGameObjectWithTag("PlayerSpawnPoint");
            if (spawnPoint != null)
            {
                SpawnPlayerAt(player, spawnPoint.transform.position);
            }
            else
            {
                SpawnPlayerAt(player, Vector3.zero);
            }
        }
        
        // Özel spawn bayrağını sıfırla
        ClearSpawnPrefs("UseCustomSpawn");
    }
    
    // Spawn preferences'larını temizle
    private void ClearSpawnPrefs(string specificKey = "")
    {
        if (!string.IsNullOrEmpty(specificKey))
        {
            PlayerPrefs.SetInt(specificKey, 0);
        }
        
        PlayerPrefs.SetInt("PreserveX", 0);
        PlayerPrefs.SetInt("PreserveY", 0);
        PlayerPrefs.Save();
    }
    
    public void LoadCheckpointScene()
    {
        if (PlayerPrefs.GetInt("CheckpointActivated", 0) == 1)
        {
            int checkpointSceneIndex = PlayerPrefs.GetInt("CheckpointSceneIndex", 0);
            UnityEngine.SceneManagement.SceneManager.LoadScene(checkpointSceneIndex);
        }
    }
    
    public void LoadBossArena(int sceneIndex)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneIndex);
    }
}

