using UnityEngine;

public abstract class ArmorStateMachine : MonoBehaviour
{
    protected ArmorState currentState = ArmorState.Idle;
    public Animator animator;
    protected Player player;

    protected virtual void Start()
    {
        
        player = GetComponentInParent<Player>();
    }

    public virtual void ChangeState(ArmorState newState)
    {
        if (currentState == newState) return;
        currentState = newState;
        HandleStateChange();
    }

    protected virtual void Update()
    {
       
    }

    protected abstract void HandleStateChange();
}
