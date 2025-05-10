using UnityEngine;

public class Bear_AttackState : EnemyState
{
    private Enemy_Bear enemy;
    private float attackTime = 0.5f; // Saldırı animasyonu süresi
    private float attackForwardSpeed = .75f; // Saldırı sırasında ileri kayma hızı
    
    public Bear_AttackState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName,Enemy_Bear _enemy) : base(_enemyBase, _stateMachine, _animBoolName)
    {
        enemy = _enemy;
    }

    public override void Enter()
    {
        base.Enter();
        
        // Timer'ı ayarla
        stateTimer = attackTime;
        
        // Saldırı başlangıcında hızı sıfırlama, böylece ani geçiş olmaz
        enemy.SetZeroVelocity();
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();
        
        // Saldırı sırasında hafif bir kayma hareketi ver
        // Boğanın baktığı yönde düşük bir hızla ilerlesin
        enemy.SetVelocity(attackForwardSpeed * enemy.facingdir, rb.linearVelocity.y);
        
        // Animasyon event'i tetiklendiyse veya süre dolduysa
        if (triggerCalled || stateTimer < 0)
        {
            Debug.Log("Saldırı tamamlandı, Chase'e geçiliyor");
            stateMachine.ChangeState(enemy.chaseState);
        }
    }
}
