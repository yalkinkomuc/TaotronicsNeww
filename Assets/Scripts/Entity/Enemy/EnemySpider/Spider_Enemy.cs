using System;
using UnityEngine;

public class Spider_Enemy : Enemy
{


    #region States

    public Spider_IdleState idleState { get; private set; }
    public Spider_MoveState moveState { get; private set; }
    public Spider_BattleState battleState { get; private set; }
    public Spider_DeadState deadState { get; private set; }
    

    #endregion
    
    protected override void Awake()
    {
        base.Awake();
        
        SetupDefaultFacingDir(1);
        
        idleState = new Spider_IdleState(this,stateMachine,"Idle",this);
        moveState = new Spider_MoveState(this,stateMachine,"Move",this);
        battleState = new Spider_BattleState(this,stateMachine,"Move",this);
        deadState = new Spider_DeadState(this,stateMachine,"Death",this);
    }

    protected override void Start()
    {
        base.Start();
        
        stateMachine.Initialize(idleState);

        Debug.Log("Test");
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
