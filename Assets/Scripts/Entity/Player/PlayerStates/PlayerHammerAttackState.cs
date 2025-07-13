using UnityEngine;

public class PlayerHammerAttackState : PlayerState, IWeaponAttackState
{
    protected int hammerComboCounter = 0;
    private float comboWindow = 3f;
    
    private float lastTimeAttacked;
   
    
    // Hammer'a özel özellikler
    
    private float hammerKnockbackMultiplier = 1.5f; // Daha güçlü knockback
   
    

    public int GetComboCounter() => hammerComboCounter;
    
    public virtual WeaponType GetWeaponType() => WeaponType.Hammer;
    
    public virtual float GetDamageMultiplier(int comboIndex)
    {
        // Hammer has different damage progression - more powerful but slower
        switch (comboIndex)
        {
            case 0:
                return 1.2f; // İlk saldırı için daha güçlü hasar
            case 1:
                return 1.5f; // İkinci saldırı için daha da güçlü
            case 2:
                return 2.0f; // Üçüncü saldırı için çok güçlü
            default:
                return 1.2f;
        }
    }
    
    public virtual float GetKnockbackMultiplier() => hammerKnockbackMultiplier;
    
    public virtual float GetComboWindow() => comboWindow;

    public PlayerHammerAttackState(Player _player, PlayerStateMachine _stateMachine, string _animBoolName) : base(_player, _stateMachine, _animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();

        if (hammerComboCounter > 2 || Time.time >= lastTimeAttacked + comboWindow)
        {
            hammerComboCounter = 0;
        }

        
        
        // Saldırı başlamadan önce yakındaki düşmana doğru bak
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
            player.SetVelocity(rb.linearVelocity.x*.5f,rb.linearVelocity.y);
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
    
   
  
    
    /// <summary>
    /// Hammer'a özel knockback katsayısı
    /// </summary>
    public float GetHammerKnockbackMultiplier()
    {
        return hammerKnockbackMultiplier;
    }
} 