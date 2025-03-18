using UnityEngine;

public class EliteSkeleton_DeadState : EnemyState
{

    private EliteSkeleton_Enemy enemy;
    public EliteSkeleton_DeadState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName,EliteSkeleton_Enemy _enemy) : base(_enemyBase, _stateMachine, _animBoolName)
    {
        enemy = _enemy;
    }

    public override void Enter()
    {
        base.Enter();
        
        enemy.boxCollider.enabled = false;
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
