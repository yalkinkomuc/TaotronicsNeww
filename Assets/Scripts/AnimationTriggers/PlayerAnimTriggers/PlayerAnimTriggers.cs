using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerAnimTriggers : MonoBehaviour
{
   
   private Player player => GetComponentInParent<Player>();
   
   // Weapon-specific attack handlers
   private Dictionary<WeaponType, IWeaponAttackHandler> weaponAttackHandlers;
   
   private void Awake()
   {
      InitializeWeaponHandlers();
   }
   
   private void InitializeWeaponHandlers()
   {
      weaponAttackHandlers = new Dictionary<WeaponType, IWeaponAttackHandler>
      {
         { WeaponType.Sword, new SwordAttackHandler() },
         { WeaponType.Hammer, new HammerAttackHandler() },
         { WeaponType.IceHammer ,new IceHammerAttackHandler()},
         { WeaponType.BurningSword, new BurningSwordAttackHandler() }
      };
   }

   public void AnimationTrigger()
   {
      player.AnimationFinishTrigger();
   }

   private void ThrowBoomerangTrigger() => player.ThrowBoomerang();
   
   // Fireball spell animation trigger - called from animation event
   public void FireballSpellTrigger()
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

      // Mevcut silah türünü belirle
      WeaponType currentWeapon = GetCurrentWeaponType();
      
      // Silah türüne göre handler'ı al ve saldırıyı işle
      if (weaponAttackHandlers.TryGetValue(currentWeapon, out IWeaponAttackHandler handler))
      {
         handler.HandleAttack(player, attackPosition);
      }
      else
      {
         // Fallback: Eğer handler bulunamazsa eski sistemi kullan
         HandleLegacyAttack(attackPosition);
      }
   }
   
   // Backward compatibility için eski saldırı sistemi
   private void HandleLegacyAttack(Vector2 attackPosition)
   {
      // Saldırı alanındaki nesneleri kontrol et
      Collider2D[] colliders = Physics2D.OverlapBoxAll(attackPosition, player.attackSize, 0, player.passableEnemiesLayerMask);

      // Tüm hedeflere saldırıyı uygula
      foreach (var hit in colliders)
      {
         // Düşmana saldır
         TryAttackEnemy(hit);
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

   // Legacy attack methods - these are now handled by weapon-specific handlers
   // Keeping for backward compatibility with legacy attacks (crouch attacks, etc.)
   private void TryAttackEnemy(Collider2D hit)
   {
      Enemy enemy = hit.GetComponent<Enemy>();
      if (enemy == null) return;
      
      // Bu düşmana zaten vurduk mu kontrol et
      if (player.HasHitEntity(enemy)) return;
            
      // Bu düşmanı vurulmuş olarak işaretle
      player.MarkEntityAsHit(enemy);
      
      // Hasar hesapla ve uygula - simplified legacy version
      ApplyLegacyDamageToEnemy(enemy);
   }

   private void ApplyLegacyDamageToEnemy(Enemy enemy)
   {
      // Basit hasar hesaplama - sadece crouch attack için
      float damage = 10f; // Varsayılan hasar
      
      if (player.stateMachine.currentState == player.crouchAttackState)
      {
         damage = 12f; // Çömelme saldırısı için %20 ekstra hasar
      }
      
      // Hasarı uygula
      enemy.stats.TakeDamage(damage, CharacterStats.DamageType.Physical);
      
      
      // Vuruş efekti göster
      if (enemy.entityFX != null)
      {
         enemy.entityFX.StartCoroutine("HitFX");
      }
      
      // Knockback uygula
      if (enemy.rb.bodyType != RigidbodyType2D.Static)
      {
         enemy.ApplyKnockback(player.transform.position);
      }
   }

  
   
   // Determine which weapon is currently active
   private WeaponType GetCurrentWeaponType()
   {
      // Check which weapon is currently active based on player weapon manager
      if (player.hammer != null && player.hammer.gameObject.activeInHierarchy)
      {
         return WeaponType.Hammer;
      }
      else if (player.burningSword != null && player.burningSword.gameObject.activeInHierarchy)
      {
         return WeaponType.BurningSword;
      }
      else if (player.iceHammer != null && player.iceHammer.gameObject.activeInHierarchy)
      {
         return WeaponType.IceHammer;
      }
      else
      {
         // Default to Sword if no specific weapon is active
         return WeaponType.Sword;
      }
   }

   
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

      // Hammer explosion alanını da çiz
      if (p.hammerExplosionCheck != null)
      {
         Gizmos.color = Color.yellow;
         Gizmos.DrawWireCube(p.hammerExplosionCheck.position, p.hammerExplosionSize);
      }
   }

   private void OnDrawGizmos()
   {
      Player p = GetComponentInParent<Player>();
      if (p == null) return;
      if (p.hammerExplosionCheck != null)
      {
         Gizmos.color = Color.yellow;
         Gizmos.DrawWireCube(p.hammerExplosionCheck.position, p.hammerExplosionSize);
      }
   }

   public void SpellOneTrigger()
   {
      player.SpellOneTrigger();
   }

   // Earth Push spell animation event trigger - called when player's foot hits the ground
   public void EarthPushTrigger()
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

         if (player.hammer != null)
         {
            player.hammer.animator.speed = 0;
         }

         if (player.iceHammer != null)
         {
            player.iceHammer.animator.speed = 0;
         }

         if (player.burningSword != null)
         {
            player.burningSword.animator.speed = 0;
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
   
   // Hammer 3. kombo patlama animasyonunda çağrılacak
   public void ClearExplosionHitEntities()
   {
      player.explosionHitEntities.Clear();
      player.iceHammerExplosionHitEntities.Clear();
   }

   private void TryExplosionAttackEnemy(Collider2D hit)
   {
      Enemy enemy = hit.GetComponent<Enemy>();
      if (enemy == null) return;
      
      // Enemy'nin hala aktif olup olmadığını kontrol et
      if (enemy.gameObject == null || !enemy.gameObject.activeInHierarchy)
         return;
         
      int id = enemy.GetInstanceID();
      if (player.explosionHitEntities.Contains(id)) return;
      player.explosionHitEntities.Add(id);
      
      if(player.iceHammerExplosionHitEntities.Contains(id)) return;
      player.iceHammerExplosionHitEntities.Add(id);
      
      

      // Hasar hesapla ve uygula
      float explosionDamage = player.lastHammerCombo3Damage * 0.25f;

      enemy.stats.TakeDamage(explosionDamage, CharacterStats.DamageType.Physical);
      
      float iceExplosionDamage = player.iceHammerlastHammerCombo3Damage * 0.25f;
      enemy.stats.TakeDamage(iceExplosionDamage,CharacterStats.DamageType.Ice);
      
      if (enemy.rb != null && enemy.rb.bodyType != RigidbodyType2D.Static)
      {
         float knockbackMultiplier = 1.0f;
         if (player.stateMachine.currentState is PlayerHammerAttackState hammerAttackState2)
            knockbackMultiplier = hammerAttackState2.GetKnockbackMultiplier(2);
         enemy.ApplyComboKnockback(player.transform.position, 2, knockbackMultiplier);
      }

     

      // EntityFX null kontrolü ve GameObject aktiflik kontrolü
      if (enemy.entityFX != null && enemy.entityFX.gameObject != null && enemy.entityFX.gameObject.activeInHierarchy)
      {
         enemy.entityFX.StartCoroutine("HitFX");
      }
   }

   public void HammerExplosionTrigger()
   {
      // Sadece hammer aktifken ve 3. kombo sırasında çalışsın
      if (GetCurrentWeaponType() != WeaponType.Hammer && GetCurrentWeaponType() != WeaponType.IceHammer)
    return;
      if (!(player.stateMachine.currentState is PlayerHammerAttackState hammerAttackState))
         return;
      if (hammerAttackState.GetComboCounter() != 2) // 3. saldırı (0-based)
         return;
      if (player.hammerExplosionCheck == null) 
         if(player.iceHammerExplosionCheck == null)
         return;

      Vector2 explosionPos = player.hammerExplosionCheck.position;
      Vector2 explosionSize = player.hammerExplosionSize;
      
     
      Collider2D[] hits = Physics2D.OverlapBoxAll(explosionPos, explosionSize, 0, player.passableEnemiesLayerMask);

      foreach (var hit in hits)
      {
         TryExplosionAttackEnemy(hit);
      }
      
      Vector2 iceHammerexplosionPos = player.iceHammerExplosionCheck.position;
      Vector2 iceHammerExplosionSize = player.iceHammerExplosionSize;
      
      Collider2D[] iceHits = Physics2D.OverlapBoxAll(iceHammerexplosionPos, iceHammerExplosionSize, 0, player.passableEnemiesLayerMask);

      foreach (var iceHit in iceHits)
      {
         TryExplosionAttackEnemy(iceHit);
      }
   }

   public void HideCurrentArmor()
   {
      var armorManager = player.GetComponentInChildren<PlayerArmorManager>();
      if (armorManager != null)
         armorManager.HideCurrentArmor();
   }

   public void ShowCurrentArmor()
   {
      var armorManager = player.GetComponentInChildren<PlayerArmorManager>();
      if (armorManager != null)
         armorManager.ShowCurrentArmor();
   }
   
}
