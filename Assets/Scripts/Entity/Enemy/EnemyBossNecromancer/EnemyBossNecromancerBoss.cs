using System;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;
using System.Collections.Generic;
using System.Linq;

public class EnemyBossNecromancerBoss : Enemy
{
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
    [SerializeField] private int maxSkeletons = 4;  // Maksimum iskelet sayısı
    public float spawnRange = 15f;
    [SerializeField] private float summonCooldown = 8f;
    private float summonCooldownTimer;
    [SerializeField] private GameObject spawnEffectPrefab;


    [Header("Teleport Settings")] 
    [SerializeField] public BoxCollider2D arenaCollider; // public yaptık

    [SerializeField] private Vector2 surroundingCheckSize;
   

    private GameObject arena;

    private List<Skeleton_Enemy> summonedSkeletons = new List<Skeleton_Enemy>();

    protected override void Awake()
    {
        base.Awake();

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

        // Cooldown timerları güncelle
        if (spellCooldownTimer > 0)
        {
            spellCooldownTimer -= Time.deltaTime;
        }

        if (summonCooldownTimer > 0)
        {
            summonCooldownTimer -= Time.deltaTime;
        }

        // Ölen iskeletleri listeden temizle
        CleanupDeadSkeletons();
    }

    // Ölen iskeletleri listeden temizle
    private void CleanupDeadSkeletons()
    {
        for (int i = summonedSkeletons.Count - 1; i >= 0; i--)
        {
            if (summonedSkeletons[i] == null || !summonedSkeletons[i].gameObject.activeInHierarchy)
            {
                summonedSkeletons.RemoveAt(i);
            }
        }
    }

    // Sıkı kontroller içeren CanSummon ve CanCastSpell metodları
    public bool CanCastSpell()
    {
        return spellCooldownTimer <= 0;
    }

    public bool CanSummon()
    {
        // Debug.Log($"CanSummon: cooldown={summonCooldownTimer}, count={summonedSkeletons.Count}, max={maxSkeletons}");
        return summonCooldownTimer <= 0 && summonedSkeletons.Count < maxSkeletons;
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
                float randomXOffset = Random.Range(-2f, 2f);
                spawnPos.x += randomXOffset;
            }
            
            // Spell'i oluştur
            Instantiate(spellPrefab, spawnPos, Quaternion.identity);
            
            // Cooldown'u kesinlikle ayarla
            spellCooldownTimer = spellCooldown;
        }
    }

    // SummonSkeletons metodunda cooldown'u kesin şekilde ayarla
    public void SummonSkeletons()
    {
        // Eğer cooldown varsa veya maksimum iskelet sayısına ulaşıldıysa çık
        if (summonCooldownTimer > 0 || summonedSkeletons.Count >= maxSkeletons)
        {
            // Debug.Log($"SummonSkeletons - İptal: cooldown={summonCooldownTimer}, count={summonedSkeletons.Count}");
            return;
        }

        int skeletonsToSpawnNow = Mathf.Min(skeletonsToSpawn, maxSkeletons - summonedSkeletons.Count);
        
        if (skeletonPrefab != null && skeletonsToSpawnNow > 0)
        {
            for (int i = 0; i < skeletonsToSpawnNow; i++)
            {
                float randomX = transform.position.x + Random.Range(-spawnRange, spawnRange);
                Vector2 spawnPosition = new Vector2(randomX, transform.position.y);
                
                GameObject skeletonObj = Instantiate(skeletonPrefab, spawnPosition, Quaternion.identity);
                Skeleton_Enemy skeleton = skeletonObj.GetComponent<Skeleton_Enemy>();
                if (skeleton != null)
                {
                    skeleton.SetSummonedByNecromancer(true);
                    summonedSkeletons.Add(skeleton);
                }
            }
            
            // Cooldown'u kesinlikle ayarla ve log ekle
            summonCooldownTimer = summonCooldown;
            // Debug.Log($"SummonSkeletons - Cooldown ayarlandı: {summonCooldown} saniye");
        }
    }

    public override void Die()
    {
        foreach (var skeleton in summonedSkeletons.ToList())
        {
            if (skeleton != null && skeleton.gameObject != null)
            {
                skeleton.Die();
            }
        }
        summonedSkeletons.Clear();

        base.Die();
        stateMachine.ChangeState(deadState);
    }

    public void CreateSpawnEffect(Vector2 position)
    {
        if (spawnEffectPrefab != null)
        {
            GameObject effect = Instantiate(spawnEffectPrefab, position, Quaternion.identity);
            SkeletonSpawnEffect spawnEffect = effect.GetComponent<SkeletonSpawnEffect>();
            if (spawnEffect != null)
            {
                spawnEffect.Initialize(this, position);
            }
        }
    }

    // Tek bir iskelet spawn etme
    public void SummonSkeletonAtPosition(Vector2 position)
    {
        // Maksimum sayı kontrolü
        if (summonedSkeletons.Count >= maxSkeletons)
            return;

        if (skeletonPrefab != null)
        {
            float clampedX = Mathf.Clamp(position.x, 
                transform.position.x - spawnRange, 
                transform.position.x + spawnRange);
            Vector2 spawnPos = new Vector2(clampedX, position.y);

            GameObject skeletonObj = Instantiate(skeletonPrefab, spawnPos, Quaternion.identity);
            Skeleton_Enemy skeleton = skeletonObj.GetComponent<Skeleton_Enemy>();
            if (skeleton != null)
            {
                skeleton.SetSummonedByNecromancer(true);
                summonedSkeletons.Add(skeleton);
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
        if (distance < 6f)
        {
            FindPosition();
            return;
        }

        transform.position = newPosition;
        transform.position = new Vector3(transform.position.x,
            transform.position.y - GroundBelow().distance + (collider.size.y / 2));

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
