using UnityEngine;

public class PCInput : IPlayerInput
{
    private bool inputEnabled = true;
   
    public float xInput => inputEnabled ? Input.GetAxisRaw("Horizontal") : 0f;
    public float yInput => inputEnabled ? Input.GetAxisRaw("Vertical") : 0f;
    public bool jumpInput => inputEnabled && Input.GetKeyDown(KeyCode.Space);
    public bool dashInput => inputEnabled && Input.GetKeyDown(KeyCode.LeftShift);
    public bool crouchInput => inputEnabled && Input.GetKey(KeyCode.S);
    public bool crouchInputReleased => inputEnabled && Input.GetKeyUp(KeyCode.S);
    public bool attackInput => inputEnabled && Input.GetKeyDown(KeyCode.Mouse0);
    public bool interactionInput => inputEnabled && Input.GetKeyDown(KeyCode.E);
    public bool parryInput => inputEnabled && Input.GetKeyDown(KeyCode.Q);
    public bool spell1Input => inputEnabled && Input.GetKeyDown(KeyCode.R);
    public bool spell2Input => inputEnabled && Input.GetKey(KeyCode.T);
    public bool boomerangInput => inputEnabled && Input.GetKeyDown(KeyCode.Mouse1);
    
    public void DisableAllInput()
    {
        inputEnabled = false;
    }
    
    public void EnableAllInput()
    {
        inputEnabled = true;
    }
}
