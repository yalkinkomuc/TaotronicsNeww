using UnityEngine;

public class SceneTransitionTrigger : MonoBehaviour
{
    public int targetSceneIndex;
    //[SerializeField] private float transitionDelay = 0.2f;
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private bool usePlayerPosition = true;
    [SerializeField] private Vector2 playerPositionOffset = Vector2.zero;
    
    [Header("Spawn Settings")]
    [SerializeField] private bool useSpawnPoint = true;
    [SerializeField] private string spawnPointName = ""; // Hedef sahnedeki spawn noktasının adı
    
    // Yeni sahneye geçiş süreci çalışıyor mu?
    private bool isTransitioning = false;
    
    private void Awake()
    {
        // Trigger adına göre otomatik olarak spawn point adını belirle
        if (useSpawnPoint && string.IsNullOrEmpty(spawnPointName))
        {
            AssignSpawnPointNameBasedOnTriggerName();
        }
    }
    
    // Trigger adına göre spawn point adını otomatik atama
    private void AssignSpawnPointNameBasedOnTriggerName()
    {
        string triggerName = gameObject.name.ToLower();
        
        // Left, Right, Top, Bottom adlarını kontrol et
        if (triggerName.Contains("left") || transform.parent?.name.ToLower().Contains("left") == true)
        {
            spawnPointName = "LeftSpawn";
        }
        else if (triggerName.Contains("right") || transform.parent?.name.ToLower().Contains("right") == true)
        {
            spawnPointName = "RightSpawn";
        }
        else if (triggerName.Contains("top") || transform.parent?.name.ToLower().Contains("top") == true) 
        {
            spawnPointName = "TopSpawn";
        }
        else if (triggerName.Contains("bottom") || transform.parent?.name.ToLower().Contains("bottom") == true)
        {
            spawnPointName = "BottomSpawn";
        }
        else
        {
            // Eğer duvar adı yoksa, pozisyona göre tahmin et
            if (transform.position.x < 0)
            {
                spawnPointName = "LeftSpawn";
            }
            else if (transform.position.x > 0)
            {
                spawnPointName = "RightSpawn";
            }
            else if (transform.position.y > 0)
            {
                spawnPointName = "TopSpawn";
            }
            else if (transform.position.y < 0)
            {
                spawnPointName = "BottomSpawn";
            }
        }
        
        Debug.Log($"Trigger '{gameObject.name}' için otomatik spawn point atandı: {spawnPointName}");
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag) && !isTransitioning)
        {
            isTransitioning = true;
            
            // Spawn noktası bilgisini kaydet
            if (useSpawnPoint)
            {
                // Eğer spawnPointName boşsa, son bir kontrol yap
                if (string.IsNullOrEmpty(spawnPointName))
                {
                    AssignSpawnPointNameBasedOnTriggerName();
                }
                
                SaveSpawnPointName();
            }
            // Veya oyuncu pozisyonunu kaydet
            else if (usePlayerPosition)
            {
                SavePlayerPosition();
            }
            
            // Sahne geçişini başlat
            StartSceneTransition();
        }
    }
    
    private void SaveSpawnPointName()
    {
        // Hedef sahneye spawn noktası adını aktar
        PlayerPrefs.SetString("TargetSpawnPointName", spawnPointName);
        PlayerPrefs.SetInt("UseNamedSpawnPoint", 1);
        PlayerPrefs.Save();
        
        Debug.Log($"Kaydedilen spawn noktası adı: {spawnPointName}");
    }
    
    private void SavePlayerPosition()
    {
        // Oyuncunun hangi kenardan girdiğini belirle
        bool isLeftEntry = transform.name.Contains("Right");
        bool isRightEntry = transform.name.Contains("Left");
        bool isTopEntry = transform.name.Contains("Bottom");
        bool isBottomEntry = transform.name.Contains("Top");
        
        // Oyuncu pozisyonunu hesapla
        Vector2 playerOffset = playerPositionOffset;
        
        if (isLeftEntry)
        {
            // Oyuncu sol taraftan giriyorsa, sağ taraftan çıkacak
            playerOffset.x = -1f;
        }
        else if (isRightEntry)
        {
            // Oyuncu sağ taraftan giriyorsa, sol taraftan çıkacak
            playerOffset.x = 1f;
        }
        else if (isTopEntry)
        {
            // Oyuncu üst taraftan giriyorsa, alt taraftan çıkacak
            playerOffset.y = -1f;
        }
        else if (isBottomEntry)
        {
            // Oyuncu alt taraftan giriyorsa, üst taraftan çıkacak
            playerOffset.y = 1f;
        }
        
        // Ayrıca oyuncunun geçiş yaptığı sıradaki Y (veya X) pozisyonunu da saklayalım
        Player player = PlayerManager.instance?.player;
        if (player != null)
        {
            // Sol/sağ geçiş için y-değerini ayarla
            if (isLeftEntry || isRightEntry)
            {
                // Y-konumunu koruma önceliği
                PlayerPrefs.SetFloat("PlayerRelativeY", player.transform.position.y);
                PlayerPrefs.SetInt("PreserveY", 1);
            }
            // Üst/alt geçiş için x-değerini ayarla
            else if (isTopEntry || isBottomEntry)
            {
                // X-konumunu koruma önceliği
                PlayerPrefs.SetFloat("PlayerRelativeX", player.transform.position.x);
                PlayerPrefs.SetInt("PreserveX", 1);
            }
        }
        
        // Pozisyon bilgisini kaydet
        PlayerPrefs.SetFloat("PlayerSpawnX", playerOffset.x);
        PlayerPrefs.SetFloat("PlayerSpawnY", playerOffset.y);
        PlayerPrefs.SetInt("UseCustomSpawn", 1);
        
        // Named spawn point kullanılmayacak
        PlayerPrefs.SetInt("UseNamedSpawnPoint", 0);
        PlayerPrefs.Save();
    }
    
    private void StartSceneTransition()
    {
        // Oyuncu silahlarını geçici olarak gizle
        Player player = PlayerManager.instance?.player;
        if (player != null)
        {
            player.HideWeapons();
            
            // Hareketi durdur, oyuncunun birden fazla triggerı tetiklemesini önle
            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
            }
        }
        
        // Gecikme ile sahne geçişi
        // İsim karmaşasından kaçınmak için SceneManager'ı doğrudan arayalım
        SceneManager customSceneManager = FindFirstObjectByType<SceneManager>();
            
        if (customSceneManager != null)
        {
            customSceneManager.LoadBossArena(targetSceneIndex);
        }
        else
        {
            // Unity'nin SceneManager'ını kullan
            UnityEngine.SceneManagement.SceneManager.LoadScene(targetSceneIndex);
        }
    }
} 