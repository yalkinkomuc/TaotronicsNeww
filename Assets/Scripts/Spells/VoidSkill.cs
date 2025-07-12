using System;
using UnityEngine;

public class VoidSkill : MonoBehaviour
{
   private BoxCollider2D boxCollider;
   [SerializeField] private float damage;

   private void Awake()
   {
      boxCollider = GetComponent<BoxCollider2D>();
   }

   private void OnTriggerEnter2D(Collider2D other)
   {
      if (other.GetComponent<Enemy>() != null)
      {
         Enemy enemyUnit = other.GetComponent<Enemy>();
         
         Player player = PlayerManager.instance?.player;
         bool isCritical = false;
         float finalDamage = damage;
         
         if (player != null && player.stats is PlayerStats playerStats)
         {
            // Use WeaponDamageManager to get spell damage from spellbook weapon
            finalDamage = WeaponDamageManager.GetSpellDamage(playerStats);
            
            // Check for critical hit
            if (playerStats.IsCriticalHit())
            {
               finalDamage *= 1.5f;
               isCritical = true;
            }
         }
         enemyUnit.Damage();
         enemyUnit.stats.TakeDamage(finalDamage,CharacterStats.DamageType.Void);
         
         if (FloatingTextManager.Instance != null)
         {
            Vector3 textPosition = enemyUnit.transform.position + Vector3.up * 1.5f;
            if (isCritical)
               FloatingTextManager.Instance.ShowCustomText(finalDamage.ToString("0"), textPosition, Color.yellow);
            else
               FloatingTextManager.Instance.ShowCustomText(finalDamage.ToString("0"), textPosition, new Color(0.5f,0,1f));
         }
      }
   }
}
