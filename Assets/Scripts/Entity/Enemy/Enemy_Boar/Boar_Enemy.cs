using System;
using UnityEngine;
using System.Collections;

public class Boar_Enemy : Enemy
{

    #region States

    public Boar_IdleState idleState { get; private set; }
    public Boar_MoveState moveState { get; private set; }
    public Boar_ChaseState chaseState { get; private set; }
    
    // Flag to prevent flipping during knockback and for a short time after
    private bool preventFlip = false;
    private float preventFlipTimer = 0f;
    private float preventFlipDuration = 1f; // Süreyi biraz daha uzattım

    // Knockback'te kullanmak için yön bilgisi
    private int lastHitDirection = 0;

    #endregion
    protected override void Awake()
    {
        base.Awake();
        
        SetupDefaultFacingDir(1);

        idleState = new Boar_IdleState(this, stateMachine, "Idle", this);
        moveState = new Boar_MoveState(this, stateMachine, "Move", this);
        chaseState = new Boar_ChaseState(this, stateMachine, "Chase", this);
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

    private void OnCollisionStay2D(Collision2D other)
    {
        Player player = other.gameObject.GetComponent<Player>();

        if (player != null)
        {
            player.Damage();
        }
    }
}
