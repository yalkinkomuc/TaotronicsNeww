using UnityEngine;

public class PlayerAirState : PlayerState
{
    
    public PlayerAirState(Player _player, PlayerStateMachine _stateMachine, string _animBoolName) : base(_player, _stateMachine, _animBoolName)
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

        player.SetVelocity(xInput *player.moveSpeed * .75f, rb.linearVelocity.y);

        if (UserInput.WasBoomerangPressed && player.CanThrowBoomerang())
        {
            stateMachine.ChangeState(player.throwBoomerangState);
        }
        
        if(rb.linearVelocity.y ==0)
            stateMachine.ChangeState(player.idleState);
        
        //player.SetVelocity(xInput*player.moveSpeed,rb.linearVelocity.y);

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

    private bool IsHammerActive()
    {
        return player.hammer != null && player.hammer.gameObject.activeInHierarchy;
    }
    
    private bool IsIceHammerActive()
    {
        return player.iceHammer != null && player.iceHammer.gameObject.activeInHierarchy;
    }
}
