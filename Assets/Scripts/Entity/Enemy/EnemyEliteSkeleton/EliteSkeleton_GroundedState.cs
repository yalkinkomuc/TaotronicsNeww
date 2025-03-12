using UnityEngine;

public class EliteSkeleton_GroundedState : EnemyState
{
    protected EliteSkeleton_Enemy enemy;
    protected Transform player;
    
    public EliteSkeleton_GroundedState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName,EliteSkeleton_Enemy _enemy) : base(_enemyBase, _stateMachine, _animBoolName)
    {
        enemy = _enemy;
    }

    public override void Enter()
    {
        base.Enter();
        
        player = PlayerManager.instance.player.transform;
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();
        
        Debug.Log("im in grounded state");
        
        if (enemy.IsPlayerDetected() ||enemy.IsTooCloseToPlayer())
        {
            stateMachine.ChangeState(enemy.battleState);
        }
    }
}
