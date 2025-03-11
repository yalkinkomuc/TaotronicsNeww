using UnityEngine;

public class PlayerAnimTriggers : MonoBehaviour
{
   private Player player => GetComponentInParent<Player>();

   private void AnimationTrigger()
   {
      player.AnimationFinishTrigger();
   }
   private void AttackTrigger()
   {
      //LayerMask layers = LayerMask.GetMask("PassableEnemy", "ShieldedEnemies");  diğer düşmanalr eklendiğinde burası düzenlenecek.
      Collider2D[] colliders = Physics2D.OverlapBoxAll(player.attackCheck.position, player.attackSize, 0,player.passableEnemiesLayerMask);

      foreach (var hit in colliders)
      {
         if (hit.GetComponent<Enemy>() != null)
         {
            hit.GetComponent<Enemy>().Damage();
            hit.GetComponent<CharacterStats>().TakeDamage(player.stats.damage.GetValue());
            
            Debug.Log(" modifierli final hasarı "+ player.stats.damage.GetValue());
            
         }
      }

   }
}
