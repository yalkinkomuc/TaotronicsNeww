using UnityEngine;

public class NecromancerBattleState : EnemyState
{
    private EnemyBossNecromancerBoss enemy;
    private float skillDecisionTimer = 0.5f;
    //private float teleportCheckTimer = 2f; // 0.5'ten 2 saniyeye çıkardık
    private const float moveSpeed = 3.5f; // Yürüme hızı
    
    // Teleport değişkenlerini düşürdük
    private const float EMERGENCY_TELEPORT_DISTANCE = 2.5f;    // 3'ten 2'ye düşürdük
    private const float PREFERRED_DISTANCE = 4f;            
    private const float TELEPORT_CHANCE = 0.10f;      // Teleport şansını %10'a düşürelim
    private const float DANGER_TELEPORT_CHANCE = 0.3f;     // 0.6'dan 0.3'e düşürdük
    private int consecutiveCloseCallCount = 0;             

    // Skill şanslarını daha agresif hale getirelim
    private const float SPELL_CAST_CHANCE = 0.75f;    // Büyü atma şansını %75'e çıkaralım
    private const float SUMMON_CHANCE = 0.10f;        // İskelet çağırma şansını %10'a düşürelim
    // Geriye %5 boş hareket kalıyor

    private const float TELEPORT_COOLDOWN = 3f;
    private float teleportCooldownTimer = 0f;
    private int teleportCount = 0;
    private const int MAX_TELEPORTS_NORMAL = 3; // Normal durumda 3 TP
    private const int MAX_TELEPORTS_LOW_HEALTH = 7; // Düşük sağlıkta 7 TP

    public NecromancerBattleState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, EnemyBossNecromancerBoss _enemy) : base(_enemyBase, _stateMachine, _animBoolName)
    {
        enemy = _enemy;
    }

    public override void Enter()
    {
        base.Enter();
        stateTimer = skillDecisionTimer;
        
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
        float xDirection = enemy.player.transform.position.x - enemy.transform.position.x;

        // Duvar kontrolü - direkt TP
        if (enemy.IsWallDetected())
        {
            stateMachine.ChangeState(enemy.teleportState);
            return;
        }

        // Acil Teleport kontrolü
        if (distanceToPlayer < EMERGENCY_TELEPORT_DISTANCE)
        {
            consecutiveCloseCallCount++;
            if (consecutiveCloseCallCount >= 3 && CanTeleport())
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

        // Skill seçimi
        if (stateTimer <= 0)
        {
            float roll = Random.value;

            bool canCastSpell = enemy.CanCastSpell();
            bool canSummon = enemy.CanSummon();
            bool canTeleport = CanTeleport();

            // Önce spell cast'i kontrol et (daha yüksek şans)
            if (roll < SPELL_CAST_CHANCE && canCastSpell)
            {
                stateMachine.ChangeState(enemy.spellCastState);
                return;
            }
            // Summon şansı (düşük)
            else if (roll < SPELL_CAST_CHANCE + SUMMON_CHANCE && canSummon)
            {
                stateMachine.ChangeState(enemy.summonState);
                return;
            }
            // Teleport şansı (düşük)
            else if (roll < SPELL_CAST_CHANCE + SUMMON_CHANCE + TELEPORT_CHANCE && canTeleport)
            {
                HandleTeleport();
                return;
            }
            // Eğer ilk seçilen skill yapılamazsa başka birini dene
            else if (canCastSpell)
            {
                stateMachine.ChangeState(enemy.spellCastState);
                return;
            }
            else if (canSummon)
            {
                stateMachine.ChangeState(enemy.summonState);
                return;
            }
            else if (canTeleport)
            {
                HandleTeleport();
                return;
            }

            stateTimer = skillDecisionTimer;
        }

        // Hareket mantığı
        if (distanceToPlayer > PREFERRED_DISTANCE - 1f)
        {
            enemy.SetVelocity(moveSpeed, enemy.rb.linearVelocity.y);
            enemy.FlipController(moveSpeed);
        }
        else
        {
            enemy.SetVelocity(-moveSpeed, enemy.rb.linearVelocity.y);
            enemy.FlipController(-moveSpeed);
        }
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
