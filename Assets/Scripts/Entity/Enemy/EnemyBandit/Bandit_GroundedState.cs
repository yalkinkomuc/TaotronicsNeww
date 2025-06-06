using UnityEngine;

public class Bandit_GroundedState : EnemyState
{
    protected Bandit_Enemy enemy;
    protected Transform player;
    
    
    public Bandit_GroundedState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName,Bandit_Enemy _enemy) : base(_enemyBase, _stateMachine, _animBoolName)
    {
        enemy = _enemy;
    }

    public override void Enter()
    {
        base.Enter();
        
        player = PlayerManager.instance.player.transform;
        
        if (enemy.fightBegun)
        {
            stateMachine.ChangeState(enemy.battleState);
            return;
        }
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();
        
        // Debug.Log("im in grounded state");
        
        if (enemy.fightBegun)
        {
            stateMachine.ChangeState(enemy.battleState);
            return;
        }
        
        if (enemy.IsPlayerDetected() || enemy.IsTooCloseToPlayer())
        {
            stateMachine.ChangeState(enemy.battleState);
        }
    }
}
