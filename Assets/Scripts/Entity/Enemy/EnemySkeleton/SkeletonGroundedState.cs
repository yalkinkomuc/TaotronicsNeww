using UnityEngine;

public class SkeletonGroundedState : EnemyState
{
    protected Skeleton_Enemy enemy;
    public SkeletonGroundedState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName,Skeleton_Enemy _enemy) : base(_enemyBase, _stateMachine, _animBoolName)
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
        
        //Debug.Log("im in groundedState");

        if (enemy.IsPlayerDetected() ||enemy.IsTooCloseToPlayer())
        {
            stateMachine.ChangeState(enemy.battleState);
        }
    }
}
