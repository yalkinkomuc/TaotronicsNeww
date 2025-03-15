using UnityEngine;

public class PlayerJumpAttackState : PlayerState
{
    public PlayerJumpAttackState(Player _player, PlayerStateMachine _stateMachine, string _animBoolName) : base(_player, _stateMachine, _animBoolName)
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
        
        
        if (triggerCalled)
        {
            stateMachine.ChangeState(player.idleState);
        }

        if (player.playerInput.attackInput)
        {
            stateMachine.ChangeState(player.JumpAttackState);
        }
    }

    public override void Exit()
    {
        base.Exit();
    }
}
