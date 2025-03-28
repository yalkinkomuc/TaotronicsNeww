using System;
using UnityEngine;

public class Boar_Enemy : Enemy
{

    #region States

    public Boar_IdleState idleState { get; private set; }
    public Boar_MoveState moveState { get; private set; }
    public Boar_ChaseState chaseState { get; private set; }
    
    

    #endregion
    protected override void Awake()
    {
        base.Awake();
        
        SetupDefaultFacingDir(1);

        idleState = new Boar_IdleState(this, stateMachine, "Idle", this);
        moveState = new Boar_MoveState(this, stateMachine, "Move", this);
        chaseState = new Boar_ChaseState(this, stateMachine, "Chase", this);
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
