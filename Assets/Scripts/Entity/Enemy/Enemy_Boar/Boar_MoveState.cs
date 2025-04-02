using UnityEngine;

public class Boar_MoveState : EnemyState
{

    private Boar_Enemy enemy;
    
    private Vector2 startPosition;
    private float patrolDistance = 8f;
    private float patrolSpeed = 2f;
    private int moveDirection = 1; // 1: sağ, -1: sol
    public Boar_MoveState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName,Boar_Enemy _enemy) : base(_enemyBase, _stateMachine, _animBoolName)
    {

        enemy = _enemy;
    }

    public override void Enter()
    {
        base.Enter();
        
        // Başlangıç pozisyonunu kaydet
        startPosition = enemyBase.transform.position;
        
        // Başlangıçta sola doğru hareket et
        moveDirection = -1;
        
        // Sprite'ı doğru yöne çevir
        if (moveDirection == 1 && enemyBase.facingdir == -1)
            enemy.Flip();
        else if (moveDirection == -1 && enemyBase.facingdir == 1)
            enemy.Flip();
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();
        
        // enemy.SetVelocity(enemy.moveSpeed*enemy.facingdir,enemy.rb.linearVelocity.y);
        //
        // if (enemy.IsWallDetected() || !enemy.IsGroundDetected())
        // {
        //     enemy.Flip();
        //     enemy.SetZeroVelocity();
        //     stateMachine.ChangeState(enemy.idleState);
        // }
        //
        // if (enemy.IsPlayerDetected() || enemy.IsTooCloseToPlayer())
        // {
        //     stateMachine.ChangeState(enemy.chargeState);
        // }

        
        // Oyuncuyu algılama kontrolü
        if (enemy.CheckForBattleTransition())
        {
            stateMachine.ChangeState(enemy.chaseState);
            return;
        }


        // if (enemy.IsWallDetected())
        // {
        //     enemy.Flip();
        // }
        //
        // Pozisyon kontrolü
        float distanceFromStart = enemyBase.transform.position.x - startPosition.x;
        
        // Eğer sınıra ulaştıysa yönü değiştir
        if (distanceFromStart >= patrolDistance && moveDirection == 1)
        {
            moveDirection = -1;
            enemyBase.Flip(); // Sprite'ı çevir
        }
        else if (distanceFromStart <= -patrolDistance && moveDirection == -1)
        {
            moveDirection = 1;
            enemyBase.Flip(); // Sprite'ı çevir
        }
        
        // Belirlenen yönde hareket et
        enemyBase.SetVelocity(moveDirection * patrolSpeed, 0);
        
    }
}
