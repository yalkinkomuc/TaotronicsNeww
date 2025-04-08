using System;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;
using System.Collections.Generic;
using System.Linq;

public class EnemyBossNecromancerBoss : Enemy
{
    public static bool isBossFightOver { get; private set; } = false;

    #region States
    public NecromancerIdleState idleState { get; private set; }
    public NecromancerBattleState battleState { get; private set; }
    public NecromancerSpellCastState spellCastState { get; private set; } // Büyü için
    public NecromancerSummonState summonState { get; private set; }        // İskelet çağırma için
    public NecromancerTeleportState teleportState { get; private set; }
    public NecromancerDeadState deadState { get; private set; }
   
    #endregion

    [Header("Spell Settings")]
    [SerializeField] private GameObject spellPrefab;
    [SerializeField] private Transform spellSpawnPoint;
    [SerializeField] private float spellCooldown = 2f;
    private float spellCooldownTimer;

    [Header("Summon Settings")]
    [SerializeField] private GameObject skeletonPrefab;
    [SerializeField] public int skeletonsToSpawn = 2;
    [SerializeField] public int maxSkeletons = 4;  // Public olarak değiştirdik
    public float spawnRange = 15f;
    [SerializeField] public float summonCooldown = 5f; // 8f'den 5f'e düşürüldü
    public float summonCooldownTimer; // Public olarak değiştirdik
    [SerializeField] private GameObject spawnEffectPrefab;
    [SerializeField] private float spawnEffectCooldown = 3f;
    private float spawnEffectCooldownTimer = 0f;

    public List<Skeleton_Enemy> summonedSkeletons = new List<Skeleton_Enemy>(); // Public olarak değiştirdik

    [Header("Teleport Settings")] 
    [SerializeField] public BoxCollider2D arenaCollider; // public yaptık

    [SerializeField] private Vector2 surroundingCheckSize;
   

    private GameObject arena;

    [Header("Debug")]
    [SerializeField] private bool debugMode = false;  // Hata ayıklama için eklendi

    //private bool isSpawningSkeletons = false; // Spawn işlemi kontrolü için

    protected override void Awake()
    {
        base.Awake();
        isBossFightOver = false; // Boss savaşı başladığında false'a çek

        idleState = new NecromancerIdleState(this, stateMachine, "Idle", this);
        battleState = new NecromancerBattleState(this, stateMachine, "Move", this);
        spellCastState = new NecromancerSpellCastState(this, stateMachine, "Cast", this);
        summonState = new NecromancerSummonState(this, stateMachine, "CastSpawn", this);
        teleportState = new NecromancerTeleportState(this, stateMachine, "Teleport", this);
        deadState = new NecromancerDeadState(this, stateMachine, "Death", this);
        
    }

    protected override void Start()
    {
        base.Start();
        stateMachine.Initialize(idleState);
    }

    protected override void Update()
    {
        base.Update();

        if (spellCooldownTimer > 0)
            spellCooldownTimer -= Time.deltaTime;

        if (summonCooldownTimer > 0)
            summonCooldownTimer -= Time.deltaTime;
        
        if (spawnEffectCooldownTimer > 0)
            spawnEffectCooldownTimer -= Time.deltaTime;

        // Her frame'de değil, belirli aralıklarla kontrol et
        if (Time.frameCount % 30 == 0) // Her 30 frame'de bir
        {
            CleanupDeadSkeletons();
        }
    }

    private void CleanupDeadSkeletons()
    {
        // Listeden silinecek iskeletleri geçici bir listede topla
        List<Skeleton_Enemy> skeletonsToRemove = new List<Skeleton_Enemy>();
        
        foreach (var skeleton in summonedSkeletons)
        {
            if (skeleton == null || !skeleton.gameObject.activeInHierarchy)
            {
                skeletonsToRemove.Add(skeleton);
            }
        }
        
        // Silinecek iskeletleri ana listeden kaldır
        foreach (var skeleton in skeletonsToRemove)
        {
            summonedSkeletons.Remove(skeleton);
            if (debugMode)
                Debug.Log("İskelet listeden kaldırıldı");
        }
    }

