using UnityEngine;

public class Bandit_AttackState : EnemyState
{
    
    private Bandit_Enemy enemy;
    public Bandit_AttackState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName,Bandit_Enemy _enemy) : base(_enemyBase, _stateMachine, _animBoolName)
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
        
        // Make sure attack and parry windows are closed when exiting
        enemy.isAttackActive = false;
        enemy.isParryWindowOpen = false;
        
        enemy.lastTimeAttacked = Time.time;
    }

    public override void Update()
    {
        base.Update();
        
        
        // Eğer düşman stunned state'e geçerse (parry yediyse) bu state'den çık
        // if (enemy.stateMachine.currentState == enemy.stunnedState)
        // {
        //     return;
        // }
        
        enemy.SetZeroVelocity();

        if (triggerCalled)
        {
            stateMachine.ChangeState(enemy.battleState);
        }
    }
    
    public bool IsParryWindowOpen()
    {
        return enemy.isParryWindowOpen;
    }
}
