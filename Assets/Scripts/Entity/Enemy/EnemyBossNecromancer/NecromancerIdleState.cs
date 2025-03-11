using UnityEngine;

public class NecromancerIdleState : EnemyState
{

    private EnemyBossNecromancerBoss enemy;
    public NecromancerIdleState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName,EnemyBossNecromancerBoss _enemy) : base(_enemyBase, _stateMachine, _animBoolName)
    {
        enemy = _enemy;
    }

    public override void Enter()
    {
        base.Enter();
        
        stateTimer = enemy.idleTime;
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();
        
        enemy.SetZeroVelocity();
        
        if (enemy.IsPlayerDetected() || enemy.IsTooCloseToPlayer())
        {
            stateMachine.ChangeState(enemy.battleState);
        }
        
       
        
       // Debug.Log("necro in IdleState");
    }
}
