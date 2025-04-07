using UnityEngine;

public class PlayerParryState : PlayerState
{
    //[SerializeField] private float parryRadius = 2f; // Parry etki yarıçapı
    private bool parrySuccessful = false; // Parry başarılı oldu mu?
    private float parryInvulnerabilityDuration = 0.8f; // Başarılı parry sonrası hasar alma koruması süresi
    private float parryCheckCooldown = 0.05f; // Parry kontrolü arasındaki süre
    private float lastParryCheckTime = 0f; // Son parry kontrolü zamanı
    
    public PlayerParryState(Player _player, PlayerStateMachine _stateMachine, string _animBoolName) : base(_player, _stateMachine, _animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();

        stateTimer = 0.2f; // Parry penceresi (saniye)
        parrySuccessful = false; // Parry başarı durumunu sıfırla
        lastParryCheckTime = 0f; // Son parry kontrolü zamanını sıfırla
        
        // Yeni bir saldırı başladığını belirt (hit listesini temizler)
        player.StartNewAttack();
    }

    public override void Update()
    {
        base.Update();

        if (stateTimer < 0f)
        {
            stateMachine.ChangeState(player.idleState);
        }
        
        player.SetZeroVelocity();
        
        // Belirli aralıklarla sürekli parry kontrolü yap
        if (Time.time >= lastParryCheckTime + parryCheckCooldown)
        {
            lastParryCheckTime = Time.time;
            CheckParryableEnemies();
            
            // Ayrıca player'ın CheckForParryableEnemies metodunu da çağır
            player.CheckForParryableEnemies();
        }
    }

    public override void Exit()
    {
        base.Exit();
        
        // Başarılı parry sonrası invulnerability ver
        if (parrySuccessful)
        {
            player.SetTemporaryInvulnerability(parryInvulnerabilityDuration);
        }
    }
    
    private void CheckParryableEnemies()
    {
        // Oyuncunun etrafındaki tüm düşmanları kontrol et (parry yarıçapı içinde)
        Collider2D[] colliders = Physics2D.OverlapCircleAll(player.transform.position, player.parryRadius, player.passableEnemiesLayerMask);
        
        foreach (var col in colliders)
        {
            // Elite Skeleton kontrolü
            EliteSkeleton_Enemy eliteSkeleton = col.GetComponent<EliteSkeleton_Enemy>();
            
            if (eliteSkeleton != null && eliteSkeleton.isParryWindowOpen)
            {
                // Bu düşmana zaten parry yaptık mı kontrol et
                if (player.HasHitEntity(eliteSkeleton))
                    continue;
                
                // Bu düşmanı vurulmuş olarak işaretle
                player.MarkEntityAsHit(eliteSkeleton);
                
                // Parry başarılı!
                parrySuccessful = true;
                Debug.Log("Parry successful!");
                
                // Düşmana parry bilgisini bildir
                eliteSkeleton.GetParried();
                
                // Parry başarılı olduğunda süreyi uzat
                stateTimer = 0.6f;
            }
        }
    }
}
