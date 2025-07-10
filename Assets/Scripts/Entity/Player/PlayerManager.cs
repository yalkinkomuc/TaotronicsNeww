using System;
using UnityEngine;
using System.Collections;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager instance;
    public Player player;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            //DontDestroyOnLoad(gameObject);
            
            // Player'ı da DontDestroyOnLoad yap
            if (player != null)
            {
                DontDestroyOnLoad(player.gameObject);
                
            }
            else
            {
                Debug.LogError("PlayerManager: Player referansı atanmamış!");
            }
            
            // Subscribe to scene loading events to maintain references
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void OnDestroy()
    {
        // Unsubscribe from events
        if (instance == this)
        {
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }
    
    // Called when a scene is loaded
    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        
        
        // Ensure player reference is maintained
        StartCoroutine(ValidatePlayerReference());
        
        // Force UI reference refresh if InGameUI exists
       
    }
    
    // Coroutine to validate and restore player reference after scene loading
    private IEnumerator ValidatePlayerReference()
    {
        // Wait a bit for scene to fully load
        yield return new WaitForSeconds(0.1f);
        
        // Check if player reference is still valid
        if (player == null || player.gameObject == null)
        {
            Debug.LogWarning("PlayerManager: Player reference lost, attempting to find player...");
            
            // Try to find player in scene
            Player foundPlayer = FindFirstObjectByType<Player>();
            if (foundPlayer != null)
            {
                player = foundPlayer;
                DontDestroyOnLoad(player.gameObject);
                Debug.Log("PlayerManager: Player reference restored");
            }
            else
            {
                Debug.LogError("PlayerManager: Could not find Player in scene!");
            }
        }
        else
        {
            Debug.Log("PlayerManager: Player reference is valid");
        }
        
        // Force health and mana bar reinitialization if needed
        if (player != null)
        {
            HealthBar healthBar = player.GetComponent<HealthBar>();
            if (healthBar != null)
            {
                healthBar.ForceReinitialize();
            }
            
            ManaBar manaBar = player.GetComponent<ManaBar>();
            if (manaBar != null)
            {
                manaBar.ForceReinitialize();
            }
            
            // Refresh player stats UI references
            PlayerStats playerStats = player.GetComponent<PlayerStats>();
            if (playerStats != null)
            {
                // Force a UI update to trigger reference refresh
                playerStats.UpdateLevelUI();
            }
        }
    }

    void Update()
    {
        ClampPositionToArena();
    }

    void ClampPositionToArena()
    {
        if (BossArenaManager.ArenaCollider != null)
        {
            Vector3 newPos = transform.position;
            
            // X pozisyonunu sınırla
            newPos.x = Mathf.Clamp(newPos.x, 
                BossArenaManager.ArenaCollider.bounds.min.x + 1f, 
                BossArenaManager.ArenaCollider.bounds.max.x - 1f);
            
            // Y pozisyonunu sınırla
            newPos.y = Mathf.Clamp(newPos.y, 
                BossArenaManager.ArenaCollider.bounds.min.y + 1f, 
                BossArenaManager.ArenaCollider.bounds.max.y - 1f);
            
            transform.position = newPos;
        }
    }
}
