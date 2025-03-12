using UnityEngine;

public class EliteSkeleton_MoveState : EliteSkeleton_GroundedState
{
    public EliteSkeleton_MoveState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, EliteSkeleton_Enemy _enemy) : base(_enemyBase, _stateMachine, _animBoolName, _enemy)
    {
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
        
        Debug.Log("im in move state");
        
        enemy.SetVelocity(enemy.moveSpeed*enemy.facingdir,enemy.rb.linearVelocity.y);

        if (enemy.IsWallDetected() || !enemy.IsGroundDetected())
        {
            enemy.Flip();
            enemy.SetZeroVelocity();
            stateMachine.ChangeState(enemy.idleState);
        }
    }
}
