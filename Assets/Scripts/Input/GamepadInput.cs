using UnityEngine;

public class GamepadInput : IPlayerInput
{
    public float xInput => Input.GetAxisRaw("Horizontal");
    public float yInput => Input.GetAxisRaw("Vertical");
    public bool jumpInput => Input.GetKeyDown(KeyCode.JoystickButton0);
    public bool dashInput => Input.GetKeyDown(KeyCode.JoystickButton1);
    public bool crouchInput => yInput < -0.5f;
    public bool crouchInputReleased => yInput >= -0.5f;
    public bool attackInput => Input.GetKeyDown(KeyCode.JoystickButton2);
    public bool interactionInput => Input.GetKeyDown(KeyCode.JoystickButton3);
}
