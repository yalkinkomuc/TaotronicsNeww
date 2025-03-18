using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneManager : MonoBehaviour
{
    public static SceneManager instance { get; private set; }
    

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
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
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }

    public void LoadBossArena(int sceneIndex)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneIndex);
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
        }
    }

    
}

