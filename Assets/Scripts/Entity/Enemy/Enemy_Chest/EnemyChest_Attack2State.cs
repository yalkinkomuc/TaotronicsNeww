using UnityEngine;

public class EnemyChest_Attack2State : EnemyState
{
    private Chest_Enemy enemy;
    
    public EnemyChest_Attack2State(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName,Chest_Enemy _enemy) : base(_enemyBase, _stateMachine, _animBoolName)
    {
        enemy = _enemy;

    }

    public override void Enter()
    {
        base.Enter();
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();
    }
}
