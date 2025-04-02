using UnityEngine;

public class Boar_DeadState : EnemyState
{
    private Boar_Enemy enemy;

    public Boar_DeadState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName,Boar_Enemy _enemy) : base(_enemyBase, _stateMachine, _animBoolName)
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
