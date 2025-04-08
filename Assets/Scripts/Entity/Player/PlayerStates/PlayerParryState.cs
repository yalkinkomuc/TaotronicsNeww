using UnityEngine;

public class PlayerParryState : PlayerState
{
    public PlayerParryState(Player _player, PlayerStateMachine _stateMachine, string _animBoolName) : base(_player, _stateMachine, _animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();

        stateTimer = .5f;
    }

    public override void Update()
    {
        base.Update();

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
