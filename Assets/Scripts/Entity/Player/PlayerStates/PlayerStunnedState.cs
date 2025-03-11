using UnityEngine;

public class PlayerStunnedState : PlayerState
{
    public PlayerStunnedState(Player _player, PlayerStateMachine _stateMachine, string _animBoolName) : base(_player, _stateMachine, _animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();

        stateTimer = 1f;
    }

    public override void Update()
    {
        base.Update();
        
        player.SetZeroVelocity();
        
        

        if (stateTimer < 0f)
        {
            stateMachine.ChangeState(player.idleState);
        }
    }

    public override void Exit()
    {
        base.Exit();
    }
}
