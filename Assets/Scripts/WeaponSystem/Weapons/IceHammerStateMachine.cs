using UnityEngine;

public class IceHammerStateMachine : WeaponStateMachine
{
   protected override void Start()
    {
        base.Start();  // Üst sınıftaki Start metodunu çağır
        
        ChangeState(WeaponState.Idle);  // Başlangıçta Idle state'ini aktif et
    }

    protected override void HandleStateChange()
    {
        // Önce tüm bool değerlerini false yap
        animator.SetBool("IceHammerIdle", false);
        animator.SetBool("IceHammerMove", false);
        animator.SetBool("IceHammerSpell1", false);
        animator.SetBool("IceHammerSpell2",false);
        
        animator.SetBool("IceHammerDash", false);
        animator.SetBool("IceHammerJump",false);
        
        animator.SetBool("IceHammerAttack", false);
        animator.SetBool("IceHammerGroundDash", false);
        animator.SetBool("IceHammerCrouch", false);
        animator.SetBool("IceHammerGroundAttack",false);
        animator.SetBool("IceHammerDeath",false);
        animator.SetBool("IceHammerStunned",false);
        animator.SetBool("IceHammerThrowBoomerang",false);
        animator.SetBool("IceHammerCatchBoomerang",false);
        animator.SetBool("IceHammerBlock",false);
        animator.SetBool("IceHammerSuccesfulParry",false);
        animator.SetBool("IceHammerAirPush",false);
        animator.SetBool("IceHammerFireball",false);
       
        // Bool yerine integer kullanıyoruz
        animator.SetInteger("IceHammerComboCounter", 0);

        switch (currentState)
        {
            case WeaponState.Idle:
                animator.SetBool("IceHammerIdle", true);
                break;
                
            case WeaponState.Move:
                animator.SetBool("IceHammerMove", true);
                break;
            case WeaponState.Dash:
                animator.SetBool("IceHammerDash", true);
                break;
                
            case WeaponState.Jump:
                animator.SetBool("IceHammerJump", true);
                break;
            // case WeaponState.Attack:
            //     animator.SetBool("HammerAttack",true);
            //     if (player.stateMachine.currentState is PlayerAttackState attackState)
            //     {
            //         // Direkt combo counter'ı set ediyoruz
            //         animator.SetInteger("HammerComboCounter", attackState.GetComboCounter());
            //     }
            //     break;
            case WeaponState.HammerAttack:
                animator.SetBool("IceHammerAttack",true);
                if (player.stateMachine.currentState is PlayerHammerAttackState hammerAttackState)
                {
                    // Direkt combo counter'ı set ediyoruz
                    animator.SetInteger("IceHammerComboCounter", hammerAttackState.GetComboCounter());
                }
                break;
            case WeaponState.Crouch:
                animator.SetBool("IceHammerCrouch", true);
                break;
            case WeaponState.GroundDash:
                animator.SetBool("IceHammerGroundDash", true);
                break;
            case WeaponState.CrouchAttack:
                animator.SetBool("IceHammerGroundAttack", true);
                break;
            case WeaponState.Death:
                animator.SetBool("IceHammerDeath", true);
                break;
            case WeaponState.Stunned:
                animator.SetBool("IceHammerStunned", true);
                break;
            case WeaponState.ThrowBoomerang:
                animator.SetBool("IceHammerThrowBoomerang", true);
                break;
            case WeaponState.CatchBoomerang:
                animator.SetBool("IceHammerCatchBoomerang", true);
                break;
            case WeaponState.Parry:
                animator.SetBool("IceHammerBlock", true);
                break;
            case WeaponState.Spell1:
                animator.SetBool("IceHammerSpell1", true);
                break;
            case WeaponState.Spell2:
                animator.SetBool("IceHammerSpell2", true);
                break;
            case WeaponState.SuccesfulParry:
                animator.SetBool("IceHammerSuccesfulParry", true);
                break;
            case WeaponState.AirPush:
                animator.SetBool("IceHammerAirPush", true);
                break;
            case WeaponState.FireballSpell:
                animator.SetBool("IceHammerFireball", true);
                break;
        }
    }
    
    protected override void Update()
    {
        base.Update();
        
       
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
        else if (player.stateMachine.currentState == player.airState)
        {
            ChangeState(WeaponState.Jump);
        }
        else if (player.stateMachine.currentState == player.crouchState)
        {
            ChangeState(WeaponState.Crouch);
        }
        else if (player.stateMachine.currentState == player.groundDashState)
        {
            ChangeState(WeaponState.GroundDash);
        }
        else if (player.stateMachine.currentState == player.crouchAttackState)
        {
            ChangeState(WeaponState.CrouchAttack);
        }
        else if (player.stateMachine.currentState == player.deadState)
        {
            ChangeState(WeaponState.Death);
        }
        else if (player.stateMachine.currentState == player.stunnedState)
        {
            ChangeState(WeaponState.Stunned);
        }
        else if (player.stateMachine.currentState == player.throwBoomerangState)
        {
            ChangeState(WeaponState.ThrowBoomerang);
        }
        else if (player.stateMachine.currentState == player.catchBoomerangState)
        {
            ChangeState(WeaponState.CatchBoomerang);
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
        else if (player.stateMachine.currentState == player.succesfulParryState)
        {
            ChangeState(WeaponState.SuccesfulParry);
        }
        else if (player.stateMachine.currentState == player.parryState)
        {
            ChangeState(WeaponState.Parry);
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
