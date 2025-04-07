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
            Debug.Log("Parry window opened!");
        }
    }
    
    // Parry penceresi kapatma - Animation Event tarafından çağrılacak
    private void CloseParryWindow()
    {
        if (enemyEliteSkeleton != null)
        {
            enemyEliteSkeleton.isParryWindowOpen = false;
            Debug.Log("Parry window closed!");
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
                
                // Eğer parry penceresi açıkken oyuncu parry state'deyse parry olur
                if (enemyEliteSkeleton.isParryWindowOpen && player.stateMachine.currentState is PlayerParryState)
                {
                    // Parry olayını tetikle
                    enemyEliteSkeleton.GetParried();
                    continue;
                }

                // Parry durumunda değilse normal hasar ver
                if (!(player.stateMachine.currentState is PlayerParryState))
                {
                    // Hasar ver
                    float currentDamage = enemyEliteSkeleton.stats.baseDamage.GetValue();
                    player.Damage();
                    hit.GetComponent<PlayerStats>().TakeDamage(currentDamage);
                }
            }
        }
    }
}
