using UnityEngine;

public class NecromancerDeadState : EnemyState
{

    private EnemyBossNecromancerBoss enemy;
    public NecromancerDeadState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName,EnemyBossNecromancerBoss _enemy) : base(_enemyBase, _stateMachine, _animBoolName)
    {
        enemy = _enemy;
    }

    public override void Enter()
    {
        base.Enter();
        
        enemy.collider.enabled = false;
        rb.bodyType = RigidbodyType2D.Static;
        
        // Boss'un yenildiğini kaydet
        if (GameProgressManager.instance != null)
        {
            GameProgressManager.instance.MarkBossAsDefeated("Necromancer");
        }
        
        // Health bar'ı deaktive et
        BossHealthBar healthBar = UnityEngine.Object.FindFirstObjectByType<BossHealthBar>();
        if (healthBar != null)
        {
            healthBar.gameObject.SetActive(false);
        }
        
        // Portal/çıkış objesini aktive et
        if (BossArenaManager.instance != null)
        {
            BossArenaManager.instance.ShowExitPortal();
        }
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
