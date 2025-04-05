using UnityEngine;

public class Spider_MoveState : Spider_GroundedState
{
    public Spider_MoveState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Spider_Enemy _enemy) : base(_enemyBase, _stateMachine, _animBoolName, _enemy)
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
