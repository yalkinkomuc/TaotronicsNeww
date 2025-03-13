using UnityEngine;

public class Bat_BattleState : EnemyState
{
    private Bat_Enemy enemy;
    private Transform player;
    private float chaseSpeed = 3f; // Yarasa uçuş hızı
    
    public Bat_BattleState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Bat_Enemy _enemy) 
        : base(_enemyBase, _stateMachine, _animBoolName)
    {
        enemy = _enemy;
    }

    public override void Enter()
    {
        base.Enter();
        
        // Player referansını al
        if (PlayerManager.instance != null && PlayerManager.instance.player != null)
        {
            player = PlayerManager.instance.player.transform;
        }
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();
        
        if (player != null)
        {
            // Oyuncuya doğru yön vektörü
            Vector2 direction = ((Vector2)player.position - (Vector2)enemyBase.transform.position).normalized;
            
            // Oyuncuya doğru hareket et
            enemyBase.SetVelocity(direction.x * chaseSpeed, direction.y * chaseSpeed);
            
            // Düşmanın yüzünü oyuncuya doğru çevir
            if (direction.x > 0 && enemyBase.facingdir == -1)
                enemyBase.Flip();
            else if (direction.x < 0 && enemyBase.facingdir == 1)
                enemyBase.Flip();
            
            // Oyuncu menzil dışına çıktıysa idle state'e dön
            float distanceToPlayer = Vector2.Distance(enemyBase.transform.position, player.position);
            if (distanceToPlayer > 20f) // Geri dönüş mesafesi
            {
                stateMachine.ChangeState(enemy.idleState);
            }
        }
        else
        {
            // Player referansı kaybolursa idle state'e dön
            stateMachine.ChangeState(enemy.idleState);
        }
    }
}
