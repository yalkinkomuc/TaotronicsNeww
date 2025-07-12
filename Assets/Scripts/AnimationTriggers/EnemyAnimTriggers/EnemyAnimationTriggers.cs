using UnityEngine;

public class EnemyAnimationTriggers : MonoBehaviour
{
    protected Enemy enemy => GetComponentInParent<Enemy>();
    
    protected virtual void AnimationTrigger()
    {
        if (enemy != null)
        {
            enemy.AnimationFinishTrigger();
        }
    }
    
    // Attack hitbox'ını aktif et - Animation Event tarafından çağrılır
    protected virtual void EnableAttackCollider()
    {
        if (enemy != null)
        {
            // Saldırı aktif olarak işaretle
            enemy.isAttackActive = true;
        }
    }
    
    // Attack hitbox'ını deaktif et - Animation Event tarafından çağrılır
    protected virtual void DisableAttackCollider()
    {
        if (enemy != null)
        {
            // Saldırıyı deaktif yap
            enemy.isAttackActive = false;
        }
    }
    
    // Bu metot tüm düşman türleri için overridable base implementasyon sağlar
    // Alt sınıflarda override edilebilir
    protected virtual void AttackTrigger()
    {
        // Düşman yoksa veya saldırı aktif değilse işlem yapma
        if (enemy == null || !enemy.isAttackActive)
            return;
    }
} 