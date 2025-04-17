using UnityEngine;

public class SkeletonDeadState : EnemyState
{

    private Skeleton_Enemy enemy;
    public SkeletonDeadState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName,Skeleton_Enemy _enemy) : base(_enemyBase, _stateMachine, _animBoolName)
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
        
       // Debug.Log("im in battleState");

       if (triggerCalled)
       {
           
        enemy.entityFX.StartFadeOutAndDestroy();
       }
        
        
        
    }
}
