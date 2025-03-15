using UnityEngine;

public class PlayerGroundedState : PlayerState
{
    public PlayerGroundedState(Player _player, PlayerStateMachine _stateMachine, string _animBoolName) : base(_player, _stateMachine, _animBoolName)
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

        if (!player.IsGroundDetected())
        {
            stateMachine.ChangeState(player.airState);
        }
        
        if (player.playerInput.jumpInput&&player.IsGroundDetected())
        {
            stateMachine.ChangeState(player.jumpState);
        }

        if (player.playerInput.crouchInput && player.IsGroundDetected())
        {
            stateMachine.ChangeState(player.crouchState);
        }
        
        if (player.playerInput.attackInput)
        {
            stateMachine.ChangeState(player.attackState);
        }

        if (player.playerInput.parryInput)
        {
            stateMachine.ChangeState(player.parryState);
        }

        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            stateMachine.ChangeState(player.throwBoomerangState);
        }
    }
}
