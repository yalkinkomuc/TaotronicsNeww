using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneManager : MonoBehaviour
{
    public static SceneManager instance { get; private set; }
    
    [Header("Scene Transition")]
    //[SerializeField] private float fadeTime = 0.5f;
    [SerializeField] private Color fadeColor = Color.black;
    [SerializeField] private GameObject transitionEffectPrefab;
    
    // İlk sahne yüklendiğinde kontrol için
    private bool isFirstSceneLoad = true;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Aktif checkpoint varsa, checkpoint sahnesini yükle
            if (PlayerPrefs.GetInt("CheckpointActivated", 0) == 1)
            {
                int checkpointSceneIndex = PlayerPrefs.GetInt("CheckpointSceneIndex", 0);
                int currentSceneIndex = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
                
                // Farklı sahnedeyse, checkpoint sahnesini yükle
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

    private void Start()
    {
        // Start'ta yeniden kontrol - çoğu Unity örneğinin Awake'ten sonra başlatıldığından emin olmak için
        if (isFirstSceneLoad && PlayerPrefs.GetInt("CheckpointActivated", 0) == 1)
        {
            int checkpointSceneIndex = PlayerPrefs.GetInt("CheckpointSceneIndex", 0);
            int currentSceneIndex = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
            string currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            
            Debug.Log($"[SCENE MANAGER START] Current Scene: {currentSceneName} (Index: {currentSceneIndex}), Checkpoint Scene: {checkpointSceneIndex}");
            
            if (checkpointSceneIndex != currentSceneIndex)
            {
                Debug.Log($"[SCENE MANAGER START] Loading checkpoint scene: {checkpointSceneIndex}");
                LoadBossArena(checkpointSceneIndex);
            }
        }
    }

    public void CallLoadActiveScene(float delay)
    {
        Invoke("LoadActiveScene", delay);
    }

    public void LoadActiveScene()
    {
        StartCoroutine(LoadSceneWithTransition(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex));
    }

    public void LoadBossArena(int sceneIndex)
    {
        StartCoroutine(LoadSceneWithTransition(sceneIndex));
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
    
    private IEnumerator LoadSceneWithTransition(int sceneIndex)
    {
        // Geçiş efekti oluştur
        if (transitionEffectPrefab != null)
        {
            GameObject transitionObj = Instantiate(transitionEffectPrefab);
            DontDestroyOnLoad(transitionObj);
        }
        
        // Sahneyi yükle
        AsyncOperation asyncLoad = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneIndex);
        asyncLoad.allowSceneActivation = false;
        
        // Yükleme ilerlemesini bekle
        while (asyncLoad.progress < 0.9f)
        {
            yield return null;
        }
        
        // Sahneyi aktifleştir
        asyncLoad.allowSceneActivation = true;
        
        // Sahne tamamen yüklenene kadar bekle
        while (!asyncLoad.isDone)
        {
            yield return null;
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
        string sceneName = scene.name;
        int sceneIndex = scene.buildIndex;
        Debug.Log($"[SCENE LOADED] Scene: {sceneName} (Index: {sceneIndex})");
        
        Player player = PlayerManager.instance?.player;
        if (player != null)
        {
            // Checkpoint kontrol et
            if (PlayerPrefs.GetInt("CheckpointActivated", 0) == 1)
            {
                int checkpointSceneIndex = PlayerPrefs.GetInt("CheckpointSceneIndex", 0);
                int currentSceneIndex = scene.buildIndex;
                
                Debug.Log($"[SCENE LOADED] Checkpoint Scene: {checkpointSceneIndex}, Current Scene: {currentSceneIndex}");
                
                // Eğer yüklenen sahne checkpoint'in olduğu sahne ise
                if (currentSceneIndex == checkpointSceneIndex)
                {
                    float x = PlayerPrefs.GetFloat("CheckpointX");
                    float y = PlayerPrefs.GetFloat("CheckpointY");
                    Vector2 checkpointPosition = new Vector2(x, y);
                    
                    // Oyuncuyu checkpoint konumuna ışınla
                    player.gameObject.SetActive(true);
                    player.transform.position = checkpointPosition;
                    player.ResetPlayerFacing();
                    
                    // Silahları göster
                    player.ShowWeapons();
                    
                    Debug.Log($"[SCENE LOADED] Player respawned at checkpoint position: ({x}, {y}) in scene {currentSceneIndex}");
                    return;
                }
                else
                {
                    Debug.LogWarning($"[SCENE LOADED] Current scene {currentSceneIndex} does not match checkpoint scene {checkpointSceneIndex}");
                }
            }
            else
            {
                Debug.Log("[SCENE LOADED] No active checkpoint found. Using spawn point.");
            }
            
            // Checkpoint yoksa veya farklı sahneyi yüklediyse, normal spawn noktasını kullan
            GameObject spawnPoint = GameObject.FindGameObjectWithTag("PlayerSpawnPoint");
            
            if (spawnPoint != null)
            {
                player.gameObject.SetActive(true);
                // Oyuncuyu spawn noktasına ışınla
                player.transform.position = spawnPoint.transform.position;
                player.ResetPlayerFacing();
                Debug.Log($"[SCENE LOADED] Player spawned at spawn point: ({spawnPoint.transform.position.x}, {spawnPoint.transform.position.y})");
            }
            else
            {
                Debug.LogWarning("[SCENE LOADED] PlayerSpawnPoint bulunamadı! Lütfen sahnede 'PlayerSpawnPoint' tag'li bir obje olduğundan emin olun.");
            }
            
            // Silahları göster
            player.ShowWeapons();
        }
        else
        {
            Debug.LogWarning("[SCENE LOADED] PlayerManager.instance.player reference is null!");
        }
    }
    
    private void PlacePlayerAtSceneBorder(Player player)
    {
        // Oyuncunun hangi taraftan geleceği bilgisini al
        float spawnX = PlayerPrefs.GetFloat("PlayerSpawnX", 0);
        float spawnY = PlayerPrefs.GetFloat("PlayerSpawnY", 0);
        
        // Sahne sınırlarını bul
        SceneBoundary boundary = FindFirstObjectByType<SceneBoundary>();
        if (boundary == null)
        {
            Debug.LogWarning("SceneBoundary bulunamadı! Oyuncu varsayılan spawn noktasına yerleştirilecek.");
            return;
        }
        
        Vector3 targetPosition = player.transform.position;
        
        // Oyuncunun geçiş yaptığı andaki pozisyonunu koru
        bool preserveY = PlayerPrefs.GetInt("PreserveY", 0) == 1;
        bool preserveX = PlayerPrefs.GetInt("PreserveX", 0) == 1;
        
        // Oyuncuyu doğru kenara yerleştir
        if (spawnX < 0) // Sağ kenardan giriş
        {
            targetPosition.x = boundary.rightBoundary - 1.5f; // Biraz daha içeriden başlasın
            player.facingdir = -1;
            player.transform.localScale = new Vector3(-1, 1, 1);
            
            // Y pozisyonunu koru
            if (preserveY)
            {
                float relativeY = PlayerPrefs.GetFloat("PlayerRelativeY");
                // Yeni sahnenin sınırları içinde kaldığından emin ol
                relativeY = Mathf.Clamp(relativeY, boundary.bottomBoundary + 1f, boundary.topBoundary - 1f);
                targetPosition.y = relativeY;
            }
        }
        else if (spawnX > 0) // Sol kenardan giriş
        {
            targetPosition.x = boundary.leftBoundary + 1.5f; // Biraz daha içeriden başlasın
            player.facingdir = 1;
            player.transform.localScale = new Vector3(1, 1, 1);
            
            // Y pozisyonunu koru
            if (preserveY)
            {
                float relativeY = PlayerPrefs.GetFloat("PlayerRelativeY");
                // Yeni sahnenin sınırları içinde kaldığından emin ol
                relativeY = Mathf.Clamp(relativeY, boundary.bottomBoundary + 1f, boundary.topBoundary - 1f);
                targetPosition.y = relativeY;
            }
        }
        
        if (spawnY < 0) // Üst kenardan giriş
        {
            targetPosition.y = boundary.topBoundary - 1.5f;
            
            // X pozisyonunu koru
            if (preserveX)
            {
                float relativeX = PlayerPrefs.GetFloat("PlayerRelativeX");
                // Yeni sahnenin sınırları içinde kaldığından emin ol
                relativeX = Mathf.Clamp(relativeX, boundary.leftBoundary + 1f, boundary.rightBoundary - 1f);
                targetPosition.x = relativeX;
            }
        }
        else if (spawnY > 0) // Alt kenardan giriş
        {
            targetPosition.y = boundary.bottomBoundary + 1.5f;
            
            // X pozisyonunu koru
            if (preserveX)
            {
                float relativeX = PlayerPrefs.GetFloat("PlayerRelativeX");
                // Yeni sahnenin sınırları içinde kaldığından emin ol
                relativeX = Mathf.Clamp(relativeX, boundary.leftBoundary + 1f, boundary.rightBoundary - 1f);
                targetPosition.x = relativeX;
            }
        }
        
        // Oyuncuyu konumlandır
        player.gameObject.SetActive(true);
        player.transform.position = targetPosition;
        
        // Kullanılan değerleri sıfırla
        PlayerPrefs.SetInt("PreserveY", 0);
        PlayerPrefs.SetInt("PreserveX", 0);
    }
}

