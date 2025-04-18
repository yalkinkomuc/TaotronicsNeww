using UnityEngine;

public class PlayerParryState : PlayerState
{
    public PlayerParryState(Player _player, PlayerStateMachine _stateMachine, string _animBoolName) : base(_player, _stateMachine, _animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();

        stateTimer = .25f;
    }

    public override void Update()
    {
        base.Update();
        
        player.SetZeroVelocity();

        if (IsEnemyParryWindowOpen())
        {
            stateMachine.ChangeState(player.succesfulParryState);
            return;
        }

        if (stateTimer < 0f)
        {
            stateMachine.ChangeState(player.idleState);
        }
    }

    public override void Exit()
    {
        base.Exit();
    }
    
    private bool IsEnemyParryWindowOpen()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(player.transform.position, player.parryRadius, player.passableEnemiesLayerMask);
        
        foreach (var hit in colliders)
        {
            EliteSkeleton_Enemy eliteSkeleton = hit.GetComponent<EliteSkeleton_Enemy>();
            if (eliteSkeleton != null && eliteSkeleton.isParryWindowOpen)
            {
                return true;
            }
            
            Bandit_Enemy bandit = hit.GetComponent<Bandit_Enemy>();
            if (bandit != null && bandit.isParryWindowOpen)
            {
                return true;
            }
        }
        
        return false;
    }
}
