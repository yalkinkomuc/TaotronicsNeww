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
         
         enemyUnit.Damage();
         enemyUnit.stats.TakeDamage(damage);
      }
   }
}
