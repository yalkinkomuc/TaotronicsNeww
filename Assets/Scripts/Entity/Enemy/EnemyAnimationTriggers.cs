using UnityEngine;

public class EnemyAnimationTriggers : MonoBehaviour
{
    private EnemyBossNecromancerBoss enemyBoss;

    private void Start()
    {
        enemyBoss = GetComponentInParent<EnemyBossNecromancerBoss>();
    }

    private void AnimationTriggerCastSpell()
    {
        //Debug.Log("AnimationTriggerCastSpell çağrıldı");
        if (enemyBoss != null)
        {
            enemyBoss.CastSpell();
        }
    }

    private void AnimationTrigger()
    {
        //Debug.Log("AnimationTrigger çağrıldı");
        if (enemyBoss != null)
        {
            enemyBoss.AnimationFinishTrigger();
        }
    }

    private void AnimationTriggerSummonSkeletons()
    {
        //Debug.Log("AnimationTriggerSummonSkeletons çağrıldı");
        if (enemyBoss != null)
        {
            enemyBoss.SummonSkeletons();
        }
    }

    private void AnimationTriggerCreateSpawnEffect()
    {
        if (enemyBoss != null)
        {
            // Belirtilen sayıda spawn efekti oluştur
            for (int i = 0; i < enemyBoss.skeletonsToSpawn; i++)
            {
                float randomX = Random.Range(-enemyBoss.spawnRange, enemyBoss.spawnRange);
                Vector2 spawnPos = new Vector2(enemyBoss.transform.position.x + randomX, enemyBoss.transform.position.y);
                enemyBoss.CreateSpawnEffect(spawnPos);
            }
        }
    }
} 