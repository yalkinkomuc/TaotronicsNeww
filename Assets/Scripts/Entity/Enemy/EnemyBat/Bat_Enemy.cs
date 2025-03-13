using UnityEngine;

public class Bat_Enemy : Enemy
{
    #region States

    public Bat_IdleState idleState { get; private set; }
    public Bat_MoveState moveState { get; private set; }
    public Bat_BattleState battleState { get; private set; }
    public Bat_DeadState deadState { get; private set; }

    #endregion


    protected override void Awake()
    {
        base.Awake();
        
        SetupDefaultFacingDir(-1);
        
        idleState = new Bat_IdleState(this,stateMachine,"Idle",this);
        moveState = new Bat_MoveState(this,stateMachine,"Move",this);
        battleState = new Bat_BattleState(this,stateMachine,"Move",this);
        deadState = new Bat_DeadState(this,stateMachine,"Death",this);
    }

    protected override void Start()
    {
        base.Start();
        
        stateMachine.Initialize(idleState);
    }

    public override void Die()
    {
        base.Die();
        
        stateMachine.ChangeState(deadState);
    }
    
    private void OnCollisionStay2D(Collision2D other)
    {
        Player player = other.gameObject.GetComponent<Player>();

        if (player != null)
        {
            player.Damage();
        }
    }
}
