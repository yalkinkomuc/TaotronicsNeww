using System;
using UnityEngine;

public class Skeleton_Enemy : Enemy
{
    #region States

    public SkeletonIdleState idleState { get; private set; }
    public SkeletonMoveState moveState {get; private set;}
    public SkeletonBattleState battleState {get; private set;}
    
    public SkeletonDeadState deadState {get; private set;}
    
    #endregion

    private bool isSummonedByNecromancer = false;

    public float battleTime = 4f;
    protected override void Awake()
    {
        base.Awake();
        
        SetupDefaultFacingDir(1);
        
        idleState = new SkeletonIdleState(this, stateMachine, "Idle",this);
        moveState = new SkeletonMoveState(this, stateMachine, "Move",this);
        battleState = new SkeletonBattleState(this, stateMachine, "Move",this);
        deadState = new SkeletonDeadState(this, stateMachine, "Death",this);
    }

    protected override void Start()
    {
        base.Start();
        
        if (isSummonedByNecromancer)
        {
            stateMachine.Initialize(battleState);
        }
        else
        {
            stateMachine.Initialize(idleState);
        }
    }

    protected override void Update()
    {
        base.Update();
        
       
    }

    public override void Die()
    {
        base.Die();
        
        stateMachine.ChangeState(deadState);
    }

    private void OnCollisionStay2D(Collision2D other) 
    {
        Player player = other.gameObject.GetComponent<Player>();

        if (player != null)
        {
            // Apply contact damage
            var enemyStats = stats as EnemyStats;
            if (enemyStats != null)
                player.TakePlayerDamage(enemyStats.enemyDamage, CharacterStats.DamageType.Physical);
            else
                player.TakePlayerDamage(null, CharacterStats.DamageType.Physical);
        }
    }

    public void SetSummonedByNecromancer(bool value)
    {
        isSummonedByNecromancer = value;
    }
}
