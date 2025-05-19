using System;
using UnityEngine;
using System.Collections;

public class Boar_Enemy : Enemy
{

    #region States

    public Boar_IdleState idleState { get; private set; }
    public Boar_MoveState moveState { get; private set; }
    public Boar_ChaseState chaseState { get; private set; }
    public Boar_ChargeState chargeState { get; private set; }
    public Boar_AttackState attackState { get; private set; }
    public Boar_DeadState deadState { get; private set; }



    // Flag to prevent flipping during knockback and for a short time after
    private bool preventFlip = false;
    private float preventFlipTimer = 0f;
    private float preventFlipDuration = 1f; // Süreyi biraz daha uzattım
    
    [SerializeField] private float detectionRange = 15f;

    // Knockback'te kullanmak için yön bilgisi
    private int lastHitDirection = 0;

    #endregion
    
    
    [Header("AttackInfo")] 
    public float attackDistance = 2f;
    public float attackCooldown = 2f;
    public float battleTime = 10f;
    public Transform attackCheck;
    public Vector2 attackSize;
    
    
    
    [HideInInspector] public float lastTimeAttacked;
    
    protected override void Awake()
    {
        base.Awake();
        
        SetupDefaultFacingDir(1);

        idleState = new Boar_IdleState(this, stateMachine, "Idle", this);
        moveState = new Boar_MoveState(this, stateMachine, "Move", this);
        chaseState = new Boar_ChaseState(this, stateMachine, "Chase", this);
        chargeState = new Boar_ChargeState(this, stateMachine, "Charge", this);
        attackState = new Boar_AttackState(this, stateMachine,"Attack",this);
        deadState = new Boar_DeadState(this, stateMachine, "Death", this);
    }

    protected override void Start()
    {
        base.Start();
        
        stateMachine.Initialize(idleState);
    }

    protected override void Update()
    {
        base.Update();
        
        // Update the flip prevention timer
        if (preventFlip && !isKnocked)
        {
            preventFlipTimer -= Time.deltaTime;
            if (preventFlipTimer <= 0)
            {
                preventFlip = false;
            }
        }
    }

    public override void Die()
    {
        base.Die();
        
        stateMachine.ChangeState(deadState);
    }
    
    // Animasyon olaylarını tetiklemek için kullanılacak
    // public void AnimationFinishTrigger()
    // {
    //     stateMachine.currentState.AnimationFinishTrigger();
    // }

    public override void Damage()
    {
        if (stats.isInvincible)
        {
            return;
        }
        
        // Oyuncunun pozisyonunu kullanarak çarpma yönünü belirle
        if (player != null)
        {
            lastHitDirection = (player.transform.position.x > transform.position.x) ? 1 : -1;
        }
        else
        {
            lastHitDirection = -facingdir;
        }
        
        entityFX.StartCoroutine("HitFX");
        StartCoroutine("HitKnockback", knockbackDirection);
        
        Debug.Log(gameObject.name + " was damaged ");
        if (stats is EnemyStats enemyStats)
            stats.TakeDamage(enemyStats.enemyDamage.GetValue(), CharacterStats.DamageType.Physical);
        else
            stats.TakeDamage(0, CharacterStats.DamageType.Physical);
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
    
    // Override FlipController to prevent automatic flipping during and after knockback
    public override void FlipController(float _x)
    {
        if (isKnocked || preventFlip)
            return; // Skip flipping if knocked or in prevention period
            
        // Only call base method if we're not preventing flips
        base.FlipController(_x);
    }
    
    // Boğanın dönmesini sadece belirli durumlarda izin ver
    public override void Flip()
    {
        // Eğer knockback veya preventFlip aktifse, dönmeyi engelle
        if (isKnocked || preventFlip)
            return;
            
        // Diğer durumlarda normal dönüş yap
        base.Flip();
    }
    
    public override bool CheckForBattleTransition()
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
