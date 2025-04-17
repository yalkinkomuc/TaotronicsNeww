using UnityEngine;

public class SkeletonMoveState : SkeletonGroundedState
{
    public SkeletonMoveState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Skeleton_Enemy _enemy) : base(_enemyBase, _stateMachine, _animBoolName, _enemy)
    {
    }

    public override void Enter()
    {
        base.Enter();
        // Patrol için başlangıç pozisyonunu kaydet
        enemy.startPosition = enemy.transform.position;
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();
        
        // // Eğer düşman savaşta ise patrol etme, battle state'e geç
        // if (enemy.fightBegun)
        // {
        //     stateMachine.ChangeState(enemy.battleState);
        //     return;
        // }
        
        // Patrol davranışını güncelle
        enemy.UpdatePatrol();
        
        // // Savaş durumuna geçiş kontrolü
        // if (enemy.CheckForBattleTransition())
        // {
        //     stateMachine.ChangeState(enemy.idleState);
        // }
    }
}
