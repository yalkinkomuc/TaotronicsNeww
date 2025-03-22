using UnityEngine;
using Cinemachine;

public class CameraManager : MonoBehaviour, IManager
{
    public static CameraManager instance { get; private set; }
    
    [SerializeField] private CinemachineVirtualCamera cameraPrefab; // Ana kamera prefabınız
    private CinemachineVirtualCamera activeCamera;
    
    // Kamera ayarları için sabit değerler
    [SerializeField] private float xDamping = 1f;
    [SerializeField] private float yDamping = 1f;
    private bool isInitialized = false;

    private void Awake()
    {
        // Singleton setup
        if (instance == null)
        {
            instance = this;
            //DontDestroyOnLoad(gameObject);
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
        Initialize();
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

    public void Initialize()
    {
        if (!isInitialized)
        {
            SetupCamera();
            isInitialized = true;
        }
    }

    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        // Sahnedeki tüm virtual kameraları bul
        var sceneCameras = UnityEngine.Object.FindObjectsByType<CinemachineVirtualCamera>(
            FindObjectsSortMode.None // Sıralama gerekmiyorsa None kullanarak performans kazanıyoruz
        );
        
        // Sahnedeki fazla kameraları devre dışı bırak veya sil
        foreach (var cam in sceneCameras)
        {
            if (cam != activeCamera)
            {
                Debug.Log($"Disabling extra camera: {cam.name}");
                Destroy(cam.gameObject);
            }
        }

        // Eğer aktif kamera yoksa yeni bir tane oluştur
        if (activeCamera == null)
        {
            CreateMainCamera();
        }

        UpdateCameraTarget();
    }

    private void CreateMainCamera()
    {
        if (cameraPrefab != null)
        {
            activeCamera = Instantiate(cameraPrefab);
            activeCamera.name = "Main Virtual Camera";
            SetupCamera();
        }
        else
        {
            Debug.LogError("Camera prefab is not assigned in CameraManager!");
        }
    }

    private void SetupCamera()
    {
        if (activeCamera != null)
        {
            // Rotasyon takibini kapat
            var composer = activeCamera.GetCinemachineComponent<CinemachineComposer>();
            if (composer != null)
            {
                composer.m_TrackedObjectOffset = Vector3.zero;
                composer.m_LookaheadTime = 0;
            }

            // Kamera pozisyonunu ayarla
            var transposer = activeCamera.GetCinemachineComponent<CinemachineTransposer>();
            if (transposer != null)
            {
                transposer.m_BindingMode = CinemachineTransposer.BindingMode.SimpleFollowWithWorldUp;
                transposer.m_XDamping = xDamping;
                transposer.m_YDamping = yDamping;
            }

            UpdateCameraTarget();
        }
    }

    private void UpdateCameraTarget()
    {
        if (activeCamera != null && PlayerManager.instance?.player != null)
        {
            activeCamera.Follow = PlayerManager.instance.player.transform;
            Debug.Log("Camera target updated to player");
        }
    }

    // Gerekirse manuel olarak kamera hedefini değiştirmek için
    public void SetCameraTarget(Transform target)
    {
        if (activeCamera != null)
        {
            activeCamera.Follow = target;
        }
    }
} 