    // Sıkı kontroller içeren CanSummon ve CanCastSpell metodları
    public bool CanCastSpell()
    {
        bool canCast = spellCooldownTimer <= 0;
        if (debugMode)
            Debug.Log($"CanCastSpell: {canCast} (cooldown={spellCooldownTimer.ToString("F1")})");
        return canCast;
    }

    public bool CanSummon()
    {
        bool canSpawn = summonCooldownTimer <= 0 && summonedSkeletons.Count < maxSkeletons;
        if (debugMode)
            Debug.Log($"CanSummon: {canSpawn} (cooldown={summonCooldownTimer.ToString("F1")}, count={summonedSkeletons.Count}, max={maxSkeletons})");
        return canSpawn;
    }

    private RaycastHit2D GroundBelow() => Physics2D.Raycast(transform.position,Vector2.down,100,whatIsGround);
    private bool SomethingIsAround() => Physics2D.BoxCast(transform.position,surroundingCheckSize,0,Vector2.zero,0,whatIsGround);

    // CastSpell metodunda cooldown'u kesin şekilde ayarla
    public void CastSpell()
    {
        if (spellPrefab != null && player != null)
        {
            Vector3 spawnPos = player.transform.position;
            spawnPos.y += .55f;

            // Oyuncunun Rigidbody2D'sini al ve hızını kontrol et
            Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
            if (playerRb != null && Mathf.Abs(playerRb.linearVelocity.x) > 0.1f)
            {
                float randomXOffset = Random.Range(-1f, 1f);
                spawnPos.x += randomXOffset;
            }
            
            // Spell'i oluştur
            Instantiate(spellPrefab, spawnPos, Quaternion.identity);
            
            // Cooldown'u kesinlikle ayarla
            spellCooldownTimer = spellCooldown;
        }
    }

    // SummonSkeletons metodunu düzelt - yerin altında değil üstünde spawn olsun
    // public void SummonSkeletons()
    // {
    //     int skeletonsToSpawnNow = Mathf.Min(skeletonsToSpawn, maxSkeletons - summonedSkeletons.Count);
    //     
    //     if (skeletonPrefab != null && arenaCollider != null)
    //     {
    //         // Sabit X aralığı (Arena görüntüsüne göre)
    //         float minX = -10f; // Sol sınır
    //         float maxX = 9f;   // Sağ sınır
    //         
    //         // ARENA ZEMİNİ - arenaCollider'ın alt kısmı (min.y)
    //         float groundY = arenaCollider.bounds.min.y;
    //         
    //         for (int i = 0; i < skeletonsToSpawnNow; i++)
    //         {
    //             float randomX = Random.Range(minX, maxX);
    //             
    //             // ZEMİNİN ÜSTÜNDE spawn et, küçük bir offset ekle
    //             Vector2 spawnPosition = new Vector2(randomX, groundY + 0.5f);
    //             
    //             GameObject skeletonObj = Instantiate(skeletonPrefab, spawnPosition, Quaternion.identity);
    //             Skeleton_Enemy skeleton = skeletonObj.GetComponent<Skeleton_Enemy>();
    //             if (skeleton != null)
    //             {
    //                 skeleton.SetSummonedByNecromancer(true);
    //                 summonedSkeletons.Add(skeleton);
    //             }
    //         }
    //         
    //         summonCooldownTimer = summonCooldown;
    //     }
    // }

