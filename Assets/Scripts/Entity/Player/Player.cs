using System;
using UnityEngine;
using UnityEngine.Serialization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public class Player : Entity
{
   
    
    
    
    [HideInInspector]
    public HealthBar healthBar;

    public Transform weaponHolderTranform;


    [Header("AttackDetails")] 
    //public Vector2[] attackMovement;

    public Transform attackCheck;
    //public float attackCheckRadius;
    public Vector2 attackSize;
    
    [SerializeField] private GameObject boomerangPrefab;
    [SerializeField] private Transform boomerangLaunchPoint;
    
    [FormerlySerializedAs("boxCollider")] [Header("Collider")]
    [HideInInspector]
    public CapsuleCollider2D capsuleCollider;
    
    
    [Header("Movement")]
    public float moveSpeed;
    public float jumpForce;
    
    

    [Header("Dash Info")] 
    [SerializeField] private float dashCooldown;
    private float dashTimer;
    public float dashDuration;
    public float dashSpeed;
    public float groundDashSpeed;
    
    [Header("Electric Dash Info")]
    [SerializeField] public float electricDashSpeed = 15f;
    [SerializeField] public float electricDashDuration = 0.3f;
    [SerializeField] public float electricDashDistance = 6f;
    [SerializeField] public GameObject electricDashPrefab;
    [SerializeField] public Transform electricDashSpawnPoint;
    [SerializeField] private float electricDashManaCost = 30f;
    [SerializeField] public GameObject shockwavePrefab; // Shockwave efekti için referans
    
    [Header("DashVFX")]
    public GameObject dashEffectPrefab;
    public GameObject groundDashEffectPrefab;
    public int effectCount = 3;
    public float spawnInterval = 0.05f;
    
    
    public float dashDirection {get;private set;}
    
    


    [Header("Ghost Mode Info")] 
    public int playerLayer = 7;

    [Header("CrouchMode")] 
    private Vector2 normalOffset;
    private Vector2 crouchOffset;
    private float normalHeight = 0.9f;  // Normal duruş yüksekliği
    private float crouchHeight = 0.70f;
        

    [FormerlySerializedAs("passableEnemiesLayer")] [FormerlySerializedAs("passableEnemies")] public int passableEnemiesLayerIndex = 8;
    
    public LayerMask passableEnemiesLayerMask;
    public int arrowLayerMaskIndex;

    [Header("Damage Control")]
    [SerializeField] private float invulnerabilityDuration = 1f; // Hasar alamama süresi
    private bool isInvulnerable = false;

    #region States
    
    public PlayerStateMachine stateMachine;
    public PlayerIdleState idleState { get; private set; }
    public PlayerMoveState moveState { get; private set; }
    
    
    public PlayerDashState dashState { get; private set; }
    public PlayerAirState airState { get; private set; }
    public PlayerCrouchState crouchState { get; private set; }
    public PlayerGroundDashState groundDashState { get; private set; }
    public PlayerAttackState attackState {get;private set;}
    public PlayerHammerAttackState hammerAttackState {get;private set;}
    public PlayerCrouchAttackState crouchAttackState {get;private set;}
    public PlayerDeadState deadState {get;private set;}
    public PlayerStunnedState stunnedState {get;private set;}
    
    public PlayerParryState parryState {get;private set;}
    
    public PlayerSpell1State spell1State {get;private set;}
    public PlayerSpell2State spell2State {get;private set;}
    public PlayerEarthPushSpellState earthPushState {get;private set;}
    public PlayerAirPushState airPushState {get;private set;}
    
    public PlayerThrowBoomerangState throwBoomerangState {get;private set;}
    public PlayerCatchBoomerangState catchBoomerangState {get;private set;}
    public PlayerVoidState voidState {get;private set;}
    public PlayerSuccesfulParryState succesfulParryState {get;private set;}
    public PlayerElectricDashState electricDashState {get;private set;}
    public PlayerFireballSpellState fireballSpellState {get;private set;}
    
    #endregion
    
    private IInteractable currentInteractable;

    [Header("Crouch Attack")]
    public Vector2 crouchAttackOffset; // Örnek: (0, 0.2f) 

    [Header("Weapon References")]
    public BoomerangWeaponStateMachine boomerangWeapon;
    public SpellbookWeaponStateMachine spellbookWeapon;
    public SwordWeaponStateMachine swordWeapon;
    public BurningSwordStateMachine burningSword;
    public HammerSwordStateMachine hammer;
    public IceHammerStateMachine iceHammer;
    public ShieldStateMachine shield;
    
    [Header("Armor References")]
    public ArmorStateMachine basicArmor;

    [Header("Boomerang Settings")]
    [SerializeField] private float boomerangCooldown = 2f;
    private float boomerangCooldownTimer;
    
    [HideInInspector] public bool isBoomerangInAir = false; // Boomerang havada mı kontrol etmek için

    #region Spell Section

    [Header("Mana Costs")]
    [SerializeField] public float iceShardManaCost = 20f;
    [SerializeField] public float fireSpellManaDrainPerSecond = 5f; // Saniyede tüketilecek mana
    [SerializeField] public float earthPushManaCost = 25f;
    [SerializeField] public float voidSkillManaCost = 40f;
    [SerializeField] public float airPushManaCost = 15f;

    [Header("Ice Spell Settings")]
    [SerializeField] private GameObject iceShardPrefab;
    [SerializeField] public float spellSpacing = 1f;
    [SerializeField] private float delayBetweenShards = 0.1f;
    private float iceShardCooldownTimer;

   
    
    [Header("Void Skill Settings")]
    [SerializeField] public GameObject voidSlashPrefab;

    [Header("Fire Spell Settings")]
    public GameObject fireSpellPrefab;
    public Transform fireSpellPoint;
    private bool isChargingFire = false;
    
    [Header("Fireball Spell Settings")]
    public GameObject fireballPrefab;
    public Transform fireballSpawnPoint;

    [Header("Earth Spell Settings")]
    public GameObject earthPushPrefab;
    public Transform earthPushSpawnPoint;
    private float earthPushCooldown = 3f; // 3 saniye cooldown
    private float earthPushCooldownTimer;
    
    [Header("Air Spell Settings")]
    public GameObject airPushPrefab;
    public Transform airPushSpawnPoint;
    private float airPushCooldown = 2f; // 2 saniye cooldown
    private float airPushCooldownTimer;

    #endregion
    
   [Header(("CameraSettings"))]
   protected CameraFollowObject cameraFollowObject;
   [SerializeField] protected GameObject cameraFollowGO;
    

   

    [Header("Checkpoint")]
    private Vector2 lastCheckpointPosition;

    // Weapon visibility control
    private bool weaponsHidden = false;
    private WeaponState lastActiveWeaponState = WeaponState.Idle;

    [Header("Ground Check")]
    [SerializeField] private float groundCheckWidth = 0.4f;
    [SerializeField] private float groundCheckHeight = 0.1f;
    [SerializeField] private float groundCheckExtraHeight = 0.05f;
    [SerializeField] private float sideRaySpacing = 0.1f;

    [Header("Currency and Experience")]
    [SerializeField] private int gold = 0;
    [SerializeField] private int experience = 0;
    [SerializeField] private int level = 1;
    [SerializeField] private int experienceToNextLevel = 100;

    // Dummy nesneleri için ayrı bir liste (Entity'den türemedikleri için)
    [HideInInspector] public HashSet<int> hitDummyIDs = new HashSet<int>();

    [Header("Parry Settings")]
    [SerializeField] public float parryRadius = 2f; // Parry etki yarıçapı (public yaptık)
    [SerializeField] private float parryCooldown = 1f; // Parry cooldown süresi
    private float parryTimer = 0f; // Parry cooldown sayacı
    [SerializeField] public float parryWindowTime;
    [SerializeField] public GameObject parryEffectPrefab; // Başarılı parry efekti prefabı

    private float fallSpeedYDampingChangeThreshold;

    
    
    
    protected override void Awake()
    {
        base.Awake();
        
        // Check if there's already a player in the scene (duplicate prevention)
#pragma warning disable CS0618 // Type or member is obsolete
        Player[] existingPlayers = FindObjectsOfType<Player>();
#pragma warning restore CS0618 // Type or member is obsolete
        if (existingPlayers.Length > 1)
        {
            // If this is not the first player, destroy this instance
            if (existingPlayers[0] != this)
            {
                Debug.LogWarning($"Player duplicate detected, destroying: {gameObject.name}");
                Destroy(gameObject);
                return;
            }
        }
        
        stateMachine = new PlayerStateMachine();
        
        
        SetupStates();
        
        
    }
    
    protected override void Start()
    {
        base.Start();
        
        stateMachine.Initialize(idleState);
        
        SetupRigidbody();
        SetupComponents();
        SetupCrouchCollider();
        
        ResetPlayerFacing();
        LoadCheckpoint();

        AssignEquipments();
    }

    

    protected override void Update()
    {
        base.Update();
        
       
        
        // Yetenek girişleri
        CheckForDashInput();
        CheckForGroundDashInput();
        CheckForElectricDashInput();



        if (rb.linearVelocityY < fallSpeedYDampingChangeThreshold && !CameraManager.instance.isLerpingYDamping &&
            !CameraManager.instance.lerpedFromPlayerFalling)
        {
            CameraManager.instance.LerpYDamping(true);
        }
        
        if (rb.linearVelocityY >= 0f && !CameraManager.instance.isLerpingYDamping && CameraManager.instance.lerpedFromPlayerFalling)
        {
            CameraManager.instance.lerpedFromPlayerFalling = false;
            CameraManager.instance.LerpYDamping(false);
        }
        
        
        // Stun durumunu kontrol et
        bool isStunned = stateMachine.currentState == stunnedState;
        
        if (!isStunned)
        {
            // Parry başarısını kontrol et
            CheckForSuccessfulParry();
            
            // Spelleri kontrol et
            CheckForSpellInput();
            
            // Void becerisi kontrolü
            CheckForVoidSkillInput();
            
            // Air Push becerisi kontrolü
            CheckForAirPushInput();
            
            // Etkileşim kontrolü
            if (UserInput.WasInteractPressed && currentInteractable != null)
            {
                // DialogueManager kontrolü
                if (currentInteractable is DialogueNPC && DialogueManager.Instance == null)
                {
                    Debug.LogWarning("DialogueManager bulunamadı!");
                    return;
                }
                
                currentInteractable.Interact();
            }
        }
        
        // UI kontrolü (stunned durumunda bile çalışmalı)
        CheckForUIInput();
        
        // State machine güncellemesi
        stateMachine.currentState.Update();
        
        // Boomerang havadayken secondary silahları gizle
        ManageWeaponVisibilityBasedOnBoomerang();
        
        // Silah referanslarını güncelle
        if (weaponsHidden == false)
        {
            UpdateWeaponReferences();
        }
        
        // Bumerang cooldown'unu güncelle
        if (boomerangCooldownTimer > 0)
        {
            boomerangCooldownTimer -= Time.deltaTime;
        }
        
        // Earth Push cooldown'unu güncelle
        if (earthPushCooldownTimer > 0)
        {
            earthPushCooldownTimer -= Time.deltaTime;
        }
        
        // Ice Shard cooldown'unu güncelle
        if (iceShardCooldownTimer > 0)
        {
            iceShardCooldownTimer -= Time.deltaTime;
        }
        
        // Parry cooldown'unu güncelle
        if (parryTimer > 0)
        {
            parryTimer -= Time.deltaTime;
        }
        
        // Düşük can ve mana durumlarını kontrol et
        if (stats.currentHealth <= 0)
        {
            Die();
        }
        
        // Otomatik mana yenileme
        if (stats.currentMana < stats.maxMana.GetValue())
        {
            RefillMana();
        }
    }

    #region SetupSection

    private void SetupCrouchCollider()
    {
        normalOffset = capsuleCollider.offset;
        crouchOffset = new Vector2(normalOffset.x, normalOffset.y-0.1f);
    }

    private void SetupComponents()
    {
        healthBar = GetComponent<HealthBar>();
        capsuleCollider = GetComponent<CapsuleCollider2D>();

        cameraFollowObject = cameraFollowGO.GetComponent<CameraFollowObject>();
    }
    
    private void SetupRigidbody()
    {
        // Rigidbody2D ayarlarını optimize et
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.sleepMode = RigidbodySleepMode2D.NeverSleep;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }
    
    private void SetupStates()
    {
        idleState = new PlayerIdleState(this,stateMachine,"Idle");
        moveState = new PlayerMoveState(this,stateMachine,"Move");
        dashState = new PlayerDashState(this,stateMachine,"Dash");
        airState = new PlayerAirState(this,stateMachine,"Jump");
        crouchState = new PlayerCrouchState(this,stateMachine,"Crouch");
        groundDashState = new PlayerGroundDashState(this,stateMachine,"GroundDash");
        attackState = new PlayerAttackState(this, stateMachine, "Attack");
        hammerAttackState = new PlayerHammerAttackState(this, stateMachine, "HammerAttack");
        crouchAttackState = new PlayerCrouchAttackState(this, stateMachine, "GroundAttack");
        deadState = new PlayerDeadState(this,stateMachine,"Death");
        stunnedState = new PlayerStunnedState(this,stateMachine,"Stunned");
        parryState = new PlayerParryState(this,stateMachine,"Parry");
        throwBoomerangState = new PlayerThrowBoomerangState(this, stateMachine, "ThrowBoomerang");
        catchBoomerangState = new PlayerCatchBoomerangState(this, stateMachine, "CatchBoomerang");
        spell1State = new PlayerSpell1State(this, stateMachine, "Spell1");
        spell2State = new PlayerSpell2State(this, stateMachine, "Spell2");
        voidState = new PlayerVoidState(this,stateMachine,"VoidDisappear");
        succesfulParryState = new PlayerSuccesfulParryState(this, stateMachine, "SuccesfulParry");
        earthPushState = new PlayerEarthPushSpellState(this, stateMachine, "EarthPush");
        electricDashState = new PlayerElectricDashState(this, stateMachine, "Dash");
        airPushState = new PlayerAirPushState(this, stateMachine, "AirPush");
        fireballSpellState = new PlayerFireballSpellState(this, stateMachine, "Fireball");
    }
    
    private void AssignEquipments()
    {
        if (boomerangWeapon == null)
        {
            boomerangWeapon = GetComponentInChildren<BoomerangWeaponStateMachine>();
        }
        
        if (spellbookWeapon == null)
        {
            spellbookWeapon = GetComponentInChildren<SpellbookWeaponStateMachine>();
        }

        if (swordWeapon == null)
        {
            swordWeapon = GetComponentInChildren<SwordWeaponStateMachine>();
        }

        if (hammer == null)
        {
            hammer = GetComponentInChildren<HammerSwordStateMachine>();
        }

        if (iceHammer == null)
        {
            iceHammer = GetComponentInChildren<IceHammerStateMachine>();
        }

        if (burningSword == null)
        {
            burningSword = GetComponentInChildren<BurningSwordStateMachine>();
        }

        if (shield == null)
        {
            shield = GetComponentInChildren<ShieldStateMachine>();
        }

        if (basicArmor == null)
        {
            basicArmor = GetComponentInChildren<BasicArmorStateMachine>();
        }

        
        
    }

    #endregion
    
    

    #region DrawGizmos

#if UNITY_EDITOR
    
    
    protected override void OnDrawGizmos()
    {
        if (!Application.isPlaying || capsuleCollider == null) return;

        // Ground Check Gizmos
        Vector2 boxCenter = (Vector2)transform.position + 
                          new Vector2(0, -capsuleCollider.size.y / 2);
        
        // Ana box gizmosu
        bool isGrounded = Application.isPlaying ? IsGroundDetected() : false;
        Gizmos.color = isGrounded ? new Color(0, 1f, 0, 0.8f) : new Color(1f, 0, 0, 0.8f);
        Gizmos.DrawCube(boxCenter, new Vector3(groundCheckWidth, groundCheckHeight, 0.05f));
        
        // BoxCast için outline
        Gizmos.color = isGrounded ? new Color(0, 1f, 0, 1f) : new Color(1f, 0, 0, 1f);
        Gizmos.DrawWireCube(boxCenter, new Vector3(groundCheckWidth, groundCheckHeight, 0));
        
        // Çoklu ray check noktaları
        Vector2 leftStart = boxCenter + new Vector2(-groundCheckWidth/2, 0);
        Vector2 rightStart = boxCenter + new Vector2(groundCheckWidth/2, 0);
        
        // Tüm raycast noktaları
        Vector2[] rayPoints = new Vector2[]
        {
            leftStart, // Sol dış
            leftStart + Vector2.right * sideRaySpacing, // Sol iç
            boxCenter, // Merkez
            rightStart + Vector2.left * sideRaySpacing, // Sağ iç
            rightStart // Sağ dış
        };
        
        // Her ray için
        foreach (Vector2 point in rayPoints)
        {
            // Ray başlangıç noktası
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(point, 0.03f);
            
            // Ray çizgisi
            Gizmos.color = Color.cyan;
            Vector2 rayEnd = point + Vector2.down * (groundCheckExtraHeight + groundCheckHeight);
            Gizmos.DrawLine(point, rayEnd);
            
            // Ray bitiş noktası
            Gizmos.color = new Color(1f, 0.5f, 0, 1f); // Turuncu
            Gizmos.DrawSphere(rayEnd, 0.02f);
        }
        
        // Extra BoxCast gösterimi (merkez box)
        Gizmos.color = new Color(0, 0.8f, 0.8f, 0.5f);
        Vector2 boxCastEnd = boxCenter + Vector2.down * groundCheckExtraHeight;
        Gizmos.DrawCube(boxCastEnd, new Vector3(groundCheckWidth * 0.8f, groundCheckHeight, 0.05f));
        
        // Saldırı kutusu
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(attackCheck.position, attackSize);

        // Parry yarıçapını göster
        Gizmos.color = new Color(0, 0.5f, 1f, 0.3f); // Daha rahat görünür mavi renk
        Gizmos.DrawWireSphere(transform.position, parryRadius);
        
        if (stateMachine != null && stateMachine.currentState == parryState)
        {
            // Parry durumundayken daha belirgin göster
            Gizmos.color = new Color(0, 0.5f, 1f, 0.8f);
            Gizmos.DrawWireSphere(transform.position, parryRadius);
        }
    }
#endif
    
    private void OnDrawGizmosSelected() 
    {
        // Eğer Application.isPlaying ise bu metod zaten OnDrawGizmos'dan çağrılmış olacak
        if (Application.isPlaying) return;
        
        // Oyun çalışmıyorken de parry yarıçapını göster
        Gizmos.color = new Color(0, 0.5f, 1f, 0.5f);
        Gizmos.DrawWireSphere(transform.position, parryRadius);
        
        // Saldırı kutusu
        if (attackCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(attackCheck.position, attackSize);
        }
    }
    



    #endregion
    
    #region ManaManagement

    public bool HasEnoughMana(float manaCost)
    {
        // Mana negatif veya sıfır olamaz
        if (manaCost <= 0) return true;
        
        // Yeterli mana var mı?
        return stats.currentMana >= manaCost;
    }

    public bool UseMana(float manaCost)
    {
        // Negatif veya sıfır mana harcayamaz
        if (manaCost <= 0) return true;
        
        // Önce yeterli mana var mı kontrol et
        if (stats.currentMana < manaCost) {
            Debug.LogWarning($"Tried to use mana but not enough! Required: {manaCost}, Current: {stats.currentMana}");
            return false;
        }
        
        // Manayı doğrudan ayarla (stats.UseMana yerine)
        stats.currentMana -= manaCost;
       
        
        // UI güncellemesi yapıyoruz
        ManaBar manaBar = GetComponent<ManaBar>();
        if (manaBar != null) {
            manaBar.UpdateManaBar(stats.currentMana, stats.maxMana.GetValue());
        }
        
        return true;
    }
    
    private void RefillMana()
    {
        // Mana yenileme hızı
        float manaRegenRate = stats.maxMana.GetValue() * 0.035f; // Max mananın %1'i her frame
        
        // Mana değerini arttır ama max değeri geçme
        stats.currentMana = Mathf.Min(stats.currentMana + manaRegenRate * Time.deltaTime, stats.maxMana.GetValue());
        
        // UI güncellemesi
        ManaBar manaBar = GetComponent<ManaBar>();
        if (manaBar != null) {
            manaBar.UpdateManaBar(stats.currentMana, stats.maxMana.GetValue());
        }
    }

    #endregion
    
    #region Checkpoint System

     private void LoadCheckpoint()
    {
        // İlk başlangıçta PlayerSpawnPoint'ten başla
        GameObject spawnPoint = GameObject.FindGameObjectWithTag("PlayerSpawnPoint");
        lastCheckpointPosition = spawnPoint != null ? spawnPoint.transform.position : transform.position;

        // Aktif checkpoint varsa, onun konumunu kullan
        if (PlayerPrefs.GetInt("CheckpointActivated", 0) == 1)
        {
            float x = PlayerPrefs.GetFloat("CheckpointX");
            float y = PlayerPrefs.GetFloat("CheckpointY");
            lastCheckpointPosition = new Vector2(x, y);
            
            // İtem durumlarını yükle
            Checkpoint.LoadItemStates(this);
        }

        // Oyuncunun konumunu ayarla
        transform.position = lastCheckpointPosition;
    }

    public void RespawnAtCheckpoint()
    {
        if (stateMachine.currentState == deadState)
        {
            // Gerekli değişkenleri sıfırla
            rb.linearVelocity = Vector2.zero;
            
            // Checkpoint kontrolü
            if (PlayerPrefs.GetInt("CheckpointActivated", 0) == 1)
            {
                // Sahne kontrolü
                int currentSceneIndex = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
                int checkpointSceneIndex = PlayerPrefs.GetInt("CheckpointSceneIndex", 0);
                
                // Farklı sahnedeyse, o sahneyi yükle
                if (currentSceneIndex != checkpointSceneIndex)
                {
                    UnityEngine.SceneManagement.SceneManager.LoadScene(checkpointSceneIndex);
                    return;
                }
                
                // Checkpoint'ten devam et
                transform.position = lastCheckpointPosition;
                
                // İtem durumlarını yükle
                Checkpoint.LoadItemStates(this);
                if (Inventory.instance != null)
                {
                    Inventory.instance.ReloadInventoryAfterDeath();
                }
            }
            else
            {
                // Spawn noktasından başla
                GameObject spawnPoint = GameObject.FindGameObjectWithTag("PlayerSpawnPoint");
                if (spawnPoint != null)
                {
                    transform.position = spawnPoint.transform.position;
                }
            }
            
            // Can ve mana değerlerini sıfırla
            float roundedMaxHealth = Mathf.Round(stats.maxHealth.GetValue());
            float roundedMaxMana = Mathf.Round(stats.maxMana.GetValue());
            
            stats.currentHealth = roundedMaxHealth;
            stats.currentMana = roundedMaxMana;
            
            // UI'ı güncelle
            if (healthBar != null)
                healthBar.UpdateHealthBar(stats.currentHealth, stats.maxHealth.GetValue());
            
            // Idle durumuna geç
            stateMachine.ChangeState(idleState);
            
            // Oyuncunun yönünü sıfırla
            ResetPlayerFacing();
        }
    }

    #endregion
    
    #region BoomerangRegion
    
    public bool CanThrowBoomerang()
    {
        // Bumerang cooldown kontrolü ve bumerang silahının aktif olup olmadığını kontrol et
        bool isBoomerangActive = boomerangWeapon != null && boomerangWeapon.gameObject.activeInHierarchy;
        return boomerangCooldownTimer <= 0f && isBoomerangActive;
    }
    
    public bool CanUseEarthPush()
    {
        if (SkillManager.Instance == null)
            return earthPushCooldownTimer <= 0f && HasEnoughMana(earthPushManaCost);
        
        // SkillManager üzerinden kontrol et
        return SkillManager.Instance.IsSkillReady(SkillType.EarthPush, stats.currentMana);
    }
    
    // Removed boomerang knockback - players don't get knocked back anymore
    
    public void ThrowBoomerang()
    {
        // Bumerang atma koşullarını kontrol et
        if (!CanThrowBoomerang())
        {
            return;
        }

        Instantiate(boomerangPrefab, boomerangLaunchPoint.position, Quaternion.identity);
        boomerangCooldownTimer = boomerangCooldown;
        
        // Bumerang silahını devre dışı bırak
        if (boomerangWeapon != null)
        {
            boomerangWeapon.gameObject.SetActive(false);
        }
        
        // Boomerang havada olduğunu işaretle
        isBoomerangInAir = true;
        // Debug.Log("Boomerang thrown! isBoomerangInAir = true");
    }
    
    public void CatchBoomerang()
    {
        // Bumerang silahını bul ve aktif et
        BoomerangWeaponStateMachine boomerangWeapon = GetComponentInChildren<BoomerangWeaponStateMachine>(true);
        if (boomerangWeapon != null)
        {
            boomerangWeapon.gameObject.SetActive(true);
        }
        
        // Boomerang artık havada değil
        isBoomerangInAir = false;
        // Debug.Log("Boomerang caught! isBoomerangInAir = false");
        
        stateMachine.ChangeState(catchBoomerangState);
    }
    // Bumerangın yakalanması için metod
    
    #endregion
    
    #region Dash

    private void CheckForDashInput()
    {
        if (stateMachine.currentState == voidState)
        {
            return;
        }
        dashTimer -= Time.deltaTime;
        if (UserInput.WasDashPressed && dashTimer <0 && !IsGroundDetected())
        {
            dashTimer = dashCooldown;
            dashDirection = UserInput.MoveInput.x;
            
            if (dashDirection == 0)
            {
                dashDirection = facingdir;
            }
                
            stateMachine.ChangeState(dashState);
        }
     
       
    }
    
    private void CheckForGroundDashInput()
    {

        if (stateMachine.currentState == voidState)
        {
            return;
        }
        
        if (UserInput.WasDashPressed && dashTimer < 0 && IsGroundDetected())
        {

            if (stateMachine.currentState == crouchState)
            {
                return;
                
            }
            dashTimer = dashCooldown;
            dashDirection = UserInput.MoveInput.x;
            
            if (dashDirection == 0)
            {
                dashDirection = facingdir;
            }
        
            stateMachine.ChangeState(groundDashState);
        }

       
    }


    public void DashCoroutineController()
    {
        StartCoroutine(dashState.SpawnDashEffects());
    }

    public void GroundDashCoroutineController()
    {
        StartCoroutine(groundDashState.SpawnGroundDashEffects());
    }
    #endregion
    
    #region GhostMode

    public void EnterGhostMode()
    {
        Physics2D.IgnoreLayerCollision(playerLayer,passableEnemiesLayerIndex,true);
        Physics2D.IgnoreLayerCollision(playerLayer,arrowLayerMaskIndex,true);
        //Debug.Log("EnterGhostMode");
    }
    
    public void ExitGhostMode()
    {
        Physics2D.IgnoreLayerCollision(playerLayer,passableEnemiesLayerIndex,false);
        Physics2D.IgnoreLayerCollision(playerLayer,arrowLayerMaskIndex,false);
        //Debug.Log("ExitGhostMode");
    }

    #endregion
    
    #region CrouchMode

    public void EnterCrouchMode()
    {
        capsuleCollider.size = new Vector2(capsuleCollider.size.x, crouchHeight); 
        capsuleCollider.offset = crouchOffset;
    }

    public void ExitCrouchMode()
    {
        capsuleCollider.size = new Vector2(capsuleCollider.size.x, normalHeight); 
        capsuleCollider.offset = normalOffset; 
    }

    #endregion
    
    #region Damage Region

    public override void Damage(Stat attackerDamageStat = null)
    {
        if (!isInvulnerable)
        {
            // Use the new player damage system for better control
            TakePlayerDamage(attackerDamageStat, CharacterStats.DamageType.Physical);
        }
    }
    
    // New method to handle player-specific damage with damage type
    public void TakePlayerDamage(Stat attackerDamageStat = null, CharacterStats.DamageType damageType = CharacterStats.DamageType.Physical)
    {
        if (isInvulnerable)
            return;
        
        entityFX.StartCoroutine("HitFX");
        
        // Players don't get knocked back anymore - removed knockback application
        
        // Apply damage with type
        var playerStats = stats as PlayerStats;
        if (playerStats != null)
        {
            // Use the custom TakeDamage method in CharacterStats 
            // that properly applies defense and resistances
            stats.TakeDamage(0, damageType, attackerDamageStat);
            
            // Update the UI health bar
            if (healthBar != null)
            {
                healthBar.UpdateHealthBar(stats.currentHealth, stats.maxHealth.GetValue());
            }
        }
        
        // Start invulnerability period
        StartCoroutine(InvulnerabilityCoroutine());
    }
    
    // Method to take elemental damage (for environmental hazards, spells, etc.)
    public void TakeElementalDamage(float amount, CharacterStats.DamageType elementType)
    {
        if (isInvulnerable)
            return;
            
        // Apply appropriate visual effect based on element type
        switch (elementType)
        {
            case CharacterStats.DamageType.Fire:
                entityFX.StartCoroutine("FireFX");
                break;
            case CharacterStats.DamageType.Ice:
                entityFX.StartCoroutine("IceFX");
                break;
            default:
                entityFX.StartCoroutine("HitFX");
                break;
        }
        
        // Apply damage directly with the element type
        stats.TakeDamage(amount, elementType);
        
        // Update UI
        if (healthBar != null)
        {
            healthBar.UpdateHealthBar(stats.currentHealth, stats.maxHealth.GetValue());
        }
        
        // Start invulnerability period
        StartCoroutine(InvulnerabilityCoroutine());
    }

    public override void DamageWithoutKnockback(Stat attackerDamageStat = null)
    {
        // Since players don't get knocked back anymore, this method is identical to regular damage
        TakePlayerDamage(attackerDamageStat, CharacterStats.DamageType.Physical);
    }

    #endregion

    #region DrawGizmosAndTriggerEvents

    

    private void OnTriggerStay2D(Collider2D other)
    {
        IInteractable interactable = other.GetComponent<IInteractable>();
        if (interactable != null)
        {
            currentInteractable = interactable;
            interactable.ShowInteractionPrompt();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        IInteractable interactable = other.GetComponent<IInteractable>();
        if (interactable != null)
        {
            if (interactable == currentInteractable)
            {
                currentInteractable = null;
            }
            
            interactable.HideInteractionPrompt();
        }
    }

    #endregion

    #region Spell Section
    
    public bool CanCastSpells()
    {
        return spellbookWeapon != null && spellbookWeapon.gameObject.activeInHierarchy;
    }

    // Bu metod buz parçalarının oluşturulabileceği geçerli pozisyonların olup olmadığını kontrol eder
    private bool CanCreateIceShards()
    {
        float xOffset = 1f * facingdir;
        float startX = transform.position.x + xOffset;
        float spawnY = transform.position.y + 0.3f;
        
        for (int i = 0; i < 4; i++)
        {
            Vector3 position = new Vector3(
                startX + (spellSpacing * i * facingdir),
                spawnY,
                transform.position.z
            );
            
            // Herhangi bir pozisyonun altında zemin varsa, skillini kullanabiliriz
            bool hasGround = Physics2D.Raycast(
                position, 
                Vector2.down, 
                10f, 
                whatIsGround
            ).collider != null;
            
            if (hasGround)
            {
                return true; // Bir tane bile uygun pozisyon bulunduğunda true döner
            }
        }
        
        return false; // Hiçbir uygun pozisyon bulunamadı
    }

    public void SpellOneTrigger()
    {
        StartCoroutine(CastIceShards());
    }

    private IEnumerator CastIceShards()
    {
        Vector3[] spawnPositions = spell1State.GetSpawnPositions();
        int shardsToSpawn = spawnPositions.Length;

        // Only spawn ice shards at valid positions
        for (int i = 0; i < shardsToSpawn; i++)
        {
            GameObject iceShardObj = Instantiate(iceShardPrefab, spawnPositions[i], Quaternion.identity);
            
            // Set the ground layer for the ice shard
            IceShard iceShard = iceShardObj.GetComponent<IceShard>();
            if (iceShard != null)
            {
                iceShard.SetGroundLayer(whatIsGround);
            }
            
            yield return new WaitForSeconds(delayBetweenShards);
        }
    }
    
    private void CheckForSpellInput()
    {
        // State kontrolü
        bool canCastNewSpell = CanCastSpells();
        
        // SkillManager instance kontrolü
        bool hasSkillManager = SkillManager.Instance != null;
        
        // Ice Shard
        if (UserInput.WasSpell1Pressed && canCastNewSpell)
        {
            if (hasSkillManager)
            {
                // SkillManager üzerinden kontrol et
                if (SkillManager.Instance.IsSkillReady(SkillType.IceShard, stats.currentMana))
                {
                    stateMachine.ChangeState(spell1State);
                }
              
            }
            else
            {
                // Eskisi gibi kontrol et
                if (CanCreateIceShards())
                {
                    stateMachine.ChangeState(spell1State);
                }
               
            }
        }
        
        // Fire Spell - Charge-based skill
        if (UserInput.WasSpell2Pressed && stateMachine.currentState != spell2State && canCastNewSpell)
        {
            Debug.Log("Spell2 tuşu basıldı! Kontroller yapılıyor...");
            Debug.Log($"Mevcut state: {stateMachine.currentState}, canCastNewSpell: {canCastNewSpell}");
            
            if (hasSkillManager)
            {
                Debug.Log("SkillManager ile kontrol yapılıyor");
                // SkillManager üzerinden kontrol et
                if (SkillManager.Instance.IsSkillReady(SkillType.FireSpell, stats.currentMana))
                {
                    Debug.Log("SkillManager: Fire Spell kullanıma hazır, Spell2State'e geçiliyor");
                    stateMachine.ChangeState(spell2State);
                }
                else
                {
                    Debug.Log("SkillManager: Fire Spell kullanıma hazır değil");
                }
            }
            else
            {
                Debug.Log("SkillManager yok, eski yöntemle kontrol yapılıyor");
                // Eski yöntem - SkillManager olmadan
                if (stats.currentMana >= fireSpellManaDrainPerSecond)
                {
                    Debug.Log($"Eski yöntem: Mana yeterli ({stats.currentMana} >= {fireSpellManaDrainPerSecond}), Spell2State'e geçiliyor");
                    stateMachine.ChangeState(spell2State);
                }
                else
                {
                    Debug.Log($"Eski yöntem: Mana yetersiz ({stats.currentMana} < {fireSpellManaDrainPerSecond})");
                }
            }
        }
        
        // Earth Push
        if (UserInput.WasSpell3Pressed && canCastNewSpell)
        {
            if (hasSkillManager)
            {
                // SkillManager üzerinden kontrol et
                if (SkillManager.Instance.IsSkillReady(SkillType.EarthPush, stats.currentMana))
                {
                    stateMachine.ChangeState(earthPushState);
                }
            }
            else
            {
                // Eskisi gibi kontrol et
                if (CanUseEarthPush())
                {
                    earthPushCooldownTimer = earthPushCooldown;
                    stateMachine.ChangeState(earthPushState);
                }
            }
        }
        
        // Air Push
        if (UserInput.WasSpell4Pressed && canCastNewSpell)
        {
            if (hasSkillManager)
            {
                // SkillManager üzerinden kontrol et
                if (SkillManager.Instance.IsSkillReady(SkillType.AirPush, stats.currentMana))
                {
                    stateMachine.ChangeState(airPushState);
                }
            }
            else
            {
                // Eskisi gibi kontrol et
                if (CanUseAirPush())
                {
                    airPushCooldownTimer = airPushCooldown;
                    stateMachine.ChangeState(airPushState);
                }
            }
        }
        
        // Fireball Spell
        if (UserInput.WasSpell7Pressed && canCastNewSpell)
        {
            if (hasSkillManager)
            {
                // SkillManager üzerinden kontrol et
                if (SkillManager.Instance.IsSkillReady(SkillType.FireballSpell, stats.currentMana))
                {
                    stateMachine.ChangeState(fireballSpellState);
                }
            }
        }
    }

    private void StartFireSpell()
    {
        // SkillManager kontrolü - Fire Spell açık mı?
        if (SkillManager.Instance != null && !SkillManager.Instance.IsSkillUnlocked("FireSpell"))
        {
            Debug.Log("Fire Spell not unlocked!");
            return;
        }
        
        if (!isChargingFire && HasEnoughMana(fireSpellManaDrainPerSecond * Time.deltaTime))
        {
            isChargingFire = true;
            stateMachine.ChangeState(spell2State);
        }
    }

    private void StopFireSpell()
    {
        if (isChargingFire)
        {
            isChargingFire = false;
            stateMachine.ChangeState(idleState);
        }
    }

    #endregion
    
    #region WeaponManagement

    private void UpdateWeaponReferences() 
    {
        // Tüm silah referanslarını güncelle (aktif veya inaktif)
        boomerangWeapon = GetComponentInChildren<BoomerangWeaponStateMachine>(true);
        spellbookWeapon = GetComponentInChildren<SpellbookWeaponStateMachine>(true);
        hammer = GetComponentInChildren<HammerSwordStateMachine>(true);
        iceHammer = GetComponentInChildren<IceHammerStateMachine>(true);
        burningSword = GetComponentInChildren<BurningSwordStateMachine>(true);
        shield = GetComponentInChildren<ShieldStateMachine>(true);
        basicArmor = GetComponentInChildren<BasicArmorStateMachine>(true);
    }
    
    public void HideWeapons()
    {
        if (weaponsHidden) return;
        
        // Hangi silahın aktif olduğunu belirle ve kaydet
        if (boomerangWeapon != null && boomerangWeapon.gameObject.activeInHierarchy)
        {
            lastActiveWeaponState = WeaponState.ThrowBoomerang;
        }
        else if (spellbookWeapon != null && spellbookWeapon.gameObject.activeInHierarchy)
        {
            lastActiveWeaponState = WeaponState.Spell1;
        }
        
        // Find PlayerWeaponManager
        PlayerWeaponManager weaponManager = GetComponentInChildren<PlayerWeaponManager>();
        if (weaponManager != null && weaponManager.weapons != null)
        {
            foreach (WeaponStateMachine weapon in weaponManager.weapons)
            {
                if (weapon != null && weapon.gameObject.activeInHierarchy)
                {
                    // Hide the weapon but keep track of its previous state
                    weapon.gameObject.SetActive(false);
                }
            }
        }
        else
        {
            Debug.LogWarning("PlayerWeaponManager not found during weapon hiding!");
        }
        
        weaponsHidden = true;
    }

    public void ShowWeapons()
    {
        if (!weaponsHidden) return;
        
        // Find PlayerWeaponManager
        PlayerWeaponManager weaponManager = GetComponentInChildren<PlayerWeaponManager>();
        if (weaponManager != null && weaponManager.weapons != null)
        {
            // Always show the active primary weapon
            int primaryIndex = weaponManager.GetCurrentPrimaryWeaponIndex();
            if (weaponManager.weapons.Length > 0 && weaponManager.weapons[primaryIndex] != null)
            {
                weaponManager.weapons[primaryIndex].gameObject.SetActive(true);
            }
            
            // Refresh the secondary weapon state
            weaponManager.RefreshWeaponVisibility();
        }
        else
        {
            Debug.LogWarning("PlayerWeaponManager not found! Weapon management may not work properly.");
        }
        
        weaponsHidden = false;
    }

    // Dışarıdan lastActiveWeaponState'i güncellemek için method
    public void UpdateLastActiveWeapon(WeaponState weaponState)
    {
        lastActiveWeaponState = weaponState;
    }
    
    // Her frame silah görünürlüğünü kontrol et
    private void ManageWeaponVisibilityBasedOnBoomerang()
    {
        if (isBoomerangInAir)
        {
                    // Boomerang havadayken: Sadece aktif primary weapon görünür, diğerleri gizli
        PlayerWeaponManager weaponManager = GetComponentInChildren<PlayerWeaponManager>();
        if (weaponManager != null && weaponManager.weapons != null)
        {
            // Aktif primary weapon'ı bul ve aktif tut
            WeaponStateMachine activePrimaryWeapon = FindActivePrimaryWeapon(weaponManager);
            if (activePrimaryWeapon != null)
            {
                activePrimaryWeapon.gameObject.SetActive(true);
            }
            
            // Tüm secondary silahları gizle
            for (int i = 0; i < weaponManager.weapons.Length; i++)
            {
                if (weaponManager.weapons[i] != null && IsSecondaryWeapon(weaponManager.weapons[i]))
                {
                    weaponManager.weapons[i].gameObject.SetActive(false);
                }
            }
        }
        }
        // Boomerang havada değilse normal silah sistemine müdahale etme
    }
    
    // Aktif primary weapon'ı bul
    private WeaponStateMachine FindActivePrimaryWeapon(PlayerWeaponManager weaponManager)
    {
        for (int i = 0; i < weaponManager.weapons.Length; i++)
        {
            if (weaponManager.weapons[i] != null && 
                IsPrimaryWeapon(weaponManager.weapons[i]) && 
                weaponManager.weapons[i].gameObject.activeInHierarchy)
            {
                return weaponManager.weapons[i];
            }
        }
        
        // Eğer aktif primary weapon bulunamazsa, starting weapon'ı döndür
        if (weaponManager.weapons.Length > weaponManager.startingWeaponIndex && 
            weaponManager.weapons[weaponManager.startingWeaponIndex] != null)
        {
            return weaponManager.weapons[weaponManager.startingWeaponIndex];
        }
        
        return null;
    }
    
    // Primary weapon kontrolü
    private bool IsPrimaryWeapon(WeaponStateMachine weapon)
    {
        return weapon is SwordWeaponStateMachine || 
               weapon is BurningSwordStateMachine || 
               weapon is HammerSwordStateMachine ||
               weapon is IceHammerStateMachine;
    }
    
    // Secondary weapon kontrolü
    private bool IsSecondaryWeapon(WeaponStateMachine weapon)
    {
        return weapon is BoomerangWeaponStateMachine || 
               weapon is SpellbookWeaponStateMachine || 
               weapon is ShieldStateMachine;
    }

    #endregion

    #region Level Section

    public void AddExperience(int amount)
    {
        if (amount <= 0) return;
        
        experience += amount;
       
        
        // Seviye atlama kontrolü
        CheckLevelUp();
        
        // Burada UI güncellemesi yapılabilir
    }
    
    private void CheckLevelUp()
    {
        while (experience >= experienceToNextLevel)
        {
            // Seviye atla
            level++;
            experience -= experienceToNextLevel;
            
            // Yeni seviye için gereken tecrübe puanını artır
            experienceToNextLevel = (int)(experienceToNextLevel * 1.5f);
            
           
            
            // Seviye atlama bonusları (can, mana vs artırılabilir)
            stats.maxHealth.AddModifier(10);
            stats.maxMana.AddModifier(5);
            stats.currentHealth = stats.maxHealth.GetValue();
            stats.currentMana = stats.maxMana.GetValue();
            
            // Seviye atlama efekti oynatılabilir
            // ...
        }
    }

    #endregion

    #region Parry and Block Region

    private void CheckForSuccessfulParry()
    {
        // PARRY SADECE SHIELD AKTİFKEN ÇALIŞIR
        if (!IsShieldActive())
        {
            return;
        }
        
        // Sadece parry tuşuna anlık basış varsa (GetKeyDown) && parry cooldown'u dolmuşsa
        if (UserInput.WasParryPressed && parryTimer <= 0)
        {
            // Düşmanın parry penceresi açık mı diye kontrol et
            if (IsEnemyParryWindowOpen())
            {
                // Başarılı parry durumuna geç
                stateMachine.ChangeState(succesfulParryState);
            }
            else
            {
                // Parry penceresi açık değilse, sadece normal parry state'ine geç
                stateMachine.ChangeState(parryState);
            }
            
            // Parry için cooldown başlat
            parryTimer = parryCooldown;
        }
    }
    
    /// <summary>
    /// Shield'ın aktif olup olmadığını kontrol eder
    /// </summary>
    /// <returns>Shield aktifse true, değilse false</returns>
    private bool IsShieldActive()
    {
        return shield != null && shield.gameObject.activeInHierarchy;
    }
    
    // Genel düşman parry kontrolü - Generic versiyon
    public bool CheckAndParryEnemy<T>(T enemy) where T : Enemy
    {
        // IParryable interface'ini implemente ediyorsa parry işlemi yap
        if (enemy is IParryable parryableEnemy)
        {
            if (parryableEnemy.IsParryWindowOpen)
            {
                // Bu düşmanı vurulmuş olarak işaretle
                MarkEntityAsHit(enemy);
                
              
                
                // Parry bilgisini düşmana ilet
                parryableEnemy.GetParried();
                return true;
            }
        }
        
        return false;
    }
    
    private bool IsEnemyParryWindowOpen()
    {
        // Parry yarıçapında düşmanları kontrol et
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, parryRadius, passableEnemiesLayerMask);
        
        foreach (var hit in colliders)
        {
            IParryable parryableEnemy = hit.GetComponent<IParryable>();
            if (parryableEnemy != null && parryableEnemy.IsParryWindowOpen)
            {
                return true;
            }
        }
        
        return false;
    }

    #endregion

    #region Invulnerability Section 

    public void SetTemporaryInvulnerability(float duration)
    {
        StartCoroutine(SimpleInvulnerabilityCoroutine(duration));
    }
    
    // Flash efekti olmadan sadece invulnerability veren metot
    private IEnumerator SimpleInvulnerabilityCoroutine(float duration)
    {
        isInvulnerable = true;
        yield return new WaitForSeconds(duration);
        isInvulnerable = false;
    }
    
    private IEnumerator InvulnerabilityCoroutine()
    {
        isInvulnerable = true;
        // Flash efektini invulnerability süresi boyunca çalıştır
        StartCoroutine(entityFX.FlashFX(invulnerabilityDuration));
        yield return new WaitForSeconds(invulnerabilityDuration);
        isInvulnerable = false;
    }

    #endregion
    
    public void AddGold(int amount)
    {
        if (amount <= 0) return;
        
        gold += amount;
        
        // Burada UI güncellemesi yapılabilir
    }
  
    public void StartNewAttack()
    {
        // Vurulan entityleri sıfırla
        ClearHitEntities();
        hitDummyIDs.Clear();
        isAttackActive = false; // Aktif edilene kadar pasif
    }
    
    public void ResetPlayerFacing()
    {
        transform.rotation = Quaternion.identity;
        facingdir = 1;
        facingright = true;
    }

    public void AnimationFinishTrigger() => stateMachine.currentState.AnimationFinishTrigger();

    public override void Die()
    {
        base.Die();
        
        // Ölüm işlemi
        stateMachine.ChangeState(deadState);

        // Öldüğünde checkpoint'e dönmesi için bayrağı ayarla
        PlayerPrefs.SetInt("PlayerDied", 1);
        PlayerPrefs.Save();
        
        // Checkpoint varsa, bir süre sonra oyuncuyu checkpoint'e ışınla
        if (PlayerPrefs.GetInt("CheckpointActivated", 0) == 1)
        {
            // Ölüm animasyonu için biraz bekle, sonra sahneyi yeniden yükle
            StartCoroutine(RespawnAfterDelay(3f));
        }
    }
    
    // Removed stunned knockback - players don't get knocked back anymore
    
    public override bool IsGroundDetected()
    {
        Vector2 boxCenter = (Vector2)transform.position + 
                          new Vector2(0, -capsuleCollider.size.y / 2);

        // Ana merkez kontrolü
        RaycastHit2D centerHit = Physics2D.BoxCast(
            boxCenter,
            new Vector2(groundCheckWidth * 0.8f, groundCheckHeight),
            0f,
            Vector2.down,
            groundCheckExtraHeight,
            whatIsGround
        );

        // Çoklu yan kontroller
        Vector2 leftStart = boxCenter + new Vector2(-groundCheckWidth/2, 0);
        Vector2 rightStart = boxCenter + new Vector2(groundCheckWidth/2, 0);

        // Sol taraf kontrolleri
        RaycastHit2D leftHit = Physics2D.Raycast(leftStart, Vector2.down, groundCheckExtraHeight + groundCheckHeight, whatIsGround);
        RaycastHit2D leftInnerHit = Physics2D.Raycast(leftStart + Vector2.right * sideRaySpacing, Vector2.down, groundCheckExtraHeight + groundCheckHeight, whatIsGround);

        // Sağ taraf kontrolleri
        RaycastHit2D rightHit = Physics2D.Raycast(rightStart, Vector2.down, groundCheckExtraHeight + groundCheckHeight, whatIsGround);
        RaycastHit2D rightInnerHit = Physics2D.Raycast(rightStart + Vector2.left * sideRaySpacing, Vector2.down, groundCheckExtraHeight + groundCheckHeight, whatIsGround);

        if (centerHit.collider != null)
        {
            float slopeAngle = Vector2.Angle(centerHit.normal, Vector2.up);
            if (slopeAngle <= 45f)
            {
                // Eğim açısı uygunsa ve merkez vuruş varsa, karakteri yüzeye yapıştır
                if (rb.linearVelocity.y < 0)
                {
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
                }
                return true;
            }
        }

        // Herhangi bir yan kontrol yere değiyorsa
        bool sideHit = (leftHit.collider != null || rightHit.collider != null || 
                        leftInnerHit.collider != null || rightInnerHit.collider != null);

        if (sideHit && rb.linearVelocity.y < 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
        }

        return sideHit;
    }
   
    // Ölüm sonrası yeniden canlanma
    private IEnumerator RespawnAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        // Checkpoint sahnesi var mı kontrol et
        if (PlayerPrefs.GetInt("CheckpointActivated", 0) == 1)
        {
            int checkpointSceneIndex = PlayerPrefs.GetInt("CheckpointSceneIndex", 0);
            
            // Sahneyi yükle
            UnityEngine.SceneManagement.SceneManager.LoadScene(checkpointSceneIndex);
        }
    }

    // Void becerisi için özel kontrol (beceri açık değilse kullanılamaz)
    public bool CanUseVoidSkill()
    {
        if (SkillManager.Instance == null)
            // Eski yöntem - yerel kontrol
            return HasEnoughMana(voidSkillManaCost);
        
        // SkillManager üzerinden kontrol et
        return SkillManager.Instance.IsSkillReady(SkillType.VoidSkill, stats.currentMana);
    }
    

    
    // Diğer beceriler için genel kontrol metodu
    public bool CanUseSkill(string skillID)
    {
        return SkillManager.Instance != null && 
               SkillManager.Instance.IsSkillUnlocked(skillID);
    }
    
    // Void Skill'i kullanma metodu
    public void UseVoidSkill()
    {
        // Void Skill açılmış mı ve mana yeterli mi kontrol et
        if (CanUseVoidSkill())
        {
            // Void Skill'i kullan
            UseMana(voidSkillManaCost);
            
            // Void Slash prefabını oluştur
            if (voidSlashPrefab != null)
            {
                Instantiate(voidSlashPrefab, transform.position, Quaternion.identity);
            }
        }
    }

    // Void becerisi için giriş kontrolü
    private void CheckForVoidSkillInput()
    {
        // X tuşuna basıldığında ve beceri açıksa kullan
        if (UserInput.WasVoidInputPressed && stateMachine.currentState != voidState && CanUseVoidSkill())
        {
            // Yeterli mana ve beceri açıksa state'e geçiş yap
            stateMachine.ChangeState(voidState);
        }
    }

    public bool CanUseIceShard()
    {
        // Ice Shard cooldown kontrolü
        if (SkillManager.Instance == null)
            return iceShardCooldownTimer <= 0f && HasEnoughMana(iceShardManaCost) && CanCreateIceShards();
        
        // SkillManager üzerinden kontrol et
        return SkillManager.Instance.IsSkillReady(SkillType.IceShard, stats.currentMana) && CanCreateIceShards();
    }

    // Add new method to check for electric dash input
    private void CheckForElectricDashInput()
    {
        // Make sure we're not in void state and can use the skill
        if (stateMachine.currentState == voidState)
        {
            return;
        }
        
        // Check for the electric dash input
        if (UserInput.WasSpell5Pressed && CanUseElectricDash())
        {
            // Set direction
            dashDirection = UserInput.MoveInput.x;
            if (dashDirection == 0)
            {
                dashDirection = facingdir;
            }
            
            // Change to electric dash state
            stateMachine.ChangeState(electricDashState);
            
            // Mark the skill as used in SkillManager
            if (SkillManager.Instance != null)
            {
                SkillManager.Instance.UseSkill(SkillType.ElectricDash);
            }
        }
    }

    // Check if electric dash can be used
    public bool CanUseElectricDash()
    {
        if (SkillManager.Instance == null)
            return HasEnoughMana(electricDashManaCost);
        
        // Use SkillManager to check if skill is ready and mana is sufficient
        return SkillManager.Instance.IsSkillReady(SkillType.ElectricDash, stats.currentMana);
    }

    // Add this method where other input check methods are
    private void CheckForAirPushInput()
    {
        // Check if player can use Air Push
        if (UserInput.WasSpell6Pressed && CanUseAirPush() && CanCastSpells())
        {
            // Change to AirPush state
            stateMachine.ChangeState(airPushState);
            
            // Start cooldown
            airPushCooldownTimer = airPushCooldown;
            
            // If using SkillManager, mark skill as used
            if (SkillManager.Instance != null)
            {
                SkillManager.Instance.UseSkill(SkillType.AirPush);
            }
        }
    }

    // Add this method with other Can methods
    public bool CanUseAirPush()
    {
        if (SkillManager.Instance == null)
            return airPushCooldownTimer <= 0f && HasEnoughMana(airPushManaCost);
        
        // Use SkillManager to check if skill is ready and mana is sufficient
        return SkillManager.Instance.IsSkillReady(SkillType.AirPush, stats.currentMana);
    }

    #region UI Input Methods
    
    private void CheckForUIInput()
    {
        // Inventory toggle
        if (UserInput.WasInventoryPressed)
        {
            ToggleInventory();
        }
    }
    
    private void ToggleInventory()
    {
        // First try Instance, then fallback to FindFirstObjectByType
        AdvancedInventoryUI inventoryUI = AdvancedInventoryUI.Instance;
        
        if (inventoryUI == null)
        {
            inventoryUI = FindFirstObjectByType<AdvancedInventoryUI>();
        }
        
        if (inventoryUI != null)
        {
            if (inventoryUI.gameObject.activeInHierarchy)
            {
                inventoryUI.CloseInventory();
            }
            else
            {
                inventoryUI.OpenInventory();
            }
        }
        else
        {
            Debug.LogWarning("AdvancedInventoryUI not found in scene! Make sure the UI is present.");
        }
    }
    
    #endregion
    
    #region Flip Override
    
    public override void FlipController(float _x)
    {
        // Saldırı, parry veya crouch attack stateleri sırasında flip yapma
        if (stateMachine.currentState == attackState || 
            stateMachine.currentState == crouchAttackState ||
            stateMachine.currentState == parryState ||
            stateMachine.currentState == succesfulParryState)
        {
            return;
        }
        
        base.FlipController(_x);
    }

    public override void Flip()
    {
        base.Flip();
        cameraFollowObject.CallTurn();
    }

    #endregion
    
    #region Velocity Override
    
    // Player için isKnocked kontrolünü kaldırıyoruz - Player'lar knockback almaz
    public override void SetVelocity(float xVelocity, float yVelocity)
    {
        rb.linearVelocity = new Vector2(xVelocity, yVelocity);
        FlipController(xVelocity);
    }

    

    public override void SetZeroVelocity()
    {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
    }
    
    // Player'lar knockback almazlar - bu metodu override edip hiçbir şey yapmayız
    public override IEnumerator HitKnockback(Vector2 knockbackDirectionParam)
    {
        // Player'lar knockback almazlar, bu yüzden hiçbir şey yapmayız
        yield break;
    }
    
    #endregion

    [Header("Hammer Explosion")]
    public Transform hammerExplosionCheck; // Patlama pozisyonu için boş obje
    public Vector2 hammerExplosionSize = new Vector2(2f, 2f); // Patlama alanı boyutu
    public HashSet<int> explosionHitEntities = new HashSet<int>(); // Explosion için vurulan düşmanlar
    public float lastHammerCombo3Damage; // 3. combo gerçek hasarı (crit, buff dahil)
    
    [Header("IceHammer Explosion")]
    public Transform iceHammerExplosionCheck; // Patlama pozisyonu için boş obje
    public Vector2 iceHammerExplosionSize = new Vector2(2f, 2f); // Patlama alanı boyutu
    public HashSet<int> iceHammerExplosionHitEntities = new HashSet<int>(); // Explosion için vurulan düşmanlar
    public float iceHammerlastHammerCombo3Damage; // 3. combo gerçek hasarı (crit, buff dahil)
}

