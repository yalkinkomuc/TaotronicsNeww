using UnityEngine;

public class PlayerIdleState : PlayerGroundedState
{
    public PlayerIdleState(Player _player, PlayerStateMachine _stateMachine, string _animBoolName) : base(_player, _stateMachine, _animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();
        
        rb.bodyType = RigidbodyType2D.Kinematic;
    }

   

    public override void Exit()
    {
        base.Exit();
        
        rb.bodyType = RigidbodyType2D.Dynamic;
    }
    
    public override void Update()
    {
        base.Update();
        
        //Debug.Log("im in idle state");
        
        player.SetZeroVelocity();

        if (xInput !=0)
        {
            stateMachine.ChangeState(player.moveState);
        }

        if (!player.IsGroundDetected()&&player.playerInput.attackInput)
        {
            stateMachine.ChangeState(player.jumpAttackState);
        }

        
        
        
    }
}
