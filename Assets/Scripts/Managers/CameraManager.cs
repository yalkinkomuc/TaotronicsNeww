using UnityEngine;
using Cinemachine;
using System.Collections;

public class CameraManager : MonoBehaviour, IManager
{
    public static CameraManager instance { get; private set; }
    
    [SerializeField] private CinemachineVirtualCamera cameraPrefab; // Ana kamera prefabınız
    private CinemachineVirtualCamera activeCamera;
    
    // Kamera ayarları için sabit değerler
    [SerializeField] private float xDamping = 1f;
    [SerializeField] private float yDamping = 1f;
    private bool isInitialized = false;

    // Kamera sınırları için değişkenler
    [Header("Camera Boundaries")]
    [SerializeField] private bool useCameraBounds = true;
    [SerializeField] private float leftBoundary = float.MinValue;
    [SerializeField] private float rightBoundary = float.MaxValue;
    [SerializeField] private float topBoundary = float.MaxValue;
    [SerializeField] private float bottomBoundary = float.MinValue;
    
    [Header("Edge Behavior")]
    [SerializeField] private float edgeThreshold = 1.0f; // Kenar algılama eşiği
    [SerializeField] private float deadZoneMaxWidth = 0.95f; // Maksimum ölü bölge genişliği
    [SerializeField] private float deadZoneTransitionSpeed = 5f; // Ölü bölge geçiş hızı
    
    private float targetDeadZoneWidth = 0f;
    private float currentDeadZoneWidth = 0f;

    private CinemachineFramingTransposer framingTransposer;
    private CinemachineConfiner confiner;

    // Son bilinen oyuncu pozisyonu ve yön takibi
    private Vector3 lastPlayerPos = Vector3.zero;
    private Vector3 playerDirection = Vector3.zero;
    private float targetScreenX = 0.5f; // Hedef ekran pozisyonu
    private float screenXTransitionSpeed = 3f; // Geçiş hızı

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

