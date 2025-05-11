using UnityEngine;

public class EnemyChest_PatrolState : EnemyState
{
    private Chest_Enemy enemy;
    
    private Vector2 startPosition;
    private float patrolDistance = 8f;
    private float patrolSpeed = 2f;
    private int moveDirection = 1; // 1: sağ, -1: sol
    
    public EnemyChest_PatrolState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName,Chest_Enemy _enemy) : base(_enemyBase, _stateMachine, _animBoolName)
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
        
        // Duvar veya uçurum kontrolü
        if (enemy.IsWallDetected() || !enemy.IsGroundDetected())
        {
            moveDirection *= -1;
            enemy.Flip();
        }
        
        // Oyuncuyu algılama kontrolü
        if (enemy.CheckForBattleTransition())
        {
            stateMachine.ChangeState(enemy.chaseState);
            return;
        }
        
        // Pozisyon kontrolü
        float distanceFromStart = enemyBase.transform.position.x - startPosition.x;
        
        // Eğer sınıra ulaştıysa yönü değiştir
        if (distanceFromStart >= patrolDistance && moveDirection == 1)
        {
            moveDirection = -1;
            enemy.Flip();
        }
        else if (distanceFromStart <= -patrolDistance && moveDirection == -1)
        {
            moveDirection = 1;
            enemy.Flip();
        }
        
        // Belirlenen yönde hareket et
        enemy.SetVelocity(moveDirection * patrolSpeed, enemy.rb.linearVelocity.y);
    }
}
