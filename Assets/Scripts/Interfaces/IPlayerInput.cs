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
}
