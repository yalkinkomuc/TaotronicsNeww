using UnityEngine;

public class EnemyChest_DeadState : EnemyState
{
    private Chest_Enemy enemy;
    
    public EnemyChest_DeadState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName,Chest_Enemy _enemy) : base(_enemyBase, _stateMachine, _animBoolName)
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
