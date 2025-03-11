using UnityEngine;

public class NecromancerTeleportState : EnemyState
{
    private EnemyBossNecromancerBoss enemy;
    

    public NecromancerTeleportState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, EnemyBossNecromancerBoss _enemy) 
        : base(_enemyBase, _stateMachine, _animBoolName)
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
        
        // TP'den çıkarken oyuncuya dön
        float xDirection = enemy.player.transform.position.x - enemy.transform.position.x;
        enemy.facingdir = xDirection > 0 ? 1 : -1;
    }

    public override void Update()
    {
        base.Update();
        
        enemy.SetZeroVelocity();

        if (triggerCalled)
        {
            stateMachine.ChangeState(enemy.battleState);
        }
    }
}
