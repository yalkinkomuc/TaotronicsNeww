using UnityEngine;
using Cinemachine;
using System.Collections;

public class CameraManager : MonoBehaviour
{
    public static CameraManager instance { get; private set; }
    
    [Header("Camera References")]
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    private CinemachineFramingTransposer transposer;
    private Camera mainCamera;
    
    [Header("Camera Settings")]
    [SerializeField] private float xDamping = 1f;
    [SerializeField] private float yDamping = 1f;
    [SerializeField] private float screenY = 0.6f;
    
    [Header("Player Following")]
    private Vector3 lastPlayerPos;
    private float targetScreenX = 0.5f;
    [SerializeField] private float screenXTransitionSpeed = 3f;
    
    [Header("Boundaries")]
    private float leftBoundary;
    private float rightBoundary;
    private float topBoundary;
    private float bottomBoundary;
    private bool hasBoundaries = false;
    
    // SceneBoundary'den doğrudan çağrılır
    public void RegisterSceneBoundary(SceneBoundary boundary)
    {
        if (boundary != null)
        {
            leftBoundary = boundary.leftBoundary;
            rightBoundary = boundary.rightBoundary;
            topBoundary = boundary.topBoundary;
            bottomBoundary = boundary.bottomBoundary;
            hasBoundaries = true;
            
            Debug.Log($"Camera boundaries registered directly: L={leftBoundary}, R={rightBoundary}, T={topBoundary}, B={bottomBoundary}");
        }
    }
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(transform.root.gameObject);
            
            // Camera referanslarını al
            FindOrCreateMainCamera();
            SetupVirtualCamera();
            
