using System;
using UnityEngine;
using UnityEngine.Serialization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class Player : Entity
{
   
    
    public IPlayerInput playerInput {get;private set;}
    
    [HideInInspector]
    public HealthBar healthBar;


    [Header("AttackDetails")] 
    //public Vector2[] attackMovement;

    public Transform attackCheck;
    //public float attackCheckRadius;
    public Vector2 attackSize;
    
    [SerializeField] private GameObject boomerangPrefab;
    [SerializeField] private Transform boomerangLaunchPoint;
    [SerializeField] public Vector2 boomerangCatchForce;
    
    [FormerlySerializedAs("boxCollider")] [Header("Collider")]
    [HideInInspector]
    public CapsuleCollider2D capsuleCollider;
    
    private float xInput;
    private float yInput;
    
    
    [Header("Movement")]
    public float moveSpeed;
    public float jumpForce;
    
    

    [Header("Dash Info")] 
    [SerializeField] private float dashCooldown;
    private float dashTimer;
    public float dashDuration;
    public float dashSpeed;
    public float groundDashSpeed;
    
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
    [SerializeField] private Vector2 stunKnocback; // Hasar alamama süresi
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
    public PlayerCrouchAttackState crouchAttackState {get;private set;}
    public PlayerDeadState deadState {get;private set;}
    public PlayerStunnedState stunnedState {get;private set;}
    
    public PlayerParryState parryState {get;private set;}
    
    public PlayerSpell1State spell1State {get;private set;}
    public PlayerSpell2State spell2State {get;private set;}
    public PlayerEarthPushSpellState earthPushState {get;private set;}
    
    public PlayerThrowBoomerangState throwBoomerangState {get;private set;}
    public PlayerCatchBoomerangState catchBoomerangState {get;private set;}
    public PlayerVoidState voidState {get;private set;}
    public PlayerSuccesfulParryState succesfulParryState {get;private set;}
    
    #endregion
    
    private IInteractable currentInteractable;

    [Header("Crouch Attack")]
    public Vector2 crouchAttackOffset; // Örnek: (0, 0.2f) 

    [Header("Weapon References")]
    public BoomerangWeaponStateMachine boomerangWeapon;
    public SpellbookWeaponStateMachine spellbookWeapon;
    public SwordWeaponStateMachine swordWeapon;

    [Header("Boomerang Settings")]
    [SerializeField] private float boomerangCooldown = 2f;
    private float boomerangCooldownTimer;

    [Header("Ice Spell Settings")]
    [SerializeField] private GameObject iceShardPrefab;
    [SerializeField] public float spellSpacing = 1f;
    [SerializeField] private float delayBetweenShards = 0.1f;
    //[SerializeField] private int shardCount = 3;
    
    [Header("Mana Costs")]
    [SerializeField] public float spell1ManaCost = 20f;
    [SerializeField] public float spell2ManaDrainPerSecond = 5f; // Saniyede tüketilecek mana
    [SerializeField] public float earthPushManaCost = 25f;
    [SerializeField] public float voidSkillManaCost = 40f;

    [Header("Void Skill Settings")]
    [SerializeField] public GameObject voidSlashPrefab;

    [Header("Fire Spell Settings")]
    public GameObject fireSpellPrefab;
    public Transform fireSpellPoint;
    private bool isChargingFire = false;

    [Header("Earth Spell Settings")]
    public GameObject earthPushPrefab;
    public Transform earthPushSpawnPoint;

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
    [SerializeField] public GameObject parryEffectPrefab; // Başarılı parry efekti prefabı
    
    
    protected override void Awake()
    {
        base.Awake();
        playerInput = new PCInput(); // PCInputa çevirebilirsin********************
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

        AssignWeapons();
    }

    

    protected override void Update()
    {
        base.Update();
        
       
        
        // Yetenek girişleri
        CheckForDashInput();
        CheckForGroundDashInput();
        
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
            
            // Etkileşim kontrolü
            if (playerInput.interactionInput && currentInteractable != null)
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
        
        // State machine güncellemesi
        stateMachine.currentState.Update();
        
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
    }
    
    private void AssignWeapons()
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
    }

    #endregion
    
    

    #region DrawGizmos

