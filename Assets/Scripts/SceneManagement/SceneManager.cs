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
            DontDestroyOnLoad(gameObject);
            
            // Aktif checkpoint kontrolü
            if (PlayerPrefs.GetInt("CheckpointActivated", 0) == 1)
            {
                int checkpointSceneIndex = PlayerPrefs.GetInt("CheckpointSceneIndex", 0);
                int currentSceneIndex = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
                
                if (checkpointSceneIndex != currentSceneIndex)
                {
                    Debug.Log($"Loading checkpoint scene: {checkpointSceneIndex}");
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
        Player player = PlayerManager.instance?.player;
        if (player != null)
        {
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
                        player.gameObject.SetActive(true);
                        player.transform.position = namedSpawnPoint.transform.position;
                        player.ResetPlayerFacing();
                        player.ShowWeapons();
                        
                        // Named spawn point bayrağını sıfırla
                        PlayerPrefs.SetInt("UseNamedSpawnPoint", 0);
                        PlayerPrefs.Save();
                        
                        Debug.Log($"Oyuncu spawn noktasına yerleştirildi: {spawnPointName}");
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
                    
                    player.gameObject.SetActive(true);
                    player.transform.position = checkpointPosition;
                    player.ResetPlayerFacing();
                    player.ShowWeapons();
                    
                    // Bayrağı sıfırla
                    PlayerPrefs.SetInt("UseCheckpointRespawn", 0);
                    PlayerPrefs.Save();
                    return;
                }
            }
            
            // Normalde checkpoint varsa, ölüm sonrası checkpoint'e dön
            if (PlayerPrefs.GetInt("CheckpointActivated", 0) == 1 && PlayerPrefs.GetInt("PlayerDied", 0) == 1)
            {
                int checkpointSceneIndex = PlayerPrefs.GetInt("CheckpointSceneIndex", 0);
                int currentSceneIndex = scene.buildIndex;
                
                // Eğer yüklenen sahne checkpoint'in olduğu sahne ise
                if (currentSceneIndex == checkpointSceneIndex)
                {
                    float x = PlayerPrefs.GetFloat("CheckpointX");
                    float y = PlayerPrefs.GetFloat("CheckpointY");
                    Vector2 checkpointPosition = new Vector2(x, y);
                    
                    player.gameObject.SetActive(true);
                    player.transform.position = checkpointPosition;
                    player.ResetPlayerFacing();
                    player.ShowWeapons();
                    
                    // Ölüm bayrağını sıfırla
                    PlayerPrefs.SetInt("PlayerDied", 0);
                    PlayerPrefs.Save();
                    return;
                }
            }
            
            // Diğer durumlar için varsayılan spawn noktasına yerleştir
            GameObject spawnPoint = GameObject.FindGameObjectWithTag("PlayerSpawnPoint");
            
            if (spawnPoint != null)
            {
                player.gameObject.SetActive(true);
                player.transform.position = spawnPoint.transform.position;
                player.ResetPlayerFacing();
            }
            
            player.ShowWeapons();
        }
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
            player.gameObject.SetActive(true);
            player.transform.position = new Vector2(spawnX, spawnY);
            player.ResetPlayerFacing();
            player.ShowWeapons();
        }
        else
        {
            // Sınır yoksa varsayılan spawn noktasını kullan
            GameObject spawnPoint = GameObject.FindGameObjectWithTag("PlayerSpawnPoint");
            if (spawnPoint != null)
            {
                player.gameObject.SetActive(true);
                player.transform.position = spawnPoint.transform.position;
                player.ResetPlayerFacing();
                player.ShowWeapons();
            }
        }
        
        // Özel spawn bayrağını sıfırla
        PlayerPrefs.SetInt("UseCustomSpawn", 0);
        PlayerPrefs.SetInt("PreserveX", 0);
        PlayerPrefs.SetInt("PreserveY", 0);
        PlayerPrefs.Save();
    }
    
    public void LoadCheckpointScene()
    {
        if (PlayerPrefs.GetInt("CheckpointActivated", 0) == 1)
        {
            int checkpointSceneIndex = PlayerPrefs.GetInt("CheckpointSceneIndex", 0);
            Debug.Log($"Loading checkpoint scene: {checkpointSceneIndex}");
            UnityEngine.SceneManagement.SceneManager.LoadScene(checkpointSceneIndex);
        }
    }
    
    public void LoadBossArena(int sceneIndex)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneIndex);
    }
}

