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

        
        if (UserInput.WasCrouchReleased)
        {
            stateMachine.ChangeState(player.idleState);
        }
       

        if (UserInput.WasAttackPressed)
        {
            stateMachine.ChangeState(player.crouchAttackState);
        }
       
    }

   
  
}
