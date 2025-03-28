using UnityEngine;

public class Boar_MoveState : EnemyState
{

    private Boar_Enemy enemy;
    public Boar_MoveState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName,Boar_Enemy _enemy) : base(_enemyBase, _stateMachine, _animBoolName)
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
        
        enemy.SetVelocity(enemy.moveSpeed*enemy.facingdir,enemy.rb.linearVelocity.y);

        if (enemy.IsWallDetected() || !enemy.IsGroundDetected())
        {
            enemy.Flip();
            enemy.SetZeroVelocity();
            stateMachine.ChangeState(enemy.idleState);
        }

        if (enemy.IsPlayerDetected())
        {
            stateMachine.ChangeState(enemy.chaseState);
        }
    }
}
