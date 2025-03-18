using UnityEngine;

public class PlayerSpell2State : PlayerState
{
    public PlayerSpell2State(Player _player, PlayerStateMachine _stateMachine, string _animBoolName) : base(_player, _stateMachine, _animBoolName)
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

        player.SetZeroVelocity();

        if(triggerCalled)
        {
            stateMachine.ChangeState(player.idleState);
        }
    }
}
