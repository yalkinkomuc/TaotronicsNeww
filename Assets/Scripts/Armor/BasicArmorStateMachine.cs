using UnityEngine;

public class BasicArmorStateMachine : ArmorStateMachine
{
    protected override void Start()
    {
        base.Start(); // Üst sınıftaki Start metodunu çağır

        ChangeState(ArmorState.Idle); // Başlangıçta Idle state'ini aktif et
    }

    protected override void HandleStateChange()
    {
        // Önce tüm bool değerlerini false yap
        animator.SetBool("ArmorIdle", false);
        animator.SetBool("ArmorMove", false);
        animator.SetBool("ArmorSpell1", false);
        animator.SetBool("ArmorSpell2", false);

        animator.SetBool("ArmorDash", false);
        animator.SetBool("ArmorJump", false);

        animator.SetBool("ArmorAttack", false);
        animator.SetBool("ArmorGroundDash", false);
        animator.SetBool("ArmorCrouch", false);
        animator.SetBool("ArmorGroundAttack", false);
        animator.SetBool("ArmorDeath", false);
        animator.SetBool("ArmorStunned", false);
        animator.SetBool("ArmorThrowBoomerang", false);
        animator.SetBool("ArmorCatchBoomerang", false);
        animator.SetBool("ArmorBlock", false);
        animator.SetBool("ArmorSuccesfulParry", false);
        animator.SetBool("ArmorAirPush", false);
        animator.SetBool("ArmorFireball", false);

        // Bool yerine integer kullanıyoruz
        animator.SetInteger("ArmorSwordComboCounter", 0);
        animator.SetInteger("ArmorHammerComboCounter", 0);

        switch (currentState)
        {
            case ArmorState.Idle:
                animator.SetBool("ArmorIdle", true);
                break;

            case ArmorState.Move:
                animator.SetBool("ArmorMove", true);
                break;
            case ArmorState.Dash:
                animator.SetBool("ArmorDash", true);
                break;

            case ArmorState.Jump:
                animator.SetBool("ArmorJump", true);
                break;
            
            case ArmorState.Attack:
                animator.SetBool("ArmorAttack",true);
            if (player.stateMachine.currentState is PlayerAttackState attackState)
            {
                animator.SetInteger("ArmorSwordComboCounter",attackState.GetComboCounter());
            }
                break;
            
            case ArmorState.HammerAttack:
                animator.SetBool("ArmorHammerAttack", true);
                if (player.stateMachine.currentState is PlayerHammerAttackState hammerAttackState)
                {
                    // Direkt combo counter'ı set ediyoruz
                    animator.SetInteger("ArmorHammerComboCounter", hammerAttackState.GetComboCounter());
                }

                break;
            case ArmorState.Crouch:
                animator.SetBool("ArmorCrouch", true);
                break;
            case ArmorState.GroundDash:
                animator.SetBool("ArmorGroundDash", true);
                break;
            case ArmorState.CrouchAttack:
                animator.SetBool("ArmorGroundAttack", true);
                break;
            case ArmorState.Death:
                animator.SetBool("ArmorDeath", true);
                break;
            case ArmorState.Stunned:
                animator.SetBool("ArmorStunned", true);
                break;
            case ArmorState.ThrowBoomerang:
                animator.SetBool("ArmorThrowBoomerang", true);
                break;
            case ArmorState.CatchBoomerang:
                animator.SetBool("ArmorCatchBoomerang", true);
                break;
            case ArmorState.Parry:
                animator.SetBool("ArmorBlock", true);
                break;
            case ArmorState.Spell1:
                animator.SetBool("ArmorSpell1", true);
                break;
            case ArmorState.Spell2:
                animator.SetBool("ArmorSpell2", true);
                break;
            case ArmorState.SuccesfulParry:
                animator.SetBool("ArmorSuccesfulParry", true);
                break;
            case ArmorState.AirPush:
                animator.SetBool("ArmorAirPush", true);
                break;
            case ArmorState.FireballSpell:
                animator.SetBool("ArmorFireball", true);
                break;
        }
    }

    protected override void Update()
    {
        base.Update();


        if (player.stateMachine.currentState == player.idleState)
        {
            ChangeState(ArmorState.Idle);
        }
        else if (player.stateMachine.currentState == player.attackState)
        {
            ChangeState(ArmorState.Attack);
        }
        else if (player.stateMachine.currentState == player.hammerAttackState)
        {
            ChangeState(ArmorState.HammerAttack);
        }
        else if (player.stateMachine.currentState == player.moveState)
        {
            ChangeState(ArmorState.Move);
        }
        else if (player.stateMachine.currentState == player.dashState)
        {
            ChangeState(ArmorState.Dash);
        }
        else if (player.stateMachine.currentState == player.airState)
        {
            ChangeState(ArmorState.Jump);
        }
        else if (player.stateMachine.currentState == player.crouchState)
        {
            ChangeState(ArmorState.Crouch);
        }
        else if (player.stateMachine.currentState == player.groundDashState)
        {
            ChangeState(ArmorState.GroundDash);
        }
        else if (player.stateMachine.currentState == player.crouchAttackState)
        {
            ChangeState(ArmorState.CrouchAttack);
        }
        else if (player.stateMachine.currentState == player.deadState)
        {
            ChangeState(ArmorState.Death);
        }
        else if (player.stateMachine.currentState == player.stunnedState)
        {
            ChangeState(ArmorState.Stunned);
        }
        else if (player.stateMachine.currentState == player.throwBoomerangState)
        {
            ChangeState(ArmorState.ThrowBoomerang);
        }
        else if (player.stateMachine.currentState == player.catchBoomerangState)
        {
            ChangeState(ArmorState.CatchBoomerang);
        }
        else if (player.stateMachine.currentState == player.spell1State)
        {
            ChangeState(ArmorState.Spell1);
        }
        else if (player.stateMachine.currentState == player.spell2State)
        {
            // SkillManager kontrolü - Fire Spell açık mı?
            if (SkillManager.Instance != null && !SkillManager.Instance.IsSkillUnlocked("FireSpell"))
            {
                // Fire Spell açık değilse Idle state'ine geç
                ChangeState(ArmorState.Idle);
            }
            else
            {
                ChangeState(ArmorState.Spell2);
            }
        }
        else if (player.stateMachine.currentState == player.succesfulParryState)
        {
            ChangeState(ArmorState.SuccesfulParry);
        }
        else if (player.stateMachine.currentState == player.parryState)
        {
            ChangeState(ArmorState.Parry);
        }
        else if (player.stateMachine.currentState == player.airPushState)
        {
            ChangeState(ArmorState.AirPush);
        }
        else if (player.stateMachine.currentState == player.fireballSpellState)
        {
            ChangeState(ArmorState.FireballSpell);
        }
    }
}
