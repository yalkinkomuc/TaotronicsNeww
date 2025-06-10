using UnityEngine;

public class Enemy_BanditTriggers : EnemyAnimationTriggers
{
    
    private Bandit_Enemy banditEnemy => GetComponentInParent<Bandit_Enemy>();
   protected override void EnableAttackCollider()
    {
        if (banditEnemy != null)
        {
            banditEnemy.isAttackActive = true;
        }
    }
    
    protected override void DisableAttackCollider()
    {
        if (banditEnemy != null)
        {
            banditEnemy.isAttackActive = false;
        }
    }
    
    // Parry penceresi açma - Animation Event tarafından çağrılacak
    private void OpenParryWindow()
    {
        if (banditEnemy != null)
        {
            banditEnemy.isParryWindowOpen = true;
            //Debug.Log("Parry window opened!");
        }
    }
    
    // Parry penceresi kapatma - Animation Event tarafından çağrılacak
    private void CloseParryWindow()
    {
        if (banditEnemy != null)
        {
            banditEnemy.isParryWindowOpen = false;
            //Debug.Log("Parry window closed!");
        }
    }

    protected override void AttackTrigger()
    {
        // Düşman yoksa, saldırı aktif değilse veya stun durumundaysa işlem yapma
        if (banditEnemy == null || 
            !banditEnemy.isAttackActive  
           )
            return;
            
        Collider2D[] colliders = Physics2D.OverlapBoxAll(
            banditEnemy.attackCheck.position,
            banditEnemy.attackSize,
            0,
            banditEnemy.whatIsPlayer);

        foreach (var hit in colliders)
        {
            Player player = hit.GetComponent<Player>();
            if (player != null)
            {
                // Bu oyuncuya zaten vurduk mu kontrol et
                if (banditEnemy.HasHitEntity(player))
                    continue;  // Zaten vurmuşsak atla
                    
                // Bu oyuncuyu vurulmuş olarak işaretle
                banditEnemy.MarkEntityAsHit(player);
                
                // Eğer parry penceresi açıkken oyuncu parry state'deyse parry olur
                
                // Parry durumunda değilse normal hasar ver
                var enemyStats = banditEnemy.stats as EnemyStats;
                if (enemyStats != null)
                    player.TakePlayerDamage(enemyStats.enemyDamage, CharacterStats.DamageType.Physical);
                else
                    player.TakePlayerDamage(null, CharacterStats.DamageType.Physical);
                //Debug.Log("Bandit attacked player!");
            }
        }
    }
}
