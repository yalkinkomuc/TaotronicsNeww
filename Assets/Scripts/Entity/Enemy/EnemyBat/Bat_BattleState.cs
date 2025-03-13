using UnityEngine;

public class Bat_BattleState : EnemyState
{
    private Bat_Enemy enemy;
    private Transform player;
    private float chaseSpeed = 3f; // Yarasa uçuş hızı
    
    // Yükseklik ayarları
    private float normalFlightHeight = 1.5f; // Oyuncunun üstünde uçma yüksekliği
    private float divingDistance = 2f; // Dalış başlangıç mesafesi
    private float normalVerticalSmoothTime = 0.3f; // Normal dikey hareket yumuşatma zamanı
    private float divingVerticalSmoothTime = 0.8f; // Dalış yumuşatma zamanı (daha yüksek = daha yavaş)
    private float currentVelocityY = 0f; // SmoothDamp için yardımcı değişken
    
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
            // Oyuncuya olan yatay ve dikey mesafeleri hesapla
            float horizontalDistance = Mathf.Abs(player.position.x - enemyBase.transform.position.x);
            
            // Hedef pozisyonu hesapla
            Vector2 targetPosition = new Vector2();
            targetPosition.x = player.position.x;
            
            // Dikey yumuşatma süresini ayarla
            float verticalSmoothTime;
            
            // Eğer oyuncu çok yakınsa (2 birim) dikey olarak oyuncunun seviyesine in
            // Değilse oyuncunun 1.5 birim üstünde uç
            if (horizontalDistance <= divingDistance)
            {
                // Oyuncunun yüksekliğine daha yavaş dalış yap
                targetPosition.y = player.position.y;
                verticalSmoothTime = divingVerticalSmoothTime; // Daha yavaş dalış
            }
            else
            {
                // Oyuncunun üstünde uç
                targetPosition.y = player.position.y + normalFlightHeight;
                verticalSmoothTime = normalVerticalSmoothTime; // Normal hız
            }
            
            // Yatay yönde direkt hareket et
            float directionX = Mathf.Sign(targetPosition.x - enemyBase.transform.position.x);
            
            // Dikey yönde yumuşak geçiş için SmoothDamp kullan
            float smoothedY = Mathf.SmoothDamp(
                enemyBase.transform.position.y, 
                targetPosition.y, 
                ref currentVelocityY, 
                verticalSmoothTime
            );
            
            // Hızı hesapla
            float velocityX = directionX * chaseSpeed;
            float velocityY = (smoothedY - enemyBase.transform.position.y) / Time.deltaTime;
            
            // Hızı uygula
            enemyBase.SetVelocity(velocityX, velocityY);
            
            // Düşmanın yüzünü oyuncuya doğru çevir
            if (directionX > 0 && enemyBase.facingdir == -1)
                enemyBase.Flip();
            else if (directionX < 0 && enemyBase.facingdir == 1)
                enemyBase.Flip();
            
            // Oyuncu menzil dışına çıktıysa idle state'e dön
            float totalDistance = Vector2.Distance(enemyBase.transform.position, player.position);
            if (totalDistance > 20f) // Geri dönüş mesafesi
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
