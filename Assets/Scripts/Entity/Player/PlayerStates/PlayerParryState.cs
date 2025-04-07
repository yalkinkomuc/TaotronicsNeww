using UnityEngine;

public class PlayerParryState : PlayerState
{
    [SerializeField] private float parryRadius = 2f; // Parry etki yarıçapı
    private bool hasCheckedParry = false; // Parry kontrolü yapıldı mı
    private bool parrySuccessful = false; // Parry başarılı oldu mu?
    private float parryInvulnerabilityDuration = 0.8f; // Başarılı parry sonrası hasar alma koruması süresi
    
    public PlayerParryState(Player _player, PlayerStateMachine _stateMachine, string _animBoolName) : base(_player, _stateMachine, _animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();

        stateTimer = 0.5f; // Parry penceresi (saniye)
        hasCheckedParry = false; // Parry kontrol durumunu sıfırla
        parrySuccessful = false; // Parry başarı durumunu sıfırla
    }

    public override void Update()
    {
        base.Update();

        if (stateTimer < 0f)
        {
            stateMachine.ChangeState(player.idleState);
        }
        
        player.SetZeroVelocity();
        
        // Parry kontrolünü sadece bir kez yap
        if (!hasCheckedParry)
        {
            hasCheckedParry = true; // İşaretleme yapıldı
            CheckParryableEnemies();
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
                // Parry başarılı!
                parrySuccessful = true;
                Debug.Log("Parry successful!");
                
                // Düşmana parry bilgisini bildir
                eliteSkeleton.GetParried();
                
                // Parry başarılı olduğunda süreyi uzat
                stateTimer = 0.6f;
                
                // İlk başarılı parry sonrası diğerlerini kontrol etmeyi bırak
                break;
            }
        }
    }
}
