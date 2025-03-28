using UnityEngine;

public class Boar_ChargeState : EnemyState
{

    private Boar_Enemy enemy;
    private float chargeTime = 1.5f; // Varsayılan gerilme süresi
    
    public Boar_ChargeState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName,Boar_Enemy _enemy) : base(_enemyBase, _stateMachine, _animBoolName)
    {
        enemy = _enemy;
    }

    public override void Enter()
    {
        base.Enter();
        
        // Timer'ı ayarla
        stateTimer = chargeTime;
        
        // Oyuncuya doğru dön
        enemy.FacePlayer();
        
        // Gerilme sırasında hareketi durdur
        enemy.SetZeroVelocity();
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();
        
        // Oyuncuya doğru bak
        enemy.FacePlayer();
        
        // Gerilme süreci boyunca sabit kalması için
        enemy.SetZeroVelocity();

        // Animasyon event'i tetiklendiyse veya süre dolduysa
        if (triggerCalled || stateTimer < 0)
        {
            Debug.Log("Charge tamamlandı, Chase'e geçiliyor");
            stateMachine.ChangeState(enemy.chaseState);
        }
    }
}
