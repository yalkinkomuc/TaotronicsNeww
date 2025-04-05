using UnityEngine;

public class PlayerCrouchState : PlayerState
{

    
   
    public PlayerCrouchState(Player _player, PlayerStateMachine _stateMachine, string _animBoolName) : base(_player, _stateMachine, _animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();
        
       player.EnterCrouchMode();
    }

    public override void Exit()
    {
        base.Exit();
        player.ExitCrouchMode();
       
    }

    public override void Update()
    {
        base.Update();
        
        player.SetZeroVelocity();

        
        if (player.playerInput.crouchInputReleased)
        {
            stateMachine.ChangeState(player.idleState);
        }
       

        if (player.playerInput.attackInput)
        {
            stateMachine.ChangeState(player.crouchAttackState);
        }
       
    }

   
  
}
