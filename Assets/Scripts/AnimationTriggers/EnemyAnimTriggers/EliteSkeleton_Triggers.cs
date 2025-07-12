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
    
    // Parry penceresi açma - Animation Event tarafından çağrılacak
    private void OpenParryWindow()
    {
        if (enemyEliteSkeleton != null)
        {
            enemyEliteSkeleton.isParryWindowOpen = true;
            //Debug.Log("Parry window opened!");
        }
    }
    
    // Parry penceresi kapatma - Animation Event tarafından çağrılacak
    private void CloseParryWindow()
    {
        if (enemyEliteSkeleton != null)
        {
            enemyEliteSkeleton.isParryWindowOpen = false;
            //Debug.Log("Parry window closed!");
        }
    }

    protected override void AttackTrigger()
    {
        // Düşman yoksa, saldırı aktif değilse veya stun durumundaysa işlem yapma
        if (enemyEliteSkeleton == null || 
            !enemyEliteSkeleton.isAttackActive || 
            enemyEliteSkeleton.stateMachine.currentState == enemyEliteSkeleton.stunnedState)
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
                
                // Get enemy stats for damage calculation
                var enemyStats = enemyEliteSkeleton.stats as EnemyStats;
                
                // Log damage values for debugging
                if (enemyStats != null)
                {
                   // Debug.Log($"EliteSkeleton attacking with damage: {enemyStats.enemyDamage.GetValue()}");
                }
                
                // Parry durumunda değilse normal hasar ver
                if (enemyStats != null)
                    player.TakePlayerDamage(enemyStats.enemyDamage, CharacterStats.DamageType.Physical);
                else
                    player.TakePlayerDamage(null, CharacterStats.DamageType.Physical);
                
                //Debug.Log("Elite Skeleton attacked player!");
            }
        }
    }
}
