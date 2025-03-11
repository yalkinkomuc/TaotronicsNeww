using UnityEngine;

public class EnemyStats : CharacterStats
{

    private Enemy enemy;
    protected override void Start()
    {
        base.Start();

        enemy = GetComponent<Enemy>();

    }


    public override void TakeDamage(int _damage)
    {
        base.TakeDamage(_damage);
    }

    public override void Die()
    {
        base.Die();
        enemy.Die();
    }
}

