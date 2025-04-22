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
            // Checkpoint kontrolü
            if (PlayerPrefs.GetInt("CheckpointActivated", 0) == 1)
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
                    return;
                }
            }
            
            // Spawn noktasına yerleştir
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

