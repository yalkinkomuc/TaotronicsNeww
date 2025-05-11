using UnityEngine;

public class EnemyChest_ChaseState : EnemyState
{

    private Chest_Enemy enemy;
    
    private Transform player;
    
    private int moveDir;
    
    public EnemyChest_ChaseState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName,Chest_Enemy _enemy) : base(_enemyBase, _stateMachine, _animBoolName)
    {
        enemy = _enemy;
    }

    public override void Enter()
    {
        base.Enter();
        
        player = PlayerManager.instance.player.transform;
        
        enemy.moveSpeed = enemy.chaseSpeed;
        
        stateTimer = enemy.battleTime;
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
            stateMachine.ChangeState(enemy.patrolState);
            return;
        }
        
        
        
        if(player.position.x > enemy.transform.position.x)
            moveDir = 1;
        else if (player.position.x <enemy.transform.position.x)
            moveDir = -1;
        
        enemy.SetVelocity(enemy.chaseSpeed*moveDir,rb.linearVelocity.y);

        if (!enemy.IsGroundDetected() || enemy.IsWallDetected())
        {
            enemy.SetZeroVelocity();
            stateMachine.ChangeState(enemy.patrolState);
            return;
        }
        
        if (enemy.IsPlayerDetected())
        {
            if (enemy.IsPlayerDetected().distance < enemy.attackDistance && CanAttack())
            {
                enemy.SetZeroVelocity();
                enemy.lastTimeAttacked = Time.time;
                stateMachine.ChangeState(enemy.attackState);
            }
        }
    }
    
    private bool CanAttack()
    {
        return Time.time >= enemy.lastTimeAttacked + enemy.attackCooldown;
    }
}
