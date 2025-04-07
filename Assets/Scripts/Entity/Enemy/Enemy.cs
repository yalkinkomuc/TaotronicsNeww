using System;
using UnityEngine;
using System.Collections;

public class Enemy : Entity
{
   
   // public Animator animator { get;private set; }
   // public Rigidbody2D rb { get;private set; }
   
   public BoxCollider2D boxCollider { get; private set; }
   
   protected SpriteRenderer spriteRenderer;
   public Player player { get; protected set; }
   public EnemyStateMachine stateMachine { get; private set; }

   [SerializeField] public LayerMask whatIsPlayer;
   public float idleTime;
   public float moveSpeed;
   public float chaseSpeed;
   [SerializeField] private float tooCloseDistance = 3f; // Çok yakın mesafe
   
   public float detectDistance;
   [SerializeField] protected float patrolDistance = 5f; // Patrol mesafesi
   [SerializeField] protected float patrolSpeed = 2f; // Patrol hızı
   public Vector2 startPosition { get; set; } // Başlangıç pozisyonu
   protected int patrolDirection = 1; // 1: sağ, -1: sol

   [HideInInspector] public bool fightBegun = false;
   
   [SerializeField] protected bool isDefeated = false;

   protected override void Awake()
   {
      base.Awake();
      boxCollider = GetComponent<BoxCollider2D>();
      spriteRenderer = GetComponentInChildren<SpriteRenderer>();
      stateMachine = new EnemyStateMachine();
     
   }

   protected override void Start()
   {
      base.Start();
      player = PlayerManager.instance.player;
      
     
   }

   protected override void Update()
   {
      base.Update();
      stateMachine.currentState.Update();
      
      //Debug.Log(fightBegun);
      
      
   }

   public override void Die()
   {
       base.Die();
       
       if (!isDefeated)
       {
           isDefeated = true;
           
           // Düşman öldüğünde event'i tetikle
           GameEvents.EnemyDefeated(this);
           
           // Gerekli animasyonları oynat ve birkaç saniye sonra yok et
           StartCoroutine(DestroyAfterDelay(2f));
       }
   }
   
   private IEnumerator DestroyAfterDelay(float delay)
   {
       yield return new WaitForSeconds(delay);
       Destroy(gameObject);
   }

   public void AnimationFinishTrigger() => stateMachine.currentState.AnimationFinishTrigger();

   public RaycastHit2D IsPlayerDetected() 
   {
       if (player != null)
       {
           return Physics2D.Raycast(wallCheck.position, Vector2.right * facingdir, detectDistance, whatIsPlayer);
       }
       return new RaycastHit2D();
   }
   
   public void FacePlayer()
   {
       if (player != null)
       {
           float xDirection = player.transform.position.x - transform.position.x;
           if ((xDirection > 0 && facingdir == -1) || (xDirection < 0 && facingdir == 1))
           {
               Flip();
           }
       }
   }
   
   public bool IsTooCloseToPlayer()
   {

       if (player != null)
       {
           if (Vector2.Distance(transform.position, player.transform.position) <= tooCloseDistance)
           {
               return true;
           }
           else
           {
               return false;
           }
       }
       return false;
        
   }

   public virtual bool CheckForBattleTransition()
   {
       if (IsPlayerDetected() || IsTooCloseToPlayer())
       {
           fightBegun = true;
           return true;
       }
       return false;
   }

   public virtual bool ShouldFlipPatrol()
   {
       // Duvara çarpma kontrolü
       if (IsWallDetected())
           return true;
           
       // Uçurum kontrolü
       if (!IsGroundDetected())
           return true;
           
       // Mesafe kontrolü
       float distanceFromStart = transform.position.x - startPosition.x;
       if ((distanceFromStart >= patrolDistance && patrolDirection == 1) || 
           (distanceFromStart <= -patrolDistance && patrolDirection == -1))
           return true;
           
       return false;
   }
   
   public virtual void UpdatePatrol()
   {
       if (ShouldFlipPatrol())
       {
           patrolDirection *= -1;
           Flip();
       }
       
       SetVelocity(patrolDirection * patrolSpeed, rb.linearVelocity.y);
   }

   public override void Damage()
   {
       if (stats.isInvincible)
       {
           return;
       }
       
       entityFX.StartCoroutine("HitFX");
       
       // Oyuncunun konumuna göre knockback yönünü hesapla
       Vector2 knockbackDir = knockbackDirection;
       if (player != null)
       {
           // Oyuncunun düşmana göre konumunu belirle
           float playerDirection = player.transform.position.x - transform.position.x;
           int knockbackDirMult = playerDirection > 0 ? -1 : 1; // Oyuncunun ters yönüne knockback
           
           // Yönü ayarla
           knockbackDir = new Vector2(knockbackDirection.x * knockbackDirMult, knockbackDirection.y);
       }
       
       // Knockback uygula
       StartCoroutine(HitKnockback(knockbackDir));
      
       Debug.Log(gameObject.name + " was damaged ");
       stats.TakeDamage(stats.baseDamage.GetValue());
   }
}
