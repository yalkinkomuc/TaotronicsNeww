using UnityEngine;

public class Bandit_DeadState : EnemyState
{
    private Bandit_Enemy enemy;
    public Bandit_DeadState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName,Bandit_Enemy _enemy) : base(_enemyBase, _stateMachine, _animBoolName)
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
        
        enemy.entityFX.StartFadeOutAndDestroy();
    }
}
