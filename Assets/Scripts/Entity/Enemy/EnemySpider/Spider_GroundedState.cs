using UnityEngine;

public class Spider_GroundedState : EnemyState
{

    protected Spider_Enemy enemy;
    public Spider_GroundedState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName,Spider_Enemy _enemy) : base(_enemyBase, _stateMachine, _animBoolName)
    {
        enemy = _enemy;
    }

    public override void Enter()
    {
        base.Enter();
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();
        
        if (enemy.IsPlayerDetected() ||enemy.IsTooCloseToPlayer())
        {
            stateMachine.ChangeState(enemy.battleState);
        }
    }
}
