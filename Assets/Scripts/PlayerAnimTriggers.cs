using UnityEngine;
using System.Collections;

public class PlayerAnimTriggers : MonoBehaviour
{
   private Player player => GetComponentInParent<Player>();
   [SerializeField] private GameObject iceShardPrefab;
   [SerializeField] private Transform spellCastPoint; // Spell'in başlayacağı nokta

   private void AnimationTrigger()
   {
      player.AnimationFinishTrigger();
   }

   private void ThrowBoomerangTrigger() => player.ThrowBoomerang();
   
   private void AttackTrigger()
   {
      // Saldırı pozisyonunu belirle - normal veya crouch durumuna göre
      Vector2 attackPosition = player.attackCheck.position;

      // Eğer çömelme saldırısı yapıyorsa konumu ayarla
      if (player.stateMachine.currentState == player.crouchAttackState)
      {
         attackPosition = (Vector2)player.attackCheck.position + player.crouchAttackOffset;
      }

      Collider2D[] colliders = Physics2D.OverlapBoxAll(attackPosition, player.attackSize, 0, player.passableEnemiesLayerMask);

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

   // Debug amaçlı - saldırı kutusunu görmek için
   private void OnDrawGizmosSelected()
   {
      if (!Application.isPlaying) return;
      if (player == null) return;
      
      Player p = GetComponentInParent<Player>();
      if (p == null) return;
      
      // Saldırı pozisyonunu belirle
      Vector2 attackPosition = p.attackCheck.position;
      
      // Eğer çömelme saldırısı yapıyorsa konumu ayarla
      if (p.stateMachine.currentState == p.crouchAttackState)
      {
         attackPosition = (Vector2)p.attackCheck.position + p.crouchAttackOffset;
         Gizmos.color = Color.red;
      }
      else
      {
         Gizmos.color = Color.green;
      }
      
      Gizmos.DrawWireCube(attackPosition, p.attackSize);
   }

   private void SpellOneTrigger()
   {
      player.SpellOneTrigger();
   }

   
}
