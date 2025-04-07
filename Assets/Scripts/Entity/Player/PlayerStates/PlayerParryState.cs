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
        
        // Parry penceresi açık olan düşman var mı kontrol et
        CheckForSuccessfulParry();
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
                // Parry penceresi açıkken tam zamanında Q'ya basılmış, başarılı parry state'ine geç
                stateMachine.ChangeState(player.succesfulParryState);
                return;
            }
        }
    }
}
