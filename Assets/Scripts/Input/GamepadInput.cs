using UnityEngine;

public class GamepadInput : IPlayerInput
{
    
    private bool inputEnabled = true;

    public float xInput => inputEnabled ? Input.GetAxisRaw("Horizontal"): 0f;
    public float yInput => inputEnabled ? Input.GetAxisRaw("Vertical") : 0f;
    public bool jumpInput => inputEnabled && Input.GetKeyDown(KeyCode.JoystickButton0);
    public bool dashInput => inputEnabled && Input.GetKeyDown(KeyCode.JoystickButton1);
    public bool crouchInput => inputEnabled && yInput < -0.5f;
    public bool crouchInputReleased => inputEnabled && yInput >= -0.5f;
    public bool attackInput => inputEnabled && Input.GetKeyDown(KeyCode.JoystickButton2);
    public bool interactionInput => inputEnabled && Input.GetKeyDown(KeyCode.JoystickButton3);
    public bool parryInput => inputEnabled && Input.GetKey(KeyCode.JoystickButton4);
    public bool spell1Input => inputEnabled && Input.GetKeyDown(KeyCode.JoystickButton7);
    public bool spell2Input => inputEnabled && Input.GetKeyDown(KeyCode.JoystickButton8);
    public bool boomerangInput => inputEnabled && Input.GetKeyDown(KeyCode.JoystickButton9);
    
    public void DisableAllInput()
    {
        inputEnabled = false;
    }
    
    public void EnableAllInput()
    {
        inputEnabled = true;
    }
}
