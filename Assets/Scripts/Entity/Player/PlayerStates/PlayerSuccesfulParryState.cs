using UnityEngine;

public class PlayerSuccesfulParryState : PlayerState
{
    private float parryInvulnerabilityDuration = 0.8f; // Başarılı parry sonrası hasar alma koruması süresi
    private float animationDuration = 0.27f; // Animasyon süresini tam olarak belirt (Player_SuccesfulParry.anim'in süresine göre)
    
    [SerializeField] private GameObject parryEffectPrefab; // Başarılı parry efekti
    private Transform parryEffectSpawnPoint; // Efektin spawn edileceği nokta
    
    public PlayerSuccesfulParryState(Player _player, PlayerStateMachine _stateMachine, string _animBoolName) : base(_player, _stateMachine, _animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();
        
        // Animasyon süresine göre state süresini ayarla
        stateTimer = animationDuration;
        
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
        
//        Debug.Log("im in scfparry, timer: " + stateTimer);

        // Animasyon trigger'ı çağrıldıysa VEYA animasyon süresi bittiyse state'i değiştir
        if (triggerCalled || stateTimer <= 0)
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
        
        // Başarılı parry sonrası invulnerability ver
        player.SetTemporaryInvulnerability(parryInvulnerabilityDuration);
        
    }
    
    private void ProcessSuccessfulParry()
    {
        // Oyuncunun etrafındaki tüm düşmanları kontrol et (parry yarıçapı içinde)
        Collider2D[] colliders = Physics2D.OverlapCircleAll(player.transform.position, player.parryRadius, player.passableEnemiesLayerMask);
        
        foreach (var col in colliders)
        {
            // IParryable interface'ini implemente eden düşmanları bul
            IParryable parryableEnemy = col.GetComponent<IParryable>();
            Enemy enemy = col.GetComponent<Enemy>();
            
            // Eğer hem Enemy hem de IParryable ise parry işlemini yap
            if (parryableEnemy != null && enemy != null)
            {
                if (player.CheckAndParryEnemy(enemy))
                {
                    // Başarılı parry efekti oluştur
                    SpawnParryEffect(parryableEnemy.GetTransform().position);
                }
            }
        }
    }
    
    private void SpawnParryEffect(Vector3 enemyPosition)
    {
        // Eğer parry efekti prefab tanımlanmamışsa Player'dan al (oraya eklenebilir)
        if (parryEffectPrefab == null && player.parryEffectPrefab != null)
        {
            parryEffectPrefab = player.parryEffectPrefab;
        }
        
        // Eğer hala null ise bir uyarı ver ve çık
        if (parryEffectPrefab == null)
        {
            Debug.LogWarning("Parry effect prefab is missing!");
            return;
        }
        
        // Oyuncu ile düşman arasındaki orta noktayı hesapla (efekt için ideal pozisyon)
        Vector3 midPoint = (player.transform.position + enemyPosition) * 0.5f;
        
        // Parry efektini oluştur ve yönünü ayarla
        GameObject effect = GameObject.Instantiate(parryEffectPrefab, midPoint, Quaternion.identity);
        
        // Efektin ölçeğini veya rotasyonunu gerekirse ayarla
        
        // Efekti belli bir süre sonra yok et (eğer kendi kendini yok etmiyorsa)
        GameObject.Destroy(effect, 2f);
    }
}
