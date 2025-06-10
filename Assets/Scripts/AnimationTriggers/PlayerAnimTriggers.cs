using System;
using UnityEngine;
using System.Collections;

public class PlayerAnimTriggers : MonoBehaviour
{
   
   private Player player => GetComponentInParent<Player>();
   
   private void Awake()
   {
      
   }

   private void AnimationTrigger()
   {
      player.AnimationFinishTrigger();
   }

   private void ThrowBoomerangTrigger() => player.ThrowBoomerang();
   
   // Fireball spell animation trigger - called from animation event
   private void FireballSpellTrigger()
   {
      if (player.stateMachine.currentState is PlayerFireballSpellState fireballState)
      {
         fireballState.CastFireball();
      }
   }
   
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
      Vector2 attackPosition = GetAttackPosition();

      // Saldırı alanındaki nesneleri kontrol et
      Collider2D[] colliders = Physics2D.OverlapBoxAll(attackPosition, player.attackSize, 0, player.passableEnemiesLayerMask);

      // Tüm hedeflere saldırıyı uygula
      foreach (var hit in colliders)
      {
         // Düşmana saldır
         TryAttackEnemy(hit);
         
         // Training dummy'ye saldır
         TryAttackDummy(hit);
      }
   }

   // Saldırı pozisyonunu belirler
   private Vector2 GetAttackPosition()
   {
      // Eğer çömelme saldırısı yapıyorsa konumu ayarla
      if (player.stateMachine.currentState == player.crouchAttackState)
      {
         return (Vector2)player.attackCheck.position + player.crouchAttackOffset;
      }
      return player.attackCheck.position;
   }

   // Düşmana saldırıyı dene
   private void TryAttackEnemy(Collider2D hit)
   {
      Enemy enemy = hit.GetComponent<Enemy>();
      if (enemy == null) return;
      
      // Bu düşmana zaten vurduk mu kontrol et
      if (player.HasHitEntity(enemy)) return;
            
      // Bu düşmanı vurulmuş olarak işaretle
      player.MarkEntityAsHit(enemy);
      
      // Hasar hesapla ve uygula
      ApplyDamageToEnemy(enemy);
   }

   // Düşmana hasar uygula
   private void ApplyDamageToEnemy(Enemy enemy)
   {
      // Temel hasar değerini al
      float currentDamage = CalculateDamage(out bool isCritical);
      //int comboCounter = 0;
      
      // Saldırı türüne göre hasar ve knockback'i ayarla
      if (player.stateMachine.currentState is PlayerAttackState attackState)
      {
         // Kombo saldırısı için hasar hesapla
         HandleComboAttack(enemy, attackState, currentDamage, isCritical);
      }
      else if (player.stateMachine.currentState == player.crouchAttackState)
      {
         // Çömelme saldırısı için hasar hesapla
         HandleCrouchAttack(enemy, currentDamage, isCritical);
      }
   }

   // Kombo saldırısı için hasar ve knockback hesapla
   private void HandleComboAttack(Enemy enemy, PlayerAttackState attackState, float baseDamage, bool isCritical)
   {
      int comboCounter = attackState.GetComboCounter();
      float damage = baseDamage;
      
      // Combo sayısına göre hasarı ayarla
      switch (comboCounter)
      {
         case 0:
            // İlk saldırı için standart hasar
            break;
         case 1:
            // İkinci saldırı için arttırılmış hasar
            damage *= player.stats.secondComboDamageMultiplier.GetValue();
            break;
         case 2:
            // Üçüncü saldırı için en yüksek hasar
            damage *= player.stats.thirdComboDamageMultiplier.GetValue();
            break;
      }
      
      // Hasarı uygula
      DealDamageToEnemy(enemy, damage, comboCounter, isCritical);
      
      // Static olmayan düşmanlara yeni knockback sistemi ile knockback uygula
      if (enemy.rb.bodyType != RigidbodyType2D.Static)
      {
         enemy.ApplyComboKnockback(player.transform.position, comboCounter);
      }
   }

   // Çömelme saldırısı için hasar ve knockback hesapla
   private void HandleCrouchAttack(Enemy enemy, float baseDamage, bool isCritical)
   {
      // Çömelme saldırısı için %20 ekstra hasar
      float damage = baseDamage * 1.2f;
      
      // Hasarı uygula
      DealDamageToEnemy(enemy, damage, 0, isCritical);
      
      // Static olmayan düşmanlara yeni knockback sistemi ile knockback uygula
      if (enemy.rb.bodyType != RigidbodyType2D.Static)
      {
         enemy.ApplyKnockback(player.transform.position);
      }
   }

   // Düşmana hasarı uygula ve görsel efektleri göster
   private void DealDamageToEnemy(Enemy enemy, float damage, int comboCounter, bool isCritical)
   {
      // Hasarı doğrudan uygula
      enemy.stats.TakeDamage(damage,CharacterStats.DamageType.Physical);
      
      // Hasar metni göster
      if (FloatingTextManager.Instance != null)
      {
         Vector3 textPosition = enemy.transform.position + Vector3.up * 1.5f;
         
         if (comboCounter > 0)
         {
            FloatingTextManager.Instance.ShowComboDamageText(damage, textPosition, comboCounter);
         }
         else
         {
            FloatingTextManager.Instance.ShowDamageText(damage, textPosition);
         }
      }
      
      // Vuruş efekti göster
      if (enemy.entityFX != null)
      {
         enemy.entityFX.StartCoroutine("HitFX");
      }
   }

   // Eğitim kuklaları (dummy) için saldırı işlemi
   private void TryAttackDummy(Collider2D hit)
   {
      Dummy dummy = hit.GetComponent<Dummy>();
      if (dummy == null) return;
      
      // Dummy objelerinin ID'sini kullanarak kontrol et
      int dummyID = dummy.gameObject.GetInstanceID();
      
      // Eğer bu dummy'ye zaten vurduysak, atla
      if (player.hitDummyIDs.Contains(dummyID)) return;
            
      // Vurulan dummy ID'sini listeye ekle
      player.hitDummyIDs.Add(dummyID);
      
      // Hasarı hesapla ve uygula
      ApplyDamageToDummy(dummy);
   }

   // Dummy'ye hasar uygula
   private void ApplyDamageToDummy(Dummy dummy)
   {
      // Temel hasar değerini al
      float damage = CalculateDamage(out bool isCritical);
      int comboCounter = 0;
      
      // Saldırı türüne göre hasarı ayarla
      if (player.stateMachine.currentState is PlayerAttackState attackState)
      {
         comboCounter = attackState.GetComboCounter();
         
         // Combo sayısına göre hasarı ayarla
         switch (comboCounter)
         {
            case 1:
               damage *= player.stats.secondComboDamageMultiplier.GetValue();
               break;
            case 2:
               damage *= player.stats.thirdComboDamageMultiplier.GetValue();
               break;
         }
      }
      else if (player.stateMachine.currentState == player.crouchAttackState)
      {
         // Çömelme saldırısı için %20 ekstra hasar
         damage *= 1.2f;
      }
      
      // Dummy'ye hasarı uygula
      dummy.TakeDamage(damage, comboCounter, isCritical);
   }

   // Temel hasar ve kritik vuruş hesaplama
   private float CalculateDamage(out bool isCritical)
   {
      float damage = 0f;
      if (player.stats is PlayerStats playerStats)
         damage = playerStats.baseDamage.GetValue();
      isCritical = false;
      // Kritik vuruş kontrolü
      if (player.stats is PlayerStats ps && ps.IsCriticalHit())
      {
         damage *= 1.5f; // Kritik vuruş için %50 daha fazla hasar
         isCritical = true;
      }
      return damage;
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

   // Earth Push spell animation event trigger - called when player's foot hits the ground
   private void EarthPushTrigger()
   {
      if (player.stateMachine.currentState is PlayerEarthPushSpellState && player.earthPushPrefab != null)
      {
         // Use custom spawn point if available, otherwise calculate based on player position
         Vector3 spawnPosition;
         Quaternion spawnRotation;
         
         if (player.earthPushSpawnPoint != null)
         {
            // Use the custom spawn point's position and rotation
            spawnPosition = player.earthPushSpawnPoint.position;
            spawnRotation = player.earthPushSpawnPoint.rotation;
         }
         else
         {
            // Fallback to calculated position
            spawnPosition = player.transform.position;
            spawnPosition.x += player.facingdir * 1.5f; // Spawn in front of player
            spawnRotation = Quaternion.identity;
         }
         
         // Create the earth push effect
         GameObject earthPushObj = Instantiate(
            player.earthPushPrefab, 
            spawnPosition, 
            spawnRotation);
         
         // Set the correct scale based on player facing direction
         if (player.facingdir < 0 && player.earthPushSpawnPoint == null) // Only flip if using calculated position
         {
            earthPushObj.transform.localScale = new Vector3(
               -Mathf.Abs(earthPushObj.transform.localScale.x),
               earthPushObj.transform.localScale.y,
               earthPushObj.transform.localScale.z);
         }
      }
   }
   
   // Earth Push spell destruction animation event trigger
   [Obsolete("Obsolete")]
   private void DestroyEarthPush()
   {
      // Find any active earth push objects and destroy them
      EarthPush[] activeEarthPushes = FindObjectsOfType<EarthPush>();
      foreach (EarthPush push in activeEarthPushes)
      {
         if (push != null)
         {
            push.DestroySpell();
         }
      }
   }

   // Animation Event tarafından çağrılacak
   public void PauseSpell2Animation()
   {
      if (player.stateMachine.currentState is PlayerSpell2State spell2State)
      {
         // SkillManager kontrolü - Fire Spell açık mı?
         if (SkillManager.Instance != null && !SkillManager.Instance.IsSkillUnlocked("FireSpell"))
         {
            // Fire Spell açık değilse animasyonu durdurma ve idle state'ine geç
            Debug.Log("Fire Spell not unlocked, cancelling animation!");
            player.stateMachine.ChangeState(player.idleState);
            return;
         }
         
         // Animation'ı durdur
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
         
         // Fire Spell'i spawn et
         spell2State.SpawnFireSpell();
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

   // Parry animasyonu bittiğinde çağrılacak
   
}
