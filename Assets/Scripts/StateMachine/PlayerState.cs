using UnityEngine;

public class PlayerState
{
   protected PlayerStateMachine stateMachine;
   protected Player player;
   protected float horizontal;
   protected Rigidbody2D rb;
   protected CapsuleCollider2D CapsuleCollider;
   protected float xInput;
   protected float yInput;
   protected float stateTimer;
   protected bool triggerCalled;
   

   private string animBoolName;

   public PlayerState(Player _player, PlayerStateMachine _stateMachine, string _animBoolName)
   {
      this.player = _player;
      this.stateMachine = _stateMachine;
      this.animBoolName = _animBoolName;
      
   }

   public virtual void Enter()
   {
      player.anim.SetBool(animBoolName, true);
      
      rb = player.rb;
      CapsuleCollider = player.capsuleCollider;
      
      triggerCalled = false;
      
      
   }

   public virtual void Update()
   {
      stateTimer-=Time.deltaTime;
      xInput = player.playerInput.xInput;
      //yInput = player.playerInput.yInput;
      
      
      //Debug.Log(xInput);
   }

   public virtual void Exit()
   {
      player.anim.SetBool(animBoolName, false);
   }

   public virtual void AnimationFinishTrigger()
   {
      triggerCalled = true;   
   }
   
}
