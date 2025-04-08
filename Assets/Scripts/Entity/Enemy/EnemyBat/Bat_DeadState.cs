using UnityEngine;

public class Bat_DeadState : EnemyState
{

    private Bat_Enemy enemy;
    public Bat_DeadState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName,Bat_Enemy _enemy) : base(_enemyBase, _stateMachine, _animBoolName)
    {
        enemy = _enemy;
    }

    public override void Enter()
    {
        base.Enter();
        
        enemy.capsuleCollider.enabled = false;
        rb.gravityScale = 3f;
        rb.bodyType = RigidbodyType2D.Static;
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();
        
        enemy.entityFX.StartFadeOutAndDestroy();
    }
}
