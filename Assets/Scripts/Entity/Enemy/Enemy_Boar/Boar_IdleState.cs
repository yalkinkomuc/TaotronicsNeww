using UnityEngine;

public class Boar_IdleState : EnemyState
{

    private Boar_Enemy enemy;
    private Transform player;
    
    public Boar_IdleState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName,Boar_Enemy _enemy) : base(_enemyBase, _stateMachine, _animBoolName)
    {

        enemy = _enemy;
    }


    public override void Enter()
    {
        base.Enter();

        stateTimer = enemy.idleTime;
        
        player = PlayerManager.instance.player.transform;
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();
        
        enemy.SetZeroVelocity();

        if (stateTimer < 0f)
        {
            stateMachine.ChangeState(enemy.moveState);
        }
    }
}
