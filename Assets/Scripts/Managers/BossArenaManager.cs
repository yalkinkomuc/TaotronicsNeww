using UnityEngine;
using System.Collections;

public class BossArenaManager : MonoBehaviour
{
    public static BoxCollider2D ArenaCollider { get; private set; }
    public static BossArenaManager instance;
    
    [SerializeField] private Transform playerSpawnPoint;
    [SerializeField] private Transform bossSpawnPoint;
    [SerializeField] private GameObject bossPrefab;
    [SerializeField] private GameObject arenaPrefab; // Arena collider prefabı
    [SerializeField] private GameObject exitPortalPrefab;
    [SerializeField] private Transform exitPortalSpawnPoint;
    [SerializeField] private int mainSceneIndex = 0; // Ana sahnenin build index'i
    [SerializeField] private string bossName = "Necromancer";
    
    [Header("UI")]
    [SerializeField] private BossHealthBar bossHealthBar;
    
    private const float EXIT_PORTAL_DELAY = 1f;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        // Arena'yı spawn et
        if (arenaPrefab != null)
        {
            GameObject arena = Instantiate(arenaPrefab, Vector3.zero, Quaternion.identity);
            ArenaCollider = arena.GetComponent<BoxCollider2D>();
        }

        // Spawn noktalarını bul
        playerSpawnPoint = GameObject.Find("PlayerSpawnPoint")?.transform;
        bossSpawnPoint = GameObject.Find("BossSpawnPoint")?.transform;

        if (playerSpawnPoint != null)
        {
            // Oyuncuyu spawn et
            if (PlayerManager.instance?.player != null)
            {
                PlayerManager.instance.player.transform.position = playerSpawnPoint.position;
                PlayerManager.instance.player.gameObject.SetActive(true);
            }
        }

        // Boss'un durumunu kontrol et
        bool bossDefeated = GameProgressManager.instance != null && 
                           GameProgressManager.instance.IsBossDefeated(bossName);

        // Health bar'ı ayarla
        if (bossHealthBar != null)
        {
            if (bossDefeated)
            {
                // Boss yenilmişse health bar'ı gizle
                bossHealthBar.gameObject.SetActive(false);
            }
            else
            {
                // Boss yenilmemişse health bar'ı göster
                bossHealthBar.gameObject.SetActive(true);
            }
        }
        
        // Boss daha önce yenilmediyse spawn et
        if (bossSpawnPoint != null && bossPrefab != null)
        {
            if (!bossDefeated)
            {
                // Boss'u spawn et
                GameObject bossObj = Instantiate(bossPrefab, bossSpawnPoint.position, Quaternion.identity);
                
                // Boss'a arena collider'ını ata
                EnemyBossNecromancerBoss boss = bossObj.GetComponent<EnemyBossNecromancerBoss>();
                if (boss != null && ArenaCollider != null)
                {
                    boss.arenaCollider = ArenaCollider;
                }

                // Health bar'ı ayarla
                if (bossHealthBar != null)
                {
                    CharacterStats bossStats = bossObj.GetComponent<CharacterStats>();
                    if (bossStats != null)
                    {
                        bossHealthBar.SetupBossHealth(bossStats, bossName);
                    }
                }
            }
            else
            {
                // Boss yenilmişse direkt çıkış portalını göster
                ShowExitPortal();
            }
        }
    }

    private void SetupArena()
    {
        // Arena trigger'ını oluştur
        GameObject arenaTrigger = new GameObject("ArenaTrigger");
        
        
        // Box Collider ekle
        BoxCollider2D collider = arenaTrigger.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        
        
        // Boss'un kullanabilmesi için tag ekle
        arenaTrigger.tag = "Arena"; // Boss scriptinde bu tag'i kullanıyorsanız
    }

    private void SpawnEntities()
    {
        // ... mevcut spawn kodu ...
    }

    public void ShowExitPortal()
    {
        StartCoroutine(ShowExitPortalWithDelay());
    }

    private IEnumerator ShowExitPortalWithDelay()
    {
        yield return new WaitForSeconds(EXIT_PORTAL_DELAY);
        
        if (exitPortalPrefab != null && exitPortalSpawnPoint != null)
        {
            GameObject portal = Instantiate(exitPortalPrefab, exitPortalSpawnPoint.position, Quaternion.identity);
            
            ExitPortal exitPortal = portal.GetComponent<ExitPortal>();
            if (exitPortal == null)
            {
                exitPortal = portal.AddComponent<ExitPortal>();
            }
            exitPortal.mainSceneIndex = mainSceneIndex;
        }
    }

    public void HideBossHealthBar()
    {
        if (bossHealthBar != null)
        {
            bossHealthBar.HideBossHealthBar();
        }
    }
} 