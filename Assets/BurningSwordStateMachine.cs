using UnityEngine;

public class BurningSwordStateMachine : WeaponStateMachine
{
  //private Vector3 defaultLocalPosition; // Kılıcın varsayılan local pozisyonu
    //private Vector3 jumpLocalPosition = new Vector3(-0.229f, 0.172f, 0f); 
    //[SerializeField] private Transform SwordJumpPosition;// Jump durumundaki pozisyon

    protected override void Start()
    {
        base.Start();  // Üst sınıftaki Start metodunu çağır
        
        ChangeState(WeaponState.Idle);  // Başlangıçta Idle state'ini aktif et
    }

    protected override void HandleStateChange()
    {
        // Önce tüm bool değerlerini false yap
        animator.SetBool("BurningSwordIdle", false);
        animator.SetBool("BurningSwordMove", false);
        animator.SetBool("BurningSwordSpell1", false);
        animator.SetBool("BurningSwordSpell2",false);
        
        animator.SetBool("BurningSwordDash", false);
        animator.SetBool("BurningSwordJump",false);
        
        animator.SetBool("BurningSwordAttack", false);
        animator.SetBool("BurningSwordGroundDash", false);
        animator.SetBool("BurningSwordCrouch", false);
        animator.SetBool("BurningSwordGroundAttack",false);
        animator.SetBool("BurningSwordDeath",false);
        animator.SetBool("BurningSwordStunned",false);
        animator.SetBool("BurningSwordThrowBoomerang",false);
        animator.SetBool("BurningSwordCatchBoomerang",false);
        animator.SetBool("BurningSwordBlock",false);
        animator.SetBool("BurningSwordSuccesfulParry",false);
        animator.SetBool("BurningSwordAirPush",false);
        animator.SetBool("BurningSwordFireball",false);
       
        // Bool yerine integer kullanıyoruz
        animator.SetInteger("BurningSwordComboCounter", 0);

        switch (currentState)
        {
            case WeaponState.Idle:
                animator.SetBool("BurningSwordIdle", true);
                break;
                
            case WeaponState.Move:
                animator.SetBool("BurningSwordMove", true);
                break;
            case WeaponState.Dash:
                animator.SetBool("BurningSwordDash", true);
                break;
                
            case WeaponState.Jump:
                animator.SetBool("BurningSwordJump", true);
                break;
            case WeaponState.Attack:
                animator.SetBool("BurningSwordAttack",true);
                if (player.stateMachine.currentState is PlayerAttackState attackState)
                {
                    // Direkt combo counter'ı set ediyoruz
                    animator.SetInteger("BurningSwordComboCounter", attackState.GetComboCounter());
                }
                break;
            case WeaponState.Crouch:
                animator.SetBool("BurningSwordCrouch", true);
                break;
            case WeaponState.GroundDash:
                animator.SetBool("BurningSwordGroundDash", true);
                break;
            case WeaponState.CrouchAttack:
                animator.SetBool("BurningSwordGroundAttack", true);
                break;
            case WeaponState.Death:
                animator.SetBool("BurningSwordDeath", true);
                break;
            case WeaponState.Stunned:
                animator.SetBool("BurningSwordStunned", true);
                break;
            case WeaponState.ThrowBoomerang:
                animator.SetBool("BurningSwordThrowBoomerang", true);
                break;
            case WeaponState.CatchBoomerang:
                animator.SetBool("BurningSwordCatchBoomerang", true);
                break;
            case WeaponState.Parry:
                animator.SetBool("BurningSwordBlock", true);
                break;
            case WeaponState.Spell1:
                animator.SetBool("BurningSwordSpell1", true);
                break;
            case WeaponState.Spell2:
                animator.SetBool("BurningSwordSpell2", true);
                break;
            case WeaponState.SuccesfulParry:
                animator.SetBool("BurningSwordSuccesfulParry", true);
                break;
            case WeaponState.AirPush:
                animator.SetBool("BurningSwordAirPush", true);
                break;
            case WeaponState.FireballSpell:
                animator.SetBool("BurningSwordFireball", true);
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
