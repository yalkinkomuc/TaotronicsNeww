using System;
using UnityEngine;
using System.Collections;

public class Enemy : Entity
{
   
   // public Animator animator { get;private set; }
   // public Rigidbody2D rb { get;private set; }
   
   public BoxCollider2D collider { get; private set; }
   
   protected SpriteRenderer spriteRenderer;
   public Player player { get; protected set; }
   public EnemyStateMachine stateMachine { get; private set; }

   [SerializeField] protected LayerMask whatIsPlayer;
   public float idleTime;
   public float moveSpeed;
   public float chaseSpeed;
   [SerializeField] private float tooCloseDistance = 3f; // Çok yakın mesafe
   
   public float detectDistance;
   

   protected override void Awake()
   {
      base.Awake();
      collider = GetComponent<BoxCollider2D>();
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
      
      //Debug.Log(IsPlayerDetected().collider.gameObject.name);
   }

   public override void Die()
   {
       base.Die();
       
       
       
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

   
}
