using UnityEngine;
using System.Collections;

public class NecromancerAnimTriggers : EnemyAnimationTriggers
{
   private EnemyBossNecromancerBoss enemyBossNecromancer => GetComponentInParent<EnemyBossNecromancerBoss>();
   //private bool isSpawnTriggered = false;

   private void Relocate() => enemyBossNecromancer.FindPosition();
   
   private void MakeInvisible() => enemyBossNecromancer.entityFX.MakeTransparent(true);
   private void MakeVisible() => enemyBossNecromancer.entityFX.MakeTransparent(false);

   // Invincible metodları
   private void StartTeleportInvincibility() => enemyBossNecromancer.stats.MakeInvincible(true);
   private void EndTeleportInvincibility() => enemyBossNecromancer.stats.MakeInvincible(false);
   
   // Anim Trigger'lar için yeni metotlar

   protected override void AnimationTrigger()
   {
       base.AnimationTrigger();
       
       if (enemyBossNecromancer != null)
       {
           enemyBossNecromancer.AnimationFinishTrigger();
       }
   }

   private void AnimationTriggerCastSpell()
   {
        if (enemyBossNecromancer != null)
        {
            enemyBossNecromancer.CastSpell();
        }
   }
   
   // İkişer iskelet spawn efekti oluşturma metodu
   private void AnimationTriggerCreateSpawnEffect()
   {
        if (enemyBossNecromancer == null) return;
        
        // Kalan iskelet slotu kontrolü yap
        int remainingSlots = enemyBossNecromancer.maxSkeletons - enemyBossNecromancer.summonedSkeletons.Count;
        
        // Hiç slot kalmadıysa çık
        if (remainingSlots <= 0) return;
        
        // En fazla 2 iskelet spawn et (veya kalan slot sayısı kadar)
        int effectsToCreate = Mathf.Min(2, remainingSlots);
        
        // Arena sınırları
        float minX = -10f;
        float maxX = 9f;
        float groundY = enemyBossNecromancer.arenaCollider.bounds.min.y;
        
        // Her iskelet için ayrı bir efekt oluştur (en fazla 2 tane)
        for (int i = 0; i < effectsToCreate; i++)
        {
            float randomX = Random.Range(minX, maxX);
            
            // Aynı X konumuna çok yakın olmasınlar
            if (i > 0)
            {
                // İlk iskeletle mesafeyi koru
                randomX = (randomX < 0) ? randomX - 3f : randomX + 3f;
                // Sınırlar içinde tut
                randomX = Mathf.Clamp(randomX, minX, maxX);
            }
            
            Vector2 spawnPos = new Vector2(randomX, groundY + 0.5f);
            enemyBossNecromancer.CreateSpawnEffect(spawnPos);
        }
   }
   
  
}
