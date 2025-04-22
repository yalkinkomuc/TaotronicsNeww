using UnityEngine;

public class SceneBoundary : MonoBehaviour
{
    [Header("Boundary Settings")]
    public float leftBoundary;
    public float rightBoundary;
    public float topBoundary;
    public float bottomBoundary;
    
    [Header("Scene Transitions")]
    [SerializeField] private bool hasLeftExit = false;
    [SerializeField] private bool hasRightExit = false;
    [SerializeField] private bool hasTopExit = false;
    [SerializeField] private bool hasBottomExit = false;
    
    [SerializeField] private int leftSceneIndex = -1;
    [SerializeField] private int rightSceneIndex = -1;
    [SerializeField] private int topSceneIndex = -1;
    [SerializeField] private int bottomSceneIndex = -1;
    
    [Header("Transition Settings")]
    [SerializeField] private float exitTriggerThickness = 0.3f; // Daha ince trigger
    [SerializeField] private float exitHeight = 2f; // Çıkış yüksekliği (oyuncu boyutuna göre)
    
    [Header("Gizmo Settings")]
    [SerializeField] private Color boundaryColor = Color.yellow;
    [SerializeField] private Color exitColor = Color.green;
    
    private void Awake()
    {
        // Tag'i kontrol et ve düzelt
        if (gameObject.tag != "SceneBoundary")
        {
            gameObject.tag = "SceneBoundary";
            Debug.LogWarning("SceneBoundary tag automatically set for " + gameObject.name);
        }
    }
    
    private void Start()
    {
        // Çıkış collider'larını oluştur
        CreateExitColliders();
        
        // Sahne yüklendikten sonra kendini CameraManager'a bildir
        // Bir frame gecikme ile bildir (güvenli olması için)
        Invoke("NotifyCameraManager", 0.1f);
    }
    
    private void NotifyCameraManager()
    {
        if (CameraManager.instance != null)
        {
            // Yeni eklenen metod ile sınırları doğrudan bildir
            CameraManager.instance.RegisterSceneBoundary(this);
            //Debug.Log($"SceneBoundary registered with camera: L={leftBoundary}, R={rightBoundary}, T={topBoundary}, B={bottomBoundary}");
        }
        else
        {
            Debug.LogWarning("CameraManager instance not found!");
        }
    }
    
    private void CreateExitColliders()
    {
        // Sol çıkış
        if (hasLeftExit && leftSceneIndex >= 0)
        {
            // Tam sınırdan biraz dışarıda olsun
            CreateExitTrigger("LeftExit", new Vector2(leftBoundary - exitTriggerThickness/2, 0), 
                             new Vector2(exitTriggerThickness, exitHeight), leftSceneIndex);
        }
        
        // Sağ çıkış
        if (hasRightExit && rightSceneIndex >= 0)
        {
            // Tam sınırdan biraz dışarıda olsun
            CreateExitTrigger("RightExit", new Vector2(rightBoundary + exitTriggerThickness/2, 0), 
                             new Vector2(exitTriggerThickness, exitHeight), rightSceneIndex);
        }
        
        // Üst çıkış
        if (hasTopExit && topSceneIndex >= 0)
        {
            float width = Mathf.Min(4f, Mathf.Abs(rightBoundary - leftBoundary) * 0.3f); // Maksimum 4 birim veya genişliğin %30'u
            CreateExitTrigger("TopExit", new Vector2(0, topBoundary + exitTriggerThickness/2), 
                             new Vector2(width, exitTriggerThickness), topSceneIndex);
        }
        
        // Alt çıkış
        if (hasBottomExit && bottomSceneIndex >= 0)
        {
            float width = Mathf.Min(4f, Mathf.Abs(rightBoundary - leftBoundary) * 0.3f); // Maksimum 4 birim veya genişliğin %30'u
            CreateExitTrigger("BottomExit", new Vector2(0, bottomBoundary - exitTriggerThickness/2), 
                             new Vector2(width, exitTriggerThickness), bottomSceneIndex);
        }
        
        // Ayrıca sahne sınırlarını gösteren görünmez duvarlar ekleyelim
        CreateBoundaryWalls();
    }
    
    private void CreateExitTrigger(string name, Vector2 position, Vector2 size, int sceneIndex)
    {
        GameObject exitTrigger = new GameObject(name);
        exitTrigger.transform.parent = transform;
        exitTrigger.transform.position = new Vector3(position.x, position.y, 0);
        
        BoxCollider2D collider = exitTrigger.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        collider.size = size;
        
        SceneTransitionTrigger trigger = exitTrigger.AddComponent<SceneTransitionTrigger>();
        trigger.targetSceneIndex = sceneIndex;
    }
    
