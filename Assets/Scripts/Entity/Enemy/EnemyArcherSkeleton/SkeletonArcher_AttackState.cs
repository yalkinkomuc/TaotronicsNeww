using UnityEngine;

public class SkeletonArcher_AttackState : EnemyState
{
    private Enemy_ArcherSkeleton enemy;
    private bool hasShot;
    private float attackDelay = 1f; // Bekleme süresini 1 saniye yapalım

    public SkeletonArcher_AttackState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Enemy_ArcherSkeleton _enemy) 
        : base(_enemyBase, _stateMachine, _animBoolName)
    {
        this.enemy = _enemy;
    }

    public override void Enter()
    {
        base.Enter();
        hasShot = false;
        enemy.SetZeroVelocity();
        stateTimer = attackDelay;
        enemy.FacePlayer();

        
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();
        enemy.SetZeroVelocity();

       // Debug.Log("Archer in attack State");

        // Bekleme süresi bittiyse ve henüz ok atmadıysak
        if (stateTimer < 0 && !hasShot)
        {
            ShootArrow();
        }

        if (hasShot)
        {
            stateMachine.ChangeState(enemy.battleState);
        }

        if (!enemy.CanSeePlayer() && stateTimer < 0)
        {
            stateMachine.ChangeState(enemy.idleState);
        }
    }

    public void ShootArrow()
    {
        enemy.ShootArrow();
        stateMachine.ChangeState(enemy.battleState); // idle yerine battle'a geç
    }
}


