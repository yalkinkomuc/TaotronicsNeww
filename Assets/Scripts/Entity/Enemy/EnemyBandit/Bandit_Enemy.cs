using UnityEngine;

public class Bandit_Enemy : Enemy
{

    #region States

    public Bandit_IdleState idleState {get; private set;}
    public Bandit_MoveState moveState {get; private set;}
    public Bandit_BattleState battleState {get; private set;}
    public Bandit_AttackState attackState {get; private set;}
    public Bandit_DeadState deadState {get; private set;}
    
    

    #endregion
    
    
    [Header("AttackInfo")] 
    public float attackDistance;
    public float attackCooldown;
    public float battleTime;

    public Transform attackCheck;
    public Vector2 attackSize;

    
    [HideInInspector] public float lastTimeAttacked;
    
    
    [Header("Parry System")]
    [HideInInspector] public bool isParryWindowOpen = false; // Parry penceresi aktif mi?
    [HideInInspector] public float parryWindowDuration = 0.3f; // Parry penceresi süresi
    [SerializeField] public float parryStunDuration = 1.5f; // Parry sonrası düşmanın sersemleme süresi

    protected override void Awake()
    {
        base.Awake();
        
        SetupDefaultFacingDir(1);
        
        idleState = new Bandit_IdleState(this,stateMachine,"Idle",this);
        moveState = new Bandit_MoveState(this,stateMachine,"Move",this);
        battleState = new Bandit_BattleState(this,stateMachine,"Chase",this);
        attackState = new Bandit_AttackState(this,stateMachine,"Attack",this);
        deadState = new Bandit_DeadState(this,stateMachine,"Death",this);
        
    }

    protected override void Start()
    {
        base.Start();
        
        stateMachine.Initialize(idleState);
    }

    protected override void Update()
    {
        base.Update();
        
        
    }

    public override void Die()
    {
        base.Die();
        
        stateMachine.ChangeState(deadState);
    }
    
    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position,new Vector3(transform.position.x + attackDistance*facingdir,transform.position.y));
        
        // Parry penceresi aktifse farklı renk göster
        if (isParryWindowOpen)
        {
            Gizmos.color = Color.cyan;
        }
        else
        {
            Gizmos.color = Color.red;
        }
        
        Gizmos.DrawWireCube(attackCheck.position,attackSize);
    }
}
