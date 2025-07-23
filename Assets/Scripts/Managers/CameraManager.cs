using System;
using UnityEngine;
using Cinemachine;
using System.Collections;
using UnityEngine.Serialization;

// CameraManager: Scene-specific camera offset and clamp parameters can be set in the Inspector.
// Adjust 'screenY', 'screenYMin', 'screenYMax', 'screenYTransitionSpeed', and boundary values per scene as needed.
// This allows designers to tune camera feel for each level without code changes.

public class CameraManager : MonoBehaviour
{
    public static CameraManager instance { get; private set; }
    
    [Header("Camera References")]
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    private CinemachineFramingTransposer transposer;
    private Camera mainCamera;
    private CinemachineConfiner2D confiner;
    [FormerlySerializedAs("SceneBoundsCollider2D")] public PolygonCollider2D[] sceneBoundsCollider2D;
    
    [Header("Camera Settings (Per Scene)")]
    [Tooltip("Vertical offset for player tracking. 0.5 = center, 1 = top, 0 = bottom.")]
    [SerializeField] public float screenY = 0.7f;
    [Tooltip("Minimum vertical offset (player at lower area)")]
    [SerializeField] public float screenYMin = 0.5f;
    [Tooltip("Maximum vertical offset (player at upper area)")]
    [SerializeField] public float screenYMax = 0.8f;
    [Tooltip("How fast the camera transitions between offsets")]
    [SerializeField] public float screenYTransitionSpeed = 2f;
    [Tooltip("How fast the camera transitions horizontally")]
    [SerializeField] public float screenXTransitionSpeed = 3f;
    [Tooltip("Camera vertical damping (smoothness)")]
    [SerializeField] public float yDamping = 1f;
    [Tooltip("Camera horizontal damping (smoothness)")]
    [SerializeField] public float xDamping = 1f;
    
    [Header("Manual Clamp Boundaries (Optional, per scene)")]
    [Tooltip("Left camera clamp (world X)")]
    public float leftBoundary;
    [Tooltip("Right camera clamp (world X)")]
    public float rightBoundary;
    [Tooltip("Top camera clamp (world Y)")]
    public float topBoundary;
    [Tooltip("Bottom camera clamp (world Y)")]
    public float bottomBoundary;
    [Tooltip("Enable manual boundaries for this scene")]
    public bool hasBoundaries = false;
    
    [Header("Player Following")]
    private Vector3 lastPlayerPos;
    private float targetScreenX = 0.5f;
    
    private Enemy enemyScript;
    
    [Tooltip("SmoothDamp için yumuşatma süresi (daha küçük = daha hızlı)")]
    [SerializeField] public float screenYSmoothTime = 0.25f;
    private float screenYVelocity = 0f;
    
    [Tooltip("Orta bölgede oyuncunun ekranın neresinde olacağı (0=alt, 1=üst)")]
    [SerializeField] public float screenYCenter = 0.65f;
    
    
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
            
           // Debug.Log($"Camera boundaries registered directly: L={leftBoundary}, R={rightBoundary}, T={topBoundary}, B={bottomBoundary}");
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
            
           // Debug.Log("CameraManager initialized!");
        }
        else
        {
            Destroy(transform.root.gameObject);
        }

        confiner = GetComponentInChildren<CinemachineConfiner2D>();
        
        GameObject enemy = GameObject.FindWithTag("Enemy");

        if (enemy == null)
        {
            return;
        }
        else
        {
            enemyScript = enemy.GetComponent<Enemy>();
        }

         
    }

    private void Start()
    {
        
        
    }

    private void UpdateBoundariesOnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        int sceneIndex = scene.buildIndex;
        if (sceneIndex < sceneBoundsCollider2D.Length && sceneBoundsCollider2D[sceneIndex] != null)
        {
            confiner.m_BoundingShape2D = sceneBoundsCollider2D[sceneIndex];
            confiner.InvalidateCache(); // Yeni collider'ı kullanması için cache'i temizle
        }
    }


    private void FindOrCreateMainCamera()
    {
        mainCamera = Camera.main;
        
        if (mainCamera == null)
        {
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
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += UpdateBoundariesOnSceneLoaded;


    }
    
    private void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= UpdateBoundariesOnSceneLoaded;
    }
    
    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        //Debug.Log($"Scene loaded: {scene.name}");
        
        // Kamera referanslarını güncelle
        StartCoroutine(SetupCameraAfterSceneLoad());
    }
    
    private IEnumerator SetupCameraAfterSceneLoad()
    {
        // Sahnenin tam olarak yüklenmesi için bir süre bekle
        yield return new WaitForSeconds(0.1f);
        
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
        
        // Oyuncunun dikey pozisyonuna göre kamerayı ayarla
        AdjustCameraVerticalPosition();
    }
    
    private void LateUpdate()
    {
        ManuallyConstrainCamera();
    }
    
    // Kamerayı manuel olarak sınırlandır
    private void ManuallyConstrainCamera()
    {
        if (!hasBoundaries || mainCamera == null || virtualCamera == null) return;
        
        float halfHeight = mainCamera.orthographicSize;
        float halfWidth = halfHeight * mainCamera.aspect;
        
        // Kameranın mevcut pozisyonu
        Vector3 vcamPos = virtualCamera.transform.position;
        
        // Sınırları kontrol et ve uygula
        float newX = Mathf.Clamp(vcamPos.x, leftBoundary + halfWidth, rightBoundary - halfWidth);
        float newY = Mathf.Clamp(vcamPos.y, bottomBoundary + halfHeight, topBoundary - halfHeight);
        
        // Eğer pozisyon değiştiyse güncelle
        if (Mathf.Abs(newX - vcamPos.x) > 0.01f || Mathf.Abs(newY - vcamPos.y) > 0.01f)
        {
            virtualCamera.transform.position = new Vector3(newX, newY, vcamPos.z);
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
        
        // Enemy script null kontrolü
        if (enemyScript != null && enemyScript.fightBegun)
        {
            return;
        }
        
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
        else
        {
            return;
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
            
            //Debug.Log($"Camera boundaries set: L={leftBoundary}, R={rightBoundary}, T={topBoundary}, B={bottomBoundary}");
        }
        else
        {
            hasBoundaries = false;
            Debug.LogWarning("No SceneBoundary found in scene!");
        }
    }
    
    private void AdjustCameraVerticalPosition()
    {
        if (!hasBoundaries || PlayerManager.instance?.player == null || transposer == null) return;
        
        // Enemy savaşı kontrol et
        if (enemyScript != null && enemyScript.fightBegun)
        {
            return;
        }
        
        // Sahne yüksekliği
        float sceneHeight = topBoundary - bottomBoundary;
        float playerY = PlayerManager.instance.player.transform.position.y;

        // Alt, orta ve üst bölgeleri tanımla
        float lowerMid = bottomBoundary + sceneHeight * 0.3f;
        float upperMid = bottomBoundary + sceneHeight * 0.7f;

        float targetScreenY = GetTargetScreenY(playerY, lowerMid, upperMid);

        // Kamera konumunu SmoothDamp ile yumuşak geçişle ayarla
        transposer.m_ScreenY = Mathf.SmoothDamp(transposer.m_ScreenY, targetScreenY, ref screenYVelocity, screenYSmoothTime);
    }

    private float GetTargetScreenY(float playerY, float lowerMid, float upperMid)
    {
        if (playerY < lowerMid)
            return screenYMin; // Alt bölge
        else if (playerY > upperMid)
            return screenYMax; // Üst bölge
        else
            return screenYCenter; // Orta bölge (biraz yukarıda)
    }
} 