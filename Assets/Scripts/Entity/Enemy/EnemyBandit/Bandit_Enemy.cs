using UnityEngine;

public class Bandit_Enemy : Enemy, IParryable
{

    #region States

    public Bandit_IdleState idleState {get; private set;}
    public Bandit_MoveState moveState {get; private set;}
    public Bandit_BattleState battleState {get; private set;}
    public Bandit_AttackState attackState {get; private set;}
    public Bandit_DeadState deadState {get; private set;}
    public Bandit_StunnedState stunnedState {get; private set;}
    
    

    #endregion
    
    
    [Header("AttackInfo")] 
    public float attackDistance;
    public float attackCooldown;
    public float battleTime;

    public Transform attackCheck;
    public Vector2 attackSize;

    
    [HideInInspector] public float lastTimeAttacked;
    
    
    [Header("Parry System")]
    [HideInInspector] public bool isParryWindowOpen = false; // Parry penceresi aktif mi?
    
    [SerializeField] public float parryStunDuration = 1.5f; // Parry sonrası düşmanın sersemleme süresi

    // IParryable implementasyonu
    public bool IsParryWindowOpen => isParryWindowOpen;
    
    
    protected override void Awake()
    {
        base.Awake();
        
        SetupDefaultFacingDir(1);
        
        idleState = new Bandit_IdleState(this,stateMachine,"Idle",this);
        moveState = new Bandit_MoveState(this,stateMachine,"Move",this);
        battleState = new Bandit_BattleState(this,stateMachine,"Chase",this);
        attackState = new Bandit_AttackState(this,stateMachine,"Attack",this);
        deadState = new Bandit_DeadState(this,stateMachine,"Death",this);
        stunnedState = new Bandit_StunnedState(this,stateMachine,"Stunned",this);
        
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
        // Eğer oyuncu succesfulParryState'de değilse (yani sadece block yapıyorsa), stun uygulama
        
        
        
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
        
        Debug.Log("bandit was parried!");
        
        // Parry knockback'i uygula - düşmanın her zaman arkaya (baktığı yönün tersine) doğru savrulması için
        // Knockback'i karakterin baktığı yönün tersine uygula (facing direction)
        Vector2 parryKnockbackForce = new Vector2(knockbackDirection.x * -facingdir * 1.5f, knockbackDirection.y * 0.8f);
        
        // Knockback uygula
        StartCoroutine(HitKnockback(parryKnockbackForce));
        
        // Sersemleme durumuna geç
        stateMachine.ChangeState(stunnedState);
    }

    public Transform GetTransform()
    {
        return this.transform;
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
    }
}
