using UnityEngine;

public class EliteSkeleton_MoveState : EliteSkeleton_GroundedState
{
    public EliteSkeleton_MoveState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, EliteSkeleton_Enemy _enemy) : base(_enemyBase, _stateMachine, _animBoolName, _enemy)
    {
    }

    public override void Enter()
    {
        base.Enter();
        enemy.startPosition = enemy.transform.position;
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();
        
        enemy.UpdatePatrol();
        
        if (enemy.CheckForBattleTransition())
        {
            stateMachine.ChangeState(enemy.idleState);
        }
    }
}
