using UnityEngine;

public class EnemyChest_TransformState : EnemyState
{
    private Chest_Enemy enemy;
    
    public EnemyChest_TransformState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName,Chest_Enemy _enemy) : base(_enemyBase, _stateMachine, _animBoolName)
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

        if (triggerCalled)
        {
            stateMachine.ChangeState(enemy.chaseState);
        }
    }
}
