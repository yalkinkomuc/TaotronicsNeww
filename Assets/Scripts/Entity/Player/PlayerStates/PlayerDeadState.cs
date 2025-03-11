using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerDeadState : PlayerState
{
    public PlayerDeadState(Player _player, PlayerStateMachine _stateMachine, string _animBoolName) : base(_player, _stateMachine, _animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();
        
        capsuleCollider.enabled = false;
        rb.bodyType = RigidbodyType2D.Static;
        
    }

    public override void Update()
    {
        base.Update();
        
        //player.entityFX.StartFadeOutAndDestroy();
        

        if (triggerCalled)
        {
            SceneManager.instance.LoadActiveScene();
        }
        
    }

    public override void Exit()
    {
        base.Exit();
        
        
    }
    
    

    public override void AnimationFinishTrigger()
    {
        base.AnimationFinishTrigger();
    }
}
