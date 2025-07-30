using UnityEngine;

public class PlayerJumpState : PlayerState
{
    public PlayerJumpState(Player _player, PlayerStateMachine _stateMachine, string _animBoolName) : base(_player, _stateMachine, _animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();
        
        // Ground dash'ten geçiş yapıldığında jump force uygulama
        if (stateMachine.currentState != player.groundDashState)
        {
            player.SetVelocity(rb.linearVelocity.x,player.jumpForce);
        }
    }

    public override void Update()
    {
        base.Update();
        
        // Ground dash'ten geçiş yapıldığında normal hareket kontrollerini devre dışı bırak
        if (stateMachine.currentState == player.groundDashState)
        {
            return;
        }
        
        player.SetVelocity(xInput *player.moveSpeed, rb.linearVelocity.y);
        
        if (UserInput.WasBoomerangPressed && player.CanThrowBoomerang())
        {
            stateMachine.ChangeState(player.throwBoomerangState);
        }

        if (rb.linearVelocity.y < 0)
        {
            stateMachine.ChangeState(player.airState);
        }
        
        if (UserInput.WasAttackPressed)
        {
            // Hangi silahın aktif olduğunu kontrol et
            if (IsHammerActive() || IsIceHammerActive())
            {
                stateMachine.ChangeState(player.hammerAttackState);
            }
            else
            {
                stateMachine.ChangeState(player.attackState);
            }
        }
        
    }

    public override void Exit()
    {
        base.Exit();
    }
    
    private bool IsHammerActive()
    {
        return player.hammer != null && player.hammer.gameObject.activeInHierarchy;
    }
    
    private bool IsIceHammerActive()
    {
        return player.iceHammer != null && player.iceHammer.gameObject.activeInHierarchy;
    }
}
