using UnityEngine;

public class EnemyChest_IdleState : EnemyState
{
    private Chest_Enemy enemy;
    
    public EnemyChest_IdleState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName,Chest_Enemy _enemy) : base(_enemyBase, _stateMachine, _animBoolName)
    {
        enemy = _enemy;
    }

    public override void Enter()
    {
        base.Enter();
        
        enemy.rb.bodyType = RigidbodyType2D.Static;
    }

    public override void Exit()
    {
        base.Exit();
        
        enemy.rb.bodyType = RigidbodyType2D.Dynamic;

        enemy.capsuleCollider.isTrigger = false;
    }

    public override void Update()
    {
        base.Update();
    }
}