    private void CreateBoundaryWalls()
    {
        // Sol duvar - Eğer çıkış yoksa
        if (!hasLeftExit)
        {
            CreateBoundaryWall("LeftWall", new Vector2(leftBoundary, 0), 
                              new Vector2(0.1f, Mathf.Abs(topBoundary - bottomBoundary)));
        }
        
        // Sağ duvar - Eğer çıkış yoksa
        if (!hasRightExit)
        {
            CreateBoundaryWall("RightWall", new Vector2(rightBoundary, 0), 
                              new Vector2(0.1f, Mathf.Abs(topBoundary - bottomBoundary)));
        }
        
        // Üst duvar - Eğer çıkış yoksa
        if (!hasTopExit)
        {
            CreateBoundaryWall("TopWall", new Vector2(0, topBoundary), 
                              new Vector2(Mathf.Abs(rightBoundary - leftBoundary), 0.1f));
        }
        
        // Alt duvar - Eğer çıkış yoksa
        if (!hasBottomExit)
        {
            CreateBoundaryWall("BottomWall", new Vector2(0, bottomBoundary), 
                              new Vector2(Mathf.Abs(rightBoundary - leftBoundary), 0.1f));
        }
    }
    
    private void CreateBoundaryWall(string name, Vector2 position, Vector2 size)
    {
        GameObject wall = new GameObject(name);
        wall.transform.parent = transform;
        wall.transform.position = new Vector3(position.x, position.y, 0);
        
        BoxCollider2D collider = wall.AddComponent<BoxCollider2D>();
        collider.size = size;
    }
    
    private void OnDrawGizmos()
    {
        // Sınırları görselleştir
        Gizmos.color = new Color(0f, 1f, 0f, 0.8f); // Daha belirgin yeşil
        
        // Sol sınır
        Gizmos.DrawLine(
            new Vector3(leftBoundary, bottomBoundary - 2, 0),
            new Vector3(leftBoundary, topBoundary + 2, 0)
        );
        
        // Sağ sınır
        Gizmos.DrawLine(
            new Vector3(rightBoundary, bottomBoundary - 2, 0),
            new Vector3(rightBoundary, topBoundary + 2, 0)
        );
        
        // Üst sınır
        Gizmos.DrawLine(
            new Vector3(leftBoundary - 2, topBoundary, 0),
            new Vector3(rightBoundary + 2, topBoundary, 0)
        );
        
        // Alt sınır
        Gizmos.DrawLine(
            new Vector3(leftBoundary - 2, bottomBoundary, 0),
            new Vector3(rightBoundary + 2, bottomBoundary, 0)
        );
        
        // Köşeleri belirtmek için küçük kutular çiz
        float boxSize = 0.5f;
        Gizmos.DrawCube(new Vector3(leftBoundary, topBoundary, 0), Vector3.one * boxSize);
        Gizmos.DrawCube(new Vector3(rightBoundary, topBoundary, 0), Vector3.one * boxSize);
        Gizmos.DrawCube(new Vector3(leftBoundary, bottomBoundary, 0), Vector3.one * boxSize);
        Gizmos.DrawCube(new Vector3(rightBoundary, bottomBoundary, 0), Vector3.one * boxSize);
        
        // Bölgenin içini hafif transparan olarak doldur
        Gizmos.color = new Color(0f, 1f, 0f, 0.1f);
        Vector3 size = new Vector3(rightBoundary - leftBoundary, topBoundary - bottomBoundary, 0);
        Vector3 center = new Vector3((leftBoundary + rightBoundary) / 2, (topBoundary + bottomBoundary) / 2, 0);
        Gizmos.DrawCube(center, size);
        
        // Çıkışları çiz
        Gizmos.color = exitColor;
        
        if (hasLeftExit && leftSceneIndex >= 0)
        {
            Vector3 leftExitCenter = new Vector3(leftBoundary - exitTriggerThickness/2, 0, 0);
            Vector3 leftExitSize = new Vector3(exitTriggerThickness, exitHeight, 0);
            Gizmos.DrawWireCube(leftExitCenter, leftExitSize);
        }
        
        if (hasRightExit && rightSceneIndex >= 0)
        {
            Vector3 rightExitCenter = new Vector3(rightBoundary + exitTriggerThickness/2, 0, 0);
            Vector3 rightExitSize = new Vector3(exitTriggerThickness, exitHeight, 0);
            Gizmos.DrawWireCube(rightExitCenter, rightExitSize);
        }
        
        if (hasTopExit && topSceneIndex >= 0)
        {
            float width = Mathf.Min(4f, Mathf.Abs(rightBoundary - leftBoundary) * 0.3f);
            Vector3 topExitCenter = new Vector3(0, topBoundary + exitTriggerThickness/2, 0);
            Vector3 topExitSize = new Vector3(width, exitTriggerThickness, 0);
            Gizmos.DrawWireCube(topExitCenter, topExitSize);
        }
        
        if (hasBottomExit && bottomSceneIndex >= 0)
        {
            float width = Mathf.Min(4f, Mathf.Abs(rightBoundary - leftBoundary) * 0.3f);
            Vector3 bottomExitCenter = new Vector3(0, bottomBoundary - exitTriggerThickness/2, 0);
            Vector3 bottomExitSize = new Vector3(width, exitTriggerThickness, 0);
            Gizmos.DrawWireCube(bottomExitCenter, bottomExitSize);
        }
    }
    
} 