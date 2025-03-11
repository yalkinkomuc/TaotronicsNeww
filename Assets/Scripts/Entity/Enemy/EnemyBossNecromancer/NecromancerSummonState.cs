using UnityEngine;

public class NecromancerSummonState : EnemyState
{
    private EnemyBossNecromancerBoss enemy;

    public NecromancerSummonState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, EnemyBossNecromancerBoss _enemy) 
        : base(_enemyBase, _stateMachine, _animBoolName)
    {
        enemy = _enemy;
    }

    public override void Enter()
    {
        base.Enter();
        
        enemy.anim.SetBool(animBoolName, true);
    }

    public override void Update()
    {
        base.Update();
        
        enemy.SetZeroVelocity();

        if (triggerCalled)
        {
           
            stateMachine.ChangeState(enemy.battleState);
        }
    }

    public override void Exit()
    {
        base.Exit();
        enemy.anim.SetBool(animBoolName, false);
    }
}
