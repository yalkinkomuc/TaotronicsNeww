using UnityEngine;

public class NecromancerAnimTriggers :EnemyAnimationTriggers
{
   private EnemyBossNecromancerBoss enemyBossNecromancer => GetComponentInParent<EnemyBossNecromancerBoss>();

   private void Relocate() => enemyBossNecromancer.FindPosition();
   
   private void MakeInvisible () => enemyBossNecromancer.entityFX.MakeTransparent(true);
   private void MakeVisible () => enemyBossNecromancer.entityFX.MakeTransparent(false);

   // Invincible metodları
   private void StartTeleportInvincibility() => enemyBossNecromancer.stats.MakeInvincible(true);
   private void EndTeleportInvincibility() => enemyBossNecromancer.stats.MakeInvincible(false);
   
   // İskelet spawn metodu - daha sıkı kontrol
   private void SpawnSkeletons()
   {
       // Debug.Log("AnimTrigger: SpawnSkeletons çağrıldı");
       // CanSummon kontrolü yerine direkt SummonSkeletons çağıralım, içeride kontrol var
       enemyBossNecromancer.SummonSkeletons();
   }
   
   // Büyü atma metodu - daha sıkı kontrol
   private void CastSpell()
   {
       // Debug.Log("AnimTrigger: CastSpell çağrıldı");
       // CanCastSpell kontrolü yerine direkt CastSpell çağıralım, içeride kontrol var
       enemyBossNecromancer.CastSpell();
   }
}
