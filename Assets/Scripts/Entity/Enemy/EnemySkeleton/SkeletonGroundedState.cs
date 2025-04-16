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

        // Eğer oyuncu aşağıdaysa savaşa girme
        if (enemy.IsPlayerBelow())
        {
            return;
        }

        // Oyuncuyu algıladıysa ve oyuncu düşmanın altında değilse savaşa gir
        if (enemy.IsPlayerDetected() || enemy.IsTooCloseToPlayer())
        {
            stateMachine.ChangeState(enemy.battleState);
        }
    }
}
