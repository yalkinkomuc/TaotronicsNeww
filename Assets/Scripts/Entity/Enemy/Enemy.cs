using System;
using UnityEngine;
using System.Collections;

public class Enemy : Entity
{
   [SerializeField] public string enemyType;
   
   public CapsuleCollider2D capsuleCollider { get; private set; }
   
   protected SpriteRenderer spriteRenderer;
   public Player player { get; protected set; }
   public EnemyStateMachine stateMachine { get; private set; }

   [SerializeField] public LayerMask whatIsPlayer;
   public float idleTime;
   public float moveSpeed;
   public float chaseSpeed;
   [SerializeField] protected float tooCloseDistance = 3f; // Minimum distance before enemy retreats
   
   public float detectDistance;
   [SerializeField] protected float patrolDistance = 5f; // Patrol distance
   [SerializeField] protected float patrolSpeed = 2f; // Patrol speed
   public Vector2 startPosition { get; set; } // Starting position
   protected int patrolDirection = 1; // 1: right, -1: left

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
   }

   public bool IsStairDetected() =>
       Physics2D.Raycast(groundCheck.position, Vector2.down, groundCheckDistance, whatIsStair);
   
   public override bool IsGroundDetected()
   {
       if (IsStairDetected())
       {
           return true;
       }
       return base.IsGroundDetected();
   }

   public override void Die()
   {
       base.Die();
       
       if (!isDefeated)
       {
           isDefeated = true;
           
           // Notify quest system
           QuestManager.instance?.RaiseEvent("EnemyKilled", enemyType);
           
           // Trigger enemy defeated event
           GameEvents.EnemyDefeated(this);
           
           // Play animations and destroy after delay
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
   /// Checks if the enemy should change direction during patrol.
   /// Returns true if it hit a wall, reached the edge of a platform, or reached maximum patrol distance.
   /// </summary>
   public virtual bool ShouldFlipPatrol()
   {
       // Wall check
       if (IsWallDetected())
           return true;
           
       // Ground check (edge detection)
       if (!IsGroundDetected())
           return true;
           
       // Distance check
       float distanceFromStart = transform.position.x - startPosition.x;
       if ((distanceFromStart >= patrolDistance && patrolDirection == 1) || 
           (distanceFromStart <= -patrolDistance && patrolDirection == -1))
           return true;
           
       return false;
   }
   
   /// <summary>
   /// Updates the enemy's patrol behavior.
   /// Changes direction if needed and moves at patrol speed.
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
       
       // Apply knockback
       StartCoroutine(HitKnockback(knockbackDir));
       
       stats.TakeDamage(stats.baseDamage.GetValue());
   }

   /// <summary>
   /// Checks if the player is significantly below the enemy
   /// </summary>
   /// <param name="minHeightDifference">Minimum height difference (default: 1.5f)</param>
   /// <returns>True if player is below the enemy by at least the minimum height</returns>
   public virtual bool IsPlayerBelow(float minHeightDifference = 1.5f)
   {
       if (player == null)
           return false;
           
       // Calculate vertical distance between enemy and player
       float heightDifference = transform.position.y - player.transform.position.y;
       
       // Return true if player is below the enemy by at least the minimum height
       return heightDifference >= minHeightDifference;
   }
}
