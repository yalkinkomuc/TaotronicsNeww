using UnityEngine;

public class PlayerAttackState : PlayerState, IWeaponAttackState
{
    protected int comboCounter = 0;
    
    private float lastTimeAttacked;
    private float comboWindow = .5f;

    public int GetComboCounter() => comboCounter;
    
    public virtual WeaponType GetWeaponType() => WeaponType.Sword;
    
    public virtual float GetDamageMultiplier(int comboIndex)
    {
        switch (comboIndex)
        {
            case 0:
                return 1.0f; // İlk saldırı için standart hasar
            case 1:
                return player.stats.secondComboDamageMultiplier.GetValue(); // İkinci saldırı
            case 2:
                return player.stats.thirdComboDamageMultiplier.GetValue(); // Üçüncü saldırı
            default:
                return 1.0f;
        }
    }
    
    public virtual float GetKnockbackMultiplier() => 1.0f; // Sword için standart knockback
    
    public virtual float GetComboWindow() => comboWindow;

    public PlayerAttackState(Player _player, PlayerStateMachine _stateMachine, string _animBoolName) : base(_player, _stateMachine, _animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();

        if (comboCounter > 2 || Time.time >= lastTimeAttacked + comboWindow)
        {
            comboCounter = 0;
        }
        
        // Saldırı başlamadan önce yakındaki düşmana doğru bak
        FaceNearestEnemy();
        
        player.anim.SetInteger("comboCounter", comboCounter);
        stateTimer = .1f;
        
        player.StartNewAttack();
    }

    public override void Exit()
    {
        base.Exit();
        comboCounter++;
        
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
            player.SetVelocity(rb.linearVelocity.x*.5f, rb.linearVelocity.y);
        }
        
       


        if (triggerCalled)
        {
            stateMachine.ChangeState(player.idleState);
        }
        
       
    }
    
    /// <summary>
    /// Saldırı alanında düşman varsa, en yakın düşmana doğru yüzü çevir
    /// </summary>
    private void FaceNearestEnemy()
    {
        // Saldırı alanında düşmanları kontrol et
        Vector2 attackPosition = player.attackCheck.position;
        Collider2D[] colliders = Physics2D.OverlapBoxAll(attackPosition, player.attackSize, 0, player.passableEnemiesLayerMask);
        
        Enemy nearestEnemy = null;
        float nearestDistance = float.MaxValue;
        
        // En yakın düşmanı bul
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
        
        // En yakın düşman bulunduysa ona doğru bak
        if (nearestEnemy != null)
        {
            float directionToEnemy = nearestEnemy.transform.position.x - player.transform.position.x;
            
            // Düşman sağdaysa ve oyuncu sola bakıyorsa, sağa dön
            if (directionToEnemy > 0 && player.facingdir == -1)
            {
                player.Flip();
            }
            // Düşman soldaysa ve oyuncu sağa bakıyorsa, sola dön
            else if (directionToEnemy < 0 && player.facingdir == 1)
            {
                player.Flip();
            }
        }
    }
}