            Debug.Log("CameraManager initialized!");
        }
        else
        {
            Destroy(transform.root.gameObject);
        }
    }
    
    private void FindOrCreateMainCamera()
    {
        mainCamera = Camera.main;
        
        if (mainCamera == null)
        {
            Debug.Log("Creating new Main Camera");
            GameObject cameraObj = new GameObject("Main Camera");
            mainCamera = cameraObj.AddComponent<Camera>();
            mainCamera.tag = "MainCamera";
            mainCamera.orthographic = true;
            mainCamera.orthographicSize = 5;
            mainCamera.clearFlags = CameraClearFlags.SolidColor;
            mainCamera.backgroundColor = Color.black;
            
            // Cinemachine Brain ekle
            CinemachineBrain brain = cameraObj.AddComponent<CinemachineBrain>();
            brain.m_DefaultBlend.m_Time = 0.5f;
        }
    }
    
    private void SetupVirtualCamera()
    {
        if (virtualCamera == null)
        {
            virtualCamera = GetComponentInChildren<CinemachineVirtualCamera>();
            
            if (virtualCamera == null)
            {
                Debug.LogError("Virtual Camera not found! Creating one...");
                GameObject vcamObj = new GameObject("CM Virtual Camera");
                vcamObj.transform.SetParent(transform);
                virtualCamera = vcamObj.AddComponent<CinemachineVirtualCamera>();
                virtualCamera.m_Lens.OrthographicSize = 5;
            }
        }
        
        // Virtual Camera'yı ayarla
        virtualCamera.m_Lens.OrthographicSize = 5;
        virtualCamera.m_Lens.NearClipPlane = -10;
        virtualCamera.m_Lens.FarClipPlane = 1000;
        
        // Framing Transposer ayarlarını yap
        transposer = virtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
        if (transposer == null)
        {
            // DestroyPipeline yerine alternatif yöntem
            // CinemachineComponentBase[] components = virtualCamera.GetComponentPipeline();
            // foreach (var component in components)
            // {
            //     if (component != null)
            //     {
            //         Destroy(component);
            //     }
            // }
            
            // Daha basit yöntem - varsayılan pipeline ile başlar
            transposer = virtualCamera.AddCinemachineComponent<CinemachineFramingTransposer>();
        }
        
        SetupCameraFollowing();
    }
    
    private void SetupCameraFollowing()
    {
        if (transposer == null) return;
        
        transposer.m_XDamping = xDamping;
        transposer.m_YDamping = yDamping;
        transposer.m_ScreenX = 0.5f;
        transposer.m_ScreenY = screenY;
        transposer.m_DeadZoneWidth = 0f;
        transposer.m_DeadZoneHeight = 0f;
        transposer.m_LookaheadTime = 0f;
        
        // Oyuncuyu bul ve takip et
        if (PlayerManager.instance?.player != null)
        {
            virtualCamera.Follow = PlayerManager.instance.player.transform;
            lastPlayerPos = PlayerManager.instance.player.transform.position;
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
        Debug.Log($"Scene loaded: {scene.name}");
        
        // Kamera referanslarını güncelle
        StartCoroutine(SetupCameraAfterSceneLoad());
    }
    
    private IEnumerator SetupCameraAfterSceneLoad()
    {
        // Sahnenin tam olarak yüklenmesi için bir frame bekle
        yield return null;
        
        // Ana kamerayı ve bileşenleri kontrol et
        FindOrCreateMainCamera();
        
        // Virtual Camera'yı kontrol et
        if (virtualCamera == null)
        {
            SetupVirtualCamera();
        }
        else
        {
            // Takip edilecek oyuncuyu ayarla
            if (PlayerManager.instance?.player != null)
            {
                virtualCamera.Follow = PlayerManager.instance.player.transform;
                lastPlayerPos = PlayerManager.instance.player.transform.position;
            }
        }
        
        // SceneBoundary'yi bul
        FindSceneBoundaries();
    }
    
    private void Update()
    {
        // Oyuncunun hareket yönünü takip et
        UpdatePlayerDirectionAndCamera();
    }
    
    private void LateUpdate()
    {
        // Kamera sınırlarını uygula
        if (!hasBoundaries || mainCamera == null) return;
        
        float halfHeight = mainCamera.orthographicSize;
        float halfWidth = halfHeight * mainCamera.aspect;
        
        // Kameranın mevcut pozisyonu
        Transform vcamTransform = virtualCamera.transform;
        Vector3 vcamPos = vcamTransform.position;
        
        // Sınırları kontrol et ve uygula
        float newX = Mathf.Clamp(vcamPos.x, leftBoundary + halfWidth, rightBoundary - halfWidth);
        float newY = Mathf.Clamp(vcamPos.y, bottomBoundary + halfHeight, topBoundary - halfHeight);
        
        // Eğer pozisyon değiştiyse güncelle
        if (Mathf.Abs(newX - vcamPos.x) > 0.01f || Mathf.Abs(newY - vcamPos.y) > 0.01f)
        {
            vcamTransform.position = new Vector3(newX, newY, vcamPos.z);
        }
        
        // Debug çizgileri çiz
        Debug.DrawLine(
            new Vector3(leftBoundary, vcamPos.y - 50, 0),
            new Vector3(leftBoundary, vcamPos.y + 50, 0),
            Color.red
        );
        
        Debug.DrawLine(
            new Vector3(rightBoundary, vcamPos.y - 50, 0),
            new Vector3(rightBoundary, vcamPos.y + 50, 0),
            Color.red
        );
        
        Debug.DrawLine(
            new Vector3(vcamPos.x - 50, topBoundary, 0),
            new Vector3(vcamPos.x + 50, topBoundary, 0),
            Color.red
        );
        
        Debug.DrawLine(
            new Vector3(vcamPos.x - 50, bottomBoundary, 0),
            new Vector3(vcamPos.x + 50, bottomBoundary, 0),
            Color.red
        );
    }
    
    private void UpdatePlayerDirectionAndCamera()
    {
        if (PlayerManager.instance?.player == null || transposer == null) return;
        
        Vector3 playerPos = PlayerManager.instance.player.transform.position;
        
        // İlk kez çağrıldığında
        if (lastPlayerPos == Vector3.zero)
        {
            lastPlayerPos = playerPos;
            return;
        }
        
        // Oyuncunun hareket yönünü belirle
        Vector3 moveDirection = playerPos - lastPlayerPos;
        
        // Eğer yeterince hareket varsa
        if (Mathf.Abs(moveDirection.x) > 0.01f)
        {
            // Sağa hareket
            if (moveDirection.x > 0)
            {
                targetScreenX = 0.35f; // Ekranın solunda tut
            }
            // Sola hareket
            else
            {
                targetScreenX = 0.65f; // Ekranın sağında tut
            }
        }
        
        // Ekran pozisyonunu yumuşak geçişle güncelle
        transposer.m_ScreenX = Mathf.Lerp(
            transposer.m_ScreenX,
            targetScreenX,
            Time.deltaTime * screenXTransitionSpeed
        );
        
        // Son pozisyonu kaydet
        lastPlayerPos = playerPos;
    }
    
    private void FindSceneBoundaries()
    {
        SceneBoundary boundary = FindFirstObjectByType<SceneBoundary>();
        
        if (boundary != null)
        {
            leftBoundary = boundary.leftBoundary;
            rightBoundary = boundary.rightBoundary;
            topBoundary = boundary.topBoundary;
            bottomBoundary = boundary.bottomBoundary;
            hasBoundaries = true;
            
            Debug.Log($"Camera boundaries set: L={leftBoundary}, R={rightBoundary}, T={topBoundary}, B={bottomBoundary}");
        }
        else
        {
            hasBoundaries = false;
            Debug.LogWarning("No SceneBoundary found in scene!");
        }
    }
} 