using UnityEngine;

public class PlayerParryState : PlayerState
{
    private float blockDuration = 0.3f; // Block durumu süresi (basılı tutulmazsa)
    private float lastCheckTime = 0f; // Son kontrol zamanı
    private float checkInterval = 0.05f; // Kontrol aralığı
    
    public PlayerParryState(Player _player, PlayerStateMachine _stateMachine, string _animBoolName) : base(_player, _stateMachine, _animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();
        
        // Block durumunda hasar almayı engelle
        player.stats.MakeInvincible(true);
        
        // Parry penceresini ilk giriş anında kontrol et (sadece Q'ya kısa basıldığında)
        if (player.playerInput.parryInput && !player.playerInput.blockInput)
        {
            CheckForParryOpportunity();
        }
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
        
        // Belirli aralıklarla parry fırsatını kontrol et (sadece Q'ya kısa basıldığında)
        if (player.playerInput.parryInput && Time.time >= lastCheckTime + checkInterval)
        {
            lastCheckTime = Time.time;
            CheckForParryOpportunity();
        }
    }

    public override void Exit()
    {
        base.Exit();
        
        // Block durumundan çıkınca invincible durumunu kaldır
        player.stats.MakeInvincible(false);
    }
    
    private void CheckForParryOpportunity()
    {
        // Parry yarıçapında düşmanları kontrol et
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
