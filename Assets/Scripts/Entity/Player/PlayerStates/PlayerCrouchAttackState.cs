using UnityEngine;

public class PlayerCrouchAttackState : PlayerState
{
    public PlayerCrouchAttackState(Player _player, PlayerStateMachine _stateMachine, string _animBoolName) : base(_player, _stateMachine, _animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();
        
        // Saldırı başlamadan önce yakındaki düşmana doğru bak
        FaceNearestEnemy();
        
        // Yeni saldırı başlatıldığında vurulan entityleri sıfırla
        player.StartNewAttack();
    }

    public override void Update()
    {
        base.Update();
        
        player.SetZeroVelocity();

        if (triggerCalled)
        {
            stateMachine.ChangeState(player.idleState);
        }
    }

    public override void Exit()
    {
        base.Exit();
    }
    
    /// <summary>
    /// Saldırı alanında düşman varsa, en yakın düşmana doğru yüzü çevir
    /// </summary>
    private void FaceNearestEnemy()
    {
        // Çömelme saldırısı için saldırı pozisyonunu hesapla
        Vector2 attackPosition = (Vector2)player.attackCheck.position + player.crouchAttackOffset;
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
