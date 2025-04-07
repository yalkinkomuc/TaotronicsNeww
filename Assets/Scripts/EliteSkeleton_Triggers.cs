using UnityEngine;

public class EliteSkeleton_Triggers : EnemyAnimationTriggers
{
    private EliteSkeleton_Enemy enemyEliteSkeleton => GetComponentInParent<EliteSkeleton_Enemy>();
    
    protected override void AnimationTrigger()
    {
        base.AnimationTrigger();
    }
    
    protected override void EnableAttackCollider()
    {
        if (enemyEliteSkeleton != null)
        {
            enemyEliteSkeleton.isAttackActive = true;
        }
    }
    
    protected override void DisableAttackCollider()
    {
        if (enemyEliteSkeleton != null)
        {
            enemyEliteSkeleton.isAttackActive = false;
        }
    }

    protected override void AttackTrigger()
    {
        // Düşman yoksa veya saldırı aktif değilse işlem yapma
        if (enemyEliteSkeleton == null || !enemyEliteSkeleton.isAttackActive)
            return;
            
        Collider2D[] colliders = Physics2D.OverlapBoxAll(
            enemyEliteSkeleton.attackCheck.position,
            enemyEliteSkeleton.attackSize,
            0,
            enemyEliteSkeleton.whatIsPlayer);

        foreach (var hit in colliders)
        {
            Player player = hit.GetComponent<Player>();
            if (player != null)
            {
                // Bu oyuncuya zaten vurduk mu kontrol et
                if (enemyEliteSkeleton.HasHitEntity(player))
                    continue;  // Zaten vurmuşsak atla
                    
                // Bu oyuncuyu vurulmuş olarak işaretle
                enemyEliteSkeleton.MarkEntityAsHit(player);
                
                // Hasar ver
                float currentDamage = enemyEliteSkeleton.stats.baseDamage.GetValue();
                player.Damage();
                hit.GetComponent<PlayerStats>().TakeDamage(currentDamage);
            }
        }
    }
}