    private void Update()
    {
        if (useCameraBounds && activeCamera != null)
        {
            // Kamera sınırlarını kontrol et
            UpdateCameraPosition();
            
            // Oyuncunun hareket yönünü kontrol et ve kamerayı ayarla
            UpdatePlayerDirectionAndCamera();
        }
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
        
        // Yeni sahnede sınırlayıcıları kontrol et
        LookForSceneBoundaries();
        
        // Ölü bölgeyi sıfırla
        ResetCameraDeadZone();
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

            // Framing transposer referansını al
            framingTransposer = activeCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
            if (framingTransposer != null)
            {
                // Başlangıçta dead zone olmadan başla
                currentDeadZoneWidth = 0f;
                targetDeadZoneWidth = 0f;
                framingTransposer.m_DeadZoneWidth = currentDeadZoneWidth;
                
                // Kameranın oyuncuyu ekranda konumlandırmasını ortadan biraz kaydır (0.5 = orta)
                framingTransposer.m_ScreenX = 0.5f; // Başlangıçta ortalı
                
                // Lookahead kapatılsın, manuel kontrol yapacağız
                framingTransposer.m_LookaheadTime = 0f;
                
                // Düşey konumlandırma
                framingTransposer.m_ScreenY = 0.6f; // Oyuncuyu ekranın biraz üstünde göster
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
    
    // Sahne sınırlarını belirle
    public void SetCameraBoundaries(float left, float right, float top, float bottom)
    {
        leftBoundary = left;
        rightBoundary = right;
        topBoundary = top;
        bottomBoundary = bottom;
        
        useCameraBounds = true;
        
        // Sınırlar değiştiğinde kamera ölü bölgesi sıfırla
        ResetCameraDeadZone();
        
        Debug.Log($"Camera boundaries set: L:{left}, R:{right}, T:{top}, B:{bottom}");
    }
    
    // Kamera ölü bölgesini sıfırla
    private void ResetCameraDeadZone()
    {
        targetDeadZoneWidth = 0f;
        if (framingTransposer != null)
        {
            framingTransposer.m_DeadZoneWidth = 0f;
            currentDeadZoneWidth = 0f;
        }
    }
    
    // Kamera pozisyonunu sınırlara göre güncelle
    private void UpdateCameraPosition()
    {
        if (activeCamera == null) return;
        
        // Kameranın mevcut pozisyonu
        Vector3 cameraPos = activeCamera.transform.position;
        
        // Kamera boyutlarını hesapla (ortografik boyutlar)
        float height = 2f * Camera.main.orthographicSize;
        float width = height * Camera.main.aspect;
        
        // Sınırlar içinde kalmayı zorla
        float clampedX = Mathf.Clamp(cameraPos.x, leftBoundary + width/2, rightBoundary - width/2);
        float clampedY = Mathf.Clamp(cameraPos.y, bottomBoundary + height/2, topBoundary - height/2);
        
        // Eğer kamera pozisyonu değiştiyse, direkt olarak pozisyonu güncelle
        if (clampedX != cameraPos.x || clampedY != cameraPos.y)
        {
            activeCamera.transform.position = new Vector3(clampedX, clampedY, cameraPos.z);
        }
        
        // Dead zone mantığını tamamen kaldır - artık buna ihtiyacımız yok
        if (framingTransposer != null)
        {
            framingTransposer.m_DeadZoneWidth = 0f;
        }
        
        // Debug çizgileri
        Debug.DrawLine(
            new Vector3(leftBoundary, cameraPos.y - 50, 0),
            new Vector3(leftBoundary, cameraPos.y + 50, 0),
            Color.red
        );
        
        Debug.DrawLine(
            new Vector3(rightBoundary, cameraPos.y - 50, 0),
            new Vector3(rightBoundary, cameraPos.y + 50, 0),
            Color.red
        );
    }
    
    // Sahne sınırlayıcılarını bul
    private void LookForSceneBoundaries()
    {
        // "SceneBoundary" etiketli nesneleri bul
        GameObject[] boundaries = GameObject.FindGameObjectsWithTag("SceneBoundary");
        Debug.Log($"Found {boundaries.Length} SceneBoundary objects");

        if (boundaries.Length > 0)
        {
            SceneBoundary sceneBoundary = boundaries[0].GetComponent<SceneBoundary>();
            if (sceneBoundary != null)
            {
                Debug.Log($"SceneBoundary found with bounds: Left={sceneBoundary.leftBoundary}, Right={sceneBoundary.rightBoundary}");
                SetCameraBoundaries(
                    sceneBoundary.leftBoundary,
                    sceneBoundary.rightBoundary,
                    sceneBoundary.topBoundary,
                    sceneBoundary.bottomBoundary
                );
            }
            else
            {
                Debug.LogWarning("Found object with SceneBoundary tag but no SceneBoundary component!");
            }
        }
        else
        {
            Debug.LogWarning("No objects with SceneBoundary tag found in the scene!");
            useCameraBounds = false;
        }
    }

    private void UpdatePlayerDirectionAndCamera()
    {
        if (PlayerManager.instance?.player != null && framingTransposer != null)
        {
            Player player = PlayerManager.instance.player;
            Vector3 playerPos = player.transform.position;
            
            // İlk çağrı için başlangıç değeri
            if (lastPlayerPos == Vector3.zero)
            {
                lastPlayerPos = playerPos;
                return;
            }
            
            // Oyuncunun hareket yönünü belirle
            playerDirection = playerPos - lastPlayerPos;
            
            // Oyuncu sağa hareket ediyorsa (pozitif X)
            if (playerDirection.x > 0.01f)
            {
                // Oyuncu sağa gidiyorsa, ekranın solunda kalsın (0.35 = solda)
                targetScreenX = 0.35f;
            }
            // Oyuncu sola hareket ediyorsa (negatif X)
            else if (playerDirection.x < -0.01f)
            {
                // Oyuncu sola gidiyorsa, ekranın sağında kalsın (0.65 = sağda)
                targetScreenX = 0.65f;
            }
            
            // ScreenX değerini yumuşak geçişle güncelle
            float currentScreenX = framingTransposer.m_ScreenX;
            framingTransposer.m_ScreenX = Mathf.Lerp(currentScreenX, targetScreenX, Time.deltaTime * screenXTransitionSpeed);
            
            // Oyuncu pozisyonunu güncelle
            lastPlayerPos = playerPos;
        }
    }
} 