#if UNITY_EDITOR
    
    
    protected override void OnDrawGizmos()
    {
        if (!Application.isPlaying || capsuleCollider == null) return;

        Vector2 boxCenter = (Vector2)transform.position + 
                            new Vector2(0, -capsuleCollider.size.y / 2);
        
        // Ana box
        Gizmos.color = IsGroundDetected() ? Color.green : Color.red;
        Gizmos.DrawWireCube(boxCenter, new Vector3(groundCheckWidth, groundCheckHeight, 0));
        
        
        Gizmos.DrawWireCube(attackCheck.position,attackSize);
        
        // Kenar raycast'leri
        Vector2 leftPoint = boxCenter + new Vector2(-groundCheckWidth/2, 0);
        Vector2 rightPoint = boxCenter + new Vector2(groundCheckWidth/2, 0);
        
        Gizmos.DrawLine(leftPoint, leftPoint + Vector2.down * (groundCheckExtraHeight + groundCheckHeight));
        Gizmos.DrawLine(rightPoint, rightPoint + Vector2.down * (groundCheckExtraHeight + groundCheckHeight));

        // Parry yarıçapını her zaman göster
        Gizmos.color = new Color(0, 0.5f, 1f, 0.3f); // Daha rahat görünür mavi renk
        Gizmos.DrawWireSphere(transform.position, parryRadius);
        
        if (stateMachine != null)
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
    
    public void StartBoomerangKnockbackCoroutine()
    {
        StartCoroutine(HitKnockback(boomerangCatchForce));
    }
    
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
        
        
    }
    
    public void CatchBoomerang()
    {
        // Bumerang silahını bul ve aktif et
        BoomerangWeaponStateMachine boomerangWeapon = GetComponentInChildren<BoomerangWeaponStateMachine>(true);
        if (boomerangWeapon != null)
        {
            boomerangWeapon.gameObject.SetActive(true);
        }
        
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
        if (playerInput.dashInput&&dashTimer<0&&!IsGroundDetected())
        {
            dashTimer = dashCooldown;
            dashDirection = playerInput.xInput;
            
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
        
        if (playerInput.dashInput && dashTimer < 0 && IsGroundDetected())
        {

            if (stateMachine.currentState == crouchState)
            {
                return;
                
            }
            dashTimer = dashCooldown;
            dashDirection = playerInput.xInput;
            
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

    public override void Damage()
    {
        if (!isInvulnerable)
        {
            base.Damage();
            StartCoroutine(InvulnerabilityCoroutine());
        }
    }

    public override void DamageWithoutKnockback()
    {
        if (!isInvulnerable)
        {
            base.DamageWithoutKnockback();
            StartCoroutine(InvulnerabilityCoroutine());
        }
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
        
        for (int i = 0; i < 3; i++)
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
        // Eğer spellbook aktif değilse büyü kullanamaz
        if (!CanCastSpells() || IsGroundDetected() == false)
            return;

        // Spell1 kontrolü
        if (playerInput.spell1Input && HasEnoughMana(spell1ManaCost))
        {
            // Geçerli buz parçası pozisyonu var mı kontrol et
            if (!CanCreateIceShards())
                return; // Pozisyon yoksa direkt çık
                
            // Pozisyon varsa state'e geç
            stateMachine.ChangeState(spell1State);
        }
        // Spell2 kontrolü
        else if (playerInput.spell2Input && HasEnoughMana(spell2ManaDrainPerSecond * Time.deltaTime))
        {
            StartFireSpell();
        }
        // Earth Push kontrolü
        else if (playerInput.earthPushInput && HasEnoughMana(earthPushManaCost))
        {
            stateMachine.ChangeState(earthPushState);
        }
        else if (!playerInput.spell2Input && isChargingFire)
        {
            StopFireSpell();
        }
    }

    private void StartFireSpell()
    {
        if (!isChargingFire && HasEnoughMana(spell2ManaDrainPerSecond * Time.deltaTime))
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
        PlayerWeaponManager weaponManager = GetComponent<PlayerWeaponManager>();
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
            // Fallback if no weapon manager found
            if (swordWeapon != null) swordWeapon.gameObject.SetActive(false);
            if (boomerangWeapon != null) boomerangWeapon.gameObject.SetActive(false);
            if (spellbookWeapon != null) spellbookWeapon.gameObject.SetActive(false);
        }
        
        weaponsHidden = true;
    }

    public void ShowWeapons()
    {
        if (!weaponsHidden) return;
        
        // Find PlayerWeaponManager
        PlayerWeaponManager weaponManager = GetComponent<PlayerWeaponManager>();
        if (weaponManager != null && weaponManager.weapons != null)
        {
            // Always show the primary weapon (sword)
            if (weaponManager.weapons.Length > 0 && weaponManager.weapons[0] != null)
            {
                weaponManager.weapons[0].gameObject.SetActive(true);
            }
            
            // Refresh the secondary weapon state
            weaponManager.RefreshWeaponVisibility();
        }
        else
        {
            // Fallback if no weapon manager found - we need to manually activate the weapons
            if (swordWeapon != null) 
                swordWeapon.gameObject.SetActive(true);
            
            // Son aktif silahı belirle
            if (lastActiveWeaponState == WeaponState.ThrowBoomerang || 
                lastActiveWeaponState == WeaponState.CatchBoomerang)
            {
                // Boomerang silahını aktif et
                if (boomerangWeapon != null)
                    boomerangWeapon.gameObject.SetActive(true);
                    
                if (spellbookWeapon != null)
                    spellbookWeapon.gameObject.SetActive(false);
            }
            else if (lastActiveWeaponState == WeaponState.Spell1 || 
                     lastActiveWeaponState == WeaponState.Spell2)
            {
                // Spellbook silahını aktif et
                if (spellbookWeapon != null)
                    spellbookWeapon.gameObject.SetActive(true);
                    
                if (boomerangWeapon != null)
                    boomerangWeapon.gameObject.SetActive(false);
            }
            else
            {
                // Hiçbir özel durum yoksa varsayılan olarak bumerangı aktif et
                if (boomerangWeapon != null)
                    boomerangWeapon.gameObject.SetActive(true);
                    
                if (spellbookWeapon != null)
                    spellbookWeapon.gameObject.SetActive(false);
            }
        }
        
        weaponsHidden = false;
    }

    // Dışarıdan lastActiveWeaponState'i güncellemek için method
    public void UpdateLastActiveWeapon(WeaponState weaponState)
    {
        lastActiveWeaponState = weaponState;
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
        // Sadece parry tuşuna anlık basış varsa (GetKeyDown) && parry cooldown'u dolmuşsa
        if (playerInput.parryInput && parryTimer <= 0)
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
    
    public void StartStunnedKnockbackCoroutine()
    {
        StartCoroutine(HitKnockback(stunKnocback));
    }
    
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
        // Void becerisi açık mı, mana yeterli mi kontrol et
        return SkillManager.Instance != null && 
               SkillManager.Instance.IsSkillUnlocked("void_skill") && 
               HasEnoughMana(voidSkillManaCost);
    }
    
    // XXX becerisi için özel kontrol (beceri açık değilse kullanılamaz)
    public bool CanUseXXXSkill()
    {
        // XXX becerisi açık mı, mana yeterli mi kontrol et
        return SkillManager.Instance != null && 
               SkillManager.Instance.IsSkillUnlocked("xxx_skill") && 
               HasEnoughMana(40f); // XXX becerisi için mana maliyeti
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
        if (playerInput.voidSkillInput && stateMachine.currentState != voidState && CanUseVoidSkill())
        {
            // Yeterli mana ve beceri açıksa state'e geçiş yap
            stateMachine.ChangeState(voidState);
        }
    }
}

