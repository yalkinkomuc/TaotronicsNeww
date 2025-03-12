using UnityEngine;
using UnityEngine.Rendering;

public class EliteSkeleton_Enemy : Enemy
{
    #region States

    
    public EliteSkeleton_IdleState idleState { get; private set; }
    public EliteSkeleton_MoveState moveState { get; private set; }
   
    public EliteSkeleton_BattleState battleState { get; private set; }
    public EliteSkeleton_AttackState attackState { get; private set; }
    public EliteSkeleton_DeadState deadState { get; private set; }
    #endregion


    [Header("AttackInfo")] 
    public float attackDistance;
    public float attackCooldown;
    public float battleTime;
    [HideInInspector] public float lastTimeAttacked;
    
    
    protected override void Awake()
    {
        base.Awake();
        
        SetupDefaultFacingDir(1);
        
      
        idleState = new EliteSkeleton_IdleState(this,stateMachine,"Idle",this);
        moveState = new EliteSkeleton_MoveState(this,stateMachine,"Move",this);
        battleState = new EliteSkeleton_BattleState(this,stateMachine,"Move",this);
        attackState = new EliteSkeleton_AttackState(this,stateMachine,"Attack",this);
        deadState = new EliteSkeleton_DeadState(this,stateMachine,"Death",this);
        
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
    }
}
