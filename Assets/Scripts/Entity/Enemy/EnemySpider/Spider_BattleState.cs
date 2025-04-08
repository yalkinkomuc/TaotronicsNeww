using UnityEngine;

public class Spider_BattleState : EnemyState
{
    private Spider_Enemy enemy;
    
    private Transform player;
    private int moveDir;
    public Spider_BattleState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName,Spider_Enemy _enemy) : base(_enemyBase, _stateMachine, _animBoolName)
    {
        enemy = _enemy;
    }

    public override void Enter()
    {
        base.Enter();
        
        player = PlayerManager.instance.player.transform;

        stateTimer = enemy.battleTime;
    }

    public override void Exit()
    {
        base.Exit();
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
            enemy.SetZeroVelocity(); // simdilik calisiyor ama sonradan değiştirelbilir.
            enemy.Flip();
            stateMachine.ChangeState(enemy.idleState);
        }
    }
    
}
