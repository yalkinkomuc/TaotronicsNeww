using UnityEngine;

public class PlayerStats : CharacterStats
{
    private Player player;

    protected override void Start()
    {
        base.Start();
        
        player = GetComponent<Player>();
    }

    public override void TakeDamage(float _damage)
    {
        base.TakeDamage(_damage);
    }

   public override void Die()
   {
        base.Die();
        
        player.Die();
   }
}