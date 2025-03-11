using UnityEngine;

public class NecromancerSpellCastState : EnemyState
{
    private EnemyBossNecromancerBoss enemy;

    public NecromancerSpellCastState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, EnemyBossNecromancerBoss _enemy) 
        : base(_enemyBase, _stateMachine, _animBoolName)
    {
        enemy = _enemy;
    }

    public override void Enter()
    {
        base.Enter();
       
    }

    public override void Update()
    {
        base.Update();
        
        enemy.SetZeroVelocity();

        // AnimationFinishTrigger çağrıldığında battle state'e dön
        if (triggerCalled)
        {
            
            stateMachine.ChangeState(enemy.battleState);
        }
    }

    public override void Exit()
    {
        base.Exit();
        //enemy.anim.SetBool(animBoolName, false);
    }
}


