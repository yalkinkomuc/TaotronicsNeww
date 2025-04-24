using UnityEngine;

public interface IPlayerInput 
{
     float xInput { get; }
     float yInput { get; }
     bool jumpInput { get; }
     bool dashInput { get; }
     bool crouchInput { get; }
     bool crouchInputReleased { get; }
     bool attackInput { get; }
     bool interactionInput { get; }
     
     bool parryInput { get; }
     bool spell1Input { get; }
     bool spell2Input { get; }
     bool boomerangInput { get; }
     bool voidSkillInput { get; }
     
     void DisableAllInput();
     void EnableAllInput();
     
     void DisableGameplayInput();
}
