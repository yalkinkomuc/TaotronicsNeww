using UnityEngine;

public class NecromancerBattleState : EnemyState
{
    private EnemyBossNecromancerBoss enemy;
    private float skillDecisionTimer = 0.5f;
    //private float teleportCheckTimer = 2f; // 0.5'ten 2 saniyeye çıkardık
    private const float moveSpeed = 3.5f; // Yürüme hızı
    
    // Teleport değişkenlerini düşürdük
    private const float EMERGENCY_TELEPORT_DISTANCE = 5f;    // 2.5f'den 5f'e artırıldı
    private const float MIN_PREFERRED_DISTANCE = 6f;
    private const float MAX_PREFERRED_DISTANCE = 12f;
   
    private int consecutiveCloseCallCount = 0;             

 

    private const float TELEPORT_COOLDOWN = 3f;
    private float teleportCooldownTimer = 0f;
    private int teleportCount = 0;
    private const int MAX_TELEPORTS_NORMAL = 3; // Normal durumda 3 TP
    private const int MAX_TELEPORTS_LOW_HEALTH = 7; // Düşük sağlıkta 7 TP

    private float battleTime;
    private float decisionTime = 2f;
    private float lastSummonTime = 0f; // Son summon zamanı
    private const float summonCooldown = 3f; // Summon sonrası 3 saniye bekleme süresi

    public NecromancerBattleState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, EnemyBossNecromancerBoss _enemy) : base(_enemyBase, _stateMachine, _animBoolName)
    {
        enemy = _enemy;
    }

    public override void Enter()
    {
        base.Enter();
        stateTimer = skillDecisionTimer;
        battleTime = 0f;
        enemy.SetVelocity(0, 0);
        
        // Battle state'e girerken oyuncuya dön
        float xDirection = enemy.player.transform.position.x - enemy.transform.position.x;
        enemy.facingdir = xDirection > 0 ? 1 : -1;
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();
        battleTime += Time.deltaTime;

        if (battleTime >= decisionTime)
        {
            battleTime = 0;
            
            // Teleport ve spell şansları
            float teleportChance = 0.1f;
            float spellChance = 0.4f;
            float summonChance = 0.5f;
            
            // Eğer son summon'dan yeterli süre geçmediyse summon şansını 0 yap
            if (Time.time - lastSummonTime < summonCooldown)
            {
                summonChance = 0f; // Summon şansını sıfırla
                
                // Diğer şansları paylaştır
                teleportChance = 0.2f;
                spellChance = 0.8f;
            }

            // Düşük sağlık teleport önceliği
            float healthPercentage = (float)enemy.stats.currentHealth / enemy.stats.maxHealth.GetValue();
            if (healthPercentage <= 0.25f)
            {
                // Çok düşük sağlıkta TP atamasın - ölüm animasyonu için
                teleportChance = 0f;
            }
            else if (healthPercentage <= 0.5f)
            {
                // Düşük sağlıkta daha fazla teleport şansı
                teleportChance = 0.3f;
                spellChance = 0.3f;
                summonChance = 0.4f;
            }

            float decision = Random.value;
            
            if (decision < teleportChance && CanTeleportSafely())
            {
                stateMachine.ChangeState(enemy.teleportState);
            }
            else if (decision < teleportChance + spellChance && enemy.CanCastSpell())
            {
                stateMachine.ChangeState(enemy.spellCastState);
            }
            else if (enemy.CanSummon())
            {
                lastSummonTime = Time.time; // Son summon zamanını kaydet
                stateMachine.ChangeState(enemy.summonState);
            }
            else
            {
                // Sağlık düşükse ve teleport güvenli değilse spell at
                if (healthPercentage <= 0.25f || !CanTeleportSafely())
                {
                    if (enemy.CanCastSpell())
                        stateMachine.ChangeState(enemy.spellCastState);
                    else
                        enemy.SetZeroVelocity(); // Sadece dur
                }
                else
                {
                    stateMachine.ChangeState(enemy.teleportState);
                }
            }
        }

        // Cooldown timer'ı güncelle
        if (teleportCooldownTimer > 0)
        {
            teleportCooldownTimer -= Time.deltaTime;
            if (teleportCooldownTimer <= 0)
            {
                teleportCount = 0;
            }
        }

        float distanceToPlayer = Vector2.Distance(enemy.transform.position, enemy.player.transform.position);
       

        // Duvar kontrolü - direkt TP
        if (enemy.IsWallDetected())
        {
            if (CanTeleportSafely())
                stateMachine.ChangeState(enemy.teleportState);
            return;
        }

        // Acil Teleport kontrolü
        if (distanceToPlayer < EMERGENCY_TELEPORT_DISTANCE)
        {
            consecutiveCloseCallCount++;
            if (consecutiveCloseCallCount >= 2 && CanTeleport() && CanTeleportSafely())
            {
                HandleTeleport();
                consecutiveCloseCallCount = 0;
                return;
            }
        }
        else
        {
            consecutiveCloseCallCount = 0;
        }

        // Hareket mantığı - tercih edilen mesafedeyken spell at
        if (distanceToPlayer < MIN_PREFERRED_DISTANCE) 
        {
            // Çok yakın, uzaklaş
            enemy.SetVelocity(-moveSpeed, enemy.rb.linearVelocity.y);
            enemy.FlipController(-moveSpeed);
        }
        else if (distanceToPlayer > MAX_PREFERRED_DISTANCE)
        {
            // Çok uzak, yaklaş
            enemy.SetVelocity(moveSpeed, enemy.rb.linearVelocity.y);
            enemy.FlipController(moveSpeed);
        }
        else
        {
            // İdeal mesafedeyiz, direkt spell at
            stateMachine.ChangeState(enemy.spellCastState);
        }
    }

    // Ölüm animasyonu bug'ını önlemek için güvenli TP kontrolü
    private bool CanTeleportSafely()
    {
        // Eğer canı çok azsa (örneğin %25'in altındaysa) teleport yapmasın
        float healthPercentage = (float)enemy.stats.currentHealth / enemy.stats.maxHealth.GetValue();
        return healthPercentage > 0.25f;
    }

    private bool CanTeleport()
    {
        // Sağlık durumuna göre maksimum teleport sayısını belirle
        int maxTeleports = GetMaxTeleports();
        
        return teleportCooldownTimer <= 0 || teleportCount < maxTeleports;
    }
    
    private int GetMaxTeleports()
    {
        // Boss'un sağlık durumunu kontrol et
        float healthPercentage = (float)enemy.stats.currentHealth / enemy.stats.maxHealth.GetValue();
        
        // Canı %50'nin altına düştüyse daha fazla teleport hakkı ver
        if (healthPercentage <= 0.5f)
        {
            return MAX_TELEPORTS_LOW_HEALTH; // Düşük sağlıkta 7 TP
        }
        else
        {
            return MAX_TELEPORTS_NORMAL; // Normal durumda 3 TP
        }
    }

    private void HandleTeleport()
    {
        teleportCount++;
        
        // Sağlık durumuna göre maksimum teleport sayısını kontrol et
        int maxTeleports = GetMaxTeleports();
        
        if (teleportCount >= maxTeleports)
        {
            teleportCooldownTimer = TELEPORT_COOLDOWN;
        }
        
        stateMachine.ChangeState(enemy.teleportState);
    }
}
