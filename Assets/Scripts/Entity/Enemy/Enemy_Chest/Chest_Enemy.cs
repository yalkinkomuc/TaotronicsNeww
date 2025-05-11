using UnityEngine;
using System.Collections;

public class Chest_Enemy : Enemy,IInteractable
{

    #region States

    
    public EnemyChest_IdleState idleState {get; private set;}
    public EnemyChest_ChaseState chaseState {get; private set;}
    public EnemyChest_TransformState transformState {get; private set;}
    public EnemyChest_AttackState attackState {get; private set;}
    public EnemyChest_Attack2State attack2State {get; private set;}
    public EnemyChest_DeadState deadState {get; private set;}
    public EnemyChest_PatrolState patrolState {get; private set;}
    

    #endregion

    #region Components

     public Rigidbody2D rb;

     public CapsuleCollider2D capsuleCollider;

     
     private int lastHitDirection = 0;
     
     [Header("AttackInfo")] 
     public float attackDistance = 2f;
     public float attackCooldown = 2f;
     public float battleTime = 3f;
     public Transform attackCheck;
     public Vector2 attackSize;
     
     [HideInInspector] public float lastTimeAttacked;
     
     
     private bool preventFlip = false;
     private float preventFlipTimer = 0f;
     private float preventFlipDuration = 1f; // Süreyi biraz daha uzattım
    
     [SerializeField] private float detectionRange = 15f;

    #endregion
    [SerializeField] private InteractionPrompt prompt;
    
    protected override void Awake()
    {
        base.Awake();
        
        
        capsuleCollider = GetComponent<CapsuleCollider2D>();
        rb = gameObject.GetComponent<Rigidbody2D>();
        
        SetupDefaultFacingDir(-1);

        idleState = new EnemyChest_IdleState(this, stateMachine, "Idle", this);
        chaseState = new EnemyChest_ChaseState(this, stateMachine, "Chase", this);
        transformState = new EnemyChest_TransformState(this, stateMachine, "Transform", this);
        attackState = new EnemyChest_AttackState(this, stateMachine, "Attack", this);
        attack2State = new EnemyChest_Attack2State(this, stateMachine, "Attack2", this);
        deadState = new EnemyChest_DeadState(this, stateMachine, "Death", this);
        patrolState = new EnemyChest_PatrolState(this, stateMachine, "Move", this);
    }

    protected override void Start()
    {
        base.Start();
        
        stateMachine.Initialize(idleState);
    }

    protected override void Update()
    {
        base.Update();
    }
    
    public override void Damage()
    {
        if (stats.isInvincible)
        {
            return;
        }
        
        // Oyuncunun pozisyonunu kullanarak çarpma yönünü belirle
        if (player != null)
        {
            // Oyuncunun boğaya göre hangi tarafta olduğunu belirle
            // Oyuncu sağdaysa 1, soldaysa -1
            lastHitDirection = (player.transform.position.x > transform.position.x) ? 1 : -1;
        }
        else
        {
            // Oyuncu referansı yoksa, varsayılan olarak facingdir'in tersini kullan
            lastHitDirection = -facingdir;
        }
        
        entityFX.StartCoroutine("HitFX");
        StartCoroutine("HitKnockback", knockbackDirection);
        
        Debug.Log(gameObject.name + " was damaged ");
        stats.TakeDamage(stats.baseDamage.GetValue());
    }

    public override IEnumerator HitKnockback(Vector2 knockbackDirectionParam)
    {
        isKnocked = true;
        preventFlip = true;
        preventFlipTimer = preventFlipDuration;
        
        // Oyuncunun pozisyonuna göre hesaplanan yönü kullan
        // lastHitDirection = -1 ise oyuncu solda, boğa sağa doğru knockback almalı
        // lastHitDirection = 1 ise oyuncu sağda, boğa sola doğru knockback almalı
        float xForce = knockbackDirectionParam.x * -lastHitDirection;
        
        // Knockback uygula ama kesinlikle yön değiştirme
        rb.linearVelocity = new Vector2(xForce, knockbackDirectionParam.y);
        
        yield return new WaitForSeconds(knockbackDuration);
        isKnocked = false;
        
        // Knockback bittikten sonra hızı sıfırla
        rb.linearVelocity = Vector2.zero;
    }

    public override void Die()
    {
        base.Die();
        
        stateMachine.ChangeState(deadState);
    }

    public void Interact()
    {
        if (PlayerManager.instance?.player != null)
        {
           stateMachine.ChangeState(transformState);
        }
    }

    public void ShowInteractionPrompt()
    {
        if (prompt != null)
            prompt.ShowPrompt();
    }

    public void HideInteractionPrompt()
    {
        if (prompt != null)
            prompt.HidePrompt();
    }
    
    private void OnCollisionStay2D(Collision2D other)
    {
        Player player = other.gameObject.GetComponent<Player>();

        if (player != null)
        {
            player.Damage();
        }
    }
    
    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position,new Vector3(transform.position.x + attackDistance*facingdir,transform.position.y));
        
        Gizmos.DrawWireCube(attackCheck.position,attackSize);
    }
}
