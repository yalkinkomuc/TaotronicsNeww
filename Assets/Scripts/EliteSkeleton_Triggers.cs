using UnityEngine;

public class EliteSkeleton_Triggers : EnemyAnimationTriggers
{
    
    private EliteSkeleton_Enemy enemyEliteSkeleton => GetComponentInParent<EliteSkeleton_Enemy>();
    protected override void AnimationTrigger()
    {
        base.AnimationTrigger();

        if (enemyEliteSkeleton != null)
        {
            enemyEliteSkeleton.AnimationFinishTrigger();
        }
    }

    private void AttackTrigger()
    {
        Collider2D[] colliders = Physics2D.OverlapBoxAll(enemyEliteSkeleton.attackCheck.position,enemyEliteSkeleton.attackSize,0,enemyEliteSkeleton.whatIsPlayer);

        foreach (var hit in colliders)
        {
            if (hit.GetComponent<Player>())
            {
                float currentDamage = enemyEliteSkeleton.stats.baseDamage.GetValue();
                hit.GetComponent<Player>().Damage();
                hit.GetComponent<PlayerStats>().TakeDamage(currentDamage);
            }
        }
    }
}
