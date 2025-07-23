using UnityEngine;
using UnityEngine.Rendering;
using System.Collections;

public class EliteSkeleton_Enemy : Enemy, IParryable
{
    #region States

    
    public EliteSkeleton_IdleState idleState { get; private set; }
    public EliteSkeleton_MoveState moveState { get; private set; }
   
    public EliteSkeleton_BattleState battleState { get; private set; }
    public EliteSkeleton_AttackState attackState { get; private set; }
    public EliteSkeleton_DeadState deadState { get; private set; }
    public EliteSkeleton_StunnedState stunnedState { get; private set; }
    #endregion


    [Header("AttackInfo")] 
    public float attackDistance;
    public float attackCooldown;
    public float battleTime;

    public Transform attackCheck;
    public Vector2 attackSize;

    
    [Header("Parry System")]
    [HideInInspector] public bool isParryWindowOpen = false; // Parry penceresi aktif mi?
    
    [SerializeField] public float parryStunDuration = 1.5f; // Parry sonrası düşmanın sersemleme süresi

    // IParryable implementasyonu
    public bool IsParryWindowOpen => isParryWindowOpen;

    public Transform GetTransform()
    {
        return this.transform;
    }
    
    [HideInInspector] public float lastTimeAttacked;
    
    
    protected override void Awake()
    {
        base.Awake();
        
        SetupDefaultFacingDir(1);
        
      
        idleState = new EliteSkeleton_IdleState(this,stateMachine,"Idle",this);
        moveState = new EliteSkeleton_MoveState(this,stateMachine,"Move",this);
        battleState = new EliteSkeleton_BattleState(this,stateMachine,"Move",this);
        attackState = new EliteSkeleton_AttackState(this,stateMachine,"Attack",this);
        deadState = new EliteSkeleton_DeadState(this,stateMachine,"Death",this);
        stunnedState = new EliteSkeleton_StunnedState(this,stateMachine,"Stunned",this);
        
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
    
    public void GetParried()
    {
        // Parry yediğinde saldırı ve parry collider'larını devre dışı bırak
        isAttackActive = false;
        isParryWindowOpen = false;
        
        // Düşmanın hızını hemen sıfırla
        SetZeroVelocity();
        
        // Sersemlemiş efekti
        if (entityFX != null)
        {
            entityFX.StartCoroutine("HitFX");
        }
      
        // Parry knockback'i uygula - düşmanın her zaman arkaya (baktığı yönün tersine) doğru savrulması için
        Vector2 parryKnockbackForce = new Vector2(knockbackDirection.x * -facingdir * 1.5f, knockbackDirection.y * 0.8f);
        
        // Knockback uygula
        StartCoroutine(HitKnockback(parryKnockbackForce));
        
        // Sersemleme durumuna geç
        stateMachine.ChangeState(stunnedState);
    }
    
    public override void Die()
    {
        base.Die();
        
        stateMachine.ChangeState(deadState);
    }
    
    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position,new Vector3(transform.position.x + attackDistance*facingdir,transform.position.y));
        
        // Parry penceresi aktifse farklı renk göster
        if (isParryWindowOpen)
        {
            Gizmos.color = Color.cyan;
        }
        else
        {
            Gizmos.color = Color.red;
        }
        
        Gizmos.DrawWireCube(attackCheck.position,attackSize);
        
        // Görüş mesafesi gösterimi
        Gizmos.color = new Color(0.2f, 0.9f, 0.3f, 0.5f);
        Gizmos.DrawWireSphere(transform.position, detectDistance);
        
        // Yakın mesafe gösterimi
        Gizmos.color = new Color(0.9f, 0.2f, 0.2f, 0.5f);
        Gizmos.DrawWireSphere(transform.position,tooCloseRadius);
    }
}
