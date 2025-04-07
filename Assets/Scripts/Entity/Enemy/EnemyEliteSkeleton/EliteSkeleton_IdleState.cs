using UnityEngine;

public class EliteSkeleton_IdleState : EliteSkeleton_GroundedState
{
    public EliteSkeleton_IdleState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, EliteSkeleton_Enemy _enemy) : base(_enemyBase, _stateMachine, _animBoolName, _enemy)
    {
    }

    public override void Enter()
    {
        base.Enter();
        
        if (enemy.fightBegun)
            return;
            
        stateTimer = enemy.idleTime;
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();
        
        Debug.Log("im in idle state");
        
        if (stateTimer < 0f)
        {
            stateMachine.ChangeState(enemy.moveState);
        }
    }
}
