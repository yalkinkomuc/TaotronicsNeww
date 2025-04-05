using UnityEngine;

public class Boar_ChaseState : EnemyState
{
    
    private Boar_Enemy enemy;

    private Transform player;
    
    private int moveDir;

    public Boar_ChaseState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName,Boar_Enemy _enemy) : base(_enemyBase, _stateMachine, _animBoolName)
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
        //enemy.moveSpeed = enemy.moveSpeed;
    }

    public override void Update()
    {
        base.Update();
        
        if (stateTimer < 0)
        {
            stateMachine.ChangeState(enemy.idleState);
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
            stateMachine.ChangeState(enemy.idleState);
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
