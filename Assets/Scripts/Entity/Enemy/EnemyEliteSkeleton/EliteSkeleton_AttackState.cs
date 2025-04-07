using UnityEngine;

public class EliteSkeleton_AttackState : EnemyState
{

    private EliteSkeleton_Enemy enemy;
     
    public EliteSkeleton_AttackState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName,EliteSkeleton_Enemy _enemy) : base(_enemyBase, _stateMachine, _animBoolName)
    {
        enemy = _enemy;
    }

    public override void Enter()
    {
        base.Enter();
        
        enemy.fightBegun = true;
        
        // Reset hit entities when starting a new attack
        enemy.ClearHitEntities();
        enemy.isAttackActive = false; // Will be enabled by animation event
    }

    public override void Exit()
    {
        base.Exit();
        
        enemy.lastTimeAttacked = Time.time;
    }

    public override void Update()
    {
        base.Update();
        
        Debug.Log("im in attack state");
        
        enemy.SetZeroVelocity();

        if (triggerCalled)
        {
            stateMachine.ChangeState(enemy.battleState);
        }
    }
}
