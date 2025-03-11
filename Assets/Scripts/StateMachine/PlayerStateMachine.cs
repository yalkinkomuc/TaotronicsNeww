using UnityEngine;

public class PlayerStateMachine 
{
   public PlayerState currentState { get; private set; }

   public void Initialize(PlayerState _startState)
   {
      currentState = _startState;
      currentState.Enter();
   }

   public void ChangeState(PlayerState _newState)
   {
      // Diyalog açıkken state değişimlerine izin verme
      if (DialogueManager.Instance.IsDialogueActive)
         return;
            
      currentState.Exit();
      currentState = _newState;
      currentState.Enter();
   }
   
   
}
