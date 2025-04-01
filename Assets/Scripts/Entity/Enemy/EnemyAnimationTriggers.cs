using UnityEngine;

public class EnemyAnimationTriggers : MonoBehaviour
{

    private Boar_Enemy enemyBoar => GetComponentInParent<Boar_Enemy>();
    protected virtual void AnimationTrigger()
    {
       
    }
    
    
    private void AttackTrigger()
    {
        Collider2D[] colliders = Physics2D.OverlapBoxAll(enemyBoar.attackCheck.position,enemyBoar.attackSize,0,enemyBoar.whatIsPlayer);

        foreach (var hit in colliders)
        {
            if (hit.GetComponent<Player>())
            {
                float currentDamage = enemyBoar.stats.baseDamage.GetValue();
                hit.GetComponent<Player>().Damage();
                hit.GetComponent<PlayerStats>().TakeDamage(currentDamage);
            }
        }
    }
} 