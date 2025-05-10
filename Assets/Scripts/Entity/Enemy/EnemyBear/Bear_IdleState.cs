using UnityEngine;

public class Bear_IdleState : EnemyState
{
    
    private Enemy_Bear enemy;
    private Transform player;
    
    public Bear_IdleState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName,Enemy_Bear _enemy) : base(_enemyBase, _stateMachine, _animBoolName)
    {
        enemy = _enemy;
    }

    public override void Enter()
    {
        base.Enter();
        
        stateTimer = enemy.idleTime;
        
        player = PlayerManager.instance.player.transform;
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();
        
        enemy.SetZeroVelocity();

        if (stateTimer < 0f)
        {
            stateMachine.ChangeState(enemy.moveState);
        }
        
        // if (enemy.IsPlayerDetected())
        // {
        //     stateMachine.ChangeState(enemy.chaseState);
        // }
    }
}
