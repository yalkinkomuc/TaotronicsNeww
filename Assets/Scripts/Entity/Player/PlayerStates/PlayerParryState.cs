using UnityEngine;

public class PlayerParryState : PlayerState
{
    private float blockDuration = 0.3f; // Block durumu süresi (basılı tutulmazsa)
    
    public PlayerParryState(Player _player, PlayerStateMachine _stateMachine, string _animBoolName) : base(_player, _stateMachine, _animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();
        
        // Block durumunda hasar almayı engelle
        player.stats.MakeInvincible(true);
    }

    public override void Update()
    {
        base.Update();

        // Block tuşu bırakılırsa idle'a dön
        if (!player.playerInput.blockInput)
        {
            stateMachine.ChangeState(player.idleState);
            return;
        }
        
        player.SetZeroVelocity();
    }

    public override void Exit()
    {
        base.Exit();
        
        // Block durumundan çıkınca invincible durumunu kaldır
        player.stats.MakeInvincible(false);
    }
}
