using UnityEngine;

public class PlayerAttackState : PlayerState
{
    protected int comboCounter = 0;
    
    private float lastTimeAttacked;
    private float comboWindow = 2;

    public int GetComboCounter() => comboCounter;

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
        stateTimer = .1f;
        
        player.StartNewAttack();
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

        if (stateTimer < 0f)
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
