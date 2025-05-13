using UnityEngine;

public class Enemy_Bear : Enemy
{

    #region States

    public Bear_IdleState idleState { get; private set; }
    public Bear_MoveState moveState { get; private set; }
    public Bear_ChaseState chaseState { get; private set; }
    public Bear_AttackState attackState { get; private set; }
    public Bear_DeathState deathState { get; private set; }
    
    
    
    

    #endregion


    [Header("AttackInfo")]
    
    public float attackDistance = 2f;
    public float attackCooldown = 2f;
    public float battleTime = 10f;
    public Transform attackCheck;
    public Vector2 attackSize;
    
    
    
    [HideInInspector] public float lastTimeAttacked;

    
    protected override void Awake()
    {
        base.Awake();
        
        SetupDefaultFacingDir(1);
        
        idleState = new Bear_IdleState(this,stateMachine,"Idle",this);
        moveState = new Bear_MoveState(this,stateMachine,"Move",this);  
        chaseState = new Bear_ChaseState(this,stateMachine,"Chase",this);
        attackState = new Bear_AttackState(this,stateMachine,"Attack",this);
        deathState = new Bear_DeathState(this,stateMachine,"Death",this);
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
        
        stateMachine.ChangeState(deathState);
    }
    
    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position,new Vector3(transform.position.x + attackDistance*facingdir,transform.position.y));
        
        Gizmos.DrawWireCube(attackCheck.position,attackSize);
    }
}
