using System.Collections;
using UnityEngine;

public class Entity : MonoBehaviour
{
    
    public Rigidbody2D rb { get;private set; }
    public Animator anim {get;private set;}
    
    public EntityFX entityFX {get;private set;}
    public CharacterStats stats {get;private set;}
    
    [Header("Ground Check")]
    [SerializeField] protected Transform groundCheck;
    [SerializeField] protected float groundCheckDistance;
    [SerializeField] protected LayerMask whatIsGround;
    
    [Header("Wall Check")]
    [SerializeField] protected Transform wallCheck;
    [SerializeField] protected float wallCheckDistance = 0;
    
    [Header("Flip Controller")]
    public int facingdir  = 1;
    protected bool facingright = true;
    
    [Header("KnockbackInfo")]
    [SerializeField] protected Vector2 knockbackDirection;
    [SerializeField] protected float knockbackDuration;

    protected bool isKnocked;
    protected float originalMoveSpeed;
    protected float originalChaseSpeed;

    protected virtual void Awake()
    {
        
    }

    protected virtual void Start()
    {
        anim = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody2D>();
        entityFX = GetComponent<EntityFX>();
        stats = GetComponent<CharacterStats>();
    }

    protected virtual void Update()
    {
        
    }

  
    public virtual void Damage()
    {
        if (stats.isInvincible)
        {
            return;
        }
        
        entityFX.StartCoroutine("HitFX");
        StartCoroutine("HitKnockback",knockbackDirection);
       
        Debug.Log(gameObject.name + " was damaged ");
        stats.TakeDamage(1);
    }

    public virtual void DamageWithoutKnockback()
    {

        if (stats.isInvincible)
        {
            return;
        }
        entityFX.StartCoroutine("HitFX");
        Debug.Log(gameObject.name + " was damaged without knockback ");
        stats.TakeDamage(1);
    }
    
    
    protected virtual IEnumerator HitKnockback(Vector2 knockbackDirectionParam)
    {
       
        isKnocked = true;


        
        rb.linearVelocity = new Vector2(knockbackDirectionParam.x * -facingdir, knockbackDirectionParam.y);
        yield return new WaitForSeconds(knockbackDuration);
        isKnocked = false;
    }
    
    
    public virtual void Die()
    {

    }
    
    
    public virtual bool IsGroundDetected() => Physics2D.Raycast(groundCheck.position,Vector2.down,groundCheckDistance,whatIsGround);
    public virtual bool IsWallDetected() => Physics2D.Raycast(wallCheck.position, Vector2.right * facingdir, wallCheckDistance, whatIsGround);
    

    
    public void SetVelocity(float xVelocity, float yVelocity)
    {

        if (isKnocked)
        {
            return;
        }
        
        rb.linearVelocity = new Vector2(xVelocity, yVelocity);
        
        FlipController(xVelocity);
    }
   

    public void SetZeroVelocity ()
    {
        if (isKnocked)
        {
            return;
        }
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
    }
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
        facingdir = facingdir * -1;
        facingright = !facingright;
        transform.Rotate(0, 180, 0);

    }
    
    public virtual void SetupDefaultFacingDir (int _direction)
    {
        facingdir = _direction;

        if (facingdir == -1)
            facingright = false;
    }


    #endregion
   
    
    protected virtual void OnDrawGizmos()
    {
        Gizmos.DrawLine(groundCheck.position,
            new Vector3(groundCheck.position.x, groundCheck.position.y - groundCheckDistance));
        
        Gizmos.DrawLine(wallCheck.position, new Vector3(wallCheck.position.x + wallCheckDistance*facingdir, wallCheck.position.y));
    }

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
}
