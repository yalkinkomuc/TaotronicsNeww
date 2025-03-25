using UnityEngine;

public class PlayerAttackState : PlayerState
{

    private int comboCounter;
    
    private float lastTimeAttacked;
    private float comboWindow = 2;
    public PlayerAttackState(Player _player, PlayerStateMachine _stateMachine, string _animBoolName) : base(_player, _stateMachine, _animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();

        if (comboCounter > 2 || Time.time >= lastTimeAttacked + comboWindow)
        {
            comboCounter = 0;
        }
        
        player.anim.SetInteger("comboCounter", comboCounter);
        
    }

    public override void Exit()
    {
        base.Exit();
        comboCounter++;
        
        lastTimeAttacked = Time.time;
    }

    public override void Update()
    {
        base.Update();

        if (player.IsGroundDetected())
        {
            player.SetZeroVelocity();
        }
        else
        {
            player.SetVelocity(rb.linearVelocity.x*.5f, rb.linearVelocity.y);
        }


        if (triggerCalled)
        {
            stateMachine.ChangeState(player.idleState);
        }
        
       
    }
}
