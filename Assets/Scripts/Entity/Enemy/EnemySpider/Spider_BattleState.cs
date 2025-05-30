using UnityEngine;

public class Spider_BattleState : EnemyState
{
    private Spider_Enemy enemy;
    
    private Transform player;
    private int moveDir;
    
    // Dead zone ve yön kontrolü için eklemeler
    private float directionCheckCooldown = 0.2f; // Yön kontrolü için cooldown
    private float lastDirectionCheckTime;
    
    public Spider_BattleState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName,Spider_Enemy _enemy) : base(_enemyBase, _stateMachine, _animBoolName)
    {
        enemy = _enemy;
    }

    public override void Enter()
    {
        base.Enter();
        
        player = PlayerManager.instance.player.transform;
        stateTimer = enemy.battleTime;
        
        // İlk yön kontrolünü yap
        UpdateFacingDirection();
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();
        
        if (enemy.IsPlayerBelow())
        {
            // Debug.Log("Oyuncu çok aşağıda, savaş bırakılıyor!");
            enemy.fightBegun = false;
            stateMachine.ChangeState(enemy.idleState);
            return;
        }
        
        if (stateTimer < 0)
        {
            stateMachine.ChangeState(enemy.idleState);
            return;
        }
        
        // Yön kontrolünü daha az sıklıkla yap
        if (Time.time >= lastDirectionCheckTime + directionCheckCooldown)
        {
            UpdateFacingDirection();
            lastDirectionCheckTime = Time.time;
        }
        
        enemy.SetVelocity(enemy.chaseSpeed*moveDir,rb.linearVelocity.y);

        if (!enemy.IsGroundDetected() || enemy.IsWallDetected())
        {
            enemy.SetZeroVelocity(); // simdilik calisiyor ama sonradan değiştirelbilir.
            enemy.Flip();
            stateMachine.ChangeState(enemy.idleState);
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
}
