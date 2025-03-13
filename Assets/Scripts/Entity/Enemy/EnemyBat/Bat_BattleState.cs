using UnityEngine;

public class Bat_BattleState : EnemyState
{
    private Bat_Enemy enemy;
    public Bat_BattleState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName,Bat_Enemy _enemy) : base(_enemyBase, _stateMachine, _animBoolName)
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
