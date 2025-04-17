using UnityEngine;

public class SkeletonArcher_DeadState : EnemyState
{
    private Enemy_ArcherSkeleton enemy;
    public SkeletonArcher_DeadState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName,Enemy_ArcherSkeleton _enemy) : base(_enemyBase, _stateMachine, _animBoolName)
    {
        enemy = _enemy;
    }

    public override void Enter()
    {
        base.Enter();
        
        enemy.capsuleCollider.enabled = false;
        rb.bodyType = RigidbodyType2D.Static;
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();
        
        if (triggerCalled)
        {
           
            enemy.entityFX.StartFadeOutAndDestroy();
        }
    }
}
