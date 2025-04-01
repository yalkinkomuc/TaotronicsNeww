using UnityEngine;

public class EliteSkeleton_BattleState : EnemyState
{
    private EliteSkeleton_Enemy enemy;
    private Transform player;
    private int moveDir;
    private float directionCheckCooldown = 0.2f; // Yön kontrolü için cooldown
    private float lastDirectionCheckTime;
    
    public EliteSkeleton_BattleState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, EliteSkeleton_Enemy _enemy) : base(_enemyBase, _stateMachine, _animBoolName)
    {
        enemy = _enemy;
    }

    public override void Enter()
    {
        base.Enter();
        enemy.fightBegun = true;
        player = PlayerManager.instance.player.transform;
        // State'e girerken ilk yön kontrolünü yap
        UpdateFacingDirection();
    }

    public override void Exit()
    {
        base.Exit();
        enemy.fightBegun = false;
    }

    public override void Update()
    {
        base.Update();

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

        // Saldırı mesafesi kontrolü
        if (enemy.IsPlayerDetected())
        {
            stateTimer = enemy.battleTime;
            if (enemy.IsPlayerDetected().distance < enemy.attackDistance && CanAttack())
            {
                enemy.SetZeroVelocity(); // Saldırı öncesi durmayı sağla
                stateMachine.ChangeState(enemy.attackState);
            }
        }
        else
        {
            if (stateTimer < 0 || Vector2.Distance(player.transform.position,enemy.transform.position) <15)
            {
                stateMachine.ChangeState(enemy.idleState);
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

