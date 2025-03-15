using UnityEngine;

public class PlayerCatchBoomerangState : PlayerState
{
    public PlayerCatchBoomerangState(Player _player, PlayerStateMachine _stateMachine, string _animBoolName) : base(_player, _stateMachine, _animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();
        // Yakalama animasyonunu başlat
        
    }

    public override void Update()
    {
        base.Update();
        
        player.SetZeroVelocity();
        // Animasyon bittiğinde idle state'e geç
        if (triggerCalled)
        {
            stateMachine.ChangeState(player.idleState);
        }
        
    }

    public override void Exit()
    {
        base.Exit();
        // Animasyonu kapat
        
    }
}
