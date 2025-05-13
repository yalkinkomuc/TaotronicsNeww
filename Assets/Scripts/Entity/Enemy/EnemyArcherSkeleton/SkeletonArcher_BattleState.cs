using UnityEngine;

public class SkeletonArcher_BattleState : EnemyState
{
    private Enemy_ArcherSkeleton enemy;
    private float walkSpeed = 2f;
    

    public SkeletonArcher_BattleState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Enemy_ArcherSkeleton _enemy) 
        : base(_enemyBase, _stateMachine, _animBoolName)
    {
        this.enemy = _enemy;
    }

    public override void Enter()
    {
        base.Enter();
        stateTimer = 1f;
    }

   

    public override void Update()
    {
        base.Update();
        if (enemy.IsPlayerBelow())
        {
            // Debug.Log("Oyuncu çok aşağıda, savaş bırakılıyor!");
            enemy.fightBegun = false;
            stateMachine.ChangeState(enemy.idleState);
            return;
        }

        if (!enemy.CanSeePlayer())
        {
            if (stateTimer <= 0)
            {
                stateMachine.ChangeState(enemy.idleState);
                return;
            }
        }
        else
        {
            stateTimer = 2f;
        }

        if (enemy.CanAttack())
        {
            stateMachine.ChangeState(enemy.attackState);
            return;
        }

        // Cooldown sırasında oyuncuya doğru yürü
        if (enemy.player != null)
        {
            float moveDirection = enemy.player.transform.position.x > enemy.transform.position.x ? 1 : -1;
            enemy.SetVelocity(moveDirection * walkSpeed, 0);
            
            // Yüzünü hareket yönüne çevir
            if ((moveDirection > 0 && enemy.facingdir == -1) || (moveDirection < 0 && enemy.facingdir == 1))
            {
                enemy.Flip();
            }
        }
    }

    public override void Exit()
    {
        base.Exit();
        enemy.SetZeroVelocity();
    }
}
