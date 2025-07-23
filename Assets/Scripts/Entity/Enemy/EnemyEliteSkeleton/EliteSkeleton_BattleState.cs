using UnityEngine;

public class EliteSkeleton_BattleState : EnemyState
{
    private EliteSkeleton_Enemy enemy;
    private Transform player;
    private int moveDir;
    private float directionCheckCooldown = 0.2f; // Yön kontrolü için cooldown
    private float lastDirectionCheckTime;
    private float battleTimeCounter; // Battle time'ı takip etmek için sayaç
    
    public EliteSkeleton_BattleState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, EliteSkeleton_Enemy _enemy) : base(_enemyBase, _stateMachine, _animBoolName)
    {
        enemy = _enemy;
    }

    public override void Enter()
    {
        base.Enter();
        
        enemy.fightBegun = true;
        player = PlayerManager.instance.player.transform;
        
        // Battle time sayacını ayarla
        battleTimeCounter = enemy.battleTime;
        
        // State'e girerken ilk yön kontrolünü yap
        UpdateFacingDirection();
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();
        
        // Oyuncu aşağıdaysa savaşı bırak
        if (enemy.IsPlayerBelow())
        {
            // Debug.Log("Oyuncu çok aşağıda, savaş bırakılıyor!");
            enemy.fightBegun = false;
            stateMachine.ChangeState(enemy.idleState);
            return;
        }

        // Yön kontrolünü daha az sıklıkla yap
        if (Time.time >= lastDirectionCheckTime + directionCheckCooldown)
        {
            UpdateFacingDirection();
            lastDirectionCheckTime = Time.time;
        }

        // Hareket ettir
        enemy.SetVelocity(enemy.chaseSpeed * moveDir, rb.linearVelocity.y);

        if (!enemy.IsGroundDetected() || enemy.IsWallDetected())
        {
            enemy.SetZeroVelocity();
            stateMachine.ChangeState(enemy.idleState);
        }
        
        // Debug.Log(enemy.IsPlayerBelow());

       

        // Eğer oyuncuyu görüyorsa battle time sayacını sıfırla
        if (enemy.IsPlayerDetected() || enemy.IsTooCloseToPlayer())
        {
            battleTimeCounter = enemy.battleTime; // Oyuncuyu gördüğünde sayacı sıfırla
            
            // Saldırı mesafesi kontrolü
            if (enemy.IsPlayerDetected() && enemy.IsPlayerDetected().distance < enemy.attackDistance && CanAttack())
            {
                enemy.SetZeroVelocity(); // Saldırı öncesi durmayı sağla
                stateMachine.ChangeState(enemy.attackState);
                return;
            }
        }
        else
        {
            // Oyuncuyu görmüyorsa battle time sayacını azalt
            battleTimeCounter -= Time.deltaTime;
            
            // Eğer süre dolduysa savaşı bitir
            if (battleTimeCounter <= 0)
            {
                enemy.fightBegun = false;
                stateMachine.ChangeState(enemy.idleState);
            }
        }
    }

    private void UpdateFacingDirection()
    {
        if (player != null)
        {
            float distanceToPlayer = Vector2.Distance(enemy.transform.position, player.position);
            float horizontalDistance = player.position.x - enemy.transform.position.x;
            
            // Saldırı mesafesi kontrolü - eğer saldırı mesafesinden daha yakınsa dur veya geri çekil
            if (distanceToPlayer <= enemy.attackDistance + 0.5f) // 0.5f buffer zone
            {
                // Çok yakın - dur veya hafifçe geri çekil
                moveDir = 0; // Durma
               
            }
            else
            {
                // Uzak - yaklaş (normal hareket)
                // Dead zone (ölü bölge) ekle - çok yakınken sürekli dönmeyi engelle
                if (Mathf.Abs(horizontalDistance) > 0.5f)
                {
                    moveDir = horizontalDistance > 0 ? 1 : -1;
                }
            }
            
            // Yön değişimi gerekiyorsa Flip() kullan (sadece hareket ediyorsa)
            if (moveDir != 0)
            {
                if ((moveDir > 0 && enemy.facingdir < 0) || (moveDir < 0 && enemy.facingdir > 0))
                {
                    enemy.Flip();
                }
            }
        }
    }

    private bool CanAttack()
    {
        return Time.time >= enemy.lastTimeAttacked + enemy.attackCooldown;
    }
}

