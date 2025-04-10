using UnityEngine;

public class Bandit_IdleState : Bandit_GroundedState
{
    public Bandit_IdleState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Bandit_Enemy _enemy) : base(_enemyBase, _stateMachine, _animBoolName, _enemy)
    {
    }

    public override void Enter()
    {
        base.Enter();
        
        if (enemy.fightBegun)
            return;
            
        stateTimer = enemy.idleTime;
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();
        
        // Debug.Log("im in idle state");
       
        enemy.SetZeroVelocity();
        
        if (stateTimer < 0f)
        {
            stateMachine.ChangeState(enemy.moveState);
        }
    }
}
