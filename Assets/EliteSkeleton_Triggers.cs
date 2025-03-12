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
}
