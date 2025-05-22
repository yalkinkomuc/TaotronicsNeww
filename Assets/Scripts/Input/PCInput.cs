using UnityEngine;

public class PCInput : IPlayerInput
{
    // Input states
    private bool inputEnabled = true;
    private bool gameplayInputEnabled = true;
   
    // Movement and action inputs - affected by gameplayInputEnabled
    public float xInput => inputEnabled && gameplayInputEnabled ? Input.GetAxisRaw("Horizontal") : 0f;
    public float yInput => inputEnabled && gameplayInputEnabled ? Input.GetAxisRaw("Vertical") : 0f;
    public bool jumpInput => inputEnabled && gameplayInputEnabled && Input.GetKeyDown(KeyCode.Space);
    public bool dashInput => inputEnabled && gameplayInputEnabled && Input.GetKeyDown(KeyCode.LeftShift);
    public bool crouchInput => inputEnabled && gameplayInputEnabled && Input.GetKey(KeyCode.S);
    public bool crouchInputReleased => inputEnabled && gameplayInputEnabled && Input.GetKeyUp(KeyCode.S);
    public bool attackInput => inputEnabled && gameplayInputEnabled && Input.GetKeyDown(KeyCode.Mouse0);
    public bool interactionInput => inputEnabled && gameplayInputEnabled && Input.GetKeyDown(KeyCode.E);
    public bool parryInput => inputEnabled && gameplayInputEnabled && Input.GetKeyDown(KeyCode.Q);
    public bool spell1Input => inputEnabled && gameplayInputEnabled && Input.GetKeyDown(KeyCode.R);
    public bool spell2Input => inputEnabled && gameplayInputEnabled && Input.GetKey(KeyCode.T);
    public bool earthPushInput => inputEnabled && gameplayInputEnabled && Input.GetKeyDown(KeyCode.F);
    public bool boomerangInput => inputEnabled && gameplayInputEnabled && Input.GetKeyDown(KeyCode.Mouse1);
    public bool voidSkillInput => inputEnabled && gameplayInputEnabled && Input.GetKeyDown(KeyCode.X);
    public bool electricDashInput => inputEnabled && gameplayInputEnabled && Input.GetKeyDown(KeyCode.C);
    public bool airPushInput => inputEnabled && gameplayInputEnabled && Input.GetKeyDown(KeyCode.V);
    public bool fireballInput => inputEnabled && gameplayInputEnabled && Input.GetKeyDown(KeyCode.G);
    
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
