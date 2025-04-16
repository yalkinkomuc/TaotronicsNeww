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
        
        // Eğer düşman zaten savaşta ise, hemen battle state'e geç
        if (enemy.fightBegun)
        {
            stateMachine.ChangeState(enemy.battleState);
            return;
        }
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
        
        enemy.UpdatePatrol();
        
        // // Normal savaş geçişi kontrolü
        // if (enemy.CheckForBattleTransition())
        // {
        //     stateMachine.ChangeState(enemy.battleState);
        // }
    }
}
