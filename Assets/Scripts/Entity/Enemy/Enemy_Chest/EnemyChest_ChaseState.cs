using UnityEngine;

public class EnemyChest_ChaseState : EnemyState
{

    private Chest_Enemy enemy;
    
  
    
    private Transform player;
    private int moveDir;
    private float directionCheckCooldown = 0.2f; // Yön kontrolü için cooldown
    private float lastDirectionCheckTime;
    private float battleTimeCounter; // Battle time'ı takip etmek için sayaç

    
    public EnemyChest_ChaseState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName,Chest_Enemy _enemy) : base(_enemyBase, _stateMachine, _animBoolName)
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
            stateMachine.ChangeState(enemy.patrolState);
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
            stateMachine.ChangeState(enemy.patrolState);
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
                stateMachine.ChangeState(enemy.patrolState);
            }
        }
    }
    
    private void UpdateFacingDirection()
    {
        if (player != null)
        {
            float distanceToPlayer = player.position.x - enemy.transform.position.x;
            
            // Dead zone (ölü bölge) ekle - çok yakınken sürekli dönmeyi engelle
            if (Mathf.Abs(distanceToPlayer) > 0.5f)
            {
                moveDir = distanceToPlayer > 0 ? 1 : -1;
                
                // Yön değişimi gerekiyorsa Flip() kullan
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
