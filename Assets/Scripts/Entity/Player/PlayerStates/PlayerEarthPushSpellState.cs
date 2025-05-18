using UnityEngine;
using System.Collections;

public class PlayerEarthPushSpellState : PlayerState
{
    private bool isAnimationFinished;
    
    public PlayerEarthPushSpellState(Player _player, PlayerStateMachine _stateMachine, string _animBoolName) 
        : base(_player, _stateMachine, _animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();
        
        // Reset state variables
        isAnimationFinished = false;
        
        // Check if player has enough mana
        if (!player.HasEnoughMana(player.earthPushManaCost))
        {
            Debug.Log($"Not enough mana for Earth Push! Required: {player.earthPushManaCost}, Current: {player.stats.currentMana}");
            stateMachine.ChangeState(player.idleState);
            return;
        }
        
        // Consume mana - cooldown is managed in the Player class
        player.UseMana(player.earthPushManaCost);
    }

    public override void Update()
    {
        base.Update();
        
        // Keep the player in place during casting
        player.SetZeroVelocity();
        
        // Exit the state when animation is finished
        if (isAnimationFinished)
        {
            stateMachine.ChangeState(player.idleState);
        }
    }

    public override void Exit()
    {
        base.Exit();
    }
    
    public override void AnimationFinishTrigger()
    {
        base.AnimationFinishTrigger();
        isAnimationFinished = true;
    }
} 