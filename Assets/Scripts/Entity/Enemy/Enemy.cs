using System;
using UnityEngine;
using System.Collections;

public class Enemy : Entity
{
   
   // public Animator animator { get;private set; }
   // public Rigidbody2D rb { get;private set; }


   [SerializeField] public string enemyType; // Inspector'dan değiştirilebilir
   
   public CapsuleCollider2D capsuleCollider { get; private set; }
   
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
      capsuleCollider = GetComponent<CapsuleCollider2D>();
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
           
           Debug.Log($"{enemyType} öldü");

           // Görev sistemi ile iletişim:
           QuestManager.instance?.RaiseEvent("EnemyKilled", enemyType);
           
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

   /// <summary>
   /// Düşmanın yön değiştirmesi gerekip gerekmediğini kontrol eder.
   /// Duvar, uçurum veya maksimum patrol mesafesine ulaşılırsa true döner.
   /// </summary>
   public virtual bool ShouldFlipPatrol()
   {
       // Duvar kontrolü
       if (IsWallDetected())
           return true;
           
       // Zemin kontrolü (uçurum)
       if (!IsGroundDetected())
           return true;
           
       // Basit mesafe kontrolü
       float distanceFromStart = transform.position.x - startPosition.x;
       if ((distanceFromStart >= patrolDistance && patrolDirection == 1) || 
           (distanceFromStart <= -patrolDistance && patrolDirection == -1))
           return true;
           
       return false;
   }
   
   /// <summary>
   /// Düşmanın devriye davranışını günceller.
   /// Gerekirse yön değiştirir ve ilgili hızda hareket eder.
   /// </summary>
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
       
       // Knocked back in the opposite direction the enemy is facing
       Vector2 knockbackDir = new Vector2(knockbackDirection.x * -facingdir, knockbackDirection.y);
       
       // Knockback uygula
       StartCoroutine(HitKnockback(knockbackDir));
      
       Debug.Log(gameObject.name + " was damaged ");
       stats.TakeDamage(stats.baseDamage.GetValue());
   }

   /// <summary>
   /// Oyuncunun düşmandan belirli bir mesafe aşağıda olup olmadığını kontrol eder.
   /// </summary>
   /// <param name="minHeightDifference">Minimum yükseklik farkı (varsayılan: 1.5f)</param>
   /// <returns>Oyuncu düşmandan yeterince aşağıdaysa true, değilse false</returns>
   public virtual bool IsPlayerBelow(float minHeightDifference = 1.5f)
   {
       if (player == null)
           return false;
           
       // Düşman ve oyuncu arasındaki dikey mesafe farkını hesapla
       float heightDifference = transform.position.y - player.transform.position.y;
       
       // Eğer oyuncu düşmandan belirli bir mesafe daha aşağıdaysa true döndür
       return heightDifference >= minHeightDifference;
   }
}
