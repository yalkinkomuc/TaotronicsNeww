using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneManager : MonoBehaviour
{
    public static SceneManager instance { get; private set; }
    
    [Header("Scene Transition")]
    [SerializeField] private float fadeTime = 0.5f;
    [SerializeField] private Color fadeColor = Color.black;
    [SerializeField] private GameObject transitionEffectPrefab;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            //DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        
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
    
    private IEnumerator LoadSceneWithTransition(int sceneIndex)
    {
        // Geçiş efekti varsa kullan
        if (transitionEffectPrefab != null)
        {
            GameObject transitionObj = Instantiate(transitionEffectPrefab);
            DontDestroyOnLoad(transitionObj);
            
            // Geçiş efekti tamamlanması için bekleme süresi
            yield return new WaitForSeconds(fadeTime);
        }
        
        // Sahneyi yükle
        AsyncOperation asyncLoad = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneIndex);
        
        // Yükleme tamamlanana kadar bekle
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
        
        // Oyuncuyu doğru pozisyona yerleştir (OnSceneLoaded'da yapılacak)
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
            // PlayerSpawnPoint'i bul
            GameObject spawnPoint = GameObject.FindGameObjectWithTag("PlayerSpawnPoint");
            
            if (spawnPoint != null)
            {
                player.gameObject.SetActive(true);
                // Oyuncuyu spawn noktasına ışınla
                player.transform.position = spawnPoint.transform.position;
                player.ResetPlayerFacing();
            }
            else
            {
                Debug.LogWarning("PlayerSpawnPoint bulunamadı! Lütfen sahnede 'PlayerSpawnPoint' tag'li bir obje olduğundan emin olun.");
            }
            
            // Silahları göster
            player.ShowWeapons();
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

