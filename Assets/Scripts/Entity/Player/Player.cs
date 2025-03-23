using System;
using UnityEngine;
using UnityEngine.Serialization;
using System.Collections;

public class Player : Entity
{
   
    
    public IPlayerInput playerInput {get;private set;}


    [Header("AttackDetails")] 
    public Vector2[] attackMovement;

    public Transform attackCheck;
    public float attackCheckRadius;
    public Vector2 attackSize;
    
    [SerializeField] private GameObject boomerangPrefab;
    [SerializeField] private Transform boomerangLaunchPoint;
    [SerializeField] public Vector2 boomerangCatchForce;
    
    [Header("Collider")]
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
    
    #endregion
    
    private IInteractable currentInteractable;

    [Header("Crouch Attack")]
    public Vector2 crouchAttackOffset; // Örnek: (0, 0.2f) 

    [Header("Weapon References")]
    public BoomerangWeaponStateMachine boomerangWeapon;
    public SpellbookWeaponStateMachine spellbookWeapon;

    [Header("Boomerang Settings")]
    [SerializeField] private float boomerangCooldown = 2f;
    private float boomerangCooldownTimer;

    [Header("Spell Settings")]
    [SerializeField] private GameObject iceShardPrefab;
    [SerializeField] private float spellSpacing = 1f; // Buz parçaları arası mesafe
    [SerializeField] private float delayBetweenShards = 0.1f;
    [SerializeField] private int shardCount = 3;

    [Header("Fire Spell Settings")]
    public GameObject fireSpellPrefab;
    public Transform fireSpellPoint;

    [Header("Fire Spell Settings")]
    private bool isChargingFire = false;

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


    }

    protected override void Start()
    {
        base.Start();
        ResetPlayerFacing();
        
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
    }

    public void ResetPlayerFacing()
    {
        transform.rotation = Quaternion.identity;
        facingdir = 1;
        facingright = true;
    }

    
   
   
    protected override void Update()
    {
        
        base.Update();
        stateMachine.currentState.Update();

        // Her frame'de referansları güncelle
        UpdateWeaponReferences();

        // Bumerang cooldown'unu güncelle
        if (boomerangCooldownTimer > 0)
        {
            boomerangCooldownTimer -= Time.deltaTime;
        }

        CheckForDashInput();
        CheckForGroundDashInput();
        CheckForSpellInput();

        if (Input.GetKeyDown(KeyCode.X))
        {
            stateMachine.ChangeState(stunnedState);
            StartStunnedKnockbackCoroutine();
        }

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

        // Spell state kontrolü - boomerang'daki gibi
        if (playerInput.spell1Input && spellbookWeapon != null && spellbookWeapon.gameObject.activeInHierarchy && IsGroundDetected())
        {
            stateMachine.ChangeState(spell1State);
        }
        else if (playerInput.spell2Input && spellbookWeapon != null && spellbookWeapon.gameObject.activeInHierarchy&& IsGroundDetected())
        {
            StartFireSpell();
        }
        else if (!playerInput.spell2Input && isChargingFire)
        {
            StopFireSpell();
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
        // if (playerInput.spell1Input )
        // {
        //     stateMachine.ChangeState(spell1State);
        // }
        // // Spell2 kontrolü
        // else if (playerInput.spell2Input )
        // {
        //     stateMachine.ChangeState(spell2State);
        // }
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
        float xOffset = 1f * facingdir;
        float startX = transform.position.x + xOffset;
        float spawnY = transform.position.y + 0.3f;

        for (int i = 0; i < shardCount; i++)
        {
            Vector3 spawnPos = new Vector3(
                startX + (spellSpacing * i * facingdir),
                spawnY,
                transform.position.z
            );

            // Sadece instantiate et, collider'ı animasyon event ile aktifleştirelim
            Instantiate(iceShardPrefab, spawnPos, Quaternion.identity);

            yield return new WaitForSeconds(delayBetweenShards);
        }
    }

    private void StartFireSpell()
    {
        if (!isChargingFire)
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
}

