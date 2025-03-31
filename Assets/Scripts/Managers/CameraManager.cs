using UnityEngine;
using Cinemachine;

public class CameraManager : MonoBehaviour
{
    public static CameraManager instance { get; private set; }
    
    [Header("References")]
    private CinemachineVirtualCamera virtualCamera;
    private CinemachineFramingTransposer transposer;
    private Camera mainCamera;
    
    [Header("Boundaries")]
    private float leftBoundary;
    private float rightBoundary;
    private float topBoundary;
    private float bottomBoundary;
    private bool hasBoundaries = false;
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(transform.root.gameObject);
            
            // Referansları otomatik bul
            mainCamera = Camera.main;
            virtualCamera = GetComponentInChildren<CinemachineVirtualCamera>();
            
            if (virtualCamera == null)
            {
                Debug.LogError("Virtual Camera not found in children of CameraSystem!");
                return;
            }
            
            transposer = virtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
            SetupCamera();
        }
        else
        {
            Destroy(transform.root.gameObject);
        }
    }

    private void SetupCamera()
    {
        if (transposer != null)
        {
            transposer.m_XDamping = 1f;
            transposer.m_YDamping = 1f;
            transposer.m_ScreenX = 0.5f;
            transposer.m_ScreenY = 0.6f;
            transposer.m_DeadZoneWidth = 0;
            transposer.m_DeadZoneHeight = 0;
            transposer.m_LookaheadTime = 0;
        }
    }

    private void LateUpdate()
    {
        if (virtualCamera == null || mainCamera == null || !hasBoundaries) return;

        // Kamera boyutlarını hesapla
        float halfHeight = mainCamera.orthographicSize;
        float halfWidth = halfHeight * mainCamera.aspect;
        
        // Cinemachine'in MEVCUT pozisyonunu al
        Vector3 currentPos = transform.position;
        
        // Sınırları KESIN olarak uygula
        float newX = Mathf.Clamp(currentPos.x, leftBoundary + halfWidth, rightBoundary - halfWidth);
        float newY = Mathf.Clamp(currentPos.y, bottomBoundary + halfHeight, topBoundary - halfHeight);
        
        // Pozisyonu doğrudan güncelle
        transform.position = new Vector3(newX, newY, currentPos.z);
        
        // Virtual camera'yı da güncelle
        if (virtualCamera != null)
        {
            virtualCamera.transform.position = transform.position;
        }
        
        // Debug çizgileri
        Debug.DrawLine(
            new Vector3(leftBoundary, currentPos.y - 50, 0),
            new Vector3(leftBoundary, currentPos.y + 50, 0),
            Color.red
        );
        
        Debug.DrawLine(
            new Vector3(rightBoundary, currentPos.y - 50, 0),
            new Vector3(rightBoundary, currentPos.y + 50, 0),
            Color.red
        );
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
        // Ana kamerayı yeniden al (sahne değişince değişebilir)
        mainCamera = Camera.main;
        
        if (PlayerManager.instance?.player != null)
        {
            virtualCamera.Follow = PlayerManager.instance.player.transform;
        }
        
        FindSceneBoundaries();
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