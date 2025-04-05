using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerDeadState : PlayerState
{
    private bool hasTriggeredRespawn = false;

    public PlayerDeadState(Player _player, PlayerStateMachine _stateMachine, string _animBoolName) : base(_player, _stateMachine, _animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();
        
         CapsuleCollider.enabled = false;
         rb.bodyType = RigidbodyType2D.Static;
        
        rb.linearVelocity = Vector2.zero;
        
        player.playerInput.DisableAllInput();
        
        hasTriggeredRespawn = false;
        
        player.entityFX.PlayDeathEffect();
    }

    public override void Update()
    {
        base.Update();
        
        if (triggerCalled && !hasTriggeredRespawn)
        {
            hasTriggeredRespawn = true;
            player.RespawnAtCheckpoint();
        }
    }

    public override void Exit()
    {
        base.Exit();
        
         CapsuleCollider.enabled = true;
         rb.bodyType = RigidbodyType2D.Dynamic;
        
        player.playerInput.EnableAllInput();
    }
    
    

    public override void AnimationFinishTrigger()
    {
        base.AnimationFinishTrigger();
    }
}
