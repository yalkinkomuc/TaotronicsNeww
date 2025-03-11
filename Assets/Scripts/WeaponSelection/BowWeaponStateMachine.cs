using UnityEngine;

public class BowWeaponStateMachine : WeaponStateMachine
{
    private Enemy_ArcherSkeleton archerSkeleton; // Player yerine Archer'ı referans alacağız

    protected override void Start()
    {
        base.Start();
        archerSkeleton = GetComponentInParent<Enemy_ArcherSkeleton>();
    }

    protected override void HandleStateChange()
    {
        // Önce tüm bool değerlerini false yap
        animator.SetBool("BowIdle", false);
        animator.SetBool("BowMove", false);
        animator.SetBool("BowAttack", false);
        animator.SetBool("BowDeath",false);
        // Diğer state'ler için de ekleyebilirsiniz

        switch (currentState)
        {
            case WeaponState.Idle:
                animator.SetBool("BowIdle", true);
                break;
            case WeaponState.Move:
                animator.SetBool("BowMove", true);
                break;
            case WeaponState.Attack:
                animator.SetBool("BowAttack", true);
                break;
            case WeaponState.Death:
                animator.SetBool("BowDeath", true);
                break;
            // Diğer state'leri de ekleyebilirsiniz
        }
    }

    protected override void Update()
    {
        base.Update();

        // Archer'ın state'ine göre yayın state'ini güncelle
        if (archerSkeleton.stateMachine.currentState == archerSkeleton.idleState)
        {
            ChangeState(WeaponState.Idle);
        }
        else if (archerSkeleton.stateMachine.currentState == archerSkeleton.moveState)
        {
            ChangeState(WeaponState.Move);
        }
        else if (archerSkeleton.stateMachine.currentState == archerSkeleton.attackState)
        {
            ChangeState(WeaponState.Attack);
        }
        else if (archerSkeleton.stateMachine.currentState == archerSkeleton.deadState)
        {
            ChangeState(WeaponState.Death);
        }
    }
} 