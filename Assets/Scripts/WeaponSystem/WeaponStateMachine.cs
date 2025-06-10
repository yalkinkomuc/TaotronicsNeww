using System;
using UnityEngine;

public abstract class WeaponStateMachine : MonoBehaviour
{
    protected WeaponState currentState = WeaponState.Idle;
    public Animator animator;
    protected Player player;

    protected virtual void Start()
    {
        animator = GetComponent<Animator>();
        player = GetComponentInParent<Player>();
    }

    public virtual void ChangeState(WeaponState newState)
    {
        if (currentState == newState) return;
        currentState = newState;
        HandleStateChange();
    }

    protected virtual void Update()
    {
        if (animator == null)
        {

        }
    }

    protected abstract void HandleStateChange();
}