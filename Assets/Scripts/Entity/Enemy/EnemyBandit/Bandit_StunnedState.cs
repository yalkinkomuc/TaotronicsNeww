using UnityEngine;

public class Bandit_StunnedState : EnemyState
{
    private Bandit_Enemy enemy;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private Color stunnedColor = new Color(0.7f, 0.7f, 1f, 1f); // Hafif mavi ton

    public Bandit_StunnedState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Bandit_Enemy _enemy) : base(_enemyBase, _stateMachine, _animBoolName)
    {
        enemy = _enemy;
        
    }

    public override void Enter()
    {
        base.Enter();
        
        // Sprite renderer referansını al
        spriteRenderer = enemy.GetComponentInChildren<SpriteRenderer>();
        
        if (spriteRenderer != null)
        {
            // Orijinal rengi kaydet
            originalColor = spriteRenderer.color;
            
            // Stun rengi uygula
            spriteRenderer.color = stunnedColor;
        }
        
        // Stun süresini ayarla
        stateTimer = enemy.parryStunDuration;
        
        // Artık hızı sıfırlamıyoruz, çünkü knockback'i GetParried'de uyguluyoruz
        // enemy.SetZeroVelocity();
        
        // Saldırı collider'ını devre dışı bırak
        enemy.isAttackActive = false;
        enemy.isParryWindowOpen = false;
        
        // Stunned efekti eklenebilir (görsel gösterim için)
        if (enemy.entityFX != null)
        {
            // Flash efekti
            enemy.entityFX.StartCoroutine("HitFX");
        }
    }

    public override void Exit()
    {
        base.Exit();
        
        // Orijinal rengi geri yükle
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }
    }

    public override void Update()
    {
        base.Update();
        
        // Knockback bittiğinde (isKnocked false olduğunda) düşmanın hızını sıfırla
        if (!enemy.isKnocked)
        {
            enemy.SetZeroVelocity();
        }
        
        // Stun süresi bittiğinde battle state'e dön
        if (stateTimer < 0)
        {
            stateMachine.ChangeState(enemy.battleState);
        }
    }
}
