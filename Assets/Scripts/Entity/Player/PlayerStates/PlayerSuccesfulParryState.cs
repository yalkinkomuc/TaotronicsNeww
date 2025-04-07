using UnityEngine;

public class PlayerSuccesfulParryState : PlayerState
{
    private float parryInvulnerabilityDuration = 0.8f; // Başarılı parry sonrası hasar alma koruması süresi
    
    public PlayerSuccesfulParryState(Player _player, PlayerStateMachine _stateMachine, string _animBoolName) : base(_player, _stateMachine, _animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();
        
        stateTimer = 0.6f; // Başarılı parry animasyonu süresi
        
        Debug.Log("i enter scfparry");
        
        // Block durumunda hasar almayı engelle
        player.stats.MakeInvincible(true);
        
        // Yeni bir saldırı başladığını belirt (hit listesini temizler)
        player.StartNewAttack();
        
        // En yakın parry penceresi açık olan düşmanı bul ve parry yap
        ProcessSuccessfulParry();
    }

    public override void Update()
    {
        base.Update();
        
        Debug.Log("im in scfparry");

        if (stateTimer < 0f)
        {
            stateMachine.ChangeState(player.idleState);
        }
        
        player.SetZeroVelocity();
    }

    public override void Exit()
    {
        base.Exit();
        
        // Block durumundan çıkınca invincible durumunu kaldır
        player.stats.MakeInvincible(false);
        
        // Başarılı parry sonrası invulnerability ver
        player.SetTemporaryInvulnerability(parryInvulnerabilityDuration);
        
        Debug.Log("i exit scfparry");
    }
    
    private void ProcessSuccessfulParry()
    {
        // Oyuncunun etrafındaki tüm düşmanları kontrol et (parry yarıçapı içinde)
        Collider2D[] colliders = Physics2D.OverlapCircleAll(player.transform.position, player.parryRadius, player.passableEnemiesLayerMask);
        
        bool foundParryableEnemy = false;
        
        foreach (var col in colliders)
        {
            // Elite Skeleton kontrolü
            EliteSkeleton_Enemy eliteSkeleton = col.GetComponent<EliteSkeleton_Enemy>();
            
            if (eliteSkeleton != null && eliteSkeleton.isParryWindowOpen)
            {
                // Bu düşmana zaten parry yaptık mı kontrol etmeye gerek yok
                // Çünkü başarılı parry state'ine girdiysen zaten yapacaksın
                
                // Bu düşmanı vurulmuş olarak işaretle
                player.MarkEntityAsHit(eliteSkeleton);
                
                // Parry başarılı olduğunu logla
                Debug.Log("Parry successful!");
                
                // Düşmana parry bilgisini bildir
                eliteSkeleton.GetParried();
                
                foundParryableEnemy = true;
                break;
            }
        }
        
        // Eğer parry yapılacak düşman bulunamadıysa (çok nadir bir durum), idle durumuna dön
        if (!foundParryableEnemy)
        {
            Debug.LogWarning("No parryable enemy found in SuccessfulParryState!");
            stateTimer = 0; // State'den hızlıca çıkmasını sağla
        }
    }
}
