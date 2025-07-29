using UnityEngine;

public class SpellbookWeaponStateMachine : WeaponStateMachine
{
    private const string SPELLBOOK_SPELL2 = "SpellBook_Spell2";
    private const string SPELLBOOK_IDLE = "Spellbook_Idle";

    public void PlaySpell2Animation()
    {
        if (animator != null)
        {
            animator.Play(SPELLBOOK_SPELL2);
        }
    }

    public void PlayIdleAnimation()
    {
        if (animator != null)
        {
            animator.Play(SPELLBOOK_IDLE);
        }
    }

    public void PauseAnimation()
    {
        if (animator != null)
        {
            animator.speed = 0;
        }
    }

    public void ResumeAnimation()
    {
        if (animator != null)
        {
            animator.speed = 1;
        }
    }

    protected override void Start()
    {
        base.Start();

        ChangeState(WeaponState.Idle);
    }

    protected override void HandleStateChange()
    {
        animator.SetBool("SpellbookIdle",false);
        animator.SetBool("SpellbookMove",false);
        animator.SetBool("SpellbookSpell1",false);
        animator.SetBool("SpellbookSpell2",false);
        animator.SetBool("SpellbookAttack",false);
        animator.SetBool("SpellbookDash",false);
        animator.SetBool("SpellbookJump",false);
        animator.SetBool("SpellbookFall",false);
        animator.SetBool("SpellbookGroundDash",false);
        animator.SetBool("SpellbookCrouch",false);
        animator.SetBool("SpellbookGroundAttack",false);
        animator.SetBool("SpellbookDeath",false);
        animator.SetBool("SpellbookStunned",false);
        animator.SetBool("SpellbookParry",false);
        animator.SetBool("SpellbookAirPushSpell",false);
        animator.SetBool("SpellbookFireBallSpell",false);

        switch (currentState)
        {
            case WeaponState.Idle:
                animator.SetBool("SpellbookIdle",true);
                break;
            case WeaponState.Spell1:
                animator.SetBool("SpellbookSpell1", true);
                break;
            case WeaponState.Spell2:
                animator.SetBool("SpellbookSpell2", true);
                break;
            case WeaponState.Move:
                animator.SetBool("SpellbookMove", true);
                break;
            case WeaponState.Attack:
                animator.SetBool("SpellbookAttack",true);
                break;
            case WeaponState.Dash:
                animator.SetBool("SpellbookDash", true);
                break;
            case WeaponState.Jump:
                animator.SetBool("SpellbookJump", true);
                break;
            case WeaponState.Fall:
                animator.SetBool("SpellbookFall",true);
                break;
            case WeaponState.Crouch:
                animator.SetBool("SpellbookCrouch", true);
                break;
            case WeaponState.GroundDash:
                animator.SetBool("SpellbookGroundDash", true);
                break;
            case WeaponState.CrouchAttack:
                animator.SetBool("SpellbookGroundAttack", true);
                break;
            case WeaponState.Parry:
                animator.SetBool("SpellbookParry", true);
                break;
            case WeaponState.Stunned:
                animator.SetBool("SpellbookStunned", true);
                break;
            case WeaponState.Death:
                animator.SetBool("SpellbookDeath", true);
                break;
            case WeaponState.AirPush:
                animator.SetBool("SpellbookAirPushSpell", true);
                break;
            case WeaponState.FireballSpell:
                animator.SetBool("SpellbookFireBallSpell", true);
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
        else if (player.stateMachine.currentState == player.jumpState)
        {
            ChangeState(WeaponState.Jump);
        }
        else if (player.stateMachine.currentState == player.airState)
        {
            ChangeState(WeaponState.Fall);
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
        else if (player.stateMachine.currentState == player.earthPushState)
        {
            ChangeState(WeaponState.Spell1);
        }
    }

}
