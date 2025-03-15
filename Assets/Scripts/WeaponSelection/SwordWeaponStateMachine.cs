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
        animator.SetBool("SwordJumpAttack", false);
        animator.SetBool("SwordDash", false);
        animator.SetBool("SwordJump",false);
        animator.SetBool("SwordFall", false);
        animator.SetBool("SwordAttack", false);
        animator.SetBool("SwordGroundDash", false);
        animator.SetBool("SwordCrouch", false);
        animator.SetBool("SwordGroundAttack",false);
        animator.SetBool("SwordDeath",false);
        animator.SetBool("SwordStunned",false);
       

        switch (currentState)
        {
            case WeaponState.Idle:
                animator.SetBool("SwordIdle", true);
                break;
                
            case WeaponState.Move:
                animator.SetBool("SwordMove", true);
                break;
            
            case WeaponState.JumpAttack:
                animator.SetBool("SwordJumpAttack", true);
                break;
                
            case WeaponState.Dash:
                animator.SetBool("SwordDash", true);
                break;
                
            case WeaponState.Jump:
                animator.SetBool("SwordJump", true);
                break;
                
            case WeaponState.Fall:
                animator.SetBool("SwordFall", true);
                break;
                
            case WeaponState.Attack:
                animator.SetBool("SwordAttack",true);
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
            
        }
    }

    protected override void Update()
    {
        base.Update();
        
        // Y ekseni hızını animator'a gönder
        animator.SetFloat("SwordyYVelocity", player.rb.linearVelocity.y);  
        
        // Input kontrolünü kaldırıyoruz, bunun yerine JumpAttackState'i ilk sıraya alıyoruz
        
        // JumpAttackState kontrolünü EN BAŞA getir
        if (!player.IsGroundDetected() && player.playerInput.attackInput)
        {
            ChangeState(WeaponState.JumpAttack);
        }
        else if (player.stateMachine.currentState == player.attackState)
        {
            ChangeState(WeaponState.Attack);
        }
        else if (player.stateMachine.currentState == player.idleState)
        {
            ChangeState(WeaponState.Idle);
        }
        else if (player.stateMachine.currentState == player.moveState) 
        {
            ChangeState(WeaponState.Move);
        }
        else if (player.stateMachine.currentState == player.dashState) 
        {
            ChangeState(WeaponState.Dash);
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
    }
}