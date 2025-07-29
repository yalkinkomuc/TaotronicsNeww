using UnityEngine;

public class BoomerangWeaponStateMachine : WeaponStateMachine
{
    protected override void Start()
    {
        base.Start();
        
        ChangeState(WeaponState.Idle);
    }

    protected override void HandleStateChange()
    {
        animator.SetBool("BoomerangIdle",false);
        animator.SetBool("BoomerangMove",false);
        animator.SetBool("BoomerangDash",false);
        animator.SetBool("BoomerangJump",false);
        animator.SetBool("BoomerangFall",false);
        
        animator.SetBool("BoomerangAttack",false);
        animator.SetBool("BoomerangGroundDash",false);
        animator.SetBool("BoomerangCrouch",false);
        animator.SetBool("BoomerangGroundAttack",false);
        animator.SetBool("BoomerangDeath",false);
        animator.SetBool("BoomerangStunned",false);
        animator.SetBool("BoomerangThrow",false);
        
        
        switch (currentState)
        {
            case WeaponState.Idle:
                animator.SetBool("BoomerangIdle", true);
                break;
                
            case WeaponState.Move:
                animator.SetBool("BoomerangMove", true);
                break;
                
            case WeaponState.Dash:
                animator.SetBool("BoomerangDash", true);
                break;
                
            case WeaponState.Jump:
                animator.SetBool("BoomerangJump", true);
                break;
            
            case WeaponState.Fall:
                animator.SetBool("BoomerangFall", true);
                break;
            case WeaponState.Attack:
                animator.SetBool("BoomerangAttack",true);
                break;
            case WeaponState.Crouch:
                animator.SetBool("BoomerangCrouch", true);
                break;
            case WeaponState.GroundDash:
                animator.SetBool("BoomerangGroundDash", true);
                break;
            case WeaponState.CrouchAttack:
                animator.SetBool("BoomerangGroundAttack", true);
                break;
            case WeaponState.Death:
                animator.SetBool("BoomerangDeath", true);
                break;
            case WeaponState.Stunned:
                animator.SetBool("BoomerangStunned", true);
                break;
            case WeaponState.ThrowBoomerang:
                animator.SetBool("BoomerangThrow", true);
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
        else if (player.stateMachine.currentState == player.attackState)
        {
            ChangeState(WeaponState.Attack);
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
        
        
    }
}
