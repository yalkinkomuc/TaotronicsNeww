using UnityEngine;

public class Spider_IdleState : Spider_GroundedState
{
    
    public Spider_IdleState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Spider_Enemy _enemy) : base(_enemyBase, _stateMachine, _animBoolName, _enemy)
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
        
        enemy.SetZeroVelocity();
        
        if (stateTimer < 0f)
        {
            stateMachine.ChangeState(enemy.moveState);
        }
    }

   
}
