using UnityEngine;

public class ShieldStateMachine : WeaponStateMachine
{
    protected override void Start()
    {
        base.Start();

        ChangeState(WeaponState.Idle);
    }

    protected override void HandleStateChange()
    {
        animator.SetBool("ShieldIdle",false);
        animator.SetBool("ShieldMove",false);
        animator.SetBool("ShieldSpell1",false);
        animator.SetBool("ShieldSpell2",false);
        animator.SetBool("ShieldAttack",false);
        animator.SetBool("ShieldDash",false);
        animator.SetBool("ShieldJump",false);
        animator.SetBool("ShieldGroundDash",false);
        animator.SetBool("ShieldCrouch",false);
        animator.SetBool("ShieldGroundAttack",false);
        animator.SetBool("ShieldDeath",false);
        animator.SetBool("ShieldStunned",false);
        animator.SetBool("ShieldParry",false);
        animator.SetBool("ShieldAirPushSpell",false);
        animator.SetBool("ShieldFireBallSpell",false);

        switch (currentState)
        {
            case WeaponState.Idle:
                animator.SetBool("ShieldIdle",true);
                break;
            case WeaponState.Spell1:
                animator.SetBool("ShieldSpell1", true);
                break;
            case WeaponState.Spell2:
                animator.SetBool("ShieldSpell2", true);
                break;
            case WeaponState.Move:
                animator.SetBool("ShieldMove", true);
                break;
            case WeaponState.Attack:
                animator.SetBool("ShieldAttack",true);
                break;
            case WeaponState.Dash:
                animator.SetBool("ShieldDash", true);
                break;
            case WeaponState.Jump:
                animator.SetBool("ShieldJump", true);
                break;
            case WeaponState.Crouch:
                animator.SetBool("ShieldCrouch", true);
                break;
            case WeaponState.GroundDash:
                animator.SetBool("ShieldGroundDash", true);
                break;
            case WeaponState.CrouchAttack:
                animator.SetBool("ShieldGroundAttack", true);
                break;
            case WeaponState.Parry:
                animator.SetBool("ShieldParry", true);
                break;
            case WeaponState.Stunned:
                animator.SetBool("ShieldStunned", true);
                break;
            case WeaponState.Death:
                animator.SetBool("ShieldDeath", true);
                break;
            case WeaponState.AirPush:
                animator.SetBool("ShieldAirPushSpell", true);
                break;
            case WeaponState.FireballSpell:
                animator.SetBool("ShieldFireBallSpell", true);
                break;
        }
    }


    protected override void Update()
    {
        base.Update();

        // State kontrolleri
        if (player.stateMachine.currentState == player.idleState)
        {
            ChangeState(WeaponState.Idle);
        }
        else if (player.stateMachine.currentState == player.attackState)
        {
            ChangeState(WeaponState.Attack);
        }
        else if (player.stateMachine.currentState == player.spell1State)
        {
            ChangeState(WeaponState.Spell1);
        }
        else if (player.stateMachine.currentState == player.spell2State)
        {
            // SkillManager kontrolü - Fire Spell açık mı?
            if (SkillManager.Instance != null && !SkillManager.Instance.IsSkillUnlocked("FireSpell"))
            {
                // Fire Spell açık değilse Idle state'ine geç
                ChangeState(WeaponState.Idle);
            }
            else
            {
                ChangeState(WeaponState.Spell2);
            }
        }
        else if (player.stateMachine.currentState == player.moveState)
        {
            ChangeState(WeaponState.Move);
        }
        else if (player.stateMachine.currentState == player.dashState)
        {
            ChangeState(WeaponState.Dash);
        }
        else if (player.stateMachine.currentState == player.groundDashState)
        {
            ChangeState(WeaponState.GroundDash);
        }
        else if (player.stateMachine.currentState == player.crouchAttackState)
        {
            ChangeState(WeaponState.CrouchAttack);
        }
        else if (player.stateMachine.currentState == player.airState)
        {
            ChangeState(WeaponState.Jump);
        }
        else if (player.stateMachine.currentState == player.crouchState)
        {
            ChangeState(WeaponState.Crouch);
        }
        else if (player.stateMachine.currentState == player.deadState)
        {
            ChangeState(WeaponState.Death);
        }
        else if (player.stateMachine.currentState == player.stunnedState)
        {
            ChangeState(WeaponState.Stunned);
        }
        else if (player.stateMachine.currentState == player.airPushState)
        {
            ChangeState(WeaponState.AirPush);
        }
        else if (player.stateMachine.currentState ==player.fireballSpellState)
        {
            ChangeState(WeaponState.FireballSpell);
        }
    }
}
