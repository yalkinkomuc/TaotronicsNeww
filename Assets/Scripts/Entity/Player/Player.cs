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
    private bool isInvulnerable = false;

    #region States
    
    public PlayerStateMachine stateMachine;
    public PlayerIdleState idleState { get; private set; }
    public PlayerMoveState moveState { get; private set; }
    public PlayerJumpState jumpState { get; private set; }
    
    public PlayerDashState dashState { get; private set; }
    public PlayerAirState airState { get; private set; }
    public PlayerCrouchState crouchState { get; private set; }
    public PlayerGroundDashState groundDashState { get; private set; }
    public PlayerAttackState attackState {get;private set;}
    public PlayerCrouchAttackState crouchAttackState {get;private set;}
    public PlayerDeadState deadState {get;private set;}
    public PlayerStunnedState stunnedState {get;private set;}
    public PlayerParryState parryState {get;private set;}
    
    public PlayerJumpAttackState jumpAttackState {get;private set;}
    
    public PlayerThrowBoomerangState throwBoomerangState {get;private set;}
    public PlayerCatchBoomerangState catchBoomerangState {get;private set;}
    #endregion
    
    private IInteractable currentInteractable;

    [Header("Crouch Attack")]
    public Vector2 crouchAttackOffset; // Örnek: (0, 0.2f) 

    protected override void Awake()
    {
        base.Awake();
        playerInput = new PCInput();
        
        stateMachine = new PlayerStateMachine();
        
        idleState = new PlayerIdleState(this,stateMachine,"Idle");
        moveState = new PlayerMoveState(this,stateMachine,"Move");
        jumpState = new PlayerJumpState(this,stateMachine,"Jump");
        dashState = new PlayerDashState(this,stateMachine,"Dash");
        airState = new PlayerAirState(this,stateMachine,"Jump");
        crouchState = new PlayerCrouchState(this,stateMachine,"Crouch");
        groundDashState = new PlayerGroundDashState(this,stateMachine,"GroundDash");
        attackState = new PlayerAttackState(this, stateMachine, "Attack");
        crouchAttackState = new PlayerCrouchAttackState(this, stateMachine, "GroundAttack");
        deadState = new PlayerDeadState(this,stateMachine,"Death");
        stunnedState = new PlayerStunnedState(this,stateMachine,"Stunned");
        parryState = new PlayerParryState(this,stateMachine,"Parry");
        jumpAttackState = new PlayerJumpAttackState(this, stateMachine, "JumpAttack");
        throwBoomerangState = new PlayerThrowBoomerangState(this, stateMachine, "ThrowBoomerang");
        catchBoomerangState = new PlayerCatchBoomerangState(this, stateMachine, "CatchBoomerang");




    }

    protected override void Start()
    {
        base.Start();
        stateMachine.Initialize(idleState);
        capsuleCollider = GetComponent<CapsuleCollider2D>();

        normalOffset = capsuleCollider.offset;
        crouchOffset = new Vector2(normalOffset.x, normalOffset.y-0.1f);
        
        
        
    }

    
   
   
    protected override void Update()
    {
        
        base.Update();
        stateMachine.currentState.Update();

        CheckForDashInput();
        CheckForGroundDashInput();

        if (Input.GetKeyDown(KeyCode.X))
        {
            stateMachine.ChangeState(stunnedState);
            StartCoroutine("HitKnockback");
        }

        // Interaction kontrolü
        if (playerInput.interactionInput && currentInteractable != null)
        {
            currentInteractable.Interact();
        }

       
    }

    public void AnimationFinishTrigger() => stateMachine.currentState.AnimationFinishTrigger();

    public void ThrowBoomerang()
    {
        Instantiate(boomerangPrefab,boomerangLaunchPoint.position,Quaternion.identity);
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
  
}