    // SummonSkeletonAtPosition metodunu da güncelle
    public void SummonSkeletonAtPosition(Vector2 position)
    {
        if (summonedSkeletons.Count >= maxSkeletons || skeletonPrefab == null || arenaCollider == null)
            return;

        float minX = arenaCollider.bounds.min.x + 2f;
        float maxX = arenaCollider.bounds.max.x - 2f;
        float groundY = arenaCollider.bounds.min.y;
        
        float clampedX = Mathf.Clamp(position.x, minX, maxX);
        Vector2 spawnPos = new Vector2(clampedX, groundY + 0.5f);
        
        GameObject skeletonObj = Instantiate(skeletonPrefab, spawnPos, Quaternion.identity);
        Skeleton_Enemy skeleton = skeletonObj.GetComponent<Skeleton_Enemy>();
        if (skeleton != null)
        {
            skeleton.SetSummonedByNecromancer(true);
            summonedSkeletons.Add(skeleton);
            // Ana spawn cooldown'u burada başlat
            summonCooldownTimer = summonCooldown;
        }
    }

    public override void Die()
    {
        isBossFightOver = true;

        // Tüm iskeletleri dead state'e geçir
        foreach (var skeleton in summonedSkeletons.ToList())
        {
            if (skeleton != null && skeleton.gameObject != null)
            {
                skeleton.Die(); // Bu şekilde iskeletler de kendi ölüm animasyonlarını oynatacak
            }
        }
        summonedSkeletons.Clear();

        base.Die();
        stateMachine.ChangeState(deadState);
    }

    public void CreateSpawnEffect(Vector2 position)
    {
        // Sadece spawn effect cooldown kontrolü
        if (spawnEffectCooldownTimer > 0)
        {
            if (debugMode)
                Debug.Log($"Spawn efekti cooldown: {spawnEffectCooldownTimer}");
            return;
        }

        // Maksimum iskelet kontrolü
        if (summonedSkeletons.Count >= maxSkeletons)
        {
            if (debugMode)
                Debug.Log($"Maksimum iskelet sayısına ulaşıldı: {summonedSkeletons.Count}/{maxSkeletons}");
            return;
        }

        if (spawnEffectPrefab != null && arenaCollider != null)
        {
            float minX = arenaCollider.bounds.min.x + 2f;
            float maxX = arenaCollider.bounds.max.x - 2f;
            float groundY = arenaCollider.bounds.min.y;
            
            float clampedX = Mathf.Clamp(position.x, minX, maxX);
            Vector2 effectPosition = new Vector2(clampedX, groundY + 0.5f);
            
            GameObject effect = Instantiate(spawnEffectPrefab, effectPosition, Quaternion.identity);
            SkeletonSpawnEffect spawnEffect = effect.GetComponent<SkeletonSpawnEffect>();
            
            if (spawnEffect != null)
            {
                spawnEffect.Initialize(this, effectPosition);
                // Sadece efekt cooldown'unu başlat
                spawnEffectCooldownTimer = spawnEffectCooldown;
            }
            else
            {
                if (debugMode)
                    Debug.LogError("SkeletonSpawnEffect component bulunamadı!");
                Destroy(effect);
            }
        }
    }

    public void FindPosition()
    {
        Vector3 originalPosition = transform.position;
        float x = Random.Range(arenaCollider.bounds.min.x + 3, arenaCollider.bounds.max.x - 3);
        float y = Random.Range(arenaCollider.bounds.min.y + 3, arenaCollider.bounds.max.y - 3);

        Vector3 newPosition = new Vector3(x, y);
        float distance = Vector2.Distance(originalPosition, newPosition);

        // Eğer yeni konum mevcut konuma çok yakınsa yeni konum bul
        if (distance < 15f)
        {
            FindPosition();
            return;
        }

        transform.position = newPosition;
        transform.position = new Vector3(transform.position.x,
            transform.position.y - GroundBelow().distance + (capsuleCollider.size.y / 2));

        if (!GroundBelow() || SomethingIsAround())
        {
            FindPosition();
        }
    }

    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
        Gizmos.DrawLine(transform.position,new Vector3(transform.position.x,transform.position.y-GroundBelow().distance));
        Gizmos.DrawWireCube(transform.position,surroundingCheckSize);
    }

   
}
