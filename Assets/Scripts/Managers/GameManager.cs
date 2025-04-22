using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Managers")]
    public PlayerManager playerManager;
    public SceneManager sceneManager;
    public DialogueManager dialogueManager;
    public VFXManager vfxManager;
    public CameraManager cameraManager;
    public QuestManager questManager;
    // Diğer manager referansları...

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeManagers();
            
            // Checkpoint kontrolü
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

    private void Update()
    {
        // Debug key to test checkpoint loading (can be removed in final build)
        if (Input.GetKeyDown(KeyCode.F9))
        {
            if (PlayerPrefs.GetInt("CheckpointActivated", 0) == 1)
            {
                int checkpointSceneIndex = PlayerPrefs.GetInt("CheckpointSceneIndex", 0);
                Debug.Log($"Loading checkpoint scene: {checkpointSceneIndex}");
                UnityEngine.SceneManagement.SceneManager.LoadScene(checkpointSceneIndex);
            }
        }
    }

    private void InitializeManagers()
    {
        // Manager'ların hepsinin doğru şekilde oluştuğundan emin ol
        foreach (Transform child in transform)
        {
            var managerComponent = child.GetComponent<IManager>();
            if (managerComponent != null)
            {
                managerComponent.Initialize();
            }
        }
    }
}

// Tüm Manager'lar için ortak interface
public interface IManager
{
    void Initialize();
} 