using UnityEngine;

public class SwordWeaponStateMachine : WeaponStateMachine
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
        animator.SetBool("SwordIdle", false);
        animator.SetBool("SwordMove", false);
        animator.SetBool("SwordSpell1", false);
        animator.SetBool("SwordSpell2",false);
        animator.SetBool("SwordEarthPush",false);
        
        animator.SetBool("SwordDash", false);
        animator.SetBool("SwordJump",false);
        
        animator.SetBool("SwordAttack", false);
        animator.SetBool("SwordGroundDash", false);
        animator.SetBool("SwordCrouch", false);
        animator.SetBool("SwordGroundAttack",false);
        animator.SetBool("SwordDeath",false);
        animator.SetBool("SwordStunned",false);
        animator.SetBool("SwordThrowBoomerang",false);
        animator.SetBool("SwordCatchBoomerang",false);
        animator.SetBool("SwordBlock",false);
        animator.SetBool("SwordSuccesfulParry",false);
        animator.SetBool("SwordAirPush",false);
        animator.SetBool("SwordFireball",false);
       
        // Bool yerine integer kullanıyoruz
        animator.SetInteger("SwordComboCounter", 0);

        switch (currentState)
        {
            case WeaponState.Idle:
                animator.SetBool("SwordIdle", true);
                break;
                
            case WeaponState.Move:
                animator.SetBool("SwordMove", true);
                break;
            case WeaponState.Dash:
                animator.SetBool("SwordDash", true);
                break;
                
            case WeaponState.Jump:
                animator.SetBool("SwordJump", true);
                break;
            case WeaponState.Attack:
                animator.SetBool("SwordAttack",true);
                if (player.stateMachine.currentState is PlayerAttackState attackState)
                {
                    // Direkt combo counter'ı set ediyoruz
                    animator.SetInteger("SwordComboCounter", attackState.GetComboCounter());
                }
                break;
            case WeaponState.Crouch:
                animator.SetBool("SwordCrouch", true);
                break;
            case WeaponState.GroundDash:
                animator.SetBool("SwordGroundDash", true);
                break;
            case WeaponState.CrouchAttack:
                animator.SetBool("SwordGroundAttack", true);
                break;
            case WeaponState.Death:
                animator.SetBool("SwordDeath", true);
                break;
            case WeaponState.Stunned:
                animator.SetBool("SwordStunned", true);
                break;
            case WeaponState.ThrowBoomerang:
                animator.SetBool("SwordThrowBoomerang", true);
                break;
            case WeaponState.CatchBoomerang:
                animator.SetBool("SwordCatchBoomerang", true);
                break;
            case WeaponState.Parry:
                animator.SetBool("SwordBlock", true);
                break;
            case WeaponState.Spell1:
                animator.SetBool("SwordSpell1", true);
                break;
            case WeaponState.Spell2:
                animator.SetBool("SwordSpell2", true);
                break;
            case WeaponState.EarthPush:
                animator.SetBool("SwordEarthPush",true);
                break;
            case WeaponState.SuccesfulParry:
                animator.SetBool("SwordSuccesfulParry", true);
                break;
            case WeaponState.AirPush:
                animator.SetBool("SwordAirPush", true);
                break;
            case WeaponState.FireballSpell:
                animator.SetBool("SwordFireball", true);
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
        else if (player.stateMachine.currentState == player.earthPushState)
        {
            ChangeState(WeaponState.EarthPush);
        }
        else if (player.stateMachine.currentState ==player.fireballSpellState)
        {
            ChangeState(WeaponState.FireballSpell);
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
}