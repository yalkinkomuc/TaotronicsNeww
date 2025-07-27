using UnityEngine;

public class HammerSwordStateMachine : WeaponStateMachine
{
      protected override void Start()
    {
        base.Start();  // Üst sınıftaki Start metodunu çağır
        
        ChangeState(WeaponState.Idle);  // Başlangıçta Idle state'ini aktif et
    }

    protected override void HandleStateChange()
    {
        // Önce tüm bool değerlerini false yap
        animator.SetBool("HammerIdle", false);
        animator.SetBool("HammerMove", false);
        animator.SetBool("HammerSpell1", false);
        animator.SetBool("HammerSpell2",false);
        animator.SetBool("HammerEarthPush",false);
        
        animator.SetBool("HammerDash", false);
        animator.SetBool("HammerJump",false);
        
        animator.SetBool("HammerAttack", false);
        animator.SetBool("HammerGroundDash", false);
        animator.SetBool("HammerCrouch", false);
        animator.SetBool("HammerGroundAttack",false);
        animator.SetBool("HammerDeath",false);
        animator.SetBool("HammerStunned",false);
        animator.SetBool("HammerThrowBoomerang",false);
        animator.SetBool("HammerCatchBoomerang",false);
        animator.SetBool("HammerBlock",false);
        animator.SetBool("HammerSuccesfulParry",false);
        animator.SetBool("HammerAirPush",false);
        animator.SetBool("HammerFireball",false);
       
        // Bool yerine integer kullanıyoruz
        animator.SetInteger("HammerComboCounter", 0);

        switch (currentState)
        {
            case WeaponState.Idle:
                animator.SetBool("HammerIdle", true);
                break;
                
            case WeaponState.Move:
                animator.SetBool("HammerMove", true);
                break;
            case WeaponState.Dash:
                animator.SetBool("HammerDash", true);
                break;
                
            case WeaponState.Jump:
                animator.SetBool("HammerJump", true);
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
                animator.SetBool("HammerAttack",true);
                if (player.stateMachine.currentState is PlayerHammerAttackState hammerAttackState)
                {
                    // Direkt combo counter'ı set ediyoruz
                    animator.SetInteger("HammerComboCounter", hammerAttackState.GetComboCounter());
                }
                break;
            case WeaponState.Crouch:
                animator.SetBool("HammerCrouch", true);
                break;
            case WeaponState.GroundDash:
                animator.SetBool("HammerGroundDash", true);
                break;
            case WeaponState.CrouchAttack:
                animator.SetBool("HammerGroundAttack", true);
                break;
            case WeaponState.Death:
                animator.SetBool("HammerDeath", true);
                break;
            case WeaponState.Stunned:
                animator.SetBool("HammerStunned", true);
                break;
            case WeaponState.ThrowBoomerang:
                animator.SetBool("HammerThrowBoomerang", true);
                break;
            case WeaponState.CatchBoomerang:
                animator.SetBool("HammerCatchBoomerang", true);
                break;
            case WeaponState.Parry:
                animator.SetBool("HammerBlock", true);
                break;
            case WeaponState.Spell1:
                animator.SetBool("HammerSpell1", true);
                break;
            case WeaponState.Spell2:
                animator.SetBool("HammerSpell2", true);
                break;
            case WeaponState.EarthPush:
                animator.SetBool("HammerEarthPush",true);
                break;
            case WeaponState.SuccesfulParry:
                animator.SetBool("HammerSuccesfulParry", true);
                break;
            case WeaponState.AirPush:
                animator.SetBool("HammerAirPush", true);
                break;
            case WeaponState.FireballSpell:
                animator.SetBool("HammerFireball", true);
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
        else if (player.stateMachine.currentState == player.earthPushState)
        {
            ChangeState(WeaponState.EarthPush);
        }
        else if (player.stateMachine.currentState ==player.fireballSpellState)
        {
            ChangeState(WeaponState.FireballSpell);
        }
    }
}
