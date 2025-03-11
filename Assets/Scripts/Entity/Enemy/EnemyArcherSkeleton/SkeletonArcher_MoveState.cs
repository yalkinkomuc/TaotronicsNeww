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
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();
        Debug.Log("Archer in move State");
        enemy.SetVelocity(enemy.moveSpeed*enemy.facingdir,enemy.rb.linearVelocity.y);

        if (enemy.IsWallDetected() || !enemy.IsGroundDetected())
        {
            
            Debug.Log("Archer aşağı düşecek");
            enemy.Flip();
            enemy.SetZeroVelocity();
            stateMachine.ChangeState(enemy.idleState);
        }

    }
}
