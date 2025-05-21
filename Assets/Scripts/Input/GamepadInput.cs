using UnityEngine;

public class GamepadInput : IPlayerInput
{
    // Input states
    private bool inputEnabled = true;
    private bool gameplayInputEnabled = true;

    // Movement and action inputs - affected by gameplayInputEnabled
    public float xInput => inputEnabled && gameplayInputEnabled ? Input.GetAxisRaw("Horizontal") : 0f;
    public float yInput => inputEnabled && gameplayInputEnabled ? Input.GetAxisRaw("Vertical") : 0f;
    public bool jumpInput => inputEnabled && gameplayInputEnabled && Input.GetKeyDown(KeyCode.JoystickButton0);
    public bool dashInput => inputEnabled && gameplayInputEnabled && Input.GetKeyDown(KeyCode.JoystickButton1);
    public bool crouchInput => inputEnabled && gameplayInputEnabled && yInput < -0.5f;
    public bool crouchInputReleased => inputEnabled && gameplayInputEnabled && yInput >= -0.5f;
    public bool attackInput => inputEnabled && gameplayInputEnabled && Input.GetKeyDown(KeyCode.JoystickButton2);
    public bool interactionInput => inputEnabled && gameplayInputEnabled && Input.GetKeyDown(KeyCode.JoystickButton3);
    public bool parryInput => inputEnabled && gameplayInputEnabled && Input.GetKeyDown(KeyCode.JoystickButton4);
    public bool spell1Input => inputEnabled && gameplayInputEnabled && Input.GetKeyDown(KeyCode.JoystickButton7);
    public bool spell2Input => inputEnabled && gameplayInputEnabled && Input.GetKeyDown(KeyCode.JoystickButton8);
    public bool boomerangInput => inputEnabled && gameplayInputEnabled && Input.GetKeyDown(KeyCode.JoystickButton9);
    public bool voidSkillInput => inputEnabled && gameplayInputEnabled && Input.GetKeyDown(KeyCode.JoystickButton5); // Void becerisi için JoystickButton5
    
    public bool electricDashInput => inputEnabled && gameplayInputEnabled && Input.GetKeyDown(KeyCode.Joystick1Button11);
    
    public bool earthPushInput => inputEnabled && gameplayInputEnabled && Input.GetKeyDown(KeyCode.Joystick1Button10);
    
    public bool airPushInput => inputEnabled && gameplayInputEnabled && Input.GetKeyDown(KeyCode.Joystick1Button12);
    
    // Disables all input (including UI)
    public void DisableAllInput()
    {
        inputEnabled = false;
        gameplayInputEnabled = false;
    }
    
    // Enables all input
    public void EnableAllInput()
    {
        inputEnabled = true;
        gameplayInputEnabled = true;
    }
    
    // Disables ONLY gameplay input (UI input remains active)
    public void DisableGameplayInput()
    {
        gameplayInputEnabled = false;
        inputEnabled = true; // Input system itself stays active, only gameplay inputs disabled
    }
}
