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
     
     bool inventoryInput { get; }
     
     // Tab switching inputs (UI only)
     bool tabLeftInput { get; }
     bool tabRightInput { get; }
     
     // Menu close input (UI only)
     bool escapeInput { get; }
     
     
}
