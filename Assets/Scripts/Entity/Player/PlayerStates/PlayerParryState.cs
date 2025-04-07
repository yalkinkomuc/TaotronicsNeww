using UnityEngine;

public class PlayerParryState : PlayerState
{
    private float parryInvulnerabilityDuration = 0.8f; // Başarılı parry sonrası hasar alma koruması süresi
    
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

        // Q tuşu bırakıldığında idle'a dön
        if (!player.playerInput.parryInput)
        {
            stateMachine.ChangeState(player.idleState);
            return;
        }
        
        player.SetZeroVelocity();
        
        // EliteSkeleton'un parry penceresi içinde olup olmadığını kontrol et
        CheckForSuccessfulParry();
    }

    public override void Exit()
    {
        base.Exit();
        
        // Block durumundan çıkınca invincible durumunu kaldır
        player.stats.MakeInvincible(false);
    }
    
    private void CheckForSuccessfulParry()
    {
        // Oyuncunun etrafındaki tüm düşmanları kontrol et (parry yarıçapı içinde)
        Collider2D[] colliders = Physics2D.OverlapCircleAll(player.transform.position, player.parryRadius, player.passableEnemiesLayerMask);
        
        foreach (var col in colliders)
        {
            // Elite Skeleton kontrolü
            EliteSkeleton_Enemy eliteSkeleton = col.GetComponent<EliteSkeleton_Enemy>();
            
            if (eliteSkeleton != null && eliteSkeleton.isParryWindowOpen)
            {
                // Parry penceresi içindeyken başarılı parry state'ine geç
                stateMachine.ChangeState(player.succesfulParryState);
                return;
            }
        }
    }
}
