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
