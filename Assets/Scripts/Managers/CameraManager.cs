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
        virtualCamera = FindObjectOfType<CinemachineVirtualCamera>();
        
        if (virtualCamera != null)
        {
            // Rotasyon takibini kapat
            var composer = virtualCamera.GetCinemachineComponent<CinemachineComposer>();
            if (composer != null)
            {
                composer.m_TrackedObjectOffset = Vector3.zero;
                composer.m_LookaheadTime = 0;
            }

            // Kamera pozisyonunu ayarla
            var transposer = virtualCamera.GetCinemachineComponent<CinemachineTransposer>();
            if (transposer != null)
            {
                transposer.m_BindingMode = CinemachineTransposer.BindingMode.SimpleFollowWithWorldUp;
                transposer.m_XDamping = 1f;
                transposer.m_YDamping = 1f;
            }

            if (PlayerManager.instance?.player != null)
            {
                virtualCamera.Follow = PlayerManager.instance.player.transform;
                Debug.Log("Camera target set to player");
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