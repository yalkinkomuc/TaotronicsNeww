using UnityEngine;

public class Enemy_ArcherSkeleton : Enemy
{
    #region States

   public SkeletonArcher_IdleState idleState { get; private set; }
   public SkeletonArcher_MoveState moveState { get; private set; }
   public SkeletonArcher_BattleState battleState { get; private set; }
   public SkeletonArcher_AttackState attackState { get; private set; }
   public SkeletonArcher_DeadState deadState { get; private set; }
    
    #endregion

    //[SerializeField] private BowWeaponStateMachine bowWeapon;

    [Header("Archer Settings")]
    
   
    
    [SerializeField] private Transform arrowSpawnPoint;
    [SerializeField] private GameObject arrowPrefab;
    [SerializeField] private float attackCooldown = 2f;
    private float attackCooldownTimer;

    private float losePlayerTimer = 3f;
    private float currentLosePlayerTimer;
    private bool wasPlayerVisible = false;

    private bool isSummoned = false;

    protected override void Awake()
    {
        base.Awake();
        
        SetupDefaultFacingDir(1);
        
        idleState = new SkeletonArcher_IdleState(this, stateMachine, "Idle",this);
        moveState = new SkeletonArcher_MoveState(this, stateMachine, "Move", this);
        battleState = new SkeletonArcher_BattleState(this, stateMachine, "Move",this);
        attackState = new SkeletonArcher_AttackState(this, stateMachine, "Shoot",this);
        deadState = new SkeletonArcher_DeadState(this, stateMachine, "Death",this);
        
       
    }

    protected override void Start()
    {
        base.Start();
        
      
        if (isSummoned)
        {
            stateMachine.Initialize(battleState);
        }
        else
        {
            stateMachine.Initialize(idleState);
        }
        
        currentLosePlayerTimer = losePlayerTimer;
    }

    protected override void Update()
    {
        base.Update();
        
        if (attackCooldownTimer > 0)
            attackCooldownTimer -= Time.deltaTime;
        
        // Debug.Log(IsTooCloseToPlayer());
    }

    public override void Die()
    {
        base.Die();
        
        stateMachine.ChangeState(deadState);
    }

    public bool CanSeePlayer()
    {
        if (player != null)
        {
            float directionToPlayer = player.transform.position.x - transform.position.x;
            bool isPlayerInFront = (directionToPlayer > 0 && facingdir == 1) || 
                                 (directionToPlayer < 0 && facingdir == -1);

            float distanceToPlayer = Vector2.Distance(transform.position, player.transform.position);
            bool isInRange = distanceToPlayer <= detectDistance;
            
            Vector2 direction = (player.transform.position - transform.position).normalized;
            RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, detectDistance, whatIsGround);
            bool noObstacles = hit.collider == null;

            bool canCurrentlySeePlayer = isPlayerInFront && isInRange && noObstacles;

            if (canCurrentlySeePlayer)
            {
                wasPlayerVisible = true;
                currentLosePlayerTimer = losePlayerTimer;
                return true;
            }
            else if (wasPlayerVisible)
            {
                currentLosePlayerTimer -= Time.deltaTime;
                if (currentLosePlayerTimer <= 0)
                {
                    wasPlayerVisible = false;
                    return false;
                }
                return true;
            }
        }

        wasPlayerVisible = false;
        return false;
    }

   

    

    public bool CanAttack()
    {
        return attackCooldownTimer <= 0;
    }

    public void ShootArrow()
    {
        if (arrowPrefab != null && arrowSpawnPoint != null && base.player != null)
        {
            GameObject arrow = Instantiate(arrowPrefab, arrowSpawnPoint.position, Quaternion.identity);
            Vector2 direction = (base.player.transform.position - arrowSpawnPoint.position).normalized;
            direction.y += 0.035f;
            direction = direction.normalized;
            
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            arrow.transform.rotation = Quaternion.Euler(0, 0, angle);
            
            SkeletonArrow arrowComponent = arrow.GetComponent<SkeletonArrow>();
            if (arrowComponent != null)
            {
                arrowComponent.SetDirection(direction);
            }

            attackCooldownTimer = attackCooldown;
        }
    }

   
  

    // Animation Event'ten çağrılacak
   

    public void InitializeAsSummoned()
    {
        isSummoned = true;
    }
}
