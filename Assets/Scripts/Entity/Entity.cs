using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{
    public Rigidbody2D rb { get;private set; }
    public Animator anim {get;private set;}
    
    public EntityFX entityFX {get;private set;}
    public CharacterStats stats {get;private set;}
    
    [Header("Ground Check")]
    [SerializeField] public Transform groundCheck;
    [SerializeField] protected float groundCheckDistance;
    [SerializeField] public LayerMask whatIsGround;
    [SerializeField] protected LayerMask whatIsStair;
    
    [Header("Wall Check")]
    [SerializeField] protected Transform wallCheck;
    [SerializeField] protected float wallCheckDistance = 0;
    
    [Header("Flip Controller")]
    [HideInInspector]
    public int facingdir  = 1;
    protected bool facingright = true;
    
    [Header("KnockbackInfo")]
    [SerializeField] public Vector2 knockbackDirection;
    [SerializeField] public float secondComboKnockbackXMultiplier;
    [SerializeField] public float thirdComboKnockbackXMultiplier;
    [SerializeField] protected float knockbackDuration;

    public bool isKnocked;
    protected float originalMoveSpeed;
    protected float originalChaseSpeed;

    [Header("Combat")]
    [HideInInspector] public bool isAttackActive = false;
    [HideInInspector] public List<Entity> hitEntitiesInCurrentAttack = new List<Entity>();

    protected virtual void Awake() { }

    protected virtual void Start()
    {
        SetupComponents();
    }
    
    protected virtual void Update() { }

    private void SetupComponents()
    {
        anim = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody2D>();
        entityFX = GetComponent<EntityFX>();
        stats = GetComponent<CharacterStats>();
    }
    
    public virtual void Damage(Stat attackerDamageStat = null)
    {
        if (stats.isInvincible)
        {
            return;
        }
        
        entityFX.StartCoroutine("HitFX");
        
        // Only apply old knockback system if this is not a Player (backward compatibility)
        if (!(this is Player))
        {
            StartCoroutine("HitKnockback", knockbackDirection);
        }
        
        Stat damageStat = attackerDamageStat;
        if (damageStat == null)
        {
            if (stats is PlayerStats playerStats)
                damageStat = playerStats.baseDamage;
            else if (stats is EnemyStats enemyStats)
                damageStat = enemyStats.enemyDamage;
        }
        stats.TakeDamage(0, CharacterStats.DamageType.Physical, damageStat);
    }
    
    public virtual void Die() { }

    #region Knockback

    public virtual void DamageWithoutKnockback(Stat attackerDamageStat = null)
    {
        if (stats.isInvincible)
        {
            return;
        }
        entityFX.StartCoroutine("HitFX");
        Stat damageStat = attackerDamageStat;
        if (damageStat == null)
        {
            if (stats is PlayerStats playerStats)
                damageStat = playerStats.baseDamage;
            else if (stats is EnemyStats enemyStats)
                damageStat = enemyStats.enemyDamage;
        }
        stats.TakeDamage(0, CharacterStats.DamageType.Physical, damageStat);
    }
    
    /// <summary>
    /// New knockback system that calculates direction based on attacker position
    /// </summary>
    /// <param name="attackerPosition">Position of the entity dealing damage</param>
    /// <param name="knockbackForce">Force of the knockback (default uses knockbackDirection.x)</param>
    /// <param name="knockbackHeight">Vertical force of knockback (default uses knockbackDirection.y)</param>
    public virtual void ApplyKnockback(Vector2 attackerPosition, float knockbackForce = 0f, float knockbackHeight = 0f)
    {
        // Players don't get knocked back
        if (this is Player)
            return;
            
        // Use default values if not specified
        if (knockbackForce == 0f)
            knockbackForce = knockbackDirection.x;
        if (knockbackHeight == 0f)
            knockbackHeight = knockbackDirection.y;
            
        // Calculate knockback direction from attacker to this entity
        Vector2 knockbackDir = (transform.position - (Vector3)attackerPosition).normalized;
        
        // Apply horizontal knockback force
        knockbackDir.x = knockbackDir.x > 0 ? knockbackForce : -knockbackForce;
        // Apply vertical knockback force
        knockbackDir.y = knockbackHeight;
        
        StartCoroutine(HitKnockback(knockbackDir));
    }
    
    /// <summary>
    /// Enhanced knockback system with combo multipliers
    /// </summary>
    /// <param name="attackerPosition">Position of the entity dealing damage</param>
    /// <param name="comboCounter">Current combo count (0 = first hit, 1 = second hit, 2 = third hit)</param>
    public virtual void ApplyComboKnockback(Vector2 attackerPosition, int comboCounter = 0)
    {
        // Players don't get knocked back
        if (this is Player)
            return;
            
        float knockbackForce = knockbackDirection.x;
        float knockbackHeight = knockbackDirection.y;
        
        // Apply combo multipliers
        switch (comboCounter)
        {
            case 1: // Second hit
                knockbackForce *= secondComboKnockbackXMultiplier;
                break;
            case 2: // Third hit
                knockbackForce *= thirdComboKnockbackXMultiplier;
                break;
        }
        
        ApplyKnockback(attackerPosition, knockbackForce, knockbackHeight);
    }
    
    public virtual IEnumerator HitKnockback(Vector2 knockbackDirectionParam)
    {
        isKnocked = true;

        // Apply knockback force directly
        rb.linearVelocity = knockbackDirectionParam;
        yield return new WaitForSeconds(knockbackDuration);
        isKnocked = false;
        
        // Reset velocity after knockback
        rb.linearVelocity = Vector2.zero;
    }

    #endregion

    #region Collider Checks

    public virtual bool IsGroundDetected()
    {
       return Physics2D.Raycast(groundCheck.position,Vector2.down,groundCheckDistance,whatIsGround);
    }
    
    public virtual bool IsWallDetected() => Physics2D.Raycast(wallCheck.position, Vector2.right * facingdir, wallCheckDistance, whatIsGround);

    #endregion
    
    #region Velocity
    
    public void SetVelocity(float xVelocity, float yVelocity)
    {
        if (isKnocked)
        {
            return;
        }
        
        rb.linearVelocity = new Vector2(xVelocity, yVelocity);
        FlipController(xVelocity);
    }
   
    public void SetZeroVelocity()
    {
        if (isKnocked)
        {
            return;
        }
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
    }
    
    #endregion
   
    #region Flip

    public virtual void FlipController(float _x)
    {
        if (_x > 0 && !facingright)
            Flip();
        else if (_x < 0 && facingright)
            Flip();
    }
    
    public virtual void Flip()
    {
        // Simple flip - just change direction and rotate sprite
        facingdir *= -1;
        facingright = !facingright;
        transform.Rotate(0, 180, 0);
    }
    
    public virtual void SetupDefaultFacingDir(int _direction)
    {
        facingdir = _direction;
        facingright = (facingdir == 1);
    }

    #endregion
  
    #region BurnEffect

    public virtual void ApplyBurnEffect()
    {
        if (this is Enemy enemy)
        {
            originalMoveSpeed = enemy.moveSpeed;
            originalChaseSpeed = enemy.chaseSpeed;
            
            enemy.moveSpeed *= 0.5f;
            enemy.chaseSpeed *= 0.5f;
        }
    }

    public virtual void RemoveBurnEffect()
    {
        if (this is Enemy enemy)
        {
            enemy.moveSpeed = originalMoveSpeed;
            enemy.chaseSpeed = originalChaseSpeed;
        }
    }

    #endregion

    #region IceEffect

    private Coroutine currentIceEffectCoroutine;

    public virtual void ApplyIceEffect()
    {
        if (this is Enemy enemy)
        {
            // Eğer zaten bir buz efekti varsa, onu durdur
            if (currentIceEffectCoroutine != null)
            {
                StopCoroutine(currentIceEffectCoroutine);
                // Önceki efektin hızlarını sıfırla
                RemoveIceEffect();
            }

            originalMoveSpeed = enemy.moveSpeed;
            originalChaseSpeed = enemy.chaseSpeed;
            
            enemy.moveSpeed *= 0.3f;
            enemy.chaseSpeed *= 0.3f;

            // Yeni efekti başlat
            currentIceEffectCoroutine = StartCoroutine(RemoveIceEffectAfterDelay(1.5f));
        }
    }

    private IEnumerator RemoveIceEffectAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        RemoveIceEffect();
        currentIceEffectCoroutine = null;
    }

    public virtual void RemoveIceEffect()
    {
        if (this is Enemy enemy)
        {
            enemy.moveSpeed = originalMoveSpeed;
            enemy.chaseSpeed = originalChaseSpeed;
        }
    }

    #endregion

    #region HitList

    /// <summary>
    /// Clears the list of entities hit in the current attack
    /// </summary>
    public void ClearHitEntities()
    {
        hitEntitiesInCurrentAttack.Clear();
    }
    
    /// <summary>
    /// Checks if this entity has already hit the given entity during this attack
    /// </summary>
    public bool HasHitEntity(Entity entity)
    {
        return hitEntitiesInCurrentAttack.Contains(entity);
    }
    
    /// <summary>
    /// Marks the entity as hit in the current attack
    /// </summary>
    public void MarkEntityAsHit(Entity entity)
    {
        if (!hitEntitiesInCurrentAttack.Contains(entity))
        {
            hitEntitiesInCurrentAttack.Add(entity);
        }
    }

    #endregion
    
    protected virtual void OnDrawGizmos()
    {
        Gizmos.DrawLine(groundCheck.position,
            new Vector3(groundCheck.position.x, groundCheck.position.y - groundCheckDistance));
        
        Gizmos.DrawLine(wallCheck.position, new Vector3(wallCheck.position.x + wallCheckDistance*facingdir, wallCheck.position.y));
    }
}
