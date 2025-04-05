using System;
using UnityEngine;
using UnityEngine.Serialization;
using System.Collections;

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
    
    public PlayerThrowBoomerangState throwBoomerangState {get;private set;}
    public PlayerCatchBoomerangState catchBoomerangState {get;private set;}
    public PlayerVoidState voidState {get;private set;}
    
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

    [Header("Spell Settings")]
    [SerializeField] private GameObject iceShardPrefab;
    [SerializeField] public float spellSpacing = 1f;
    [SerializeField] private float delayBetweenShards = 0.1f;
    [SerializeField] private int shardCount = 3;
    
    [Header("Mana Costs")]
    [SerializeField] private float spell1ManaCost = 20f;
    [SerializeField] public float spell2ManaDrainPerSecond = 5f; // Saniyede tüketilecek mana
    [SerializeField] public float voidSkillManaCost = 40f;

    [Header("Void Skill Settings")]
    [SerializeField] public GameObject voidSlashPrefab;

    [Header("Fire Spell Settings")]
    public GameObject fireSpellPrefab;
    public Transform fireSpellPoint;

    [Header("Fire Spell Settings")]
    private bool isChargingFire = false;

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

#if UNITY_EDITOR
    protected override void OnDrawGizmos()
    {
        if (!Application.isPlaying || capsuleCollider == null) return;

        Vector2 boxCenter = (Vector2)transform.position + 
                          new Vector2(0, -capsuleCollider.size.y / 2);
        
        // Ana box
        Gizmos.color = IsGroundDetected() ? Color.green : Color.red;
        Gizmos.DrawWireCube(boxCenter, new Vector3(groundCheckWidth, groundCheckHeight, 0));
        
        // Kenar raycast'leri
        Vector2 leftPoint = boxCenter + new Vector2(-groundCheckWidth/2, 0);
        Vector2 rightPoint = boxCenter + new Vector2(groundCheckWidth/2, 0);
        
        Gizmos.DrawLine(leftPoint, leftPoint + Vector2.down * (groundCheckExtraHeight + groundCheckHeight));
        Gizmos.DrawLine(rightPoint, rightPoint + Vector2.down * (groundCheckExtraHeight + groundCheckHeight));
    }
