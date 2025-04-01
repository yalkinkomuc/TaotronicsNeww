using UnityEngine;

public class SceneTransitionTrigger : MonoBehaviour
{
    public int targetSceneIndex;
    //[SerializeField] private float transitionDelay = 0.2f;
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private bool usePlayerPosition = true;
    [SerializeField] private Vector2 playerPositionOffset = Vector2.zero;
    
    // Yeni sahneye geçiş süreci çalışıyor mu?
    private bool isTransitioning = false;
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag) && !isTransitioning)
        {
            isTransitioning = true;
            
            // Oyuncu pozisyonunu kaydet
            if (usePlayerPosition)
            {
                SavePlayerPosition();
            }
            
            // Sahne geçişini başlat
            StartSceneTransition();
        }
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