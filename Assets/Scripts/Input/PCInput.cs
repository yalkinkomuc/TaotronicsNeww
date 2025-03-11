using UnityEngine;

public class PCInput : IPlayerInput
{

   
    public float xInput => Input.GetAxisRaw("Horizontal");
    public float yInput => Input.GetAxisRaw("Vertical");
    public bool jumpInput => Input.GetKeyDown(KeyCode.Space);
    public bool dashInput => Input.GetKeyDown(KeyCode.LeftShift);
    public bool crouchInput => Input.GetKey(KeyCode.S);
    public bool crouchInputReleased => Input.GetKeyUp(KeyCode.S);
    public bool attackInput => Input.GetKeyDown(KeyCode.Mouse0);
    public bool interactionInput => Input.GetKeyDown(KeyCode.E);
    
}
