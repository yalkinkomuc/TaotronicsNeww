using UnityEngine;

public class PlayerIdleState : PlayerGroundedState
{
    public PlayerIdleState(Player _player, PlayerStateMachine _stateMachine, string _animBoolName) : base(_player, _stateMachine, _animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();
    }

   

    public override void Exit()
    {
        base.Exit();
        
    }
    
    public override void Update()
    {
        base.Update();
        
        //Debug.Log("im in idle state");

        if (xInput !=0)
        {
            player.stateMachine.ChangeState(player.moveState);
        }

        
        
        
    }
}
