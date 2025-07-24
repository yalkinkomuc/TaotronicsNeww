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

        if (!player.IsGroundDetected() )
        {
            stateMachine.ChangeState(player.airState);
        }
        
        if (player.playerInput.jumpInput&&player.IsGroundDetected())
        {
            stateMachine.ChangeState(player.airState);
        }

        if (player.playerInput.crouchInput && player.IsGroundDetected())
        {
            stateMachine.ChangeState(player.crouchState);
        }
        
        if (player.playerInput.attackInput)
        {
            // Hangi silahın aktif olduğunu kontrol et
            if (IsHammerActive())
            {
                stateMachine.ChangeState(player.hammerAttackState);
            }
            else
            {
                stateMachine.ChangeState(player.attackState);
            }
            
        }

       

        if ((player.playerInput.boomerangInput) && player.CanThrowBoomerang())
        {
            stateMachine.ChangeState(player.throwBoomerangState);
        }
        
        if (Input.GetKeyDown(KeyCode.X) && player.IsGroundDetected())
        {
            stateMachine.ChangeState(player.voidState);
        }

        

        // if(Input.GetKeyDown(KeyCode.R))
        // {
        //     stateMachine.ChangeState(player.spell1State);
        // }
        //
        // if (Input.GetKeyDown(KeyCode.T))
        // {
        //     stateMachine.ChangeState(player.spell2State);
        // }
    }
    
    private bool IsHammerActive()
{
    return (player.hammer != null && player.hammer.gameObject.activeInHierarchy) ||
           (player.iceHammer != null && player.iceHammer.gameObject.activeInHierarchy);
}
}
