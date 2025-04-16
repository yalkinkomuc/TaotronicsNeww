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
        }
        else
        {
            Destroy(gameObject);
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