#endif

    public bool CanThrowBoomerang()
    {
        // Bumerang cooldown kontrolü ve bumerang silahının aktif olup olmadığını kontrol et
        bool isBoomerangActive = boomerangWeapon != null && boomerangWeapon.gameObject.activeInHierarchy;
        return boomerangCooldownTimer <= 0f && isBoomerangActive;
    }

    public bool CanCastSpells()
    {
        return spellbookWeapon != null && spellbookWeapon.gameObject.activeInHierarchy;
    }

    public bool HasEnoughMana(float manaCost)
    {
        return stats.currentMana >= manaCost;
    }

    public bool UseMana(float manaCost)
    {
        return stats.UseMana(manaCost);
    }

    protected override void Awake()
    {
        base.Awake();
        playerInput = new PCInput(); // PCInputa çevirebilirsin********************
        
        stateMachine = new PlayerStateMachine();
        
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


    }

    protected override void Start()
    {
        base.Start();
        
        // Rigidbody2D ayarlarını optimize et
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.sleepMode = RigidbodySleepMode2D.NeverSleep;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        
        healthBar = GetComponent<HealthBar>();
        ResetPlayerFacing();
        
        // Checkpoint pozisyonunu yükle
        LoadCheckpoint();
        
        stateMachine.Initialize(idleState);
        capsuleCollider = GetComponent<CapsuleCollider2D>();

        normalOffset = capsuleCollider.offset;
        crouchOffset = new Vector2(normalOffset.x, normalOffset.y-0.1f);
        
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

    #region Checkpoint System

     private void LoadCheckpoint()
    {
        // İlk başlangıçta PlayerSpawnPoint'ten başla
        GameObject spawnPoint = GameObject.FindGameObjectWithTag("PlayerSpawnPoint");
        lastCheckpointPosition = spawnPoint != null ? spawnPoint.transform.position : transform.position;

        // Eğer aktif bir checkpoint varsa, onun konumunu kullan
        if (PlayerPrefs.GetInt("CheckpointActivated", 0) == 1)
        {
            float x = PlayerPrefs.GetFloat("CheckpointX");
            float y = PlayerPrefs.GetFloat("CheckpointY");
            lastCheckpointPosition = new Vector2(x, y);
        }

        transform.position = lastCheckpointPosition;
    }

    public void RespawnAtCheckpoint()
    {
        if (stateMachine.currentState == deadState)
        {
            // Gerekli değişkenleri sıfırla
            rb.linearVelocity = Vector2.zero;
            
            // Eğer aktif bir checkpoint varsa oraya, yoksa spawn noktasına ışınla
            if (PlayerPrefs.GetInt("CheckpointActivated", 0) == 1)
            {
                // Checkpoint'ten devam et
                transform.position = lastCheckpointPosition;
                
                // Item ve envanter durumlarını yükle
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
            
            // Can ve mana değerlerini yenile
            stats.currentHealth = stats.maxHealth.GetValue();
            stats.currentMana = stats.maxMana.GetValue();
            
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
   

    public void ResetPlayerFacing()
    {
        transform.rotation = Quaternion.identity;
        facingdir = 1;
        facingright = true;
    }

    
   
   
    protected override void Update()
    {
        base.Update();

        // Hızlı düşüş kontrolü
        if (rb.linearVelocity.y < -10f) // Eşik değerini düşürdüm
        {
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            
            if (IsGroundDetected())
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
                rb.collisionDetectionMode = CollisionDetectionMode2D.Discrete;
            }
        }

        stateMachine.currentState.Update();
        
        

        // Her frame'de referansları güncelle
        UpdateWeaponReferences();

        // Bumerang cooldown'unu güncelle
        if (boomerangCooldownTimer > 0)
        {
            boomerangCooldownTimer -= Time.deltaTime;
        }

        // Spell2 için sürekli mana tüketimi
        if (isChargingFire && stateMachine.currentState is PlayerSpell2State spell2State)
        {
            // Sadece minimum şarj süresinden sonra mana tüket
            if (spell2State.GetCurrentChargeTime() >= PlayerSpell2State.MIN_CHARGE_TIME)
            {
                float manaDrainThisFrame = spell2ManaDrainPerSecond * Time.deltaTime;
                if (HasEnoughMana(manaDrainThisFrame))
                {
                    UseMana(manaDrainThisFrame);
                }
                else
                {
                    StopFireSpell();
                }
            }
        }

        // C tuşuna basıldığında manayı doldur
        if (Input.GetKeyDown(KeyCode.C))
        {
            RefillMana();
        }

        CheckForDashInput();
        CheckForGroundDashInput();
        CheckForSpellInput();

        

       

        // Interaction kontrolü
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

    public void AnimationFinishTrigger() => stateMachine.currentState.AnimationFinishTrigger();

    public void StartBoomerangKnockbackCoroutine()
    {
        StartCoroutine(HitKnockback(boomerangCatchForce));
    }

     public void StartStunnedKnockbackCoroutine()
    {
        StartCoroutine(HitKnockback(stunKnocback));
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


    public override void Die()
    {
        base.Die();
        stateMachine.ChangeState(deadState);
        
        // Artık coroutine kullanılmayacak, respawn işlemi animasyon bitiminde PlayerDeadState içinde yapılacak
    }

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

    private IEnumerator InvulnerabilityCoroutine()
    {
        isInvulnerable = true;
        // Flash efektini invulnerability süresi boyunca çalıştır
        StartCoroutine(entityFX.FlashFX(invulnerabilityDuration));
        yield return new WaitForSeconds(invulnerabilityDuration);
        isInvulnerable = false;
    }


    #region DrawGizmosAndTriggerEvents

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireCube(attackCheck.position,attackSize);
    }

    private void OnTriggerEnter2D(Collider2D other)
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
   

    private void CheckForSpellInput()
    {
        // Eğer spellbook aktif değilse büyü kullanamaz
        if (!CanCastSpells())
            return;

        // Spell1 kontrolü
        if (playerInput.spell1Input && HasEnoughMana(spell1ManaCost))
        {
            UseMana(spell1ManaCost);
            stateMachine.ChangeState(spell1State);
        }
        // Spell2 kontrolü
        else if (playerInput.spell2Input && HasEnoughMana(spell2ManaDrainPerSecond * Time.deltaTime))
        {
            StartFireSpell();
        }
        else if (!playerInput.spell2Input && isChargingFire)
        {
            StopFireSpell();
        }
    }

    private void UpdateWeaponReferences()
    {
        // Tüm silah referanslarını güncelle (aktif veya inaktif)
        boomerangWeapon = GetComponentInChildren<BoomerangWeaponStateMachine>(true);
        spellbookWeapon = GetComponentInChildren<SpellbookWeaponStateMachine>(true);
    }

    public void SpellOneTrigger()
    {
        StartCoroutine(CastIceShards());
    }

    private IEnumerator CastIceShards()
    {
        Vector3[] spawnPositions = spell1State.GetSpawnPositions();

        for (int i = 0; i < shardCount; i++)
        {
            Instantiate(iceShardPrefab, spawnPositions[i], Quaternion.identity);
            yield return new WaitForSeconds(delayBetweenShards);
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

    private void RefillMana()
    {
        stats.currentMana = stats.maxMana.GetValue();
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
}

