using UnityEngine;

public class PlayerHammerAttackState : PlayerState, IWeaponAttackState
{
    protected int hammerComboCounter = 0;
    private float lastTimeAttacked;

    public int GetComboCounter() => hammerComboCounter;
    
    public virtual WeaponType GetWeaponType() => WeaponType.Hammer;
    
    public virtual float GetDamageMultiplier(int comboIndex)
    {
        return HammerCalculations.GetDamageMultiplier(player, comboIndex);
    }
    
    public virtual float GetKnockbackMultiplier(int comboIndex) => HammerCalculations.GetKnockbackMultiplier(comboIndex);
    
    public virtual float GetComboWindow() => HammerCalculations.GetComboWindow();

    public PlayerHammerAttackState(Player _player, PlayerStateMachine _stateMachine, string _animBoolName) : base(_player, _stateMachine, _animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();

        if (hammerComboCounter > 2 || Time.time >= lastTimeAttacked + HammerCalculations.GetComboWindow())
        {
            hammerComboCounter = 0;
        }

        FaceNearestEnemy();
        
        player.anim.SetInteger("HammerComboCounter", hammerComboCounter);
        stateTimer = .1f;
        
        player.StartNewAttack();
    }

    public override void Exit()
    {
        base.Exit();
        hammerComboCounter++;
        lastTimeAttacked = Time.time;
    }

    public override void Update()
    {
        base.Update();

        if (stateTimer < 0f)
        {
            player.SetZeroVelocity();
        }
        else
        {
            player.SetVelocity(rb.linearVelocity.x * .5f, rb.linearVelocity.y);
        }

        if (triggerCalled)
        {
            stateMachine.ChangeState(player.idleState);
        }
    }
    
    private void FaceNearestEnemy()
    {
        Vector2 attackPosition = player.attackCheck.position;
        Collider2D[] colliders = Physics2D.OverlapBoxAll(attackPosition, player.attackSize, 0, player.passableEnemiesLayerMask);
        
        Enemy nearestEnemy = null;
        float nearestDistance = float.MaxValue;
        
        foreach (var hit in colliders)
        {
            Enemy enemy = hit.GetComponent<Enemy>();
            if (enemy != null)
            {
                float distance = Vector2.Distance(player.transform.position, enemy.transform.position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestEnemy = enemy;
                }
            }
        }
        
        if (nearestEnemy != null)
        {
            float directionToEnemy = nearestEnemy.transform.position.x - player.transform.position.x;
            
            if (directionToEnemy > 0 && player.facingdir == -1)
            {
                player.Flip();
            }
            else if (directionToEnemy < 0 && player.facingdir == 1)
            {
                player.Flip();
            }
        }
    }
} 