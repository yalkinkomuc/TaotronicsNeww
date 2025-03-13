using UnityEngine;

public class Bat_Enemy : Enemy
{
    [Header("Detection Settings")]
    [SerializeField] private float detectionRange = 15f;
    
    [Header("Movement Settings")]
    [SerializeField] private float flightSpeed = 3f;
    
    #region States

    public Bat_IdleState idleState { get; private set; }
    public Bat_MoveState moveState { get; private set; }
    public Bat_BattleState battleState { get; private set; }
    public Bat_DeadState deadState { get; private set; }

    #endregion


    protected override void Awake()
    {
        base.Awake();
        
        SetupDefaultFacingDir(-1);
        
        idleState = new Bat_IdleState(this,stateMachine,"Idle",this);
        moveState = new Bat_MoveState(this,stateMachine,"Move",this);
        battleState = new Bat_BattleState(this,stateMachine,"Move",this);
        deadState = new Bat_DeadState(this,stateMachine,"Death",this);
    }

    protected override void Start()
    {
        base.Start();
        
        stateMachine.Initialize(idleState);
    }

    public override void Die()
    {
        base.Die();
        
        stateMachine.ChangeState(deadState);
    }
    
    private void OnCollisionStay2D(Collision2D other)
    {
        Player player = other.gameObject.GetComponent<Player>();

        if (player != null)
        {
            player.Damage();
        }
    }
    
    // Oyuncuyu algılama ve battle state'e geçme kontrolü
    public bool CheckForBattleTransition()
    {
        if (PlayerManager.instance != null && PlayerManager.instance.player != null)
        {
            float distanceToPlayer = Vector2.Distance(
                transform.position, 
                PlayerManager.instance.player.transform.position
            );
            
            // Oyuncu algılama mesafesi içinde mi?
            if (distanceToPlayer <= detectionRange)
            {
                // Yarasa ve oyuncu arasında engel var mı kontrol et
                Vector2 direction = (PlayerManager.instance.player.transform.position - transform.position).normalized;
                RaycastHit2D hit = Physics2D.Raycast(
                    transform.position, 
                    direction, 
                    distanceToPlayer, 
                    whatIsGround // Engel katmanı
                );
                
                // Engel yoksa (ray yerle değil oyuncuyla çarpışıyorsa) true döndür
                if (hit.collider == null || hit.collider.CompareTag("Player"))
                {
                    return true;
                }
            }
        }
        
        return false;
    }

    public float GetFlightSpeed()
    {
        return flightSpeed;
    }
}
