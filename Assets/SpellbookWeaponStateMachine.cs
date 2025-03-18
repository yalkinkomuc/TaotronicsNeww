using UnityEngine;

public class SpellbookWeaponStateMachine : WeaponStateMachine
{

    protected override void Start()
    {
        base.Start();

        ChangeState(WeaponState.Idle);
    }

    protected override void HandleStateChange()
    {
        animator.SetBool("Idle",false);
        animator.SetBool("Spell1",false);
        animator.SetBool("Spell2",false);

        switch (currentState)
        {
            case WeaponState.Idle:
                animator.SetBool("Idle",true);
                break;
            case WeaponState.Spell1:
                animator.SetBool("Spell1", true);
                break;
            case WeaponState.Spell2:
                animator.SetBool("Spell2", true);
            break ;
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
        else if (player.stateMachine.currentState == player.spell1State)
        {
            ChangeState(WeaponState.Spell1);
        }
        else if (player.stateMachine.currentState == player.spell2State)
        {
            ChangeState(WeaponState.Spell2);
        }
    }

}
