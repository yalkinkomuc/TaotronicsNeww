using UnityEngine;
using Cinemachine;

public class CameraManager : MonoBehaviour
{
    public static CameraManager instance { get; private set; }
    
    private CinemachineVirtualCamera virtualCamera;

    private void Awake()
    {
        // Singleton setup
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        // Oyun başladığında kamerayı ayarla
        SetupCamera();
    }

    private void OnEnable()
    {
        // Sahne değiştiğinde kamerayı yeniden ayarla
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        // Her sahne yüklendiğinde kamerayı ayarla
        SetupCamera();
    }

    private void SetupCamera()
    {
        // Sahnedeki virtual camera'yı bul
        virtualCamera = Object.FindAnyObjectByType<CinemachineVirtualCamera>();
        
        if (virtualCamera != null)
        {
            // Player'ı follow target olarak ata
            if (PlayerManager.instance != null && PlayerManager.instance.player != null)
            {
                virtualCamera.Follow = PlayerManager.instance.player.transform;
                Debug.Log("Camera target set to player");
            }
            else
            {
                Debug.LogWarning("Player not found for camera setup!");
            }
        }
        else
        {
            Debug.LogWarning("Virtual Camera not found in scene!");
        }
    }

    // Gerekirse manuel olarak kamera hedefini değiştirmek için
    public void SetCameraTarget(Transform target)
    {
        if (virtualCamera != null)
        {
            virtualCamera.Follow = target;
        }
    }
} 