using UnityEngine;

public class SkeletonArcher_IdleState : SkeletonArcher_GroundedState
{
    public SkeletonArcher_IdleState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Enemy_ArcherSkeleton _enemy) 
        : base(_enemyBase, _stateMachine, _animBoolName,_enemy)
    {
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

        Debug.Log("Archer in idle  State");
        
        if (enemy.CanSeePlayer())
        {
            stateMachine.ChangeState(enemy.battleState);
            return;
        }
        
        if (stateTimer < 0f)
        {
            stateMachine.ChangeState(enemy.moveState);
        }
    }
}
