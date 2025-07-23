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
        animator.SetBool("ShieldAttack",false);
        animator.SetBool("ShieldHammerAttack", false);
        animator.SetBool("ShieldDash",false);
        animator.SetBool("ShieldJump",false);
        animator.SetBool("ShieldGroundDash",false);
        animator.SetBool("ShieldCrouch",false);
        animator.SetBool("ShieldGroundAttack",false);
        animator.SetBool("ShieldDeath",false);
        animator.SetBool("ShieldStunned",false);
        animator.SetBool("ShieldParry",false);
        animator.SetBool("ShieldSuccesfulParry",false);
        animator.SetBool("ShieldFireBallSpell",false);
        
        animator.SetInteger("ShieldSwordComboCounter", 0);
        animator.SetInteger("ShieldHammerComboCounter", 0);

        switch (currentState)
        {
            case WeaponState.Idle:
                animator.SetBool("ShieldIdle",true);
                break;
            case WeaponState.Move:
                animator.SetBool("ShieldMove", true);
                break;
            case WeaponState.Attack:
                animator.SetBool("ShieldAttack",true);
                if (player.stateMachine.currentState is PlayerAttackState attackState)
                {
                    animator.SetInteger("ShieldSwordComboCounter",attackState.GetComboCounter());
                }
                break;
            case WeaponState.HammerAttack:
                animator.SetBool("ShieldHammerAttack", true);
                if (player.stateMachine.currentState is PlayerHammerAttackState hammerAttackState)
                {
                    // Direkt combo counter'ı set ediyoruz
                    animator.SetInteger("ShieldHammerComboCounter", hammerAttackState.GetComboCounter());
                }
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
            case WeaponState.SuccesfulParry:
                animator.SetBool("ShieldSuccesfulParry",true);
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
        else if (player.stateMachine.currentState == player.hammerAttackState)
        {
            ChangeState(WeaponState.HammerAttack);
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
        else if (player.stateMachine.currentState ==player.fireballSpellState)
        {
            ChangeState(WeaponState.FireballSpell);
        }
        else if (player.stateMachine.currentState == player.parryState)
        {
            ChangeState(WeaponState.Parry);
        }
        else if (player.stateMachine.currentState == player.succesfulParryState)
        {
            ChangeState(WeaponState.SuccesfulParry);
        }
    }
}
