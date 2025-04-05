using UnityEngine;

public class SkeletonArcher_MoveState : SkeletonArcher_GroundedState
{
    //private Enemy_ArcherSkeleton enemy;
    public SkeletonArcher_MoveState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName,Enemy_ArcherSkeleton _enemy) : base(_enemyBase, _stateMachine, _animBoolName,_enemy)
    {
       // enemy = _enemy;
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
