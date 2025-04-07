using System;
using UnityEngine;
using System.Collections;

public class PlayerAnimTriggers : MonoBehaviour
{
   private int ogrenciNotu;
   private Player player => GetComponentInParent<Player>();
   
   private void Awake()
   {
      
   }

   private void AnimationTrigger()
   {
      player.AnimationFinishTrigger();
   }

   private void ThrowBoomerangTrigger() => player.ThrowBoomerang();
   
   // Attack hitbox'ını aktif et - Animation Event tarafından çağrılır
   private void EnableAttackCollider()
   {
      if (player != null)
      {
         // Saldırı aktif olarak işaretle
         player.isAttackActive = true;
      }
   }
   
   // Attack hitbox'ını deaktif et - Animation Event tarafından çağrılır
   private void DisableAttackCollider()
   {
      if (player != null)
      {
         // Saldırıyı deaktif yap
         player.isAttackActive = false;
      }
   }
   
   // Her animasyon frame'inde çağrılabilir
   private void AttackTrigger()
   {
      // Saldırı aktif değilse hiçbir şey yapma
      if (player == null || !player.isAttackActive)
         return;
      
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
         // Düşman kontrolü
         Enemy enemy = hit.GetComponent<Enemy>();
         if (enemy != null)
         {
            // Bu düşmana zaten vurduk mu kontrol et
            if (player.HasHitEntity(enemy))
               continue;  // Zaten vurmuşsak atla
               
            // Bu düşmanı vurulmuş olarak işaretle
            player.MarkEntityAsHit(enemy);
            
            float currentDamage = player.stats.baseDamage.GetValue();
            
            // Combo sayısına göre knockback gücünü artır
            if (player.stateMachine.currentState is PlayerAttackState attackState)
            {
                Vector2 knockbackForce;
                switch (attackState.GetComboCounter())
                {
                    case 0:
                        currentDamage *= 1f;
                        Debug.Log(currentDamage);
                        knockbackForce = enemy.knockbackDirection; // Normal knockback
                        break;
                    case 1:
                        currentDamage *= player.stats.secondComboDamageMultiplier.GetValue();
                        Debug.Log(currentDamage);
                        knockbackForce = new Vector2(enemy.knockbackDirection.x*enemy.secondComboKnockbackXMultiplier,enemy.knockbackDirection.y); // Daha güçlü
                        break;
                    case 2:
                       currentDamage *= player.stats.thirdComboDamageMultiplier.GetValue();;
                       Debug.Log(currentDamage);
                       knockbackForce = new Vector2(enemy.knockbackDirection.x*enemy.thirdComboKnockbackXMultiplier,enemy.knockbackDirection.y); // En güçlü
                        break;
                    default:
                       knockbackForce = enemy.knockbackDirection;
                        break;
                }
                
                enemy.Damage();
                hit.GetComponent<CharacterStats>().TakeDamage(currentDamage);
                
                if (enemy.rb.bodyType == RigidbodyType2D.Static)
                {
                   return;
                }
                StartCoroutine(enemy.HitKnockback(knockbackForce));
            }
            else if (player.stateMachine.currentState == player.crouchAttackState)
            {
                // Çömelme saldırısı için hasar ve knockback
                currentDamage *= 1.2f; // Çömelme saldırısı biraz daha fazla hasar versin
                enemy.Damage();
                hit.GetComponent<CharacterStats>().TakeDamage(currentDamage);
                
                if (enemy.rb.bodyType == RigidbodyType2D.Static)
                {
                   return;
                }
                StartCoroutine(enemy.HitKnockback(enemy.knockbackDirection));
            }
         }

         // Training dummy kontrolü
         Dummy dummy = hit.GetComponent<Dummy>();
         if (dummy != null)
         {
            // Dummy objelerinin ID'sini kullanarak kontrol et
            int dummyID = dummy.gameObject.GetInstanceID();
            
            // Eğer bu dummy'ye zaten vurduysak, atla
            if (player.hitDummyIDs.Contains(dummyID))
               continue;
               
            // Vurulan dummy ID'sini listeye ekle
            player.hitDummyIDs.Add(dummyID);
            
            // Dummy'nin random hit animasyonunu oynat
            dummy.PlayRandomHit();
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
      
      // Saldırı aktifse daha belirgin göster
      if (p.isAttackActive)
      {
          Gizmos.color = new Color(Gizmos.color.r, Gizmos.color.g, Gizmos.color.b, 0.8f);
      }
      else 
      {
          Gizmos.color = new Color(Gizmos.color.r, Gizmos.color.g, Gizmos.color.b, 0.3f);
      }
      
      Gizmos.DrawWireCube(attackPosition, p.attackSize);
   }

   private void SpellOneTrigger()
   {
      player.SpellOneTrigger();
   }

   // Animation Event tarafından çağrılacak
   public void PauseSpell2Animation()
   {
      if (player.stateMachine.currentState is PlayerSpell2State)
      {
         player.anim.speed = 0;
         // Spellbook ve sword için de aynı frame'de durduralım
         if (player.spellbookWeapon != null)
         {
            player.spellbookWeapon.animator.speed = 0;
         }
         if (player.swordWeapon != null)
         {
            player.swordWeapon.animator.speed = 0;
         }
      }
   }
   
   private void MakeInvisible() => player.entityFX.MakeTransparent(true);
   private void MakeVisible() => player.entityFX.MakeTransparent(false);

   private void VoidDisappearFinished()
   {
      if (player.stateMachine.currentState is PlayerVoidState)
      {
         // Disappear animasyonu tamamlandığında çağrılır
         player.anim.SetBool("VoidDisappear", false);
      }
   }

   private void VoidReappearFinished()
   {
      if (player.stateMachine.currentState is PlayerVoidState)
      {
         // Reappear animasyonu tamamlandığında çağrılır
         player.anim.SetBool("VoidReappear", false);
      }
   }

   // Başarılı parry durumunda düşmana ekstra hasar vermek için
   private void ParryAttackTrigger()
   {
      if (player == null)
         return;
         
      // Parry yarıçapında düşmanları kontrol et
      Collider2D[] colliders = Physics2D.OverlapCircleAll(player.transform.position, player.parryRadius, player.passableEnemiesLayerMask);
      
      foreach (var hit in colliders)
      {
         Enemy enemy = hit.GetComponent<Enemy>();
         if (enemy != null)
         {
            // Eğer bu düşmana zaten vurduysak tekrar vurma
            if (player.HasHitEntity(enemy))
               continue;
               
            // Düşmanı vurulmuş olarak işaretle
            player.MarkEntityAsHit(enemy);
            
            // Ekstra hasar ver (parry sonrası counter hasar)
            float parryDamage = player.stats.baseDamage.GetValue() * 1.5f; // %50 fazla hasar
            enemy.Damage();
            enemy.stats.TakeDamage(parryDamage);
            
            Vector2 knockbackForce = new Vector2(enemy.knockbackDirection.x * 1.5f, enemy.knockbackDirection.y);
            StartCoroutine(enemy.HitKnockback(knockbackForce));
         }
      }
   }
   
   // Parry animasyonu bittiğinde çağrılacak
   private void ParryAnimationFinished()
   {
      if (player != null && player.stateMachine.currentState is PlayerParryState)
      {
         player.AnimationFinishTrigger();
      }
   }
}
