using UnityEngine;

public class SkeletonArcher_GroundedState : EnemyState
{

    protected Enemy_ArcherSkeleton enemy;
    public SkeletonArcher_GroundedState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName,Enemy_ArcherSkeleton _enemy) : base(_enemyBase, _stateMachine, _animBoolName)
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
