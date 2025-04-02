using UnityEngine;

public class EnemyStats : CharacterStats
{

    private Enemy enemy;
    private ItemDrop myDropSystem;
    protected override void Start()
    {
        base.Start();

        enemy = GetComponent<Enemy>();
        myDropSystem = GetComponent<ItemDrop>();

    }


    public override void TakeDamage(float _damage)
    {
        base.TakeDamage(_damage);
    }

    public override void Die()
    {
        base.Die();
        enemy.Die();
        
        myDropSystem.GenerateDrop();
    }
}

